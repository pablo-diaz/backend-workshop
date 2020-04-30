using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Net.Sockets;
using System.Globalization;

using CsvHelper;
using System.Collections.Generic;

namespace _01_Server
{
    sealed class LogEntry
    {
        public string Machine { get; set; }
        public string Data { get; set; }
        public DateTime When { get; set; }

        public static LogEntry Parse(string message)
        {
            var messageParts = message.Split('\n');
            if (messageParts.Length != 3)
                throw new ApplicationException("Invalid message format");

            if(!messageParts[1].ToUpper().Trim().StartsWith("MACHINE:"))
                throw new ApplicationException("Invalid message format");

            var machineParts = messageParts[1].Split(':');
            if(machineParts.Length != 2)
                throw new ApplicationException("Invalid message format");

            var machine = machineParts[1].Trim().ToUpper();

            if (!messageParts[2].ToUpper().Trim().StartsWith("DATA:"))
                throw new ApplicationException("Invalid message format");

            var data = messageParts[2].Substring(6);

            return new LogEntry() { 
                Machine = machine,
                Data = data,
                When = DateTime.UtcNow
            };
        }

        public override string ToString() =>
            $"[{string.Format("{0:yyyy-MM-dd HH:mm:ss}", When)}] Machine: {Machine} - Data: {Data}";
    }

    public class Program
    {
        private const string dbFilePath = @"C:\temp\YPDDB.csv";

        public static void Main(string[] args)
        {
            var portToListenFrom = 12345;
            using (var serverSocket = StartServer(portToListenFrom))
            {
                ProcessRequests(serverSocket);
            }
        }

        private static Socket StartServer(int port)
        {
            var serverIP = GetServerIP();
            var serverSocket = CreateServerSocket(serverIP, port);

            const int maxNumberOfIncomingConnectionsExpected = 1;
            serverSocket.Listen(maxNumberOfIncomingConnectionsExpected);

            Console.WriteLine($"Listening on IP {serverIP} on port {port} ....");
            return serverSocket;
        }

        private static IPAddress GetServerIP()
        {
            var hostName = Dns.GetHostName();
            var hostAddresses = Dns.GetHostEntry(hostName).AddressList;
            var hostIPAddress = hostAddresses.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            return hostIPAddress;
        }

        private static Socket CreateServerSocket(IPAddress serverIP, int port)
        {
            var listenSocket = new Socket(AddressFamily.InterNetwork,
                                          SocketType.Stream,
                                          ProtocolType.Tcp);

            var endpoint = new IPEndPoint(serverIP, port);
            listenSocket.Bind(endpoint);

            return listenSocket;
        }

        private static void ProcessRequests(Socket serverSocket)
        {
            while(true)
            {
                using (var clientSocket = serverSocket.Accept())
                {
                    var messageFromClient = GetMessageFromClient(clientSocket);
                    var responseMessage = ProcessMessageFromClient(messageFromClient);
                    SendMessageToClient(clientSocket, responseMessage);
                }
            }
        }

        private static string GetMessageFromClient(Socket clientSocket)
        {
            int messageBufferSizeInBytes = 1024;
            var receiveBuffer = new byte[messageBufferSizeInBytes];
            var receivedByteCount = clientSocket.Receive(receiveBuffer, messageBufferSizeInBytes, SocketFlags.None);
            return Encoding.UTF8.GetString(receiveBuffer, 0, receivedByteCount);
        }

        private static string ProcessMessageFromClient(string message)
        {
            const string notCompliantWithYWPError = "No cumple con el protocolo YWP";
            if (string.IsNullOrEmpty(message))
                return notCompliantWithYWPError;

            if (message.StartsWith("GET"))
                return ProcessGet(message);

            if (message.StartsWith("LOG"))
            {
                var error = ProcessLog(message);
                if (!string.IsNullOrEmpty(error))
                    return error;
                return "Log entry saved";
            }

            return notCompliantWithYWPError;
        }

        private static void SendMessageToClient(Socket socket, string message)
        {
            var bytesToSent = Encoding.ASCII.GetBytes(message);
            socket.Send(bytesToSent, bytesToSent.Length, SocketFlags.None);
        }

        private static string ProcessLog(string message)
        {
            try
            {
                var log = LogEntry.Parse(message);

                bool doesFileExist = File.Exists(dbFilePath);
                using (var writer = new StreamWriter(dbFilePath, true, Encoding.Default))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    if(!doesFileExist)
                        csv.WriteHeader<LogEntry>();
                    csv.NextRecord();
                    csv.WriteRecord(log);
                }
                return null;
            }
            catch (ApplicationException ex)
            {
                return ex.Message;
            }
        }

        private static string ProcessGet(string message)
        {
            var machineRequested = GetMachineInRequest(message);
            var existingRecords = new List<string>();
            using (var reader = new StreamReader(dbFilePath, Encoding.Default))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                existingRecords = csv.GetRecords<LogEntry>()
                    .Where(r => string.IsNullOrEmpty(machineRequested) || r.Machine == machineRequested)
                    .Select(r => r.ToString())
                    .ToList();
            }

            var result = "";
            existingRecords.ForEach(r => result += $"{r}\n");
            return result;
        }

        private static string GetMachineInRequest(string request)
        {
            var requestParts = request.Split('\n');
            if(requestParts.Length == 2 && requestParts[1].Trim().ToUpper().StartsWith("FILTERED:"))
            {
                var filteredParts = requestParts[1].Split(':');
                if (filteredParts.Length == 2)
                    return filteredParts[1].ToUpper().Trim();
                return null;
            }
            return null;
        }
    }
}
