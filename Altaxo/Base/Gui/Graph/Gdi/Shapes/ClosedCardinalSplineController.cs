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

#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Altaxo.Collections;
using Altaxo.Geometry;
using Altaxo.Graph;
using Altaxo.Graph.Gdi.Shapes;
using Altaxo.Gui.Common;

namespace Altaxo.Gui.Graph.Gdi.Shapes
{
  [UserControllerForObject(typeof(ClosedCardinalSpline), 110)]
  [ExpectedTypeOfView(typeof(ITabbedElementViewDC))]
  public class ClosedCardinalSplineController : MVCANControllerEditOriginalDocBase<ClosedCardinalSpline, ITabbedElementViewDC>
  {
    private ClosedPathShapeController _shapeCtrl;
    private CardinalSplinePointsController _splinePointsCtrl;

    public override IEnumerable<ControllerAndSetNullMethod> GetSubControllers()
    {
      yield return new ControllerAndSetNullMethod(_shapeCtrl, () => _shapeCtrl = null);
      yield return new ControllerAndSetNullMethod(_splinePointsCtrl, () => _splinePointsCtrl = null);
    }

    #region Bindings

    public SelectableListNodeList Tabs { get; } = new();

    private int? _selectedTab;

    /// <summary>
    /// Gets or sets the selected tab. The value of -1 selectes the data tab, values &gt;= 0 select one of the style tabs.
    /// </summary>
    /// <value>
    /// The selected tab.
    /// </value>
    public int? SelectedTab
    {
      get => _selectedTab;
      set
      {
        if (!(_selectedTab == value))
        {
          var oldValue = _selectedTab;
          _selectedTab = value;
          OnPropertyChanged(nameof(SelectedTab));
        }
      }
    }

    #endregion

    protected override void Initialize(bool initData)
    {
      base.Initialize(initData);

      if (initData)
      {
        _shapeCtrl = new ClosedPathShapeController() { UseDocumentCopy = UseDocument.Directly };
        _shapeCtrl.InitializeDocument(_doc);
        Current.Gui.FindAndAttachControlTo(_shapeCtrl);

        _splinePointsCtrl = new CardinalSplinePointsController(_doc.CurvePoints, _doc.Tension, _doc);
        Current.Gui.FindAndAttachControlTo(_splinePointsCtrl);

        Tabs.Add(new SelectableListNodeWithController("Appearance/Position", 0, true) { Controller = _shapeCtrl });
        Tabs.Add(new SelectableListNodeWithController("Curve", 1, false) { Controller = _splinePointsCtrl });
        SelectedTab = 0;
      }
    }

    public override bool Apply(bool disposeController)
    {
      if (!_shapeCtrl.Apply(disposeController))
        return false;

      if (_splinePointsCtrl.Apply(disposeController))
      {
        var (list, tension)  = ((List<PointD2D>, double))_splinePointsCtrl.ModelObject;

        if (!(list.Count >= 3))
        {
          Current.Gui.ErrorMessageBox("At least three points are required for the closed cardinal spline. Please enter more points!");
          return false;
        }

        _doc.CurvePoints = list;
        _doc.Tension = tension;
      }
      else
      {
        return false;
      }

      return ApplyEnd(true, disposeController);
    }
  }
}
