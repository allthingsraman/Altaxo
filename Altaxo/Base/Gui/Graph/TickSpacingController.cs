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

using Altaxo.Collections;
using Altaxo.Graph.Gdi;
using Altaxo.Graph.Scales.Ticks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Altaxo.Gui.Graph
{
	public interface ITickSpacingView
	{
		void InitializeTickSpacingType(SelectableListNodeList names);

		void SetTickSpacingView(object guiobject);

		event Action TickSpacingTypeChanged;
	}

	[ExpectedTypeOfView(typeof(ITickSpacingView))]
	[UserControllerForObject(typeof(TickSpacing))]
	public class TickSpacingController : MVCANControllerBase<TickSpacing, ITickSpacingView>
	{
		protected SelectableListNodeList _tickSpacingTypes;
		protected IMVCAController _tickSpacingController;

		protected override void Initialize(bool initData)
		{
			InitTickSpacingTypes(initData);
			InitTickSpacingController(initData);
		}

		public void InitTickSpacingTypes(bool bInit)
		{
			if (bInit)
			{
				_tickSpacingTypes = new SelectableListNodeList();
				Type[] classes = Altaxo.Main.Services.ReflectionService.GetNonAbstractSubclassesOf(typeof(TickSpacing));
				for (int i = 0; i < classes.Length; i++)
				{
					SelectableListNode node = new SelectableListNode(Current.Gui.GetUserFriendlyClassName(classes[i]), classes[i], _doc.GetType() == classes[i]);
					_tickSpacingTypes.Add(node);
				}
			}

			if (null != _view)
				_view.InitializeTickSpacingType(_tickSpacingTypes);
		}

		public void InitTickSpacingController(bool bInit)
		{
			if (bInit)
			{
				if (_doc != null)
					_tickSpacingController = (IMVCAController)Current.Gui.GetControllerAndControl(new object[] { _doc }, typeof(IMVCAController));
				else
					_tickSpacingController = null;
			}
			if (null != _view)
			{
				_view.SetTickSpacingView(null != _tickSpacingController ? _tickSpacingController.ViewObject : null);
			}
		}

		public void EhView_TickSpacingTypeChanged()
		{
			var selNode = _tickSpacingTypes.FirstSelectedNode; // FirstSelectedNode can be null when the content of the box changes
			if (null == selNode)
				return;

			Type spaceType = (Type)_tickSpacingTypes.FirstSelectedNode.Tag;

			if (spaceType == _doc.GetType())
				return;

			_doc = (TickSpacing)Activator.CreateInstance(spaceType);
			InitTickSpacingController(true);
		}

		protected override void AttachView()
		{
			base.AttachView();

			_view.TickSpacingTypeChanged += this.EhView_TickSpacingTypeChanged;
		}

		protected override void DetachView()
		{
			_view.TickSpacingTypeChanged -= this.EhView_TickSpacingTypeChanged;

			base.DetachView();
		}

		public override bool Apply()
		{
			if (null != _tickSpacingController && false == _tickSpacingController.Apply())
				return false;

			if(!object.ReferenceEquals(_doc,_originalDoc))
				CopyHelper.Copy(ref _originalDoc, _doc);

			return true;
		}
	}
}