using System;
using System.Linq;
using System.Net;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace SharingWebSocketServer
{
    /// <summary>
    /// The main program (a CLI for a <see cref="WebSocketServer"/>).
    /// </summary>
    class Program
    {
        public static WebSocketServer Server;
        public static IPAddress HostIp;
        public static string HostId;

        static void Main(string[] args)
        {
            //If at least one argument is provided, the first one is used as server URL 
            //(otherwise, "ws://127.0.0.1:8080" is used as default value)
            string url = "ws://10.102.171.157:8080";
            //string url = "ws://169.254.20.160:8080";
            if (args.Length > 0)
            {
                //url = args[0];
                url = "ws://" + args[0] + ":8080";
                Console.WriteLine("Parameter found, using the first one as ip: " + url);
            }

            Server = new WebSocketServer(url);
            Server.AddWebSocketService<GrasshopperService>("/Grasshopper"); //Endpoint for GH
            Server.AddWebSocketService<StreamingService>("/Streaming"); //Endpoint for streaming clients
            Server.AddWebSocketService<SharingService>("/Sharing"); //Endpoint for sharing clients
            Server.AddWebSocketService<MultiService>("/Multi"); //Endpoint for multi clients
            Server.Start();
            Console.WriteLine("Started WebSockets server at: " + url);
            Console.WriteLine("(running 4 services: /Streaming, /Sharing and /Multi for the clients and /Grasshopper for GH)");
            Console.WriteLine("Press any key to stop the server.");

            Console.ReadKey(true); //Awaits a "keyboard key pressed" event
            Server.Stop();
        }
    }

    /// <summary>
    /// An enum that represents the type of a packet contained in a Flatbuffers' byte array.
    /// </summary>
    internal enum GhPacketType
    {
        Mesh,
        Param,
        Unknown
    }

    /// <summary>
    /// The WebSocket service (<see cref="WebSocketBehavior"/>) that handles the connection with Grasshopper.
    /// </summary>
    public class GrasshopperService : WebSocketBehavior
    {
        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);
            Console.WriteLine("Grasshopper connection closed (" + e.Code + ":" + e.Reason + ")");
        }

        protected override void OnError(ErrorEventArgs e)
        {
            base.OnError(e);
            Console.WriteLine("#GH-err# " + e.Message);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);
            if (e.IsBinary)
            {
                byte[] data = e.RawData;
                switch (ReadPacketType(data))
                {
                    case GhPacketType.Mesh:
                        Console.WriteLine("MeshData packet received, broadcasting to /Streaming clients.");
                        Program.Server.WebSocketServices["/Streaming"].Sessions.Broadcast(data);
                        break;
                    case GhPacketType.Param:
                        Console.WriteLine("ParameterData packet received, broadcasting to /Sharing clients.");
                        Program.Server.WebSocketServices["/Sharing"].Sessions.Broadcast(data);
                        break;
                    case GhPacketType.Unknown:
                        Console.WriteLine("Unknown binary packet type received from GH: will not be broadcasted.");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                Console.WriteLine("Unexpected non-binary message received: will not be broadcasted.");
                Console.WriteLine("Content: " + e.Data);
            }
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            if (Sessions.Count > 1) //If a GH client is already connected
            {
                Console.WriteLine("Another connection was already established, " +
                                  "aborting this attempt.");
                Sessions.CloseSession(Sessions.Sessions.Last().ID); //Abord this new connection attempt
            }
            else
            {
                Console.WriteLine("New Grasshopper connection opened.");
            }
        }

        /// <summary>
        /// Reads specific values in the given byte array to determine the <see cref="GhPacketType"/>
        /// (based on the "Flatbuffers identifier" the packet as been marked with).
        /// </summary>
        /// <param name="data">A byte array containing a Flatbuffers packet.</param>
        /// <returns>The corresponding <see cref="GhPacketType"/>.</returns>
        private static GhPacketType ReadPacketType(byte[] data)
        {
            if (data.Length < 8) return GhPacketType.Unknown;

            if (data[4] == (byte) 'M' && data[5] == (byte) 'E' && data[6] == (byte) 'S' && data[7] == (byte) 'H')
                return GhPacketType.Mesh;

            if (data[4] == (byte) 'P' && data[5] == (byte) 'A' && data[6] == (byte) 'R' && data[7] == (byte) 'A')
                return GhPacketType.Param;

            return GhPacketType.Unknown;
        }
    }

    /// <summary>
    /// The WebSocket service (<see cref="WebSocketBehavior"/>) that handles the connection with streaming clients.
    /// </summary>
    public class StreamingService : WebSocketBehavior
    {
        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);
            Console.WriteLine("Streaming connection closed (" + e.Code + ":" + e.Reason + ")");
        }

        protected override void OnError(ErrorEventArgs e)
        {
            base.OnError(e);
            Console.WriteLine("#ST-err# " + e.Message);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);
            Console.WriteLine("[ST-msg]" + (e.IsBinary ? "binary data of length " + e.RawData.Length : e.Data));
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            Console.WriteLine("New streaming connection opened.");
        }
    }

    /// <summary>
    /// The WebSocket service (<see cref="WebSocketBehavior"/>) that handles the connection with sharing clients.
    /// </summary>
    public class SharingService : WebSocketBehavior
    {
        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);
            Console.WriteLine("Sharing connection closed (" + e.Code + ":" + e.Reason + ")");
        }

        protected override void OnError(ErrorEventArgs e)
        {
            base.OnError(e);
            Console.WriteLine("#SH-err# " + e.Message);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);
            Console.WriteLine("[SH-msg]" + (e.IsBinary ? "binary data of length " + e.RawData.Length : e.Data));
            if (e.IsBinary)
                Program.Server.WebSocketServices["/Grasshopper"].Sessions.Broadcast(e.RawData);
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            Console.WriteLine("New sharing connection opened.");
        }
    }

    /// <summary>
    /// The WebSocket service (<see cref="WebSocketBehavior"/>) that handles the connection with multi clients.
    /// </summary>
    public class MultiService : WebSocketBehavior
    {
        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);
            Console.WriteLine("Multi connection closed (" + e.Code + ":" + e.Reason + ")");
            HandleDisconnection();
        }

        private void HandleDisconnection()
        {
            // "this" session is now disconnected
            if (!ID.Equals(Program.HostId)) return; // if it was a simple client, no need to do anything

            bool newHostFound = false;

            foreach (IWebSocketSession session in Program.Server.WebSocketServices["/Multi"].Sessions.Sessions)
            {
                if (session.ID.Equals(ID)) continue; //TODO: Can this happen? Or has that session been removed already?
                
                if (!newHostFound)
                {
                    // The first valid session is elected as the new host
                    newHostFound = true; 

                    Program.HostIp = session.Context.UserEndPoint.Address;
                    Program.HostId = session.ID;

                    Console.WriteLine("Previous host disconnected, new one defined: " + Program.HostIp);
                    // Tell that newly-elected host to actually start hosting
                    Program.Server.WebSocketServices["/Multi"].Sessions.SendTo("you", Program.HostId); 
                }
                else
                {
                    // Send the new host's IP to the clients
                    Program.Server.WebSocketServices["/Multi"].Sessions.SendTo(Program.HostIp.ToString(), Program.HostId);
                }
            }

            if (!newHostFound) // If no valid host candidate was found (probably because there is no active session anymore)
            {
                // Remove the old id/ip so that new sessions don't think that a host has been elected already
                Program.HostIp = null;
                Program.HostId = null;
            }
        }

        protected override void OnError(ErrorEventArgs e)
        {
            base.OnError(e);
            Console.WriteLine("#MU-err# " + e.Message + " from: " + e.Exception.Source);
            HandleDisconnection();
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);
            Console.WriteLine("[MU-msg]" + (e.IsBinary ? "binary data of length " + e.RawData.Length : e.Data));
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            Console.WriteLine("New multi connection opened.");

            if (Program.HostIp == null) // If no host is currently defined
            {
                // Elect this session as host (no need to tell others as this situation can only happen if no other client is connected)
                Program.HostIp = Context.UserEndPoint.Address;
                Program.HostId = ID;
                
                Console.WriteLine("New host defined at " + Program.HostIp);
                Send("you");
            }
            else
            {
                // Has a host had already been elected, send its IP to this new client
                Console.WriteLine("Sending current host ip: " + Program.HostIp);
                Send(Program.HostIp.ToString());
            }
        }
        
    }
}