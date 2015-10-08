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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;

namespace Altaxo.Graph3D.Shapes
{
	using Graph.Plot.Data;
	using GraphicsContext;
	using Plot;

	public partial class TextGraphic : GraphicBase
	{
		private class FontCache : IDisposable
		{
			private Dictionary<FontX3D, FontInfo> _fontInfoDictionary = new Dictionary<FontX3D, FontInfo>();

			public FontInfo GetFontInfo(FontX3D id)
			{
				FontInfo result;
				if (!_fontInfoDictionary.TryGetValue(id, out result))
				{
					result = FontManager3D.GetFontInformation(id);
					_fontInfoDictionary.Add(id, result);
				}
				return result;
			}

			public void Clear()
			{
				_fontInfoDictionary.Clear();
			}

			#region IDisposable Members

			public void Dispose()
			{
				Clear();
			}

			#endregion IDisposable Members
		}

		private class StyleContext
		{
			public FontX3D BaseFontId { get; set; }

			public FontX3D FontId { get; set; }

			public IMaterial3D brush;

			public StyleContext(FontX3D font, IMaterial3D brush)
			{
				FontId = font;
				this.brush = brush;
			}

			public StyleContext Clone()
			{
				return (StyleContext)MemberwiseClone();
			}

			public void SetFont(FontX3D font)
			{
				FontId = font;
			}

			public void ScaleFont(double scale)
			{
				FontId = FontId.GetFontWithNewSize(scale * FontId.Size);
			}

			public void SetFontStyle(Altaxo.Graph.FontXStyle style)
			{
				FontId = FontId.GetFontWithNewStyle(style);
			}

			/// <summary>
			/// Merges the providedstyle into the present style. Example: if the present style is Bold, and the style parameter is Italic, then the merged style is 'Bold Italic'.
			/// </summary>
			/// <param name="style">The style to merge with the present style.</param>
			public void MergeFontStyle(Altaxo.Graph.FontXStyle style)
			{
				var newStyle = FontId.Style | style;
				FontId = FontId.GetFontWithNewStyle(newStyle);
			}
		}

		private class MeasureContext
		{
			public object LinkedObject { get; set; }

			public FontCache FontCache { get; set; }

			public double TabStop { get; set; }
		}

		private class DrawContext
		{
			public object LinkedObject { get; set; }

			public FontCache FontCache { get; set; }

			public bool bForPreview { get; set; }

			public Dictionary<TransformedRectangularVolume, IGPlotItem> _cachedSymbolPositions = new Dictionary<TransformedRectangularVolume, IGPlotItem>();
			public MatrixD3D transformMatrix;
		}

		private class Glyph
		{
			// Modification of StringFormat is necessary to avoid
			// too big spaces between successive words
			protected static StringFormat _stringFormat;

			/// <summary>Parent of this object.</summary>
			public StructuralGlyph Parent { get; set; }

			/// <summary>Style of this object.</summary>
			public StyleContext Style { get; set; }

			/// <summary>X position.</summary>
			public double X { get; set; }

			/// <summary>Y position.</summary>
			public double Y { get; set; }

			/// <summary>Width of the object.</summary>
			public double Width { get; set; }

			public double SizeZ { get; set; }

			/// <summary>Height of the object. Setting this propery, you will set <see cref="ExtendAboveBaseline" /> and <see cref="ExtendBelowBaseline" /> both to Height/2.</summary>
			public double Height
			{
				get { return ExtendAboveBaseline + ExtendBelowBaseline; }
				set { ExtendAboveBaseline = value / 2; ExtendBelowBaseline = value / 2; }
			}

			/// <summary>Height of the object above the baseline.</summary>
			public double ExtendAboveBaseline { get; set; }

			/// <summary>Extend of the object below the baseline. (Normally positive).</summary>
			public double ExtendBelowBaseline { get; set; }

			/// <summary></summary>
			public virtual void Measure(MeasureContext mc, double x)
			{
				Width = 0;
				Height = 0;
				SizeZ = 0;
			}

			/// <summary>Draws the object.</summary>
			public virtual void Draw(IGraphicContext3D g, DrawContext dc, double xbase, double ybase, double zbase)
			{
			}

