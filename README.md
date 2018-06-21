# SimpleWebSocketServer Library
SimpleWebSocketServer Library is designed as a simplistic library in use wich provides a middleware for the programmer to handle websocket connections. The library targets the most popular frameworks within .NET such as: .NET FrameWork 4.5 to 4.7 and .NET Core 2.0 & 2.1 and should work with Mono. The programmer just needs 4 lines of code to start and stop the server. The middleware has the following features:

  - Hubs (multiple websocket end-points).
  - Multiple Clients.
  - Clients can create their own ID (By appending it to the connection url).
  - Clients can connect without specifying a ID (Automatically generated on the server).
  - Single Event Handler.
  - Configurable Port, End-points & (Receive)BufferSize.
  - Pre-configured HTTP Server as negotiater for the WebSocket Protocol. (Default runs at port 80, no SSL support for now).
  - Both Asynchronous and Synchronous methods.

In case the programmer want's to use it's own HTTP Server for the negotiation for the WebSocket Protocol, he can use the inner classes of the library such as the `WebSocketServer` class. (See down below for a quick setup).

# Installation

 You can get the NuGet package from here: [NuGet](https://www.nuget.org/packages/SimpleWebsocketServer/).
 You can get the dll files here: [GitHub Releases](https://github.com/EldinZenderink/SimpleWebSocketServer/releases).

### Usage - Middleware
The quickest way to get started:

    SimpleWebSocketServer websocketServer = new SimpleWebSocketServer();      
    websocketServer.WebsocketServerEvent += OnWebsocketEvent;
    websocketServer.StartServer();
    Console.ReadKey();
    bool stopped = websocketServer.StopAll("Server had enough, RIP server.");
    
And the event handler method:

    private static void OnWebsocketEvent(object sender, WebSocketEventArg args)
    {
        if (args.data != null && args.isText)
        {
            string received = Encoding.UTF8.GetString(args.data);
            _WebsocketServer.SendTextMessage("Client: " + args.clientId + " on url: " + args.clientBaseUrl + ", says: " + received);
        }
    }
    
As you can see this is written with synchronous methods. If you want to use asynchronous methods, just add Async behind the method name. 

For more information about the WebSocketEventArg class and other classes, go to this page: [DoxygenWiki -`WebSocketEventArg`](https://eldinzenderink.github.io/SimpleWebSocketServer/class_simple_web_socket_server_library_1_1_web_socket_event_arg.html).

You can provide the constructor of the middleware class `SimpleWebSocketServer` with the following class with parameters to customize your settings, here you can also find the default values used: [DoxygenWiki - `SimpleWebSocketServerSettings`](https://eldinzenderink.github.io/SimpleWebSocketServer/class_simple_web_socket_server_library_1_1_simple_web_socket_server_settings.html).


### Usage - `WebSocketServer` Class (Advanced)
If you just want to get WebSocket to work, just use the MiddleWare class writen above. But, in case you do want full control, or if you already have a HTTP Server and don't want to run a seperate HTTP Server just for WebSockets, and yours has the capability to Negotiate the WebSocket protocol (see: [Mozilla WebSocket API](https://developer.mozilla.org/en-US/docs/Web/API/WebSockets_API/Writing_WebSocket_server)), you can use this class to handle further WebSocket protocol communication.

Here is a basic overview on how to use it:

    WebSocketClientInfo webSocketClientInfo = new WebSocketClientInfo()
    {
        clientId = "GENERATE CLIENT ID",
        clientBaseUrl = "URL CLIENT USED TO CONNECT",
        client = new System.Net.Sockets.TcpClient() // TCP CLIENT RETREIVED FROM HTTP SERVER AFTER WEBSOCKET UPGRADE
    };
    
    WebSocketServer newServer = new WebSocketServer(webSocketClientInfo);
    newServer.WebSocketServerEvent += OnWebsocketEvent;
    newServer.StartServer();
    Console.ReadKey();
    newServer.StopServer();

If you want to use asynchronous methods instead of synchronous, you can just like with the middleware append `Async` to the end of the method name. Example `newServer.StartServer()` becomes `newServer.StartServerAsync()`.

The eventhandler is the same as with the middleware class.

### Full (Doxygen generated) Wiki
[GitHub Wiki](https://github.com/EldinZenderink/SimpleWebSocketServer/wiki/Full-Documenation)
[DoxyGen Wiki](https://eldinzenderink.github.io/SimpleWebSocketServer/)

### Development
The library was designed and made with the intention to be used in hobby projects and to mess with WebSockets. I wrote this to learn about the WebSocket protocol in general and it isn't really intended to be used within an actual production environment. Why? Because I don't want to spend to much time on this side project which in turn leads to the lack of support. I will fix big issues if there are no workarounds, but appart from that, don't expect much development & help. 

I will probably be using it alongside my other hobby projects, whenever I encounter an issue with the library, you can expect me to release the fix for it here as well.

### Todos
 - Write unit tests.

License
----

MIT


**Free Software, Hell Yeah!**

