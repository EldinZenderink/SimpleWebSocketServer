using SimpleWebSocketServerLibrary.WSocketServer;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SimpleWebSocketServerLibrary.SimpleWebSocketHandler
{
    /// <summary>
    /// Inteface for class: WebSocketHandler.
    /// </summary>
    public interface IWebSocketHandler
    {
        /// <summary>
        /// Event handler for events such as messages and errors.
        /// </summary>
        event EventHandler<WebSocketEventArg> WebsocketEvent;
        /// <summary>
        /// Send a message to a specific client.
        /// </summary>
        /// <param name="message">Message container containing everything related to the message.</param>
        /// <param name="clientId">Id of the client to send the message to.</param>
        /// <returns>True on success.</returns>
        Task<bool> SendMessage(WebSocketMessageContainer message, string clientId);
        /// <summary>
        /// Sends a message to all connected clients.
        /// </summary>
        /// <param name="message">Message container containing everything related to the message.</param>
        /// <returns>True on success.</returns>
        Task<bool> SendMessage(WebSocketMessageContainer message);
        /// <summary>
        /// Starts a connection given the settings set by caller.
        /// </summary>
        /// <param name="client">TcpClient socket handler.</param>
        /// <param name="stream">NetworkStream stream handler.</param>
        /// <param name="clientId">Id of client.</param>
        /// <param name="baseUrl">URL path used by client.</param>
        void StartConnection(TcpClient client, string clientId, string baseUrl);
        /// <summary>
        /// Stops a connection with a specific client.
        /// </summary>
        /// <param name="clientId">Id of client of which to stop the connection with.</param>
        /// <returns>True on success.</returns>
        Task<bool> StopClient(string clientId);
        /// <summary>
        /// Stops a client while specifying a reason.
        /// </summary>
        /// <param name="clientId">Id of client of which to stop the connection with.</param>
        /// <param name="reason">Reason to send to client.</param>
        /// <returns>True on success.</returns>
        Task<bool> StopClient(string clientId, string reason);
        /// <summary>
        /// Stops all connections.
        /// </summary>
        /// <returns>True on success.</returns>
        Task<bool> StopAll();
        /// <summary>
        /// Stops all connections and provides a reason to the clients.
        /// </summary>
        /// <param name="reason">Reason for disconnect.</param>
        /// <returns>True on success.</returns>
        Task<bool> StopAll(string reason);
    }


    /// <summary>
    /// Class contains handler for handeling multiple websocket clients and hubs.
    /// </summary>
    public class WebSocketHandler : IWebSocketHandler
    {
        private readonly int _BufferSize;
        private readonly Dictionary<WebSocketClientInfo, WebSocketServer> _ListWithConnections;

        public event EventHandler<WebSocketEventArg> WebsocketEvent;

        /// <summary>
        /// Constructor for websocket handler for automatic control of the websocket server.
        /// </summary>
        /// <param name="bufferSize">Sets the receive buffer size, default = 4096 bytes (Messages received larger than buffer will be cut off/incomplete).</param>
        public WebSocketHandler(int bufferSize = 4096)
        {
            _BufferSize = bufferSize;
            _ListWithConnections = new Dictionary<WebSocketClientInfo, WebSocketServer>();
        }

        /// <summary>
        /// Starts a connection given the settings set by caller.
        /// </summary>
        /// <param name="client">TcpClient socket handler.</param>
        /// <param name="stream">NetworkStream stream handler.</param>
        /// <param name="clientId">Id of client.</param>
        /// <param name="baseUrl">URL path used by client.</param>
        public async void StartConnection(TcpClient client,  string clientId, string baseUrl)
        {
            WebSocketClientInfo info = new WebSocketClientInfo()
            {
                client = client,
                clientId = clientId,
                clientBaseUrl = baseUrl
            };

            WebSocketServer newServer = new WebSocketServer(info, _BufferSize);
            newServer.WebSocketServerEvent += OnWebsocketEvent;
            _ListWithConnections.Add(info, newServer);
            await newServer.StartServerAsync();
        }

        /// <summary>
        /// Stops a connection with a specific client.
        /// </summary>
        /// <param name="clientId">Id of client of which to stop the connection with.</param>
        /// <returns>True on success.</returns>
        public async Task<bool> StopClient(string clientId)
        {
            bool succes = false;
            WebSocketClientInfo key = null;
            foreach (var connection in _ListWithConnections)
            {
                if (connection.Key.clientId == clientId)
                {
                    succes = await connection.Value.StopServerAsync();
                    key = connection.Key;
                    break;
                }
            }
            if (succes)
            {
                _ListWithConnections.Remove(key);
            }

            return succes;
        }

        /// <summary>
        /// Stops a client while specifying a reason.
        /// </summary>
        /// <param name="clientId">Id of client of which to stop the connection with.</param>
        /// <param name="reason">Reason to send to client.</param>
        /// <returns>True on success.</returns>
        public async Task<bool> StopClient(string clientId, string reason)
        {
            bool succes = false;
            WebSocketClientInfo key = null;
            foreach (var connection in _ListWithConnections)
            {
                if (connection.Key.clientId == clientId)
                {
                    succes = await connection.Value.StopServerAsync(reason);
                    key = connection.Key;
                    break;
                }
            }

            if (succes)
            {
                _ListWithConnections.Remove(key);
            }

            return succes;
        }

        /// <summary>
        /// Stops all connections.
        /// </summary>
        /// <returns>True on success.</returns>
        public async Task<bool> StopAll()
        {
            bool succes = false;
            foreach (var connection in _ListWithConnections)
            {
                succes = await connection.Value.StopServerAsync();
            }

            if (succes)
            {
                _ListWithConnections.Clear();
            }

            return succes;
        }

        /// <summary>
        /// Stops all connections and provides a reason to the clients.
        /// </summary>
        /// <param name="reason">Reason for disconnect.</param>
        /// <returns>True on success.</returns>
        public async Task<bool> StopAll(string reason)
        {
            bool succes = false;
            foreach (var connection in _ListWithConnections)
            {
                succes = await connection.Value.StopServerAsync();
            }

            if (succes)
            {
                _ListWithConnections.Clear();
            }
            return succes;
        }

        /// <summary>
        /// Send a message to a specific client.
        /// </summary>
        /// <param name="message">Message container containing everything related to the message.</param>
        /// <param name="clientId">Id of the client to send the message to.</param>
        /// <returns>True on success.</returns>
        public async Task<bool> SendMessage(WebSocketMessageContainer message, string clientId)
        {
            foreach (var connection in _ListWithConnections)
            {
                if (connection.Key.clientId == clientId)
                {
                    return await connection.Value.SendMessageAsync(message);
                }
            }

            return false;
        }

        /// <summary>
        /// Sends a message to all connected clients.
        /// </summary>
        /// <param name="message">Message container containing everything related to the message.</param>
        /// <returns>True on success.</returns>
        public async Task<bool> SendMessage(WebSocketMessageContainer message)
        {
            bool succes = false;
            foreach (var connection in _ListWithConnections)
            {
                
                succes = await connection.Value.SendMessageAsync(message);
            }

            return succes;
        }

        /// <summary>
        /// WebSocket Server event handler, fires the event.
        /// </summary>
        /// <param name="arg">Arguments containing event arguments.</param>
        public void OnWebsocketEvent(object sender, WebSocketEventArg arg)
        {

            if (arg.isClosed)
            {
                WebSocketClientInfo key = null;
                bool found = false;

                foreach (var connection in _ListWithConnections)
                {
                    if (connection.Key.clientId == arg.clientId)
                    {
                        key = connection.Key;
                        found = true;
                        break;
                    }
                }
                if (found)
                {
                    _ListWithConnections.Remove(key);
                }
            }
           

            WebsocketEvent?.Invoke(this, arg);
        }
    }
}
