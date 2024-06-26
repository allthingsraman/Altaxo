﻿#region Copyright

/////////////////////////////////////////////////////////////////////////////
//    Altaxo:  a data processing and data plotting program
//    Copyright (C) 2002-2015 Dr. Dirk Lellinger
//
//    This program is free software; you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation; either version 2 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program; if not, write to the Free Software
//    Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
//
/////////////////////////////////////////////////////////////////////////////

#endregion Copyright

#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Altaxo.Geometry;

namespace Altaxo.Graph.Graph3D.GraphicsContext
{
  using Drawing;
  using Drawing.D3D;
  using Gdi.Plot;

  public class GraphicsContextD3DPrimitivesBase
  {
    public static void DrawLine(PositionNormalIndexedTriangleBuffers buffers, PenX3D pen, PointD3D p0, PointD3D p1)
    {
      var vertexIndexOffset = buffers.IndexedTriangleBuffer.VertexCount;

      if (buffers.PositionNormalIndexedTriangleBuffer is not null)
      {
        var buf = buffers.PositionNormalIndexedTriangleBuffer;
        /*
                if (pen.Color.Name == "Beige")
                {
                */
        var solid = new SolidStraightLine();
        solid.AddGeometry(
          (position, normal) => buf.AddTriangleVertex(position.X, position.Y, position.Z, normal.X, normal.Y, normal.Z),
          (i0, i1, i2, isLeft) => buf.AddTriangleIndices(i0, i1, i2, isLeft),
          ref vertexIndexOffset,
          pen,
          new LineD3D(p0, p1));

        /*
            }
            else
            {
                        var polylinePoints = new[] { p0, p1 };
                SolidPolyline.AddWithNormals(
                (position, normal) => buf.AddTriangleVertex(position.X, position.Y, position.Z, normal.X, normal.Y, normal.Z),
                (i0, i1, i2) => buf.AddTriangleIndices(i0, i1, i2),
                ref vertexIndexOffset,
                pen,
                polylinePoints);
            }
            */
      }
      else if (buffers.PositionNormalColorIndexedTriangleBuffer is not null)
      {
        var polylinePoints = new[] { p0, p1 };
        var buf = buffers.PositionNormalColorIndexedTriangleBuffer;
        var color = pen.Color.Color;
        var r = color.ScR;
        var g = color.ScG;
        var b = color.ScB;
        var a = color.ScA;

        var solidPolyline = new SolidPolyline();
        solidPolyline.AddWithNormals(
        (position, normal) => buf.AddTriangleVertex(position.X, position.Y, position.Z, normal.X, normal.Y, normal.Z, r, g, b, a),
        (i0, i1, i2, isLeftCOS) => buf.AddTriangleIndices(i0, i1, i2, isLeftCOS),
        ref vertexIndexOffset,
        pen,
        polylinePoints);
      }
      else if (buffers.PositionNormalUVIndexedTriangleBuffer is not null)
      {
        throw new NotImplementedException("Texture on a line is not supported yet");
      }
      else
      {
        throw new NotImplementedException("Unexpected type of buffer: " + buffers.IndexedTriangleBuffer.GetType().ToString());
      }
    }

    public static void DrawLine(IIndexedTriangleBuffer buffer, PenX3D pen, PointD3D p0, PointD3D p1)
    {
      DrawLine(GetBuffers(buffer), pen, p0, p1);
    }

    private static PositionNormalIndexedTriangleBuffers GetBuffers(IIndexedTriangleBuffer buffer)
    {
      return new PositionNormalIndexedTriangleBuffers
      {
        IndexedTriangleBuffer = buffer,
        PositionNormalIndexedTriangleBuffer = buffer as IPositionNormalIndexedTriangleBuffer,
        PositionNormalColorIndexedTriangleBuffer = buffer as IPositionNormalColorIndexedTriangleBuffer,
        PositionNormalUVIndexedTriangleBuffer = buffer as IPositionNormalUVIndexedTriangleBuffer
      };
    }
  }

  public abstract class GraphicsContext3DBase : GraphicsContextD3DPrimitivesBase, IGraphicsContext3D
  {
    public abstract object SaveGraphicsState();

    public abstract void RestoreGraphicsState(object graphicsState);

    public abstract void PrependTransform(Matrix4x3 m);

    public abstract void TranslateTransform(double x, double y, double z);

    public abstract void TranslateTransform(VectorD3D translation);

    public abstract void RotateTransform(double degreeX, double degreeY, double degreeZ);

    public abstract PositionIndexedTriangleBuffers GetPositionIndexedTriangleBuffer(IMaterial material);

