using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace _01_Server
{
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
                    ProcessMessageFromClient(messageFromClient);
                }
            }
        }

        private static string GetMessageFromClient(Socket clientSocket)
        {
            int messageBufferSizeInBytes = 1024;
            var receiveBuffer = new byte[messageBufferSizeInBytes];

            int receivedByteCount;
            var messageReceived = "";
            do
            {
                receivedByteCount = clientSocket.Receive(receiveBuffer, SocketFlags.None);
                messageReceived += Encoding.UTF8.GetString(receiveBuffer, 0, receivedByteCount);
            } while (receivedByteCount > 0);
            return messageReceived;
        }

        private static void ProcessMessageFromClient(string message)
        {
            Console.WriteLine($"Este es el mensaje recibido: '{message}'");
        }
    }
}
