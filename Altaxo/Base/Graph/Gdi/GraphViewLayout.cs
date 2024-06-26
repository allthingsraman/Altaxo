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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Altaxo.Geometry;
using Altaxo.Main;

namespace Altaxo.Graph.Gdi
{
  /// <summary>
  /// Stores information about how a graph is shown in the graph view.
  /// </summary>
  public class GraphViewLayout : IProjectItemPresentationModel
  {
    private bool _isAutoZoomActive;
    private double _zoomFactor;
    private PointD2D _positionOfViewportsUpperLeftCornerInRootLayerCoordinates;
    private Altaxo.Graph.Gdi.GraphDocument _graphDocument;

    /// <summary>Initializes a new instance of the <see cref="GraphViewLayout"/> class.</summary>
    /// <param name="isAutoZoomActive">If set to <c>true</c> auto zoom is active.</param>
    /// <param name="zoomFactor">The zoom factor.</param>
    /// <param name="graphDocument">The graph document.</param>
    /// <param name="viewPortsUpperLeftInRootLayerCoord">Vector from the upper left corner of the graph to the upper left corner of the view port.</param>
    public GraphViewLayout(bool isAutoZoomActive, double zoomFactor, GraphDocument graphDocument, PointD2D viewPortsUpperLeftInRootLayerCoord)
    {
      _isAutoZoomActive = isAutoZoomActive;
      _zoomFactor = zoomFactor;
      _positionOfViewportsUpperLeftCornerInRootLayerCoordinates = viewPortsUpperLeftInRootLayerCoord;
      _graphDocument = graphDocument;
    }



    #region Serialization