    public abstract PositionNormalIndexedTriangleBuffers GetPositionNormalIndexedTriangleBufferWithClipping(IMaterial material, PlaneD3D[] planes);

    public abstract PositionNormalIndexedTriangleBuffers GetPositionNormalIndexedTriangleBuffer(IMaterial material);

    public abstract IPositionNormalUIndexedTriangleBuffer GetPositionNormalUIndexedTriangleBuffer(IMaterial material, PlaneD3D[]? clipPlanes, IColorProvider colorProvider);

    public abstract Matrix4x3 Transformation { get; }

    public abstract Matrix3x3 TransposedInverseTransformation { get; }

    #region Primitives rendering

    public void DrawTriangle(IMaterial material, PointD3D p0, PointD3D p1, PointD3D p2)
    {
      var buffers = GetPositionNormalIndexedTriangleBuffer(Materials.GetSolidMaterialWithoutColorOrTexture());

      var offset = buffers.IndexedTriangleBuffer.VertexCount;

      if (buffers.PositionNormalColorIndexedTriangleBuffer is not null)
      {
        var buf = buffers.PositionNormalColorIndexedTriangleBuffer;
        var color = material.Color.Color;
        var r = color.ScR;
        var g = color.ScG;
        var b = color.ScB;
        var a = color.ScA;

        buf.AddTriangleVertex(p0.X, p0.Y, p0.Z, 0, 0, 1, r, g, b, a);
        buf.AddTriangleVertex(p1.X, p1.Y, p1.Z, 0, 0, 1, r, g, b, a);
        buf.AddTriangleVertex(p2.X, p2.Y, p2.Z, 0, 0, 1, r, g, b, a);

        buf.AddTriangleIndices(0, 1, 2);
        buf.AddTriangleIndices(0, 2, 1);
      }
      else
      {
        throw new NotImplementedException("Unexpected type of buffer: " + buffers.IndexedTriangleBuffer.GetType().ToString());
      }
    }

    public virtual void DrawLine(PenX3D pen, PointD3D p0, PointD3D p1)
    {
      DrawLine(GetPositionNormalIndexedTriangleBuffer(pen.Material), pen, p0, p1);
    }

    public virtual void DrawLine(PenX3D pen, IPolylineD3D path)
    {
      var asStraightLine = path as StraightLineAsPolylineD3D;

      if (asStraightLine is not null)
      {
        DrawLine(pen, asStraightLine.GetPoint(0), asStraightLine.GetPoint(1));
        return;
      }
      else if (path.Count == 2)
      {
        DrawLine(pen, path.GetPoint(0), path.GetPoint(1));
        return;
      }

      //var line = new SolidPolyline(pen.CrossSection, asSweepPath.Points);
      var buffers = GetPositionNormalIndexedTriangleBuffer(pen.Material);
      var offset = buffers.IndexedTriangleBuffer.VertexCount;

      if (buffers.PositionNormalIndexedTriangleBuffer is not null)
      {
        var buf = buffers.PositionNormalIndexedTriangleBuffer;
        var solidPolyline = new SolidPolyline();
        solidPolyline.AddWithNormals(
        (position, normal) => buf.AddTriangleVertex(position.X, position.Y, position.Z, normal.X, normal.Y, normal.Z),
        (i0, i1, i2, isLeftCos) => buf.AddTriangleIndices(i0, i1, i2, isLeftCos),
        ref offset,
        pen,
        path.Points);
      }
      else if (buffers.PositionNormalColorIndexedTriangleBuffer is not null)
      {
        var buf = buffers.PositionNormalColorIndexedTriangleBuffer;
        var color = pen.Color.Color;
        var r = color.ScR;
        var g = color.ScG;
        var b = color.ScB;
        var a = color.ScA;

        var solidPolyline = new SolidPolyline();
        solidPolyline.AddWithNormals(
        (position, normal) => buf.AddTriangleVertex(position.X, position.Y, position.Z, normal.X, normal.Y, normal.Z, r, g, b, a),
        (i0, i1, i2, isLeftCos) => buf.AddTriangleIndices(i0, i1, i2, isLeftCos),
        ref offset,
        pen,
        path.Points);
      }
      else if (buffers.PositionNormalUVIndexedTriangleBuffer is not null)
      {
        throw new NotImplementedException("Texture on a line is not supported yet");
      }
      else
      {
        throw new NotImplementedException("Unexpected type of buffer: " + buffers.IndexedTriangleBuffer.GetType().ToString());
      }

      return;
    }

    public virtual VectorD3D MeasureString(string text, FontX3D font, PointD3D pointD3D)
    {
      return FontManager3D.Instance.MeasureString(text, font);
    }

