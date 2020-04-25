using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace _01_Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            bool areThereCommandArguments = args.Length == 2;
            var serverIP = areThereCommandArguments ? args[0] : "169.254.60.173";
            var serverPort = areThereCommandArguments ? Convert.ToInt32(args[1]) : 12345;

            using (var socket = ConnectToServer(serverIP, serverPort))
            {
                SendMessagesToServer(socket);
            }
        }

        private static Socket ConnectToServer(string serverIP, int serverPort)
        {
            var endpoint = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
            var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            socket.Connect(endpoint);
            if (!socket.Connected)
                throw new ApplicationException($"We cannot connect to server in IP {serverIP} on port {serverPort}");

            return socket;
        }

        private static void SendMessagesToServer(Socket socket)
        {
            var message = "Hola Mundo desde el Cliente";
            SendMessageToServer(socket, message);
        }

        private static void SendMessageToServer(Socket socket, string message)
        {
            var bytesToSent = Encoding.ASCII.GetBytes(message);
            socket.Send(bytesToSent, bytesToSent.Length, SocketFlags.None);
        }
    }
}
