using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Net.Sockets;

using CsvHelper;
using System.Globalization;
using CsvHelper.Configuration.Attributes;

namespace _01_Server
{
    sealed class LogEntry
    {
        [Name("Machine")]
        public string Machine { get; }

        [Name("Data")]
        public string Data { get; }

        [Name("When")]
        public DateTime When { get;  }

        private LogEntry(string machine, string data, DateTime when)
        {
            this.Machine = machine;
            this.Data = data;
            this.When = when;
        }

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

            return new LogEntry(machine, data, DateTime.UtcNow);
        }
    }

    public class Program
    {
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
            const string dbFilePath = @"C:\temp\YPDDB.csv";
            try
            {
                var log = LogEntry.Parse(message);
                using (var writer = new StreamWriter(dbFilePath))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
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
            // TODO: Complete
            return "Se va a hacer un GET";
        }
    }
}