    /// <summary>Private constructor for deserialization.</summary>
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    private GraphViewLayout(Altaxo.Serialization.Xml.IXmlDeserializationInfo info)
    {
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

    [Altaxo.Serialization.Xml.XmlSerializationSurrogateFor("AltaxoBase", "Altaxo.Graph.GUI.GraphController", 0)]
    [Altaxo.Serialization.Xml.XmlSerializationSurrogateFor("AltaxoSDGui", "Altaxo.Graph.GUI.SDGraphController", 0)]
    [Altaxo.Serialization.Xml.XmlSerializationSurrogateFor("AltaxoSDGui", "Altaxo.Gui.SharpDevelop.SDGraphViewContent", 1)]
    [Altaxo.Serialization.Xml.XmlSerializationSurrogateFor("AltaxoBase", "Altaxo.Gui.Graph.Viewing.GraphController", 1)] // until 2012/02/01
    [Altaxo.Serialization.Xml.XmlSerializationSurrogateFor("AltaxoBase", "Altaxo.Graph.GraphViewLayout", 0)] // since 2012/02/01 build 744
    private class XmlSerializationSurrogate0 : Altaxo.Serialization.Xml.IXmlSerializationSurrogate
    {
      private AbsoluteDocumentPath? _PathToGraph;
      private GraphViewLayout? _GraphController;

      public void Serialize(object obj, Altaxo.Serialization.Xml.IXmlSerializationInfo info)
      {
        var s = (GraphViewLayout)obj;
        info.AddValue("AutoZoom", s._isAutoZoomActive);
        info.AddValue("Zoom", s._zoomFactor);
        info.AddValue("Graph", AbsoluteDocumentPath.GetAbsolutePath(s._graphDocument));
      }

      public object Deserialize(object? o, Altaxo.Serialization.Xml.IXmlDeserializationInfo info, object? parent)
      {
        var s = (GraphViewLayout?)o ?? new GraphViewLayout(info);

        if (info.CurrentElementName == "BaseType")
          info.GetString("BaseType");

        s._isAutoZoomActive = info.GetBoolean("AutoZoom");
        s._zoomFactor = info.GetSingle("Zoom");

        var surr = new XmlSerializationSurrogate0
        {
          _GraphController = s,
          _PathToGraph = (AbsoluteDocumentPath)info.GetValue("Graph", s)
        };
        info.DeserializationFinished += surr.EhDeserializationFinished;

        return s;
      }

      private void EhDeserializationFinished(Altaxo.Serialization.Xml.IXmlDeserializationInfo info, object documentRoot, bool isFinallyCall)
      {
        var o = AbsoluteDocumentPath.GetObject(_PathToGraph!, (Main.IDocumentNode)documentRoot);
        if (o is GraphDocument gd && _GraphController is not null)
        {
          _GraphController._graphDocument = gd;
          info.DeserializationFinished -= EhDeserializationFinished;
        }
      }
    }

    /// <summary>
    /// 2013-12-01: added _positionOfViewportsUpperLeftCornerInRootLayerCoordinates. Zoom and ViewPortOffset is serialized only if AutoZoom is false.
    /// 2015-11-14 Version 2 Moved to Altaxo.Graph.Gdi namespace
    /// </summary>
    [Altaxo.Serialization.Xml.XmlSerializationSurrogateFor("AltaxoBase", "Altaxo.Graph.GraphViewLayout", 1)]
    [Altaxo.Serialization.Xml.XmlSerializationSurrogateFor(typeof(GraphViewLayout), 2)]
    private class XmlSerializationSurrogate1 : Altaxo.Serialization.Xml.IXmlSerializationSurrogate
    {
      private AbsoluteDocumentPath? _PathToGraph;
      private GraphViewLayout? _GraphController;

      public void Serialize(object obj, Altaxo.Serialization.Xml.IXmlSerializationInfo info)
      {
        var s = (GraphViewLayout)obj;
        info.AddValue("Graph", AbsoluteDocumentPath.GetAbsolutePath(s._graphDocument));
        info.AddValue("AutoZoom", s._isAutoZoomActive);
        if (false == s._isAutoZoomActive)
        {
          info.AddValue("Zoom", s._zoomFactor);
          info.AddValue("ViewportOffset", s._positionOfViewportsUpperLeftCornerInRootLayerCoordinates);
        }
      }

      public object Deserialize(object? o, Altaxo.Serialization.Xml.IXmlDeserializationInfo info, object? parent)
      {
        var s = (GraphViewLayout?)o ?? new GraphViewLayout(info);

        var surr = new XmlSerializationSurrogate1
        {
          _GraphController = s,
          _PathToGraph = (AbsoluteDocumentPath)info.GetValue("Graph", s)
        };
        info.DeserializationFinished += surr.EhDeserializationFinished;

        s._isAutoZoomActive = info.GetBoolean("AutoZoom");
        if (false == s._isAutoZoomActive)
        {
          s._zoomFactor = info.GetSingle("Zoom");
          s._positionOfViewportsUpperLeftCornerInRootLayerCoordinates = (PointD2D)info.GetValue("ViewportOffset", s);
        }

        return s;
      }

      private void EhDeserializationFinished(Altaxo.Serialization.Xml.IXmlDeserializationInfo info, object documentRoot, bool isFinallyCall)
      {
        var o = AbsoluteDocumentPath.GetObject(_PathToGraph!, (Main.IDocumentNode)documentRoot);
        if (o is GraphDocument gd && _GraphController is not null)
        {
          _GraphController._graphDocument = (GraphDocument)o;
          info.DeserializationFinished -= EhDeserializationFinished;
        }
      }
    }

    #endregion Serialization

    /// <summary>Get the instance of the graph document that is shown in the view.</summary>
    public GraphDocument GraphDocument { get { return _graphDocument; } }

    /// <summary>Gets a value indicating whether auto zoom is active for the view.</summary>
    /// <value>
    /// 	<c>Is true</c> if auto zoom is active; otherwise, <c>false</c>.
    /// </value>
    public bool IsAutoZoomActive { get { return _isAutoZoomActive; } }

    /// <summary>Gets the zoom factor The zoom factor is the relation between the physical size of the graph on the screen to the design size of the graph.
    ///  This value is only valid if <see cref="IsAutoZoomActive"/> is <c>false</c>.
    ///  </summary>
    public double ZoomFactor { get { return _zoomFactor; } }

    /// <summary>
    /// Gets the position of the viewport's upper left corner in root layer coordinates. This value is only valid if <see cref="IsAutoZoomActive"/> is <c>false</c>.
    /// </summary>
    /// <value>
    /// The position of the viewport's upper left corner in root layer coordinates.
    /// </value>
    public PointD2D PositionOfViewportsUpperLeftCornerInRootLayerCoordinates { get { return _positionOfViewportsUpperLeftCornerInRootLayerCoordinates; } }

    IProjectItem IProjectItemPresentationModel.Document
    {
      get
      {
        return _graphDocument;
      }
    }
  }
}