			/// <summary>
			/// Returns the commonly used StringFormat for all glyphs.
			/// </summary>
			public virtual StringFormat StringFormat { get { return _stringFormat; } }

			static Glyph()
			{
				_stringFormat = (StringFormat)StringFormat.GenericTypographic.Clone();
				_stringFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;

				_stringFormat.LineAlignment = StringAlignment.Near;
				_stringFormat.Alignment = StringAlignment.Near;
			}

			/// <summary>Measures the string with the appropriate generic typographic format.</summary>
			/// <param name="text">The text to measure.</param>
			/// <param name="font">The font used.</param>
			/// <returns>Width and height of the text packed into a <see cref="VectorD3D"/> structure.</returns>
			public static VectorD3D MeasureString(string text, FontX3D font)
			{
				return FontManager3D.MeasureString(text, font, _stringFormat);
			}
		}

		#region Structural glyphs

		private class StructuralGlyph : Glyph
		{
			public virtual void Add(Glyph g)
			{
			}

			public virtual void Exchange(StructuralGlyph presentchildnode, StructuralGlyph newchildnode)
			{
			}
		}

		private class MultiChildGlyph : StructuralGlyph
		{
			protected List<Glyph> _childs = new List<Glyph>();

			public override void Add(Glyph g)
			{
				g.Parent = this;
				_childs.Add(g);
			}

			public override void Exchange(StructuralGlyph presentchildnode, StructuralGlyph newchildnode)
			{
				int idx = _childs.IndexOf(presentchildnode);
				if (idx < 0)
					throw new ArgumentException("presentchildnode is not a child of this node");

				_childs[idx] = newchildnode;
				newchildnode.Parent = this;
				presentchildnode.Parent = null;
			}
		}

		private class VerticalStack : MultiChildGlyph
		{
			public double LineSpacingFactor = 1;
			public bool FixedLineSpacing = false;

			private double GetLineSpacing(Glyph line, FontInfo fontInfo)
			{
				if (FixedLineSpacing)
				{
					return fontInfo.cyLineSpace * LineSpacingFactor;
				}
				else
				{
					return line.Height;
				}
			}

			public override void Measure(MeasureContext mc, double x)
			{
				var fontInfo = mc.FontCache.GetFontInfo(Style.FontId);

				double w = 0, h = 0;
				double y = 0;

				foreach (var ch in _childs)
				{
					ch.Measure(mc, x);
					w = Math.Max(w, ch.Width);
					h = y + ch.Height;
					y += GetLineSpacing(ch, fontInfo);
				}

				Width = w;
				if (_childs.Count == 1)
				{
					ExtendAboveBaseline = _childs[0].ExtendAboveBaseline;
					ExtendBelowBaseline = _childs[0].ExtendBelowBaseline;
				}
				else if (_childs.Count == 2)
				{
					double heightDiff = h - (_childs[0].Height + _childs[1].Height);
					ExtendAboveBaseline = _childs[0].Height + heightDiff / 2;
					ExtendBelowBaseline = _childs[1].Height + heightDiff / 2;
				}
				else
				{
					Height = h;
				}
			}

			public override void Draw(IGraphicContext3D g, DrawContext dc, double xbase, double ybase, double zbase)
			{
				var fontInfo = dc.FontCache.GetFontInfo(Style.FontId);

				double y = ybase - ExtendAboveBaseline;

				foreach (var ch in _childs)
				{
					ch.Draw(g, dc, xbase, y + ch.ExtendAboveBaseline, zbase);
					y += GetLineSpacing(ch, fontInfo);
				}
			}
		}

		private class GlyphLine : MultiChildGlyph
		{
			public override void Measure(MeasureContext mc, double x)
			{
				ExtendBelowBaseline = 0;
				ExtendAboveBaseline = 0;
				Width = 0;
				foreach (var glyph in _childs)
				{
					glyph.Measure(mc, x + Width);
					ExtendAboveBaseline = Math.Max(ExtendAboveBaseline, glyph.ExtendAboveBaseline);
					ExtendBelowBaseline = Math.Max(ExtendBelowBaseline, glyph.ExtendBelowBaseline);
					Width += glyph.Width;
				}
			}

