﻿#region Copyright

/////////////////////////////////////////////////////////////////////////////
//    Altaxo:  a data processing and data plotting program
//    Copyright (C) 2014 Dr. Dirk Lellinger
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
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Altaxo.Collections;

namespace Altaxo.Gui.DataConnection
{
  /// <summary>
  /// Interaction logic for QueryPropertiesControl.xaml
  /// </summary>
  public partial class QueryPropertiesControl : UserControl, IQueryPropertiesView
  {
    public QueryPropertiesControl()
    {
      InitializeComponent();
    }

    // copy QueryBuilder values to form
    public void UpdateDialogValues(bool isDistinct, int topN, SelectableListNodeList groupBy)
    {
      _chkDistinct.IsChecked = isDistinct;
      _numTopN.Value = topN;

      if (groupBy.FirstSelectedNode is not null)
      {
        _cmbGroupBy.IsEnabled = true;
        GuiHelper.Initialize(_cmbGroupBy, groupBy);
      }
      else
      {
        _cmbGroupBy.IsEnabled = false;
        _cmbGroupBy.ItemsSource = null;
      }
    }

    // copy form values to QueryBuilder
    public int GetTopN()
    {
      return _numTopN.Value;
    }

    public bool GetDistinct()
    {
      return true == _chkDistinct.IsChecked;
    }
  }
}
