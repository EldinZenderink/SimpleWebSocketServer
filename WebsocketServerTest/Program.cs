using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SimpleWebSocketServerLibrary;

namespace WebsocketServerTest
{
    class Program
    {
        private static SimpleWebSocketServer _WebsocketServer;
        static void Main(string[] args)
        {

            Console.WriteLine("WebSocket Server is running, press a key to stop the server!");

            SimpleWebSocketServer websocketServer = new SimpleWebSocketServer(new SimpleWebSocketServerSettings());

            websocketServer.WebsocketServerEvent += OnWebsocketEvent;
            websocketServer.StartServer();

            _WebsocketServer = websocketServer;

            Console.ReadKey();

            bool stopped = _WebsocketServer.StopAll("Server had enough, RIP server.");

            if (stopped)
            {
                Console.WriteLine("Succesfully closed client :D, press key to close.");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("Unsuccesfully closed client :(");
                Console.ReadKey();
            }

        }


        private static async void OnWebsocketEvent(object sender, WebSocketEventArg args)
        {
            if (args.data != null && args.isText)
            {
                string received = Encoding.UTF8.GetString(args.data);
                Console.WriteLine("Received message with length: " + args.messageLength + " from client: " + args.clientId + " on url: " + args.clientBaseUrl + ":");
                Console.WriteLine(received);
                await _WebsocketServer.SendTextMessageAsync("Client: " + args.clientId + " on url: " + args.clientBaseUrl + ", says: " + received);
            }
            else
            {
                if (args.isOpen)
                {
                    Console.WriteLine("Client: " + args.clientId + " connected!");
                    await _WebsocketServer.SendTextMessageAsync("Client: " + args.clientId + " on url: " + args.clientBaseUrl + ", says: " + "joined!");
                }
            }
            if (args.isClosed)
            {
                if (args.data != null)
                {
                    string received = Encoding.UTF8.GetString(args.data);
                    Console.WriteLine("Client: " + args.clientId + " disconnected with message:");
                    Console.WriteLine(received);
                }
                else
                {
                    string received = Encoding.UTF8.GetString(args.data);
                    Console.WriteLine("Client: " + args.clientId + " disconnected.");
                }
            }
        }
    }
}
