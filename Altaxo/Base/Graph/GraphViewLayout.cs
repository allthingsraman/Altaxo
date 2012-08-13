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
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Altaxo.Main;
using Altaxo.Graph.Gdi;

namespace Altaxo.Graph
{
	/// <summary>
	/// Stores information about how a graph is shown in the graph view.
	/// </summary>
	public class GraphViewLayout
	{
		bool _isAutoZoomActive;
		double _zoomFactor;
		Altaxo.Graph.Gdi.GraphDocument _graphDocument;


		/// <summary>Initializes a new instance of the <see cref="GraphViewLayout"/> class.</summary>
		/// <param name="isAutoZoomActive">If set to <c>true</c> auto zoom is active.</param>
		/// <param name="zoomFactor">The zoom factor.</param>
		/// <param name="graphDocument">The graph document.</param>
		public GraphViewLayout(bool isAutoZoomActive, double zoomFactor, GraphDocument graphDocument)
		{
			_isAutoZoomActive = isAutoZoomActive;
			_zoomFactor = zoomFactor;
			_graphDocument = graphDocument;
		}

		/// <summary>Private constructor for deserialization.</summary>
		private GraphViewLayout()
		{
		}

		#region Serialization

		[Altaxo.Serialization.Xml.XmlSerializationSurrogateFor("AltaxoBase", "Altaxo.Graph.GUI.GraphController", 0)]
		[Altaxo.Serialization.Xml.XmlSerializationSurrogateFor("AltaxoSDGui", "Altaxo.Graph.GUI.SDGraphController", 0)]
		[Altaxo.Serialization.Xml.XmlSerializationSurrogateFor("AltaxoSDGui", "Altaxo.Gui.SharpDevelop.SDGraphViewContent", 1)]
		[Altaxo.Serialization.Xml.XmlSerializationSurrogateFor("AltaxoBase", "Altaxo.Gui.Graph.Viewing.GraphController", 1)] // until 2012/02/01
		[Altaxo.Serialization.Xml.XmlSerializationSurrogateFor(typeof(GraphViewLayout), 0)] // since 2012/02/01 build 744
		class XmlSerializationSurrogate0 : Altaxo.Serialization.Xml.IXmlSerializationSurrogate
		{
			DocumentPath _PathToGraph;
			GraphViewLayout _GraphController;

			public void Serialize(object obj, Altaxo.Serialization.Xml.IXmlSerializationInfo info)
			{
				var s = (GraphViewLayout)obj;
				info.AddValue("AutoZoom", s._isAutoZoomActive);
				info.AddValue("Zoom", s._zoomFactor);
				info.AddValue("Graph", DocumentPath.GetAbsolutePath(s._graphDocument));
			}
			public object Deserialize(object o, Altaxo.Serialization.Xml.IXmlDeserializationInfo info, object parent)
			{

				var s = null != o ? (GraphViewLayout)o : new GraphViewLayout();

				if (info.CurrentElementName == "BaseType")
					info.GetString("BaseType");

				s._isAutoZoomActive = info.GetBoolean("AutoZoom");
				s._zoomFactor = info.GetSingle("Zoom");

				XmlSerializationSurrogate0 surr = new XmlSerializationSurrogate0();
				surr._GraphController = s;
				surr._PathToGraph = (DocumentPath)info.GetValue("Graph", s);
				info.DeserializationFinished += new Altaxo.Serialization.Xml.XmlDeserializationCallbackEventHandler(surr.EhDeserializationFinished);

				return s;
			}

			private void EhDeserializationFinished(Altaxo.Serialization.Xml.IXmlDeserializationInfo info, object documentRoot)
			{
				object o = DocumentPath.GetObject(_PathToGraph, documentRoot, _GraphController);
				if (o is GraphDocument)
				{

					_GraphController._graphDocument = (GraphDocument)o;
					info.DeserializationFinished -= new Altaxo.Serialization.Xml.XmlDeserializationCallbackEventHandler(this.EhDeserializationFinished);
				}
			}
		}

		#endregion

		/// <summary>Get the instance of the graph document that is shown in the view.</summary>
		public GraphDocument GraphDocument { get { return _graphDocument; } }

		/// <summary>Gets a value indicating whether auto zoom is active for the view.</summary>
		/// <value>
		/// 	<c>Is true</c> if auto zoom is active; otherwise, <c>false</c>.
		/// </value>
		public bool IsAutoZoomActive { get { return _isAutoZoomActive; }}


		/// <summary>Gets the zoom factor The zoom factor is the relation between the physical size of the graph on the screen to the design size of the graph.</summary>
		public double ZoomFactor { get { return _zoomFactor; }}
	

	}
}