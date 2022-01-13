// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace GrasshopperVRBridge.IO
{

using global::System;
using global::FlatBuffers;

public struct Mesh : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static Mesh GetRootAsMesh(ByteBuffer _bb) { return GetRootAsMesh(_bb, new Mesh()); }
  public static Mesh GetRootAsMesh(ByteBuffer _bb, Mesh obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p.bb_pos = _i; __p.bb = _bb; }
  public Mesh __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public Point3D? Vertices(int j) { int o = __p.__offset(4); return o != 0 ? (Point3D?)(new Point3D()).__assign(__p.__indirect(__p.__vector(o) + j * 4), __p.bb) : null; }
  public int VerticesLength { get { int o = __p.__offset(4); return o != 0 ? __p.__vector_len(o) : 0; } }
  public Point2D? Uvs(int j) { int o = __p.__offset(6); return o != 0 ? (Point2D?)(new Point2D()).__assign(__p.__indirect(__p.__vector(o) + j * 4), __p.bb) : null; }
  public int UvsLength { get { int o = __p.__offset(6); return o != 0 ? __p.__vector_len(o) : 0; } }
  public Point3D? Normals(int j) { int o = __p.__offset(8); return o != 0 ? (Point3D?)(new Point3D()).__assign(__p.__indirect(__p.__vector(o) + j * 4), __p.bb) : null; }
  public int NormalsLength { get { int o = __p.__offset(8); return o != 0 ? __p.__vector_len(o) : 0; } }
  public MeshFace? Faces(int j) { int o = __p.__offset(10); return o != 0 ? (MeshFace?)(new MeshFace()).__assign(__p.__indirect(__p.__vector(o) + j * 4), __p.bb) : null; }
  public int FacesLength { get { int o = __p.__offset(10); return o != 0 ? __p.__vector_len(o) : 0; } }

  public static Offset<Mesh> CreateMesh(FlatBufferBuilder builder,
      VectorOffset verticesOffset = default(VectorOffset),
      VectorOffset uvsOffset = default(VectorOffset),
      VectorOffset normalsOffset = default(VectorOffset),
      VectorOffset facesOffset = default(VectorOffset)) {
    builder.StartObject(4);
    Mesh.AddFaces(builder, facesOffset);
    Mesh.AddNormals(builder, normalsOffset);
    Mesh.AddUvs(builder, uvsOffset);
    Mesh.AddVertices(builder, verticesOffset);
    return Mesh.EndMesh(builder);
  }

  public static void StartMesh(FlatBufferBuilder builder) { builder.StartObject(4); }
  public static void AddVertices(FlatBufferBuilder builder, VectorOffset verticesOffset) { builder.AddOffset(0, verticesOffset.Value, 0); }
  public static VectorOffset CreateVerticesVector(FlatBufferBuilder builder, Offset<Point3D>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartVerticesVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddUvs(FlatBufferBuilder builder, VectorOffset uvsOffset) { builder.AddOffset(1, uvsOffset.Value, 0); }
  public static VectorOffset CreateUvsVector(FlatBufferBuilder builder, Offset<Point2D>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartUvsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddNormals(FlatBufferBuilder builder, VectorOffset normalsOffset) { builder.AddOffset(2, normalsOffset.Value, 0); }
  public static VectorOffset CreateNormalsVector(FlatBufferBuilder builder, Offset<Point3D>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartNormalsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static void AddFaces(FlatBufferBuilder builder, VectorOffset facesOffset) { builder.AddOffset(3, facesOffset.Value, 0); }
  public static VectorOffset CreateFacesVector(FlatBufferBuilder builder, Offset<MeshFace>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static void StartFacesVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static Offset<Mesh> EndMesh(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<Mesh>(o);
  }
};


}
