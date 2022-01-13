using System;
using System.Drawing;
using Grasshopper.Kernel;
using WebSocketSharing.Properties;

namespace WebSocketSharing
{
    public class MeshStreamingInfo : GH_AssemblyInfo
    {
        public override string Name => "WebSocketSharing";
        
        public override Bitmap Icon => Resources.Icon;

        public override string Description =>
            "This plugin enables mesh streaming and parameter sharing through an external WebSocket server";

        public override Guid Id => new Guid("6e7b27a4-33be-4ecc-adb1-a8ed9fe46234");

        public override string AuthorName => "Adrien Coppens";

        public override string AuthorContact => "adrien.coppens@umons.ac.be";
    }
}