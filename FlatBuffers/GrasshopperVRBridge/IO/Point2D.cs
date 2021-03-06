// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace GrasshopperVRBridge.IO
{

using global::System;
using global::FlatBuffers;

public struct Point2D : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static Point2D GetRootAsPoint2D(ByteBuffer _bb) { return GetRootAsPoint2D(_bb, new Point2D()); }
  public static Point2D GetRootAsPoint2D(ByteBuffer _bb, Point2D obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p.bb_pos = _i; __p.bb = _bb; }
  public Point2D __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public float X { get { int o = __p.__offset(4); return o != 0 ? __p.bb.GetFloat(o + __p.bb_pos) : (float)0.0f; } }
  public float Y { get { int o = __p.__offset(6); return o != 0 ? __p.bb.GetFloat(o + __p.bb_pos) : (float)0.0f; } }

  public static Offset<Point2D> CreatePoint2D(FlatBufferBuilder builder,
      float x = 0.0f,
      float y = 0.0f) {
    builder.StartObject(2);
    Point2D.AddY(builder, y);
    Point2D.AddX(builder, x);
    return Point2D.EndPoint2D(builder);
  }

  public static void StartPoint2D(FlatBufferBuilder builder) { builder.StartObject(2); }
  public static void AddX(FlatBufferBuilder builder, float x) { builder.AddFloat(0, x, 0.0f); }
  public static void AddY(FlatBufferBuilder builder, float y) { builder.AddFloat(1, y, 0.0f); }
  public static Offset<Point2D> EndPoint2D(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<Point2D>(o);
  }
};


}
