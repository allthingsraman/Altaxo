﻿#region Copyright

/////////////////////////////////////////////////////////////////////////////
//    Altaxo:  a data processing and data plotting program
//    Copyright (C) 2002-2019 Dr. Dirk Lellinger
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altaxo.Gui.Data
{
  /// <summary>
  /// Interface to a view commonly used to show data source options.
  /// </summary>
  public interface IImportDataSourceView
  {
    /// <summary>
    /// Sets the control to show common import options (like when to update after change of the source, if the table script should be executed, etc.).
    /// </summary>
    /// <param name="header">The header to show above the control.</param>
    /// <param name="p">The Gui control to show.</param>
    void SetCommonImportOptionsControl(string header, object p);

    /// <summary>
    /// Sets the control to show import options specific to that kind of import (like for ASCII import the number of header lines, structure of the columns, etc.).
    /// </summary>
    /// <param name="header">The header to show above the control.</param>
    /// <param name="p">The Gui control to show.</param>
    void SetSpecificImportOptionsControl(string header, object p);

    /// <summary>
    /// Sets the control to show the source of data (like the file name for Ascii import, etc.).
    /// </summary>
    /// <param name="header">The header to show above the control.</param>
    /// <param name="p">The Gui control to show.</param>
    void SetSpecificImportSourceControl(string header, object p);
  }
}