			public override void Draw(IGraphicContext3D g, DrawContext dc, double xbase, double ybase, double zbase)
			{
				double x = xbase;
				foreach (var ch in _childs)
				{
					ch.Draw(g, dc, x, ybase, zbase);
					x += ch.Width;
				}
			}
		}

		private class SingleChildGlyph : StructuralGlyph
		{
			protected Glyph _child;

			public override void Add(Glyph g)
			{
				if (_child != null)
					throw new ArgumentException("child already present");

				g.Parent = this;
				_child = g;
			}

			public override void Exchange(StructuralGlyph presentchildnode, StructuralGlyph newchildnode)
			{
				if (_child != presentchildnode)
					throw new ArgumentException("presentchildnode is not a child of this node");

				_child = newchildnode;
				newchildnode.Parent = this;
				presentchildnode.Parent = null;
			}
		}

		private class Subscript : SingleChildGlyph
		{
			public override void Measure(MeasureContext mc, double x)
			{
				ExtendAboveBaseline = 0;
				ExtendBelowBaseline = 0;
				Width = 0;
				if (_child != null)
				{
					_child.Measure(mc, x);

					FontInfo fontInfo = mc.FontCache.GetFontInfo(Style.FontId);
					double shift = (0.35 * fontInfo.cyAscent);
					ExtendBelowBaseline = Math.Max(ExtendBelowBaseline, _child.ExtendBelowBaseline + shift);
					ExtendAboveBaseline = Math.Max(ExtendAboveBaseline, _child.ExtendAboveBaseline - shift);
					Width = Math.Max(Width, _child.Width);
				}
			}

			public override void Draw(IGraphicContext3D g, DrawContext dc, double xbase, double ybase, double zbase)
			{
				if (null != _child)
				{
					var fontInfo = dc.FontCache.GetFontInfo(Style.FontId);
					_child.Draw(g, dc, xbase, ybase + 0.35 * fontInfo.cyAscent, zbase);
				}
			}
		}

		private class Superscript : SingleChildGlyph
		{
			public override void Measure(MeasureContext mc, double x)
			{
				ExtendAboveBaseline = 0;
				ExtendBelowBaseline = 0;
				Width = 0;
				if (_child != null)
				{
					_child.Measure(mc, x);
					var fontInfo = mc.FontCache.GetFontInfo(Style.FontId);
					double shift = (0.35 * fontInfo.cyAscent);
					ExtendBelowBaseline = Math.Max(ExtendBelowBaseline, _child.ExtendBelowBaseline - shift);
					ExtendAboveBaseline = Math.Max(ExtendAboveBaseline, _child.ExtendAboveBaseline + shift);
					Width = Math.Max(Width, _child.Width);
				}
			}

			public override void Draw(IGraphicContext3D g, DrawContext dc, double xbase, double ybase, double zbase)
			{
				if (_child != null)
				{
					var fontInfo = dc.FontCache.GetFontInfo(Style.FontId);
					_child.Draw(g, dc, xbase, ybase - 0.35 * fontInfo.cyAscent, zbase);
				}
			}
		}

		private class DotOverGlyph : SingleChildGlyph
		{
			public override void Measure(MeasureContext mc, double x)
			{
				ExtendAboveBaseline = 0;
				ExtendBelowBaseline = 0;
				Width = 0;
				if (_child != null)
				{
					_child.Measure(mc, x);
					ExtendBelowBaseline = _child.ExtendBelowBaseline;
					ExtendAboveBaseline = _child.ExtendAboveBaseline;
					Width = _child.Width;
				}
			}

			public override void Draw(IGraphicContext3D g, DrawContext dc, double xbase, double ybase, double zbase)
			{
				if (_child != null)
				{
					_child.Draw(g, dc, xbase, ybase, zbase);
					FontInfo fontInfo = dc.FontCache.GetFontInfo(Style.FontId);
					double psize = FontManager3D.MeasureString(".", Style.FontId, this.StringFormat).X;
					g.DrawString(".", Style.FontId, Style.brush, new PointD3D((xbase + _child.Width / 2 - psize / 2), (ybase - _child.ExtendAboveBaseline - fontInfo.cyAscent), zbase), this.StringFormat);
				}
			}
		}