    public virtual void DrawString(string text, FontX3D font, IMaterial brush, PointD3D point, Alignment alignmentX, Alignment alignmentY, Alignment alignmentZ)
    {
      var stringSize = new VectorD3D(0, 0, font.Depth); // depth is already known, for this we don't need to call MeasureString

      if (alignmentX != Alignment.Near || alignmentY != Alignment.Near)
        stringSize = FontManager3D.Instance.MeasureString(text, font);

      switch (alignmentX)
      {
        case Alignment.Near:
          break;

        case Alignment.Center:
          point = point.WithXPlus(-0.5 * stringSize.X);
          break;

        case Alignment.Far:
          point = point.WithXPlus(-stringSize.X);
          break;

        default:
          break;
      }

      switch (alignmentY)
      {
        case Alignment.Near:
          break;

        case Alignment.Center:
          point = point.WithYPlus(-0.5 * stringSize.Y);
          break;

        case Alignment.Far:
          point = point.WithYPlus(-stringSize.Y);
          break;

        default:
          break;
      }

      switch (alignmentZ)
      {
        case Alignment.Near:
          break;

        case Alignment.Center:
          point = point.WithZPlus(-0.5 * stringSize.Z);
          break;

        case Alignment.Far:
          point = point.WithZPlus(-stringSize.Z);
          break;

        default:
          break;
      }

      DrawString(text, font, brush, point);
    }

    public virtual void DrawString(string text, FontX3D font, IMaterial brush, PointD3D point)
    {
      var txt = new SolidText(text, font);

      /*
            VectorD3D stringSize = new VectorD3D(0, 0, font.Depth);

            if (stringAlignment.AlignmentX != Alignment.Near || stringAlignment.AlignmentY != Alignment.Near)
                stringSize = FontManager3D.Instance.MeasureString(text, font);

            switch (stringAlignment.AlignmentX)
            {
                case Alignment.Near:
                    break;

                case Alignment.Center:
                    point = point.WithXPlus(-0.5 * stringSize.X);
                    break;

                case Alignment.Far:
                    point = point.WithXPlus(-stringSize.X);
                    break;

                default:
                    break;
            }

            switch (stringAlignment.AlignmentY)
            {
                case Alignment.Near:
                    break;

                case Alignment.Center:
                    point = point.WithYPlus(-0.5 * stringSize.Y);
                    break;

                case Alignment.Far:
                    point = point.WithYPlus(-stringSize.Y);
                    break;

                default:
                    break;
            }

            switch (stringAlignment.AlignmentZ)
            {
                case Alignment.Near:
                    break;

                case Alignment.Center:
                    point = point.WithZPlus(-0.5 * stringSize.Z);
                    break;

                case Alignment.Far:
                    point = point.WithZPlus(-stringSize.Z);
                    break;

                default:
                    break;
            }
            */

      var buffers = GetPositionNormalIndexedTriangleBuffer(brush);
      var offset = buffers.IndexedTriangleBuffer.VertexCount;

      if (buffers.PositionNormalIndexedTriangleBuffer is not null)
      {
        var buf = buffers.PositionNormalIndexedTriangleBuffer;
        txt.AddWithNormals(
            FontManager3D.Instance.GetCharacterGeometry,
        (position, normal) => buf.AddTriangleVertex(position.X + point.X, position.Y + point.Y, position.Z + point.Z, normal.X, normal.Y, normal.Z),
        (i0, i1, i2) => buf.AddTriangleIndices(i0, i1, i2),
        ref offset);
      }
      else if (buffers.PositionNormalColorIndexedTriangleBuffer is not null)
      {
        var buf = buffers.PositionNormalColorIndexedTriangleBuffer;
        var color = brush.Color.Color;
        var r = color.ScR;
        var g = color.ScG;
        var b = color.ScB;
        var a = color.ScA;

        txt.AddWithNormals(
            FontManager3D.Instance.GetCharacterGeometry,
        (position, normal) => buf.AddTriangleVertex(position.X + point.X, position.Y + point.Y, position.Z + point.Z, normal.X, normal.Y, normal.Z, r, g, b, a),
        (i0, i1, i2) => buf.AddTriangleIndices(i0, i1, i2),
        ref offset);
      }
      else if (buffers.PositionNormalUVIndexedTriangleBuffer is not null)
      {
        throw new NotImplementedException("Texture on a text is not supported yet");
      }
      else
      {
        throw new NotImplementedException("Unexpected type of buffer: " + buffers.IndexedTriangleBuffer.GetType().ToString());
      }
    }

    #endregion Primitives rendering
  }
}
