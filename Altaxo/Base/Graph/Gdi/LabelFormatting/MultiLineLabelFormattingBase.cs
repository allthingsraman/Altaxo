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
using System.Drawing;

using Altaxo.Data;
namespace Altaxo.Graph.Gdi.LabelFormatting
{
	/// <summary>
	/// Base class that can be used to derive a numeric abel formatting class
	/// </summary>
	public abstract class MultiLineLabelFormattingBase : LabelFormattingBase
	{
		double _relativeLineSpacing = 1;
		StringAlignment _textBlockAlignment;


		#region Serialization

		[Altaxo.Serialization.Xml.XmlSerializationSurrogateFor(typeof(MultiLineLabelFormattingBase), 0)]
		class XmlSerializationSurrogate0 : Altaxo.Serialization.Xml.IXmlSerializationSurrogate
		{
			public void Serialize(object obj, Altaxo.Serialization.Xml.IXmlSerializationInfo info)
			{
				MultiLineLabelFormattingBase s = (MultiLineLabelFormattingBase)obj;
				info.AddBaseValueEmbedded(s, typeof(LabelFormattingBase));
				info.AddValue("LineSpacing", s._relativeLineSpacing);
				info.AddEnum("BlockAlignment", s._textBlockAlignment);
			}
			public object Deserialize(object o, Altaxo.Serialization.Xml.IXmlDeserializationInfo info, object parent)
			{
				MultiLineLabelFormattingBase s = (MultiLineLabelFormattingBase)o;
				info.GetBaseValueEmbedded(s, typeof(LabelFormattingBase), parent);
				s._relativeLineSpacing = info.GetDouble("LineSpacing");
				s._textBlockAlignment = (StringAlignment)info.GetEnum("BlockAlignment", typeof(StringAlignment));

				return s;
			}
		}

		#endregion

		protected MultiLineLabelFormattingBase()
		{
		}

		protected MultiLineLabelFormattingBase(MultiLineLabelFormattingBase from)
			: base(from) // everything is done here, since CopyFrom is virtual
		{
		}


		public override bool CopyFrom(object obj)
		{
			var isCopied = base.CopyFrom(obj);
			if (isCopied && !object.ReferenceEquals(this, obj))
			{
				var from = obj as MultiLineLabelFormattingBase;
				if (null != from)
				{
					this._relativeLineSpacing = from._relativeLineSpacing;
					this._textBlockAlignment = from._textBlockAlignment;
				}
			}
			return isCopied;
		}

		public double LineSpacing
		{
			get
			{
				return _relativeLineSpacing;
			}
			set
			{
				_relativeLineSpacing = value;
			}
		}


		public StringAlignment TextBlockAlignment
		{
			get
			{
				return _textBlockAlignment;
			}
			set
			{
				_textBlockAlignment = value;
			}
		}




		/// <summary>
		/// Measures a couple of items and prepares them for being drawn.
		/// </summary>
		/// <param name="g">Graphics context.</param>
		/// <param name="font">Font used.</param>
		/// <param name="strfmt">String format used.</param>
		/// <param name="items">Array of items to be drawn.</param>
		/// <param name="prefixText">Text drawn before the label text.</param>
		/// <param name="postfixText">Text drawn after the label text.</param>
		/// <returns>An array of <see cref="IMeasuredLabelItem" /> that can be used to determine the size of each item and to draw it.</returns>
		public override IMeasuredLabelItem[] GetMeasuredItems(Graphics g, System.Drawing.Font font, System.Drawing.StringFormat strfmt, AltaxoVariant[] items)
		{
			string[] titems = FormatItems(items);
			if (!string.IsNullOrEmpty(_prefix) || !string.IsNullOrEmpty(_suffix))
			{
				for (int i = 0; i < titems.Length; ++i)
					titems[i] = _prefix + titems[i] + _suffix;
			}

			MeasuredLabelItem[] litems = new MeasuredLabelItem[titems.Length];

			Font localfont = (Font)font.Clone();
			StringFormat localstrfmt = (StringFormat)strfmt.Clone();

			StringAlignment horizontalAlignment = localstrfmt.Alignment;
			StringAlignment verticalAlignment = localstrfmt.LineAlignment;

			localstrfmt.Alignment = StringAlignment.Near;
			localstrfmt.LineAlignment = StringAlignment.Near;

			for (int i = 0; i < titems.Length; ++i)
			{
				litems[i] = new MeasuredLabelItem(g, localfont, localstrfmt, titems[i], _relativeLineSpacing, horizontalAlignment, verticalAlignment, _textBlockAlignment);
			}

			return litems;
		}

		protected new class MeasuredLabelItem : IMeasuredLabelItem
		{
			protected string[] _text;
			protected PointD2D[] _stringSize;
			protected Font _font;
			protected System.Drawing.StringFormat _strfmt;
			protected PointD2D _size;

			protected StringAlignment _horizontalAlignment;
			protected StringAlignment _verticalAlignment;
			protected StringAlignment _textBlockAligment;
			protected double _lineSpacing;

			#region IMeasuredLabelItem Members

			public MeasuredLabelItem(Graphics g, Font font, StringFormat strfmt, string itemtext, double lineSpacing, StringAlignment horizontalAlignment, StringAlignment verticalAlignment, StringAlignment textBlockAligment)
			{
				_text = itemtext.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
				_stringSize = new PointD2D[_text.Length];
				_font = font;
				_horizontalAlignment = horizontalAlignment;
				_verticalAlignment = verticalAlignment;
				_textBlockAligment = textBlockAligment;
				_strfmt = strfmt;
				_lineSpacing = lineSpacing;
				_size = SizeF.Empty;
				var bounds = RectangleD.Empty;
				var position = new PointD2D();
				for (int i = 0; i < _text.Length; ++i)
				{
					_stringSize[i] = g.MeasureString(_text[i], _font, new PointF(0, 0), strfmt);
					bounds.ExpandToInclude(new RectangleD(position, _stringSize[i]));
					position.Y += _stringSize[i].Y * _lineSpacing;

				}
				_size = bounds.Size;
			}

			public virtual SizeF Size
			{
				get
				{
					return (SizeF)_size;
				}
			}

			public virtual void Draw(Graphics g, BrushX brush, PointF point)
			{
				var positionX = point.X + GetHorizontalOffset();
				var positionY = point.Y + GetVerticalOffset();

				for (int i = 0; i < _text.Length; ++i)
				{
					var posX = positionX;
					switch (_textBlockAligment)
					{
						case StringAlignment.Center:
							posX += (_size.X - _stringSize[i].X) * 0.5;
							break;
						case StringAlignment.Far:
							posX += (_size.X - _stringSize[i].X);
							break;
					}

					g.DrawString(_text[i], _font, brush, new PointF((float)posX, (float)positionY), _strfmt);
					positionY += _stringSize[i].Y * _lineSpacing;
				}
			}


			double GetHorizontalOffset()
			{
				switch (_horizontalAlignment)
				{
					default:
					case StringAlignment.Near:
						return 0;
					case StringAlignment.Center:
						return _size.X / 2;
					case StringAlignment.Far:
						return _size.X;
				}
			}

			double GetVerticalOffset()
			{
				switch (_verticalAlignment)
				{
					default:
					case StringAlignment.Near:
						return 0;
					case StringAlignment.Center:
						return _size.Y / 2;
					case StringAlignment.Far:
						return _size.Y;
				}
			}

			#endregion

		}
	}
}