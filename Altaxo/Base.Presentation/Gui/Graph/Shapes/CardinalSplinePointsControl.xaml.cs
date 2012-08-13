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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Altaxo.Graph;
using Altaxo.Units;

namespace Altaxo.Gui.Graph.Shapes
{
	/// <summary>
	/// Interaction logic for CardinalSplinePointsControl.xaml
	/// </summary>
	public partial class CardinalSplinePointsControl : UserControl, ICardinalSplinePointsView
	{
		public event Action CurvePointsCopyTriggered;
		public event Action CurvePointsPasteTriggered;

		public CardinalSplinePointsControl()
		{
			InitializeComponent();
		}

		public double Tension
		{
			get
			{
				return _guiTension.SelectedQuantity.AsValueInSIUnits;
			}
			set
			{
				_guiTension.SelectedQuantity = new Units.DimensionfulQuantity(value, Altaxo.Units.Dimensionless.Unity.Instance).AsQuantityIn(_guiTension.UnitEnvironment.DefaultUnit);
			}
		}


		class PointD2DClass : System.ComponentModel.IEditableObject
		{
			public PointD2DClass() { }
			public PointD2DClass(PointD2D p) { X = p.X; Y = p.Y; }
			public double X { get; set; }
			public double Y { get; set; }

			public DimensionfulQuantity XQuantity
			{
				get
				{
					return new DimensionfulQuantity(X, Altaxo.Units.Length.Point.Instance).AsQuantityIn(PositionEnvironment.Instance.DefaultUnit);
				}
				set
				{
					X = value.AsValueIn(Altaxo.Units.Length.Point.Instance);
				}
			}

			public DimensionfulQuantity YQuantity
			{
				get
				{
					return new DimensionfulQuantity(Y, Altaxo.Units.Length.Point.Instance).AsQuantityIn(PositionEnvironment.Instance.DefaultUnit);
				}
				set
				{
					Y = value.AsValueIn(Altaxo.Units.Length.Point.Instance);
				}
			}




			public void BeginEdit()
			{

			}

			public void CancelEdit()
			{

			}

			public void EndEdit()
			{

			}
		}
		ObservableCollection<PointD2DClass> _curvePoints = new ObservableCollection<PointD2DClass>();
		public List<PointD2D> CurvePoints
		{
			get
			{
				List<PointD2D> pts = new List<PointD2D>();
				foreach (var p in _curvePoints)
					pts.Add(new PointD2D(p.X, p.Y));
				return pts;
			}
			set
			{
				_curvePoints.Clear();
				foreach (var p in value)
					_curvePoints.Add(new PointD2DClass(p));

				_guiCurvePoints.DataContext = null;
				_guiCurvePoints.DataContext = _curvePoints;
			}
		}

		private void EhCopyCurvePoints(object sender, RoutedEventArgs e)
		{
			if (null != CurvePointsCopyTriggered)
				CurvePointsCopyTriggered();
		}

		private void EhPasteCurvePoints(object sender, RoutedEventArgs e)
		{
			if (null != CurvePointsPasteTriggered)
				CurvePointsPasteTriggered();
		}
	}
}