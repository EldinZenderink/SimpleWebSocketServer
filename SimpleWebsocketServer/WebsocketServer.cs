using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleWebSocketServerLibrary
{
    /// <summary>
    /// Runs the websocket server to communicate with a client.
    /// </summary>
    class WebSocketServer
    {
        /// <summary>
        /// Global buffer size used.
        /// </summary>
        private readonly int _BufferSize;

        /// <summary>
        /// Global WebSocketClientInfo.
        /// </summary>
        private readonly WebSocketClientInfo _ClientInfo;

        /// <summary>
        /// Global WebSocketHandler.
        /// </summary>
        private readonly WebSocketHandler _Handler;

        /// <summary>
        /// Start a websocket server.
        /// </summary>
        /// <param name="bufferSize">Sets the receive buffer size, default = 4096</param>
        /// <param name="clientInfo">Sets the client information.</param>
        /// <param name="handler">Sets the websocket handler.</param>
        public WebSocketServer( WebSocketClientInfo clientInfo, WebSocketHandler handler, int bufferSize = 4096)
        {
            _BufferSize = bufferSize;
            _ClientInfo = clientInfo;
            _Handler = handler;
        }

        /// <summary>
        /// Sends a websocket framed message to the client.
        /// </summary>
        /// <param name="message">Message container containing all the information needed for the message.</param>
        /// <returns>True on success.</returns>
        public async Task<bool> SendMessage(WebSocketMessageContainer message)
        {
            if (_ClientInfo.client.Connected && _ClientInfo.stream.CanWrite )
            {
                UInt64 messageLength = (ulong)message.data.Length;

                List<byte> websocketFrame = new List<byte>();


                byte start = 0x00;

                if (message.isText)
                {
                    start = 0x81;
                }
                if (message.isBinary)
                {
                    start = 0x82;
                }
                if (message.isClosed)
                {
                    start = 0x88;
                }
                if (message.isPing)
                {
                    start = 0x89;
                }
                if (message.isPong)
                {
                    start = 0x8A;
                }
                
                websocketFrame.Add(start);

                if (messageLength <= 125)
                {

                    websocketFrame.Add((byte)messageLength);
                    websocketFrame.AddRange(message.data);

                }
                else if (messageLength > 125 && messageLength <= 65535)
                {
                    UInt16 messageLength16bit = (UInt16)messageLength;
                    byte[] byteArray = BitConverter.GetBytes(messageLength16bit).Reverse<byte>().ToArray();

                    websocketFrame.Add((byte)126);
                    websocketFrame.AddRange(byteArray);
                    websocketFrame.AddRange(message.data);

                }
                else
                {

                    byte[] byteArray = BitConverter.GetBytes(messageLength).Reverse<byte>().ToArray();
                    websocketFrame.Add((byte)127);
                    websocketFrame.AddRange(byteArray);
                    websocketFrame.AddRange(message.data);
                }
                await _ClientInfo.stream.WriteAsync(websocketFrame.ToArray(), 0, websocketFrame.ToArray().Length);
                await _ClientInfo.stream.FlushAsync();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Stops the websocket server and sends over a reason.
        /// </summary>
        /// <param name="reason">Overload parameter for reason. </param>
        /// <returns>True on success.</returns>
        public async Task<bool> StopServer(string reason = "Server closed connection.")
        {
            WebSocketMessageContainer message = new WebSocketMessageContainer()
            {
                data = Encoding.UTF8.GetBytes(reason),
                isClosed = true
            };
            bool succes = await SendMessage(message);

            if (succes)
            {
                _ClientInfo.stream.Close();
                _ClientInfo.client.Close();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Starts the WebSocket server / connection with the client.
        /// </summary>
        /// <returns></returns>
        public async Task StartServer()
        {
            byte[] buffer = new byte[_BufferSize];

            List<byte> data = new List<byte>();

            bool continuationFrame = false;

            WebSocketEventArg messageArg = new WebSocketEventArg();

            bool isConnected = false;

            while (_ClientInfo.client.Connected)
            {
                messageArg = new WebSocketEventArg()
                {
                    clientBaseUrl = _ClientInfo.clientBaseUrl,
                    clientId =  _ClientInfo.clientId
                };

                if (!isConnected)
                {
                    Thread.Sleep(500);
                    messageArg.clientId = _ClientInfo.clientId;
                    messageArg.isOpen = true;
                    isConnected = true;
                    _Handler.OnWebsocketEvent(messageArg);
                }

                int bytesRead = await _ClientInfo.stream.ReadAsync(buffer, 0, buffer.Length);
                await _ClientInfo.stream.FlushAsync();
                int opCode = buffer[0] & 0x0F;
                bool finalMessage = ((buffer[0] & 0x80) == 0x80);
                bool maskKey = ((buffer[1] & 0x80) == 0x80);
                UInt64 payloadLength = 0;
                int initialPayloadLength = buffer[1] & 0x7F;


                switch (opCode)
                {
                    case 0x00:
                        continuationFrame = true;
                        break;
                    case 0x01:
                        continuationFrame = false;
                        messageArg.isText = true;
                        break;
                    case 0x02:
                        continuationFrame = false;
                        messageArg.isBinary = true;
                        break;
                    case 0x08:
                        continuationFrame = false;
                        messageArg.isClosed = true;
                        break;
                    case 0x09:
                        continuationFrame = false;
                        messageArg.isPing = true;
                        break;
                    case 0x0A:
                        continuationFrame = false;
                        messageArg.isPong = true;
                        break;
                    default:
                        _ClientInfo.stream.Close();
                        _ClientInfo.client.Close();
                        break;
                }


                byte[] payloadLengthBytes;
                byte[] maskKeyBytes = new byte[4];

                switch (initialPayloadLength)
                {
                    case 126:
                        payloadLengthBytes = new byte[2];
                        Array.Copy(buffer, 2, payloadLengthBytes, 0, payloadLengthBytes.Length);
                        payloadLength = BitConverter.ToUInt16(payloadLengthBytes.Reverse<byte>().ToArray(), 0);

                        messageArg.messageLength = payloadLength;
                        if (maskKey)
                        {
                            Array.Copy(buffer, 4, maskKeyBytes, 0, maskKeyBytes.Length);
                            byte[] tempData = new byte[payloadLength];
                            Array.Copy(buffer, 8, tempData, 0, tempData.Length);

                            for (int i = 0; i < tempData.Length; i++)
                            {
                                tempData[i] = (byte)(maskKeyBytes[i % 4] ^ tempData[i]);
                            }

                            data.AddRange(tempData);
                        }
                        else
                        {
                            byte[] tempData = new byte[payloadLength];
                            Array.Copy(buffer, 4, tempData, 0, tempData.Length);
                            data.AddRange(tempData);
                        }

                        break;
                    case 127:
                        payloadLengthBytes = new byte[8];
                        Array.Copy(buffer, 2, payloadLengthBytes, 0, payloadLengthBytes.Length);
                        payloadLength = BitConverter.ToUInt64(payloadLengthBytes.Reverse<byte>().ToArray(), 0);
                        messageArg.messageLength = payloadLength;

                        if (maskKey)
                        {
                            Array.Copy(buffer,10, maskKeyBytes, 0, maskKeyBytes.Length );
                            byte[] tempData = new byte[payloadLength];

                            Array.Copy(buffer, 14, tempData, 0, tempData.Length);
                            for (int i = 0; i < tempData.Length; i++)
                            {
                                tempData[i] = (byte)(maskKeyBytes[i % 4] ^ tempData[i]);
                            }
                            data.AddRange(tempData);
                        }
                        else
                        {
                            byte[] tempData = new byte[payloadLength];

                            Array.Copy(buffer, 10, tempData, 0, tempData.Length);
                            data.AddRange(tempData);
                        }
                        break;
                    default:
                        payloadLength = (uint)initialPayloadLength;
                        messageArg.messageLength = payloadLength;

                        if (maskKey)
                        {
                            Array.Copy(buffer, 2, maskKeyBytes, 0, maskKeyBytes.Length);
                            byte[] tempData = new byte[payloadLength];
                            Array.Copy(buffer, 6, tempData, 0, tempData.Length);
                            for (int i = 0; i < tempData.Length; i++)
                            {
                                tempData[i] = (byte)(maskKeyBytes[i % 4] ^ tempData[i]);
                            }
                            data.AddRange(tempData);
                        }
                        else
                        {
                            byte[] tempData = new byte[(bytesRead - 2)];
                            Array.Copy(buffer, 2, tempData, 0, tempData.Length);
                            data.AddRange(tempData);
                        }
                        break;
                }


                if (!continuationFrame && finalMessage)
                {
                    messageArg.data = data.ToArray();
                    _Handler.OnWebsocketEvent(messageArg);
                    data.Clear();
                } 
            }

            messageArg.isOpen = false;
            _Handler.OnWebsocketEvent(messageArg);
        }
    }
}
