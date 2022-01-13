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
    public class WebSocketSharingComponent : GH_Component
    {
        private bool _isListening;
        private byte[] _lastBytes;
        private bool _wasStreamingActive = false;

        /// <summary>
        /// Initializes a new instance of the WebSocketSharing class.
        /// </summary>
        public WebSocketSharingComponent()
            : base("WebSocketSharing", "WebSocketSharing",
                "Stream meshes and share parameters through the WS connection",
                "WebSocketSharing", "Sharing")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Socket", "Socket", "Socket Data", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Mesh streaming", "Mesh streaming", "Whether mesh streaming is enabled",
                GH_ParamAccess.item);
            pManager.AddGenericParameter("Meshes", "Meshes", "Mesh(es) to send", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Parameter sharing", "Parameter sharing",
                "Whether parameter sharing is enabled", GH_ParamAccess.item);
            pManager.AddGenericParameter("Shared parameters", "Shared parameters", "Parameter(s) to share",
                GH_ParamAccess.list);
            pManager[4].Optional = true;
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
            string logOutput = "";
            WebSocket ws = null;
            bool isMeshStreamingEnabled = false;
            List<Mesh> meshes = new List<Mesh>();
            bool isParameterSharingEnabled = false;

            if (!DA.GetData(0, ref ws)) return;
            if (!DA.GetData(1, ref isMeshStreamingEnabled)) return;
            if (!DA.GetDataList(2, meshes)) return;
            if (!DA.GetData(3, ref isParameterSharingEnabled)) return;

            Dictionary<string, IGH_Param> components = new Dictionary<string, IGH_Param>();
            IList<IGH_Param> sharedParams = Params.Input[4].Sources;

            //Retrieve the input parameters and put them in the components dictionary
            #region Retrieve parameters

            if (isParameterSharingEnabled)
            {
                //Populate the dictionary of GH components (to easily search for them by guid when handling client data)
                foreach (IGH_Param source in sharedParams)
                {
                    switch (source.TypeName)
                    {
                        case "Boolean":
                            GH_BooleanToggle toggle = source as GH_BooleanToggle;
                            if (toggle != null)
                                components.Add(source.InstanceGuid.ToString(), toggle);
                            break;
                        case "Number":
                            GH_NumberSlider slider = source as GH_NumberSlider;
                            if (slider != null)
                                components.Add(source.InstanceGuid.ToString(), slider);
                            break;
                    }
                }
            }

            #endregion

            //Add a handler that will process the given parameter data (received from the clients)
            #region Add handler for incoming parameter data

            if (!_isListening)
            {
                if (isParameterSharingEnabled)
                {
                    //Add a sharing handler that will modify GH component values depending on what the client sends
                    ws.OnMessage += (sender, e) =>
                    {
                        if (isParameterSharingEnabled && e.IsBinary)
                        {
                            logOutput += "Binary data processed\n";

                            ByteBuffer buffer = new ByteBuffer(e.RawData);
                            Components receivedComponents = Components.GetRootAsComponents(buffer);

                            for (int i = 0; i < receivedComponents.ComponentsVectorLength; i++)
                            {
                                Component? nullableComponent = receivedComponents.ComponentsVector(i);
                                if (nullableComponent == null) continue;

                                Component component = nullableComponent.Value;
                                switch (component.AbstractComponentType)
                                {
                                    case GenericComponent.BooleanToggle:
                                        BooleanToggle? nullableToggle = component
                                            .AbstractComponent<BooleanToggle>();

                                        if (nullableToggle != null &&
                                            nullableToggle.Value.Name.Equals("bs"))
                                        {
                                            BooleanToggle receivedToggle = nullableToggle.Value;
                                            IGH_Param storedParam = components[receivedToggle.Guid];
                                            if (storedParam != null && storedParam is GH_BooleanToggle actualToggle)
                                            {
                                                actualToggle.Value = receivedToggle.Value;
                                            }
                                        }
                                        break;
                                    case GenericComponent.NumberSlider:
                                        NumberSlider? nullableSlider = component
                                            .AbstractComponent<NumberSlider>();
                                        if (nullableSlider != null && nullableSlider.Value.Name.Equals("bs"))
                                        {
                                            NumberSlider receivedSlider = nullableSlider.Value;
                                            IGH_Param storedParam = components[receivedSlider.Guid];
                                            if (storedParam != null && storedParam is GH_NumberSlider actualSlider)
                                            {
                                                actualSlider.SetSliderValue((decimal) receivedSlider.Value);
                                            }
                                        }
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                            }

                            //Need to "invalidate" the document to refresh the components (and therefore take new values into account)
                            Instances.DocumentEditor.Invoke((MethodInvoker) delegate { ExpireSolution(true); });
                        }
                        else
                        {
                            logOutput += "Unexpected txt data received: " + e.Data + "\n";
                        }
                    };
                    _isListening = true;

                    ws.OnClose += (sender, e) =>
                    {
                        _isListening = false;
                    };

                    ws.OnError += (sender, e) =>
                    {
                        _isListening = false;
                    };
                }
            }

            #endregion

            //Share the given input parameters
            #region Share GH parameters

            if (ws != null && ws.IsAlive)
            {
                if (isParameterSharingEnabled)
                {
                    FlatBufferBuilder builder = new FlatBufferBuilder(2);
                    List<Offset<Component>> componentsToSend = new List<Offset<Component>>();
                    foreach (KeyValuePair<string, IGH_Param> componentPair in components)
                    {
                        IGH_Param component = componentPair.Value;
                        switch (component.TypeName)
                        {
                            case "Boolean":
                                GH_BooleanToggle toggle = component as GH_BooleanToggle;
                                if (toggle == null) break;

                                //TODO: Shouldn't I set values here in addition to the event listener? 
                                //in case something goes wrong in the execution order

                                StringOffset toggleName = builder.CreateString(toggle.NickName);
                                StringOffset toggleGuid = builder.CreateString(toggle.InstanceGuid.ToString());
                                Offset<BooleanToggle> newToggle = BooleanToggle.CreateBooleanToggle(
                                    builder, toggleName, toggleGuid, toggle.Value);
                                Offset<Component> toggleComponent =
                                    Component.CreateComponent(builder, GenericComponent.BooleanToggle, newToggle.Value);
                                componentsToSend.Add(toggleComponent);
                                break;
                            case "Number":
                                GH_NumberSlider slider = component as GH_NumberSlider;
                                if (slider == null) break;

                                StringOffset sliderName = builder.CreateString(slider.NickName);
                                StringOffset sliderGuid = builder.CreateString(slider.InstanceGuid.ToString());

                                Accuracy accuracy;
                                switch (slider.Slider.Type)
                                {
                                    case GH_SliderAccuracy.Float:
                                        accuracy = Accuracy.Float;
                                        break;
                                    case GH_SliderAccuracy.Integer:
                                        accuracy = Accuracy.Integer;
                                        break;
                                    case GH_SliderAccuracy.Even:
                                        accuracy = Accuracy.Even;
                                        break;
                                    case GH_SliderAccuracy.Odd:
                                        accuracy = Accuracy.Odd;
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }

                                //TODO: Shouldn't I set values here in addition to the event listener? 
                                //in case something goes wrong in the execution order

                                Offset<NumberSlider> newSlider = NumberSlider.CreateNumberSlider(builder, sliderName,
                                    sliderGuid,
                                    (float) slider.CurrentValue, accuracy, (float) slider.Slider.Minimum,
                                    (float) slider.Slider.Maximum,
                                    (float) slider.Slider.Epsilon, (short) slider.Slider.DecimalPlaces);
                                Offset<Component> sliderComponent =
                                    Component.CreateComponent(builder, GenericComponent.NumberSlider, newSlider.Value);
                                componentsToSend.Add(sliderComponent);

                                break;
                        }
                    }

                    VectorOffset vectorOffset =
                        Components.CreateComponentsVectorVector(builder, componentsToSend.ToArray());
                    Components.StartComponents(builder);
                    Components.AddComponentsVector(builder, vectorOffset);
                    Offset<Components> endComponents = Components.EndComponents(builder);
                    Components.FinishComponentsBuffer(builder, endComponents);

                    //(unrelated) TODO: use default values to save buffer space + are we really using accuracy & cie?
                    byte[] toSendAsByteArray = builder.SizedByteArray();
                    ws.Send(toSendAsByteArray);
                    logOutput += "Data sent\n";
                }
                if (isMeshStreamingEnabled)
                {
                    //ws.Send("Getting mesh data");
                    byte[] meshData = GetMeshData(meshes);
                    // As GH calls this SolveInstance method multiple times for a single parameter change, 
                    // we minimize WS traffic by filtering "duplicate" messages (returned as null by GetMeshData)
                    if (meshData != null) ws.Send(meshData);
                    _wasStreamingActive = true;
                }
                else
                {
                    _wasStreamingActive = false;
                }
            }
            else
            {
                logOutput += "WebSocket connection is inactive\n";
            }

            #endregion

            if (!logOutput.IsNullOrEmpty())
            {
                DA.SetData(0, logOutput);
            }
        }

        /// <summary>
        /// Creates a FlatBuffer byte array containing the given meshes. 
        /// </summary>
        /// <param name="meshes">A list of meshes to include in the buffer.</param>
        /// <returns>A buffer (Flatbuffer byte array) that is ready to be streamed.</returns>
        private byte[] GetMeshData(List<Mesh> meshes)
        {
            FlatBufferBuilder builder = new FlatBufferBuilder(2);
            List<Offset<GrasshopperVRBridge.IO.Mesh>> meshesOffsets = new List<Offset<GrasshopperVRBridge.IO.Mesh>>();

            foreach (Mesh mesh in meshes)
            {
                mesh.Normals.ComputeNormals();

                List<Offset<Point3D>> verticesOffsets = mesh.Vertices
                    .Select(vertex => Point3D.CreatePoint3D(builder, vertex.X, vertex.Y, vertex.Z)).ToList();
                List<Offset<Point2D>> uvsOffsets = mesh.TextureCoordinates
                    .Select(uv => Point2D.CreatePoint2D(builder, uv.X, uv.Y)).ToList();
                List<Offset<Point3D>> normalsOffsets = mesh.Normals
                    .Select(normal => Point3D.CreatePoint3D(builder, normal.X, normal.Y, normal.Z)).ToList();
                List<Offset<MeshFace>> facesOffsets = mesh.Faces
                    .Select(face => MeshFace.CreateMeshFace(builder, face.IsQuad, face.A, face.B, face.C, face.D))
                    .ToList();

                VectorOffset verticesVector =
                    GrasshopperVRBridge.IO.Mesh.CreateVerticesVector(builder, verticesOffsets.ToArray());
                VectorOffset uvsVector = GrasshopperVRBridge.IO.Mesh.CreateUvsVector(builder, uvsOffsets.ToArray());
                VectorOffset normalsVector =
                    GrasshopperVRBridge.IO.Mesh.CreateNormalsVector(builder, normalsOffsets.ToArray());
                VectorOffset facesVector =
                    GrasshopperVRBridge.IO.Mesh.CreateFacesVector(builder, facesOffsets.ToArray());

                Offset<GrasshopperVRBridge.IO.Mesh> meshOffset =
                    GrasshopperVRBridge.IO.Mesh.CreateMesh(builder, verticesVector, uvsVector, normalsVector,
                        facesVector);
                meshesOffsets.Add(meshOffset);
            }

            VectorOffset meshesVector = MeshData.CreateMeshesVectorVector(builder, meshesOffsets.ToArray());
            MeshData.StartMeshData(builder);
            MeshData.AddMeshesVector(builder, meshesVector);
            Offset<MeshData> endMeshData = MeshData.EndMeshData(builder);
            MeshData.FinishMeshDataBuffer(builder, endMeshData);

            byte[] bytes = builder.SizedByteArray();

            if (IsDuplicate(_lastBytes, bytes) && _wasStreamingActive) return null;

            _lastBytes = bytes;
            return bytes;
        }

        // The following byte array comparison is based on the "UnsafeCompare" method from:
        // Copyright (c) 2008-2013 Hafthor Stefansson
        // Distributed under the MIT/X11 software license
        // Ref: http://www.opensource.org/licenses/mit-license.php.
        private static unsafe bool IsDuplicate (byte[] lastBytes, byte[] newBytes) 
        {
            //Only differences are minor optimizations (some safe assumptions) for our specific case
            //Could also avoid comparing the first few bytes (flatbuffers headers)
            if (lastBytes == null || lastBytes.Length != newBytes.Length)
                return false;
            fixed (byte* p1 = lastBytes, p2 = newBytes)
            {
                byte* x1 = p1, x2 = p2;
                int l = lastBytes.Length;
                for (int i = 0; i < l / 8; i++, x1 += 8, x2 += 8)
                    if (*((long*)x1) != *((long*)x2)) return false;
                if ((l & 4) != 0) { if (*((int*)x1) != *((int*)x2)) return false; x1 += 4; x2 += 4; }
                if ((l & 2) != 0) { if (*((short*)x1) != *((short*)x2)) return false; x1 += 2; x2 += 2; }
                if ((l & 1) == 0) return true;
                return *x1 == *x2;
            }
        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.ShareComponent;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("{4b24af51-c31a-420d-8029-01be5ebc18ae}");
    }
}