		private class BarOverGlyph : SingleChildGlyph
		{
			public override void Measure(MeasureContext mc, double x)
			{
				ExtendAboveBaseline = 0;
				ExtendBelowBaseline = 0;
				Width = 0;
				if (_child != null)
				{
					_child.Measure(mc, x);
					ExtendBelowBaseline = _child.ExtendBelowBaseline;
					ExtendAboveBaseline = _child.ExtendAboveBaseline;
					Width = _child.Width;
				}
			}

			public override void Draw(IGraphicContext3D g, DrawContext dc, double xbase, double ybase, double zbase)
			{
				if (_child != null)
				{
					_child.Draw(g, dc, xbase, ybase, zbase);
					FontInfo fontInfo = dc.FontCache.GetFontInfo(Style.FontId);
					g.DrawString("_", Style.FontId, Style.brush, new PointD3D((xbase), (ybase - _child.ExtendAboveBaseline - fontInfo.cyAscent), zbase), this.StringFormat);
				}
			}
		}

		private class SubSuperScript : StructuralGlyph
		{
			private Glyph _subscript;
			private Glyph _superscript;

			public override void Add(Glyph g)
			{
				if (_subscript == null)
				{
					_subscript = g;
					g.Parent = this;
				}
				else if (_superscript == null)
				{
					_superscript = g;
					g.Parent = this;
				}
				else
				{
					throw new ArgumentException("both subscript and superscript are already present");
				}
			}

			public override void Exchange(StructuralGlyph presentchildnode, StructuralGlyph newchildnode)
			{
				if (_subscript == presentchildnode)
				{
					_subscript = newchildnode;
					newchildnode.Parent = this;
					presentchildnode = null;
				}
				else if (_superscript == presentchildnode)
				{
					_superscript = newchildnode;
					newchildnode.Parent = this;
					presentchildnode = null;
				}
				else
				{
					throw new ArgumentException("presentchildnode is not member of this node");
				}
			}

			public override void Measure(MeasureContext mc, double x)
			{
				ExtendAboveBaseline = 0;
				ExtendBelowBaseline = 0;
				Width = 0;

				var fontInfo = mc.FontCache.GetFontInfo(Style.FontId);
				if (_subscript != null)
				{
					_subscript.Measure(mc, x);

					double shift = (0.35 * fontInfo.cyAscent);
					ExtendBelowBaseline = Math.Max(ExtendBelowBaseline, _subscript.ExtendBelowBaseline + shift);
					ExtendAboveBaseline = Math.Max(ExtendAboveBaseline, _subscript.ExtendAboveBaseline - shift);
					Width = Math.Max(Width, _subscript.Width);
				}
				if (_superscript != null)
				{
					_superscript.Measure(mc, x);

					double shift = (0.35 * fontInfo.cyAscent);
					ExtendBelowBaseline = Math.Max(ExtendBelowBaseline, _subscript.ExtendBelowBaseline - shift);
					ExtendAboveBaseline = Math.Max(ExtendAboveBaseline, _subscript.ExtendAboveBaseline + shift);
					Width = Math.Max(Width, _superscript.Width);
				}
			}

			public override void Draw(IGraphicContext3D g, DrawContext dc, double xbase, double ybase, double zbase)
			{
				var fontInfo = dc.FontCache.GetFontInfo(Style.FontId);
				if (_subscript != null)
					_subscript.Draw(g, dc, xbase, ybase + 0.35 * fontInfo.cyAscent, zbase);
				if (_superscript != null)
					_superscript.Draw(g, dc, xbase, ybase - 0.35 * fontInfo.cyAscent, zbase);
			}
		}

		#endregion Structural glyphs

		#region Glyph leaves

		private class TextGlyph : Glyph
		{
			protected string _text;

			public TextGlyph(string text, StyleContext style)
			{
				_text = text;
				Style = style;
			}

			public override void Measure(MeasureContext mc, double x)
			{
				var fontInfo = mc.FontCache.GetFontInfo(Style.FontId);
				Width = FontManager3D.MeasureString(_text, Style.FontId, _stringFormat).X;
				ExtendAboveBaseline = fontInfo.cyAscent;
				ExtendBelowBaseline = fontInfo.cyDescent;
			}

