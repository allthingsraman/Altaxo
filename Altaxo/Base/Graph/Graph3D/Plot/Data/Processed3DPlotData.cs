﻿#region Copyright

/////////////////////////////////////////////////////////////////////////////
//    Altaxo:  a data processing and data plotting program
//    Copyright (C) 2002-2016 Dr. Dirk Lellinger
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

#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using Altaxo.Data;
using Altaxo.Geometry;

namespace Altaxo.Graph.Graph3D.Plot.Data
{
  using Graph.Plot.Data;

  /// <summary>
  /// Allows access not only to the original physical plot data,
  /// but also to the plot ranges and to the plot points in absolute layer coordiates.
  /// </summary>
  public class Processed3DPlotData : I3DPhysicalVariantAccessor
  {
    /// <summary>List of plot ranges of the plot points. This is used to identify contiguous ranges of plot points, so that for instance it can be decided to connect them by a line or not.</summary>
    public PlotRangeList? RangeList;

    /// <summary>Holds the final coordinates of the plot points in absolute layer coordinates.</summary>
    public PointD3D[]? PlotPointsInAbsoluteLayerCoordinates;

    private IndexedPhysicalValueAccessor _getXPhysical = new IndexedPhysicalValueAccessor(GetZeroValue);
    private IndexedPhysicalValueAccessor _getYPhysical = new IndexedPhysicalValueAccessor(GetZeroValue);
    private IndexedPhysicalValueAccessor _getZPhysical = new IndexedPhysicalValueAccessor(GetZeroValue);

    /// <summary>Data of the previous plot item for temporary purposes.</summary>
    private Processed3DPlotData? _previousItemData;

    /// <summary>Gets the physical x value at a given original row index.</summary>
    /// <param name="originalRowIndex">Index of the original data row.</param>
    /// <returns>The physical x value at that given original row index.</returns>
    public AltaxoVariant GetXPhysical(int originalRowIndex)
    {
      return _getXPhysical(originalRowIndex);
    }

    /// <summary>Gets the physical y value at a given original row index.</summary>
    /// <param name="originalRowIndex">Index of the original data row.</param>
    /// <returns>The physical y value at that given original row index.</returns>
    public AltaxoVariant GetYPhysical(int originalRowIndex)
    {
      return _getYPhysical(originalRowIndex);
    }

    /// <summary>Gets the physical z value at a given original row index.</summary>
    /// <param name="originalRowIndex">Index of the original data row.</param>
    /// <returns>The physical z value at that given original row index.</returns>
    public virtual AltaxoVariant GetZPhysical(int originalRowIndex)
    {
      return _getZPhysical(originalRowIndex);
    }

    /// <summary>Gets the physical value of a given axis at a given original row index.</summary>
    /// <param name="axisNumber">0, 1, or 2 according to X, Y, or Z axis.</param>
    /// <param name="originalRowIndex">Index of the original data row.</param>
    /// <returns>The physical value of the provided axis at that given original row index.</returns>
    public virtual AltaxoVariant GetPhysical(int axisNumber, int originalRowIndex)
    {
      switch (axisNumber)
      {
        case 0:
          return _getXPhysical(originalRowIndex);

        case 1:
          return _getYPhysical(originalRowIndex);

        case 2:
          return _getZPhysical(originalRowIndex);

        default:
          throw new ArgumentOutOfRangeException(nameof(axisNumber));
      }
    }

    /// <summary>Gets or sets the X physical accessor.</summary>
    /// <value>The X physical accessor. It is awaiting the original row index of the data as argument and will return the physical x data value at that row.</value>
    public IndexedPhysicalValueAccessor XPhysicalAccessor
    {
      get
      {
        return _getXPhysical;
      }
      set
      {
        if (value is not null)
          _getXPhysical = value;
        else
          _getXPhysical = new IndexedPhysicalValueAccessor(GetZeroValue);
      }
    }

    /// <summary>Gets or sets the Y physical accessor.</summary>
    /// <value>The Y physical accessor. It is awaiting the original row index of the data as argument and will return the physical y data value at that row.</value>
    public IndexedPhysicalValueAccessor YPhysicalAccessor
    {
      get
      {
        return _getYPhysical;
      }
      set
      {
        if (value is not null)
          _getYPhysical = value;
        else
          _getYPhysical = new IndexedPhysicalValueAccessor(GetZeroValue);
      }
    }

    /// <summary>Gets or sets the Z physical accessor.</summary>
    /// <value>The Z physical accessor. It is awaiting the original row index of the data as argument and will return the z data value at that row.</value>
    public IndexedPhysicalValueAccessor ZPhysicalAccessor
    {
      get
      {
        return _getZPhysical;
      }
      set
      {
        if (value is not null)
          _getZPhysical = value;
        else
          _getZPhysical = new IndexedPhysicalValueAccessor(GetZeroValue);
      }
    }

    /// <summary>
    /// Gets/sets the processed plot data of a previous plot item for temporary usage.
    /// </summary>
    public Processed3DPlotData? PreviousItemData
    {
      get
      {
        return _previousItemData;
      }
      set
      {
        _previousItemData = value;
      }
    }

    /// <summary>
    /// Returns always a AltaxoVariant with the content of 0.0 (a double value). This function can
    /// serve as an instance for the <see cref="IndexedPhysicalValueAccessor" /> returning 0.
    /// </summary>
    /// <param name="i">Index. This parameter is ignored here.</param>
    /// <returns>Zero.</returns>
    public static AltaxoVariant GetZeroValue(int i)
    {
      return new AltaxoVariant(0.0);
    }

    /// <summary>
    /// Returns true if the z coordinate is used. Return false if the z coordinate is always 0 (zero), so we can
    /// </summary>
    public virtual bool IsZUsed { get { return true; } }

    /// <summary>
    /// Returns true if the z-value is constant. In this case some optimizations can be made.
    /// </summary>
    public virtual bool IsZConstant { get { return false; } }
  }
}
