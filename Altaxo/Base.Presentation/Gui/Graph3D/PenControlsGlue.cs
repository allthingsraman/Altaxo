#region Copyright

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

using Altaxo.Drawing.ColorManagement;
using Altaxo.Drawing.D3D;
using Altaxo.Graph.Graph3D;
using Altaxo.Gui.Graph3D.Material;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Altaxo.Gui.Graph3D
{
	public class PenControlsGlue : FrameworkElement
	{
		private bool _userChangedAbsStartCapSize;
		private bool _userChangedAbsEndCapSize;

		private bool _userChangedRelStartCapSize;
		private bool _userChangedRelEndCapSize;

		private bool _isAllPropertiesGlue;

		public PenControlsGlue()
			: this(false)
		{
		}

		public PenControlsGlue(bool isAllPropertiesGlue)
		{
			this.InternalSelectedPen = new PenX3D(BuiltinDarkPlotColorSet.Instance[0], 1);
			_isAllPropertiesGlue = isAllPropertiesGlue;
		}

		#region Pen

		private PenX3D _pen;

		/// <summary>
		/// Gets or sets the pen. The pen you get is a clone of the pen that is used internally. Similarly, when setting the pen, a clone is created, so that the pen
		/// can be used internally, without interfering with external functions that changes the pen.
		/// </summary>
		/// <value>
		/// The pen.
		/// </value>
		public PenX3D Pen
		{
			get
			{
				return _pen; // Pen is not immutable. Before giving it out, make a copy, so an external program can meddle with this without disturbing us
			}
			set
			{
				if (null == value)
					throw new NotImplementedException("Pen is null");
				InternalSelectedPen = value; // Pen is not immutable. Before changing it here in this control, make a copy, so an external program can change the old pen without interference
			}
		}

		/// <summary>
		/// Gets or sets the selected pen internally, <b>but without cloning it. Use this function only internally.</b>
		/// </summary>
		/// <value>
		/// The selected pen.
		/// </value>
		protected PenX3D InternalSelectedPen
		{
			get
			{
				return _pen;
			}
			set
			{
				if (null == value)
					throw new NotImplementedException("Pen is null");

				_pen = value;

				InitControlProperties();
			}
		}

		private void InitControlProperties()
		{
			if (null != CbBrush) CbBrush.SelectedMaterial = _pen.Material;
			if (null != CbLineThickness1) CbLineThickness1.SelectedQuantityAsValueInPoints = _pen.Thickness1;
			if (null != CbLineThickness2) CbLineThickness2.SelectedQuantityAsValueInPoints = _pen.Thickness2;

			/*
						if (null != CbDashStyle) CbDashStyle.SelectedDashStyle = _pen.DashStyleEx;
						if (null != CbDashCap) CbDashCap.SelectedDashCap = _pen.DashCap;
						if (null != CbStartCap) CbStartCap.SelectedLineCap = _pen.StartCap;
						if (null != CbStartCapAbsSize) CbStartCapAbsSize.SelectedQuantityAsValueInPoints = _pen.StartCap.MinimumAbsoluteSizePt;
						if (null != CbStartCapRelSize) CbStartCapRelSize.SelectedQuantityAsValueInSIUnits = _pen.StartCap.MinimumRelativeSize;
						if (null != CbEndCap) CbEndCap.SelectedLineCap = _pen.EndCap;
						if (null != CbEndCapAbsSize) CbEndCapAbsSize.SelectedQuantityAsValueInPoints = _pen.EndCap.MinimumAbsoluteSizePt;
						if (null != CbEndCapRelSize) CbEndCapRelSize.SelectedQuantityAsValueInSIUnits = _pen.EndCap.MinimumRelativeSize;
						if (null != CbLineJoin) CbLineJoin.SelectedLineJoin = _pen.LineJoin;
						if (null != CbMiterLimit) CbMiterLimit.SelectedQuantityAsValueInPoints = _pen.MiterLimit;
						*/
			_userChangedAbsStartCapSize = false;
			_userChangedAbsEndCapSize = false;

			_userChangedRelStartCapSize = false;
			_userChangedRelEndCapSize = false;
		}

		public event EventHandler PenChanged;

		protected virtual void OnPenChanged()
		{
			if (PenChanged != null)
				PenChanged(this, EventArgs.Empty);

			UpdatePreviewPanel();
		}

		private WeakEventHandler _weakPenChangedHandler;

		private void EhPenChanged(object sender, EventArgs e)
		{
			OnPenChanged();
		}

		#endregion Pen

		#region Brush

		private bool _showPlotColorsOnly;
		private MaterialComboBox _cbBrush;

		public MaterialComboBox CbBrush
		{
			get { return _cbBrush; }
			set
			{
				var dpd = System.ComponentModel.DependencyPropertyDescriptor.FromProperty(MaterialComboBox.SelectedMaterialProperty, typeof(MaterialComboBox));

				if (_cbBrush != null)
					dpd.RemoveValueChanged(_cbBrush, EhBrush_SelectionChangeCommitted);

				_cbBrush = value;
				if (_cbBrush != null && null != _pen)
				{
					_cbBrush.ShowPlotColorsOnly = _showPlotColorsOnly;
					_cbBrush.SelectedMaterial = _pen.Material;
				}

				if (_cbBrush != null)
				{
					dpd.AddValueChanged(_cbBrush, EhBrush_SelectionChangeCommitted);
					if (!_isAllPropertiesGlue)
					{
						var menuItem = new MenuItem();
						menuItem.Header = "Custom Pen ...";
						menuItem.Click += EhShowCustomPenDialog;
						_cbBrush.ContextMenu.Items.Insert(0, menuItem);
					}
				}
			}
		}

		public bool ShowPlotColorsOnly
		{
			get
			{
				return _showPlotColorsOnly;
			}
			set
			{
				_showPlotColorsOnly = value;
				if (null != _cbBrush)
					_cbBrush.ShowPlotColorsOnly = _showPlotColorsOnly;
			}
		}

		private void EhBrush_SelectionChangeCommitted(object sender, EventArgs e)
		{
			if (_pen != null)
			{
				_pen = _pen.WithMaterial(_cbBrush.SelectedMaterial);
				OnPenChanged();
			}
		}

		#endregion Brush

		/*

		#region Dash

		private DashStyleComboBox _cbDashStyle;

		public DashStyleComboBox CbDashStyle
		{
			get { return _cbDashStyle; }
			set
			{
				var dpd = System.ComponentModel.DependencyPropertyDescriptor.FromProperty(DashStyleComboBox.SelectedDashStyleProperty, typeof(DashStyleComboBox));

				if (_cbDashStyle != null)
					dpd.RemoveValueChanged(_cbDashStyle, EhDashStyle_SelectionChangeCommitted);

				_cbDashStyle = value;
				if (_pen != null && _cbDashStyle != null)
					_cbDashStyle.SelectedDashStyle = _pen.DashStyleEx;

				if (_cbDashStyle != null)
					dpd.AddValueChanged(_cbDashStyle, EhDashStyle_SelectionChangeCommitted);
			}
		}

		private void EhDashStyle_SelectionChangeCommitted(object sender, EventArgs e)
		{
			if (_pen != null)
			{
				_pen.DashStyleEx = _cbDashStyle.SelectedDashStyle;
				OnPenChanged();
			}
		}

		private DashCapComboBox _cbDashCap;

		public DashCapComboBox CbDashCap
		{
			get { return _cbDashCap; }
			set
			{
				var dpd = System.ComponentModel.DependencyPropertyDescriptor.FromProperty(DashCapComboBox.SelectedDashCapProperty, typeof(DashCapComboBox));

				if (_cbDashCap != null)
					dpd.RemoveValueChanged(_cbDashCap, EhDashCap_SelectionChangeCommitted);

				_cbDashCap = value;
				if (_pen != null && _cbDashCap != null)
					_cbDashCap.SelectedDashCap = _pen.DashCap;

				if (_cbDashCap != null)
					dpd.AddValueChanged(_cbDashCap, EhDashCap_SelectionChangeCommitted);
			}
		}

		private void EhDashCap_SelectionChangeCommitted(object sender, EventArgs e)
		{
			if (_pen != null)
			{
				_pen.DashCap = _cbDashCap.SelectedDashCap;
				OnPenChanged();
			}
		}

		#endregion Dash

	*/

		#region Thickness1

		private Common.Drawing.LineThicknessComboBox _cbThickness1;

		public Common.Drawing.LineThicknessComboBox CbLineThickness1
		{
			get { return _cbThickness1; }
			set
			{
				if (_cbThickness1 != null)
					_cbThickness1.SelectedQuantityChanged -= EhThickness1_ChoiceChanged;

				_cbThickness1 = value;
				if (_pen != null && _cbThickness1 != null)
					_cbThickness1.SelectedQuantityAsValueInPoints = _pen.Thickness1;

				if (_cbThickness1 != null)
					_cbThickness1.SelectedQuantityChanged += EhThickness1_ChoiceChanged;
			}
		}

		private void EhThickness1_ChoiceChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (_pen != null)
			{
				if (null != _cbThickness2)
					_pen = _pen.WithThickness1(_cbThickness1.SelectedQuantityAsValueInPoints);
				else
					_pen = _pen.WithUniformThickness(_cbThickness1.SelectedQuantityAsValueInPoints);
				OnPenChanged();
			}
		}

		#endregion Thickness1

		#region Thickness2

		private Common.Drawing.LineThicknessComboBox _cbThickness2;

		public Common.Drawing.LineThicknessComboBox CbLineThickness2
		{
			get { return _cbThickness2; }
			set
			{
				if (_cbThickness2 != null)
					_cbThickness2.SelectedQuantityChanged -= EhThickness2_ChoiceChanged;

				_cbThickness2 = value;
				if (_pen != null && _cbThickness2 != null)
					_cbThickness2.SelectedQuantityAsValueInPoints = _pen.Thickness1;

				if (_cbThickness2 != null)
					_cbThickness2.SelectedQuantityChanged += EhThickness2_ChoiceChanged;
			}
		}

		private void EhThickness2_ChoiceChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (_pen != null)
			{
				_pen = _pen.WithThickness2(_cbThickness2.SelectedQuantityAsValueInPoints);
				OnPenChanged();
			}
		}

		#endregion Thickness2

		/*

		#region StartCap

		private LineCapComboBox _cbStartCap;

		public LineCapComboBox CbStartCap
		{
			get { return _cbStartCap; }
			set
			{
				var dpd = System.ComponentModel.DependencyPropertyDescriptor.FromProperty(LineCapComboBox.SelectedLineCapProperty, typeof(LineCapComboBox));

				if (_cbStartCap != null)
					dpd.RemoveValueChanged(_cbStartCap, EhStartCap_SelectionChangeCommitted);

				_cbStartCap = value;
				if (_pen != null && _cbStartCap != null)
					_cbStartCap.SelectedLineCap = _pen.StartCap;

				if (_cbStartCap != null)
					dpd.AddValueChanged(_cbStartCap, EhStartCap_SelectionChangeCommitted);
			}
		}

		private void EhStartCap_SelectionChangeCommitted(object sender, EventArgs e)
		{
			if (_pen != null)
			{
				var cap = _cbStartCap.SelectedLineCap;
				if (_userChangedAbsStartCapSize && _cbStartCapAbsSize != null)
					cap = cap.Clone(_cbStartCapAbsSize.SelectedQuantityAsValueInPoints, cap.MinimumRelativeSize);
				if (_userChangedRelStartCapSize && _cbStartCapRelSize != null)
					cap = cap.Clone(cap.MinimumAbsoluteSizePt, _cbStartCapRelSize.SelectedQuantityAsValueInSIUnits);

				using (var suspendToken = _pen.SuspendGetToken())
				{
					_pen.StartCap = cap;
					suspendToken.ResumeSilently();
				};

				if (_cbStartCapAbsSize != null && cap != null)
				{
					var oldValue = _userChangedAbsStartCapSize;
					_cbStartCapAbsSize.SelectedQuantityAsValueInPoints = cap.MinimumAbsoluteSizePt;
					_userChangedAbsStartCapSize = oldValue;
				}
				if (_cbStartCapRelSize != null && cap != null)
				{
					var oldValue = _userChangedRelStartCapSize;
					_cbStartCapRelSize.SelectedQuantityAsValueInSIUnits = cap.MinimumRelativeSize;
					_userChangedRelStartCapSize = oldValue;
				}

				OnPenChanged();
			}
		}

		private LineCapSizeComboBox _cbStartCapAbsSize;

		public LineCapSizeComboBox CbStartCapAbsSize
		{
			get { return _cbStartCapAbsSize; }
			set
			{
				if (_cbStartCapAbsSize != null)
					_cbStartCapAbsSize.SelectedQuantityChanged -= EhStartCapAbsSize_SelectionChangeCommitted;

				_cbStartCapAbsSize = value;
				if (_pen != null && _cbStartCapAbsSize != null)
					_cbStartCapAbsSize.SelectedQuantityAsValueInPoints = _pen.StartCap.MinimumAbsoluteSizePt;

				if (_cbStartCapAbsSize != null)
					_cbStartCapAbsSize.SelectedQuantityChanged += EhStartCapAbsSize_SelectionChangeCommitted;
			}
		}

		private void EhStartCapAbsSize_SelectionChangeCommitted(object sender, DependencyPropertyChangedEventArgs e)
		{
			_userChangedAbsStartCapSize = true;

			if (_pen != null)
			{
				var cap = _pen.StartCap;
				cap = cap.Clone(_cbStartCapAbsSize.SelectedQuantityAsValueInPoints, cap.MinimumRelativeSize);

				using (var suspendToken = _pen.SuspendGetToken())
				{
					_pen.StartCap = cap;
					suspendToken.ResumeSilently();
				};

				OnPenChanged();
			}
		}

		private QuantityWithUnitTextBox _cbStartCapRelSize;

		public QuantityWithUnitTextBox CbStartCapRelSize
		{
			get { return _cbStartCapRelSize; }
			set
			{
				if (_cbStartCapRelSize != null)
					_cbStartCapRelSize.SelectedQuantityChanged -= EhStartCapRelSize_SelectionChangeCommitted;

				_cbStartCapRelSize = value;
				if (_pen != null && _cbStartCapRelSize != null)
					_cbStartCapRelSize.SelectedQuantityAsValueInSIUnits = _pen.StartCap.MinimumRelativeSize;

				if (_cbStartCapRelSize != null)
					_cbStartCapRelSize.SelectedQuantityChanged += EhStartCapRelSize_SelectionChangeCommitted;
			}
		}

		private void EhStartCapRelSize_SelectionChangeCommitted(object sender, DependencyPropertyChangedEventArgs e)
		{
			_userChangedRelStartCapSize = true;

			if (_pen != null)
			{
				var cap = _pen.StartCap;
				cap = cap.Clone(cap.MinimumAbsoluteSizePt, _cbStartCapRelSize.SelectedQuantityAsValueInSIUnits);

				using (var suspendToken = _pen.SuspendGetToken())
				{
					_pen.StartCap = cap;
					suspendToken.ResumeSilently();
				};

				OnPenChanged();
			}
		}

		#endregion StartCap

		#region EndCap

		private LineCapComboBox _cbEndCap;

		public LineCapComboBox CbEndCap
		{
			get { return _cbEndCap; }
			set
			{
				var dpd = System.ComponentModel.DependencyPropertyDescriptor.FromProperty(LineCapComboBox.SelectedLineCapProperty, typeof(LineCapComboBox));

				if (_cbEndCap != null)
					dpd.RemoveValueChanged(_cbEndCap, EhEndCap_SelectionChangeCommitted);

				_cbEndCap = value;
				if (_pen != null && _cbEndCap != null)
					_cbEndCap.SelectedLineCap = _pen.EndCap;

				if (_cbEndCap != null)
					dpd.AddValueChanged(_cbEndCap, EhEndCap_SelectionChangeCommitted);
			}
		}

		private void EhEndCap_SelectionChangeCommitted(object sender, EventArgs e)
		{
			if (_pen != null)
			{
				var cap = _cbEndCap.SelectedLineCap;
				if (_userChangedAbsEndCapSize && _cbEndCapAbsSize != null)
					cap = cap.Clone(_cbEndCapAbsSize.SelectedQuantityAsValueInPoints, cap.MinimumRelativeSize);
				if (_userChangedRelEndCapSize && _cbEndCapRelSize != null)
					cap = cap.Clone(cap.MinimumAbsoluteSizePt, _cbEndCapRelSize.SelectedQuantityAsValueInSIUnits);

				using (var suspendToken = _pen.SuspendGetToken())
				{
					_pen.EndCap = cap;
					suspendToken.ResumeSilently();
				};

				if (_cbEndCapAbsSize != null)
				{
					var oldValue = _userChangedAbsEndCapSize;
					_cbEndCapAbsSize.SelectedQuantityAsValueInPoints = cap.MinimumAbsoluteSizePt;
					_userChangedAbsEndCapSize = oldValue;
				}
				if (_cbEndCapRelSize != null)
				{
					var oldValue = _userChangedRelEndCapSize;
					_cbEndCapRelSize.SelectedQuantityAsValueInSIUnits = cap.MinimumRelativeSize;
					_userChangedRelEndCapSize = oldValue;
				}

				OnPenChanged();
			}
		}

		private LineCapSizeComboBox _cbEndCapAbsSize;

		public LineCapSizeComboBox CbEndCapAbsSize
		{
			get { return _cbEndCapAbsSize; }
			set
			{
				if (_cbEndCapAbsSize != null)
					_cbEndCapAbsSize.SelectedQuantityChanged -= EhEndCapAbsSize_SelectionChangeCommitted;

				_cbEndCapAbsSize = value;
				if (_pen != null && _cbEndCapAbsSize != null)
					_cbEndCapAbsSize.SelectedQuantityAsValueInPoints = _pen.EndCap.MinimumAbsoluteSizePt;

				if (_cbEndCapAbsSize != null)
					_cbEndCapAbsSize.SelectedQuantityChanged += EhEndCapAbsSize_SelectionChangeCommitted;
			}
		}

		private void EhEndCapAbsSize_SelectionChangeCommitted(object sender, DependencyPropertyChangedEventArgs e)
		{
			_userChangedAbsEndCapSize = true;

			if (_pen != null)
			{
				var cap = _pen.EndCap;
				cap = cap.Clone(_cbEndCapAbsSize.SelectedQuantityAsValueInPoints, cap.MinimumRelativeSize);

				using (var suspendToken = _pen.SuspendGetToken())
				{
					_pen.EndCap = cap;
					suspendToken.ResumeSilently();
				};

				OnPenChanged();
			}
		}

		private QuantityWithUnitTextBox _cbEndCapRelSize;

		public QuantityWithUnitTextBox CbEndCapRelSize
		{
			get { return _cbEndCapRelSize; }
			set
			{
				if (_cbEndCapRelSize != null)
					_cbEndCapRelSize.SelectedQuantityChanged -= EhEndCapRelSize_SelectionChangeCommitted;

				_cbEndCapRelSize = value;
				if (_pen != null && _cbEndCapRelSize != null)
					_cbEndCapRelSize.SelectedQuantityAsValueInSIUnits = _pen.EndCap.MinimumRelativeSize;

				if (_cbEndCapRelSize != null)
					_cbEndCapRelSize.SelectedQuantityChanged += EhEndCapRelSize_SelectionChangeCommitted;
			}
		}

		private void EhEndCapRelSize_SelectionChangeCommitted(object sender, DependencyPropertyChangedEventArgs e)
		{
			_userChangedRelEndCapSize = true;

			if (_pen != null)
			{
				var cap = _pen.EndCap;
				cap = cap.Clone(cap.MinimumAbsoluteSizePt, _cbEndCapRelSize.SelectedQuantityAsValueInSIUnits);

				using (var suspendToken = _pen.SuspendGetToken())
				{
					_pen.EndCap = cap;
					suspendToken.ResumeSilently();
				};

				OnPenChanged();
			}
		}

		#endregion EndCap

		#region LineJoin

		private LineJoinComboBox _cbLineJoin;

		public LineJoinComboBox CbLineJoin
		{
			get { return _cbLineJoin; }
			set
			{
				var dpd = System.ComponentModel.DependencyPropertyDescriptor.FromProperty(LineJoinComboBox.SelectedLineJoinProperty, typeof(LineJoinComboBox));

				if (_cbLineJoin != null)
					dpd.RemoveValueChanged(_cbLineJoin, EhLineJoin_SelectionChangeCommitted);

				_cbLineJoin = value;
				if (_pen != null && _cbLineJoin != null)
					_cbLineJoin.SelectedLineJoin = _pen.LineJoin;

				if (_cbLineJoin != null)
					dpd.AddValueChanged(_cbLineJoin, EhLineJoin_SelectionChangeCommitted);
			}
		}

		private void EhLineJoin_SelectionChangeCommitted(object sender, EventArgs e)
		{
			if (_pen != null)
			{
				using (var suspendToken = _pen.SuspendGetToken())
				{
					_pen.LineJoin = _cbLineJoin.SelectedLineJoin;
					suspendToken.ResumeSilently();
				};

				OnPenChanged();
			}
		}

		#endregion LineJoin

		#region Miter

		private MiterLimitComboBox _cbMiterLimit;

		public MiterLimitComboBox CbMiterLimit
		{
			get { return _cbMiterLimit; }
			set
			{
				if (_cbMiterLimit != null)
					_cbMiterLimit.SelectedQuantityChanged -= EhMiterLimit_SelectionChangeCommitted;

				_cbMiterLimit = value;
				if (_pen != null && _cbMiterLimit != null)
					_cbMiterLimit.SelectedQuantityAsValueInPoints = _pen.MiterLimit;

				if (_cbLineJoin != null)
					_cbMiterLimit.SelectedQuantityChanged += EhMiterLimit_SelectionChangeCommitted;
			}
		}

		private void EhMiterLimit_SelectionChangeCommitted(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (_pen != null)
			{
				using (var suspendToken = _pen.SuspendGetToken())
				{
					_pen.MiterLimit = (float)_cbMiterLimit.SelectedQuantityAsValueInPoints;
					suspendToken.ResumeSilently();
				};

				OnPenChanged();
			}
		}

		#endregion Miter

		*/

		#region Dialog

		private void EhShowCustomPenDialog(object sender, EventArgs e)
		{
			PenAllPropertiesController ctrler = new PenAllPropertiesController(this.Pen);
			ctrler.ShowPlotColorsOnly = this._showPlotColorsOnly;
			ctrler.ViewObject = new PenAllPropertiesControl();
			if (Current.Gui.ShowDialog(ctrler, "Edit pen properties"))
			{
				this.Pen = (PenX3D)ctrler.ModelObject;
			}
		}

		#endregion Dialog

		#region Preview

		private Image _previewPanel;
		private GdiToWpfBitmap _previewBitmap;

		public Image PreviewPanel
		{
			get
			{
				return _previewPanel;
			}
			set
			{
				if (null != _previewPanel)
				{
					_previewPanel.SizeChanged -= EhPreviewPanel_SizeChanged;
				}

				_previewPanel = value;

				if (null != _previewPanel)
				{
					_previewPanel.SizeChanged += EhPreviewPanel_SizeChanged;
					UpdatePreviewPanel();
				}
			}
		}

		private void EhPreviewPanel_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdatePreviewPanel();
		}

		private void UpdatePreviewPanel()
		{
		}

		#endregion Preview
	}
}