			public override void Draw(IGraphicContext3D g, DrawContext dc, double xbase, double ybase, double zbase)
			{
				var fontInfo = dc.FontCache.GetFontInfo(Style.FontId);
				g.DrawString(_text, Style.FontId, Style.brush, new PointD3D(xbase, (ybase - fontInfo.cyAscent), zbase), _stringFormat);
			}

			public override string ToString()
			{
				return _text;
			}
		}

		private class TabGlpyh : Glyph
		{
			public override void Measure(MeasureContext mc, double x)
			{
				Height = 0;
				Width = 0;
				SizeZ = 0;

				double tab = mc.TabStop;

				if (!(tab > 0))
					tab = FontManager3D.MeasureString("MMMM", Style.BaseFontId, _stringFormat).X;

				if (!(tab > 0))
					tab = Style.BaseFontId.Size * 4;

				if (tab > 0)
				{
					double t = Math.Floor(x / tab);
					Width = (t + 1) * tab - x;
				}
			}
		}

		private class PlotName : TextGlyph
		{
			private int _layerNumber;
			private int _plotNumber;
			private string _plotLabelStyle;
			private bool _plotLabelStyleIsPropColName;

			public PlotName(StyleContext context, int plotNumber)
				: this(context, plotNumber, -1)
			{
			}

			public PlotName(StyleContext context, int plotNumber, int plotLayer)
				: base(string.Empty, context)
			{
				_plotNumber = plotNumber;
				_layerNumber = plotLayer;
			}

			public void SetPropertyColumnName(string name)
			{
				_plotLabelStyle = name;
				_plotLabelStyleIsPropColName = true;
			}

			public override void Measure(MeasureContext mc, double x)
			{
				_text = GetName(mc.LinkedObject);
				base.Measure(mc, x);
			}

			private string GetName(object obj)
			{
				string result = string.Empty;

				// first of all, retrieve the actual name
				var mylayer = obj as HostLayer3D;
				if (null == mylayer)
					return result;

				var layer = mylayer as XYPlotLayer3D;
				if (_layerNumber >= 0 && mylayer.SiblingLayers != null && _layerNumber < mylayer.SiblingLayers.Count)
					layer = mylayer.SiblingLayers[_layerNumber] as XYPlotLayer3D;
				if (null == layer)
					return result;
				IGPlotItem pa = null;
				if (_plotNumber < layer.PlotItems.Flattened.Length)
				{
					pa = layer.PlotItems.Flattened[_plotNumber];
				}
				if (pa != null)
				{
					result = pa.GetName(0);

					if (_plotLabelStyle != null && !_plotLabelStyleIsPropColName && pa is XYColumnPlotItem)
					{
						var style = Altaxo.Graph.Gdi.Plot.XYColumnPlotItemLabelTextStyle.YS;
						try { style = (Altaxo.Graph.Gdi.Plot.XYColumnPlotItemLabelTextStyle)Enum.Parse(typeof(Altaxo.Graph.Gdi.Plot.XYColumnPlotItemLabelTextStyle), _plotLabelStyle, true); }
						catch (Exception) { }
						result = ((XYColumnPlotItem)pa).GetName((int)style);
					}

					if (_plotLabelStyleIsPropColName && _plotLabelStyle != null && pa is XYColumnPlotItem)
					{
						XYColumnPlotData pb = ((XYColumnPlotItem)pa).Data;
						Data.DataTable tbl = null;
						if (pb.YColumn is Data.DataColumn)
							tbl = Data.DataTable.GetParentDataTableOf((Data.DataColumn)pb.YColumn);

						if (tbl != null)
						{
							int colNumber = tbl.DataColumns.GetColumnNumber((Data.DataColumn)pb.YColumn);
							if (tbl.PropertyColumns.ContainsColumn(_plotLabelStyle))
								result = tbl.PropertyColumns[_plotLabelStyle][colNumber].ToString();
						}
					}
				}

				return result;
			}
		}

		private class PlotSymbol : Glyph
		{
			// Modification of StringFormat is necessary to avoid
			// too big spaces between successive words
			protected new static StringFormat _stringFormat;

			private int _layerNumber = -1;
			private int _plotNumber;

			static PlotSymbol()
			{
				_stringFormat = (StringFormat)StringFormat.GenericTypographic.Clone();
				_stringFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;

				_stringFormat.LineAlignment = StringAlignment.Near;
				_stringFormat.Alignment = StringAlignment.Near;
			}

