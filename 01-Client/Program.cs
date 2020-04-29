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
            bool areThereCommandArguments = args.Length == 3;
            var serverIP = areThereCommandArguments ? args[0] : "169.254.60.173";
            var serverPort = areThereCommandArguments ? Convert.ToInt32(args[1]) : 12345;
            var messageToSend = areThereCommandArguments ? args[2] : "Hola Mundo nuevamente desde el cliente";

            using (var socket = ConnectToServer(serverIP, serverPort))
            {
                SendMessagesToServer(socket, messageToSend);
                var messageFromServer = ReceiveMessageFromServer(socket);
                Console.WriteLine($"Mensaje recibido del server: {messageFromServer}");
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

        private static void SendMessagesToServer(Socket socket, string messageToSend)
        {
            SendMessageToServer(socket, messageToSend);
        }

        private static void SendMessageToServer(Socket socket, string message)
        {
            var bytesToSent = Encoding.ASCII.GetBytes(message);
            socket.Send(bytesToSent, bytesToSent.Length, SocketFlags.None);
        }

        private static string ReceiveMessageFromServer(Socket socket)
        {
            int messageBufferSizeInBytes = 1024;
            var receiveBuffer = new byte[messageBufferSizeInBytes];
            var receivedByteCount = socket.Receive(receiveBuffer, SocketFlags.None);
            return Encoding.UTF8.GetString(receiveBuffer, 0, receivedByteCount);
        }
    }
}
