using System;
using System.Drawing;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.Kernel;
using WebSocketSharing.Properties;
using WebSocketSharp;

namespace WebSocketSharing
{
    /// <summary>
    /// The component that handles the <see cref="WebSocket"/> connection with the server.
    /// </summary>
    public class WebSocketConnectionComponent : GH_Component
    {
        public WebSocket Ws;
        private bool _isConnected;
        private string _status = "";

        /// <summary>
        /// Initializes a new instance of the WebSocketConnectionComponent class.
        /// </summary>
        public WebSocketConnectionComponent()
            : base("WebSocketConnection", "WebSocketConnection",
                "Connect to a WebSocket server",
                "WebSocketSharing", "Sharing")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Address", "Address", "IP address of the WebSocket server", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Connect", "Connect", "Connection toggle", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Status", "Status", "WebSocket status", GH_ParamAccess.item);
            pManager.AddGenericParameter("Socket", "Socket", "The actual WebSocket instance", GH_ParamAccess.item);
        }

        /// <summary>
        /// Starts/handles/terminates the connection with the server and logs information about messages/events/errors.
        /// </summary>
        /// <param name="DA">The <see cref="IGH_DataAccess"/> object used handle inputs/outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string address = "ws://127.0.0.1:8080/Grasshopper";
            bool isConnectionToggleActive = false;

            if (!DA.GetData(0, ref address)) return;
            if (!DA.GetData(1, ref isConnectionToggleActive)) return;

            //The connection toggle is active
            if (isConnectionToggleActive)
            {
                if (!_isConnected) //we're not already connected
                {
                    _status = "Connecting";

                    Ws = new WebSocket(address);

                    Ws.OnOpen += OnConnectionOpened;
                    Ws.OnClose += OnConnectionClosed;
                    Ws.OnError += OnConnectionError;
                    Ws.OnMessage += OnConnectionMessage;

                    Ws.Connect();

                    _isConnected = true;
                }
            }
            else //The connection toggle isn't active
            {
                if (Ws != null && Ws.IsAlive) //if the connection is still active, gracefully terminate it
                {
                    Ws.Close();
                }
                _isConnected = false;
            }

            DA.SetData(0, _status);
            DA.SetData(1, Ws);
        }

        private void OnConnectionMessage(object sender, MessageEventArgs e)
        {
            _status = "Message received";
            Instances.DocumentEditor.Invoke((MethodInvoker) delegate { ExpireSolution(true); });
        }

        private void OnConnectionError(object sender, ErrorEventArgs e)
        {
            _status = "Error";
            Instances.DocumentEditor.Invoke((MethodInvoker) delegate { ExpireSolution(true); });
        }

        private void OnConnectionClosed(object sender, CloseEventArgs e)
        {
            _status = "Closed";
            Instances.DocumentEditor.Invoke((MethodInvoker) delegate { ExpireSolution(true); });
        }

        private void OnConnectionOpened(object sender, EventArgs e)
        {
            _status = "Connected";
            Instances.DocumentEditor.Invoke((MethodInvoker) delegate { ExpireSolution(true); });
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.ConnectComponent;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("{65a47e6e-3b05-4b49-a0c5-30a3c3bff638}");
    }
}