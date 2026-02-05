using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Net.WebSockets;

namespace Celestia_IDE.Core
{
    class ControlServer
    {
        private readonly Dictionary<int, InstanceServer> _instances = new();

        public async Task HandleMessage(WebSocket ws, string msg)
        {
            var parts = msg.Split(',');
            if (parts.Length != 2) return;

            if (!int.TryParse(parts[1], out int port))
                return;

            switch (parts[0])
            {
                case "REGISTER":
                    if (!IsPortFree(port) || _instances.ContainsKey(port))
                    {
                        await Send(ws, "REJECT," + port);
                        return;
                    }

                    var server = new InstanceServer(port);
                    _instances[port] = server;

                    _ = server.StartAsync();
                    await Send(ws, "READY," + port);
                    break;

                case "UNREGISTER":
                    if (_instances.TryGetValue(port, out var inst))
                    {
                        inst.Stop();
                        _instances.Remove(port);
                    }
                    break;
            }
        }

        static bool IsPortFree(int port)
        {
            try
            {
                var l = new TcpListener(IPAddress.Loopback, port);
                l.Start();
                l.Stop();
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
        }

        private static async Task Send(WebSocket ws, string msg)
        {
            var data = Encoding.UTF8.GetBytes(msg);
            await ws.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
    class InstanceServer
    {
        public int Port { get; }
        public RobloxInstance Instance { get; private set; }

        private HttpListener _listener;

        public InstanceServer(int port)
        {
            Port = port;
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        }

        public async Task StartAsync()
        {
            _listener.Start();

            var ctx = await _listener.GetContextAsync();
            var ws = (await ctx.AcceptWebSocketAsync(null)).WebSocket;

            await ReceiveLoop(ws);
        }

        private async Task ReceiveLoop(WebSocket ws)
        {
            var buffer = new byte[4096];

            while (ws.State == WebSocketState.Open)
            {
                var r = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                var text = Encoding.UTF8.GetString(buffer, 0, r.Count);

                var p = text.Split(',');
                if (p[0] == "CONNECT")
                {
                    Instance = new RobloxInstance
                    {
                        Username = p[1],
                        PlaceId = p[2],
                        JobId = p[3]
                    };
                }
            }
        }

        public void Stop()
        {
            _listener.Stop();
        }
    }


    class RobloxInstance
    {
        public string Username;
        public string PlaceId;
        public string JobId;
    }


}
