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

using Altaxo.Collections;
using Altaxo.Graph.Gdi.Shapes;
using Altaxo.Graph.Gdi;
using Altaxo.Graph;
using Altaxo.Graph.Scales.Ticks;

namespace Altaxo.Gui.Graph.Shapes
{
	using Altaxo.Gui.Common;

	public interface IFloatingScaleView
	{
		PointD2D DocPosition { get; set; }
		int ScaleNumber { get; set; }
		FloatingScale.ScaleSegmentType ScaleSegmentType { get; set; }
		void InitializeTickSpacingTypes(SelectableListNodeList names);
		double ScaleSpanValue { get; set; }
		FloatingScaleSpanType ScaleSpanType { get; set; }
		object TickSpacingView { set; }
		object TitleFormatView { set; }
		IConditionalDocumentView MajorLabelView { set; }
		IConditionalDocumentView MinorLabelView { set; }

		Altaxo.Graph.Margin2D BackgroundPadding { get; set; }
		Altaxo.Graph.Gdi.Background.IBackgroundStyle SelectedBackground { get; set; }

		event Action TickSpacingTypeChanged;

	}

	[UserControllerForObject(typeof(FloatingScale), 110)]
	[ExpectedTypeOfView(typeof(IFloatingScaleView))]
	public class FloatingScaleController : MVCANControllerBase<FloatingScale, IFloatingScaleView>
	{
		AxisStyleControllerGlue _axisStyleControllerGlue;
		protected TickSpacing _tempTickSpacing;
		protected SelectableListNodeList _tickSpacingTypes;
		protected IMVCAController _tickSpacingController;


		protected override void Initialize(bool initData)
		{
			if (initData)
			{
				_tempTickSpacing = (TickSpacing)_doc.TickSpacing.Clone();

				// Tick spacing types
				_tickSpacingTypes = new SelectableListNodeList();
				Type[] classes = Altaxo.Main.Services.ReflectionService.GetNonAbstractSubclassesOf(typeof(TickSpacing));
				for (int i = 0; i < classes.Length; i++)
				{
					SelectableListNode node = new SelectableListNode(Current.Gui.GetUserFriendlyClassName(classes[i]), classes[i], _tempTickSpacing.GetType() == classes[i]);
					_tickSpacingTypes.Add(node);
				}

				_axisStyleControllerGlue = new AxisStyleControllerGlue(_doc.AxisStyle);

			}
			if (null != _view)
			{
				_view.DocPosition = _doc.Position;
				_view.ScaleNumber = _doc.ScaleNumber;
				_view.ScaleSpanType = _doc.ScaleSpanType;
				_view.ScaleSpanValue = _doc.ScaleSpanValue;

				_view.ScaleSegmentType = _doc.ScaleType;
				_view.InitializeTickSpacingTypes(_tickSpacingTypes);

				_view.TitleFormatView = _axisStyleControllerGlue.AxisStyleView;
				_view.MajorLabelView = _axisStyleControllerGlue.MajorLabelCondView;
				_view.MinorLabelView = _axisStyleControllerGlue.MinorLabelCondView;

				_view.BackgroundPadding = _doc.BackgroundPadding;
				_view.SelectedBackground = _doc.Background;

			}

			InitTickSpacingController(initData);

		}

		public override bool Apply()
		{
			_doc.Position = _view.DocPosition;
			_doc.ScaleNumber = _view.ScaleNumber;
			_doc.ScaleSpanType = _view.ScaleSpanType;
			_doc.ScaleSpanValue = _view.ScaleSpanValue;

			// Scale/ticks
			_doc.ScaleType = _view.ScaleSegmentType;
			if (null != _tickSpacingController && false == _tickSpacingController.Apply())
				return false;
			_doc.TickSpacing = _tempTickSpacing;

			// Title/format
			if (false == _axisStyleControllerGlue.AxisStyleController.Apply())
				return false;

			if (null != _axisStyleControllerGlue.MajorLabelCondController && false == _axisStyleControllerGlue.MajorLabelCondController.Apply())
				return false;

			if (null != _axisStyleControllerGlue.MinorLabelCondController && false == _axisStyleControllerGlue.MinorLabelCondController.Apply())
				return false;

			_doc.BackgroundPadding = _view.BackgroundPadding;
			_doc.Background = _view.SelectedBackground;

			if (!object.ReferenceEquals(_doc, _originalDoc))
				_originalDoc.CopyFrom(_doc);

			return true;
		}


		protected override void AttachView()
		{
			_view.TickSpacingTypeChanged += new Action(EhTickSpacingTypeChanged);
		}

		protected override void DetachView()
		{
			_view.TickSpacingTypeChanged -= new Action(EhTickSpacingTypeChanged);
		}

		public void InitTickSpacingController(bool bInit)
		{
			if (bInit)
			{
				if (_tempTickSpacing != null)
					_tickSpacingController = (IMVCAController)Current.Gui.GetControllerAndControl(new object[] { _tempTickSpacing }, typeof(IMVCAController), UseDocument.Directly);
				else
					_tickSpacingController = null;
			}
			if (null != _view)
			{
				_view.TickSpacingView = null != _tickSpacingController ? _tickSpacingController.ViewObject : null;
			}
		}




		void EhTickSpacingTypeChanged()
		{
			var selNode = _tickSpacingTypes.FirstSelectedNode; // FirstSelectedNode can be null when the content of the box changes
			if (null == selNode)
				return;

			Type spaceType = (Type)_tickSpacingTypes.FirstSelectedNode.Tag;

			if (spaceType == _tempTickSpacing.GetType())
				return;

			_tempTickSpacing = (TickSpacing)Activator.CreateInstance(spaceType);
			InitTickSpacingController(true);
		}

	}
}