using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FlatBuffers;
using Grasshopper;
using Grasshopper.GUI.Base;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using GrasshopperVRBridge.IO;
using WebSocketSharing.Properties;
using WebSocketSharp;
using Mesh = Rhino.Geometry.Mesh;

namespace WebSocketSharing
{
    /// <summary>
    /// The component that shares parameters through the given <see cref="WebSocket"/> connection.
    /// </summary>
    public class VRSharingComponent : GH_Component
    {
        private bool _isListening;

        /// <summary>
        /// Initializes a new instance of the WebSocketSharing class.
        /// </summary>
        public VRSharingComponent()
            : base("VRSharing", "VRSharing",
                "Stream meshes and share parameters through a WS connection",
                "VRSharing", "VRSharing")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Address", "Address", "IP address of the WebSocket server", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Connect?", "Connect?", "Connection toggle", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Mesh streaming?", "Stream mesh?", "Mesh streaming toggle", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Parameter sharing?", "Share param?", "Parameter sharing toggle", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Status", "Status", "Status", GH_ParamAccess.item);
        }

        /// <summary>
        /// Streams the given meshes and shares (I/O) the given parameters through the given <see cref="WebSocket" /> connection.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            
        }

        
        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.VRIcon;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("{338b2230-aecc-4ec2-8e5a-bdd0dece2ad0}");
    }
}