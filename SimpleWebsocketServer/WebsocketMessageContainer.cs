namespace SimpleWebSocketServerLibrary
{
    /// <summary>
    /// Contains information about a websocket client message.
    /// </summary>
    public class WebSocketMessageContainer
    {
        /// <summary>
        /// Is true when message is a text message.
        /// </summary>
        public bool isText { get; set; } = false;
        /// <summary>
        /// Is true when message is a binairy message.
        /// </summary>
        public bool isBinary { get; set; }  = false;
        /// <summary>
        /// Is true when a client send a close message.
        /// </summary>
        public bool isClosed { get; set; }  = false;
        /// <summary>
        /// Is true when message from client is a ping message.
        /// </summary>
        public bool isPing { get; set; }  = false;
        /// <summary>
        /// Is true when message from client is a pong message.
        /// </summary>
        public bool isPong { get; set; }  = false;
        /// <summary>
        /// Contains the extra data send by a client.
        /// </summary>
        public byte[] data { get; set; }
    }
}
