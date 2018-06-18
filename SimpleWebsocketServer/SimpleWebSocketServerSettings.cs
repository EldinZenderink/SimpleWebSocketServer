using System.Collections.Generic;

namespace SimpleWebSocketServerLibrary
{
    /// <summary>
    /// Provides settings and default values for the SimpleWebSocketServer Class.
    /// </summary>
    public class SimpleWebSocketServerSettings
    {
        /// <summary>
        /// List with url paths where the websocket server needs to listen to. Default = "";
        /// </summary>
        public List<string> baseUrls { get; set; } = new List<string>(){"/"};
        /// <summary>
        /// Port where the server needs to listen to. Default = 80;
        /// </summary>
        public int port { get; set; } = 80;
        /// <summary>
        /// Buffer size for receiving messages. Default = 4096; (bytes)
        /// </summary>
        public int bufferSize { get; set; } = 4096;

    }
}