			public PlotSymbol(StyleContext style, int plotNumber)
				: this(style, plotNumber, -1)
			{
			}

			public PlotSymbol(StyleContext style, int plotNumber, int layerNumber)
			{
				Style = style;
				_plotNumber = plotNumber;
				_layerNumber = layerNumber;
			}

			public override void Measure(MeasureContext mc, double x)
			{
				Width = 0;
				Height = 0;

				var mylayer = mc.LinkedObject as HostLayer3D;
				if (null == mylayer)
					return;
				XYPlotLayer3D layer = mylayer as XYPlotLayer3D;
				if (_layerNumber >= 0 && null != mylayer.SiblingLayers && _layerNumber < mylayer.SiblingLayers.Count)
					layer = mylayer.SiblingLayers[_layerNumber] as XYPlotLayer3D;

				if (null == layer)
					return;

				if (_plotNumber < layer.PlotItems.Flattened.Length)
				{
					var fontInfo = mc.FontCache.GetFontInfo(Style.FontId);
					Width = FontManager3D.MeasureString("MMM", Style.FontId, _stringFormat).X;
					ExtendAboveBaseline = fontInfo.cyAscent;
					ExtendBelowBaseline = fontInfo.cyDescent;
				}
			}

			public override void Draw(IGraphicContext3D g, DrawContext dc, double xbase, double ybase, double zbase)
			{
				var mylayer = (HostLayer3D)dc.LinkedObject;

				var layer = mylayer as XYPlotLayer3D;

				if (_layerNumber >= 0 && mylayer.SiblingLayers != null && _layerNumber < mylayer.SiblingLayers.Count)
					layer = mylayer.SiblingLayers[_layerNumber] as XYPlotLayer3D;

				if (null == layer)
					return;

				if (_plotNumber < layer.PlotItems.Flattened.Length)
				{
					var fontInfo = dc.FontCache.GetFontInfo(Style.FontId);
					IGPlotItem pa = layer.PlotItems.Flattened[_plotNumber];

					PointD3D symbolpos = new PointD3D(xbase, (ybase + 0.5 * fontInfo.cyDescent - 0.5 * fontInfo.cyAscent), 0);
					RectangleD3D symbolRect = new RectangleD3D(symbolpos, new VectorD3D(Width, 0, 0));
					symbolRect.Inflate(0, fontInfo.Size, 0);
					pa.PaintSymbol(g, symbolRect);

					if (!dc.bForPreview)
					{
						var volume = new TransformedRectangularVolume(
							new RectangleD3D(symbolpos.X, symbolpos.Y - 0.5 * fontInfo.cyLineSpace, 0, Width, fontInfo.cyLineSpace, 0), dc.transformMatrix);
						dc._cachedSymbolPositions.Add(volume, pa);
					}
				}
			}
		}

		private class DocumentIdentifier : TextGlyph
		{
			public DocumentIdentifier(StyleContext style)
				: base(null, style)
			{
			}

			public override void Measure(MeasureContext mc, double x)
			{
				_text = Current.Project.DocumentIdentifier;
				base.Measure(mc, x);
			}
		}

		private class ValueOfProperty : TextGlyph
		{
			private string _propertyName;

			public ValueOfProperty(StyleContext style, string propertyName)
				: base(null, style)
			{
				_propertyName = propertyName;
			}

			public override void Measure(MeasureContext mc, double x)
			{
				_text = string.Empty;
				var suspObj = mc.LinkedObject as Altaxo.Main.IDocumentLeafNode;
				if (null != suspObj)
				{
					var context = Altaxo.PropertyExtensions.GetPropertyContext(suspObj);
					if (null != context)
					{
						Altaxo.Main.Properties.IPropertyBag bag;
						Altaxo.Main.Properties.PropertyBagInformation info;
						object value;
						if (context.TryGetValue<object>(_propertyName, out value, out bag, out info))
						{
							if (null != value)
							{
								var documentCulture = context.GetValue(Altaxo.Settings.CultureSettings.PropertyKeyDocumentCulture);
								_text = string.Format(documentCulture.Culture, "{0}", value);
							}
						}
					}
				}

				base.Measure(mc, x);
			}
		}

		#endregion Glyph leaves
	}
}