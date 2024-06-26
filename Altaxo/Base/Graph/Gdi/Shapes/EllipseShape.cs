﻿#region Copyright

/////////////////////////////////////////////////////////////////////////////
//    Altaxo:  a data processing and data plotting program
//    Copyright (C) 2002-2011 Dr. Dirk Lellinger
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
using System.Drawing;
using System.Drawing.Drawing2D;
using Altaxo.Geometry;

namespace Altaxo.Graph.Gdi.Shapes
{
  [Serializable]
  public class EllipseShape : ClosedPathShapeBase
  {
    #region Serialization

    [Altaxo.Serialization.Xml.XmlSerializationSurrogateFor("AltaxoBase", "Altaxo.Graph.EllipseGraphic", 0)]
    [Altaxo.Serialization.Xml.XmlSerializationSurrogateFor(typeof(EllipseShape), 1)]
    private class XmlSerializationSurrogate0 : Altaxo.Serialization.Xml.IXmlSerializationSurrogate
    {
      public void Serialize(object obj, Altaxo.Serialization.Xml.IXmlSerializationInfo info)
      {
        var s = (EllipseShape)obj;
        info.AddBaseValueEmbedded(s, typeof(EllipseShape).BaseType!);
      }

      public object Deserialize(object? o, Altaxo.Serialization.Xml.IXmlDeserializationInfo info, object? parent)
      {
        var s = (EllipseShape?)o ?? new EllipseShape(info);
        info.GetBaseValueEmbedded(s, typeof(EllipseShape).BaseType!, parent);

        return s;
      }
    }

    #endregion Serialization

    #region Constructors

    protected EllipseShape(Altaxo.Serialization.Xml.IXmlDeserializationInfo info)
      : base(new ItemLocationDirect(), info)
    {
    }

    public EllipseShape(Altaxo.Main.Properties.IReadOnlyPropertyBag context)
      : base(new ItemLocationDirect(), context)
    {
    }

    public EllipseShape(EllipseShape from)
      :
      base(from)
    {
      // No extra members to copy here!
    }

    #endregion Constructors

    public override object Clone()
    {
      return new EllipseShape(this);
    }

    /// <summary>
    /// Get the object outline for arrangements in object world coordinates.
    /// </summary>
    /// <returns>Object outline for arrangements in object world coordinates</returns>
    public override GraphicsPath GetObjectOutlineForArrangements()
    {
      var gp = new GraphicsPath();
      var bounds = Bounds;
      gp.AddEllipse(new RectangleF((float)(bounds.X), (float)(bounds.Y), (float)bounds.Width, (float)bounds.Height));
      return gp;
    }

    public override void Paint(Graphics g, IPaintContext context)
    {
      GraphicsState gs = g.Save();
      TransformGraphics(g);

      var bounds = Bounds;
      var boundsF = bounds.ToGdi();
      if (Brush.IsVisible)
      {
        using (var brushGdi = BrushCacheGdi.Instance.BorrowBrush(Brush, Bounds, g, Math.Max(ScaleX, ScaleY)))
        {
          g.FillEllipse(brushGdi, boundsF);
        }
      }

      using var penGdi = PenCacheGdi.Instance.BorrowPen(Pen, boundsF.ToAxo(), g, Math.Max(ScaleX, ScaleY));
      g.DrawEllipse(penGdi, boundsF);
      g.Restore(gs);
    }
  } // end class
} // end Namespace
