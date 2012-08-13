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

using Altaxo.Main.Services.PropertyReflection;

namespace Altaxo.Gui.Main
{
	public interface IInstancePropertyView
	{
		/// <summary>Initializes the items.</summary>
		/// <param name="list">The list. Every item contains text that can be used as label. The item object is a view that displays the value.</param>
		void InitializeItems(Altaxo.Collections.ListNodeList list);
	}

	[ExpectedTypeOfView(typeof(IInstancePropertyView))]
	public class InstancePropertyController : MVCANDControllerBase<object,IInstancePropertyView>
	{
		SortedList<long, KeyValuePair<Property, IMVCAController>> _controllerList = new SortedList<long, KeyValuePair<Property, IMVCAController>>();
		PropertyCollection _propertyCollection;


		/// <summary>Initializes the document. In contrast to the base function here it is allowed to call this function with <c>null</c> as model.</summary>
		/// <param name="args">The arguments.</param>
		/// <returns>True if the initialization was successfull, <c>False</c> otherwise.</returns>
		public override bool InitializeDocument(params object[] args)
		{
			if (null == args || 0 == args.Length)
				return false;

			_doc = _originalDoc = args[0];
			if (_useDocumentCopy && _originalDoc is ICloneable)
				_doc = ((ICloneable)_originalDoc).Clone();

			using (var suppressor = _suppressDirtyEvent.Suspend())
			{
				Initialize(true);
			}
			return true;
		}

		protected override void Initialize(bool initData)
		{
			if (initData)
			{
				_propertyCollection = new PropertyCollection(_doc, true, false, string.Empty);

				ClearControllerList();

				// fill the controller list
				long defaultDisplayOrder = 1L + int.MaxValue;
				foreach (Property prop in _propertyCollection.Items)
				{
					++defaultDisplayOrder;
					long currentDisplayOrder = defaultDisplayOrder;
					if(prop.IsWriteable)
					{
						var displayOrderAttribute = prop.Attributes.OfType<Altaxo.Main.Services.PropertyReflection.DisplayOrderAttribute>().FirstOrDefault();
						if (null != displayOrderAttribute)
							currentDisplayOrder = displayOrderAttribute.OrderIndex;

						var ctrl = GetControllerFor(prop);
						if (null != ctrl && null != ctrl.ViewObject)
						{
							AddToControllerList(currentDisplayOrder, prop, ctrl);
						}
					}
				}
			}

			if (null != _view)
			{
				var list = new Altaxo.Collections.ListNodeList();
				foreach(var entry in _controllerList.Values)
				{
					var prop = entry.Key;
					string label = prop.Description;
							if (string.IsNullOrEmpty(label))
								label = prop.Name;
					list.Add(new Collections.ListNode(label+ ":",entry.Value.ViewObject));
				}
				_view.InitializeItems(list);
			}

		}

		IMVCAController GetControllerFor(Property prop)
		{
			IMVCAController ctrl = null;

			var att = prop.Attributes.OfType<System.ComponentModel.EditorAttribute>().FirstOrDefault();
			if (null != att)
			{
				var ctrlType = Type.GetType(att.EditorTypeName, false);
				if (ctrlType != null)
				{
					try
					{
						var controller = Activator.CreateInstance(ctrlType) as IMVCANController;
						controller.InitializeDocument(prop.Value);
						Current.Gui.FindAndAttachControlTo(controller);
						ctrl = controller;
					}
					catch (Exception)
					{
					}
				}
			}

			if(null==ctrl)
			{
				ctrl = (IMVCAController)Current.Gui.GetControllerAndControl(new object[] { prop.Value }, typeof(IMVCAController), UseDocument.Directly);
			}
		
			return ctrl;
		}

		Property GetPropertyForController(IMVCAController ctrl)
		{
			foreach (var entry in _controllerList.Values)
			{
				if (object.ReferenceEquals(ctrl, entry.Value))
					return entry.Key;
			}
			return null;
		}


		void AttachController(IMVCAController ctrl)
		{
			if (ctrl is IMVCANDController)
			{
				(ctrl as IMVCANDController).MadeDirty += EhMadeDirty;
			}
		}

		void DetachController(IMVCAController ctrl)
		{
			if (ctrl is IMVCANDController)
			{
				(ctrl as IMVCANDController).MadeDirty -= EhMadeDirty;
				if (ctrl is IDisposable)
					((IDisposable)ctrl).Dispose();
			}
		}

		void ClearControllerList()
		{
			foreach (var entry in _controllerList.Values)
			{
				DetachController(entry.Value);
			}
			_controllerList.Clear();
		}

		void AddToControllerList(long currentDisplayOrder, Property prop, IMVCAController ctrl)
		{
			_controllerList.Add(currentDisplayOrder, new KeyValuePair<Property, IMVCAController>(prop, ctrl));
			AttachController(ctrl);
		}


		void EhMadeDirty(IMVCANDController ctrl)
		{
			var prop = GetPropertyForController(ctrl);
			prop.Value = ctrl.ProvisionalModelObject;
			OnMadeDirty();
		}

		public override bool Apply()
		{
			foreach (var entry in _controllerList.Values)
			{
				bool success = entry.Value.Apply();
				if (!success)
					return false;
				entry.Key.Value = entry.Value.ModelObject;
			}

			if (_useDocumentCopy)
			{
				if (_originalDoc is Altaxo.Main.ICopyFrom)
				{
					((Altaxo.Main.ICopyFrom)_originalDoc).CopyFrom(_doc);
				}
				else if (_doc is ICloneable)
				{
					_originalDoc = ((ICloneable)_doc).Clone();
				}
			}
			else
			{
				_originalDoc = _doc; // make sure that originalDoc and doc are the same, even if the type of doc is a value type
			}
			return true;
		}
	}
}