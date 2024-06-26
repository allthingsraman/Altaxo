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

#nullable enable
using System;
using System.Collections.Generic;
using System.Text;

namespace Altaxo.Graph.Plot.Groups
{
  /// <summary>
  /// This group style is intended to publish the symbol size to all interested
  /// plot styles.
  /// </summary>
  public class SymbolSizeGroupStyle
    :
    Main.SuspendableDocumentLeafNodeWithEventArgs,
    IPlotGroupStyle
  {
    private bool _isInitialized;
    private double _symbolSize;
    private static readonly Type MyType = typeof(SymbolSizeGroupStyle);

    #region Serialization

    [Altaxo.Serialization.Xml.XmlSerializationSurrogateFor(typeof(SymbolSizeGroupStyle), 0)]
    private class XmlSerializationSurrogate0 : Altaxo.Serialization.Xml.IXmlSerializationSurrogate
    {
      public void Serialize(object obj, Altaxo.Serialization.Xml.IXmlSerializationInfo info)
      {
        var s = (SymbolSizeGroupStyle)obj;
      }

      public object Deserialize(object? o, Altaxo.Serialization.Xml.IXmlDeserializationInfo info, object? parent)
      {
        var s = (SymbolSizeGroupStyle?)o ?? new SymbolSizeGroupStyle();
        return s;
      }
    }

    #endregion Serialization

    #region Constructors

    public SymbolSizeGroupStyle()
    {
    }

    public SymbolSizeGroupStyle(SymbolSizeGroupStyle from)
    {
      _isInitialized = from._isInitialized;
      _symbolSize = from._symbolSize;
    }

    #endregion Constructors

    #region ICloneable Members

    public SymbolSizeGroupStyle Clone()
    {
      return new SymbolSizeGroupStyle(this);
    }

    object ICloneable.Clone()
    {
      return new SymbolSizeGroupStyle(this);
    }

    #endregion ICloneable Members

    #region IGroupStyle Members

    public void TransferFrom(IPlotGroupStyle fromb)
    {
      var from = (SymbolSizeGroupStyle)fromb;
      _isInitialized = from._isInitialized;
      _symbolSize = from._symbolSize;
    }

    public void BeginPrepare()
    {
      _isInitialized = false;
    }

    public void PrepareStep()
    {
    }

    public void EndPrepare()
    {
    }

    public bool CanCarryOver
    {
      get
      {
        return false;
      }
    }

    public bool CanStep
    {
      get
      {
        return false;
      }
    }

    public int Step(int step)
    {
      return 0;
    }

    /// <summary>
    /// Get/sets whether or not stepping is allowed.
    /// </summary>
    public bool IsStepEnabled
    {
      get
      {
        return false;
      }
      set
      {
      }
    }

    #endregion IGroupStyle Members

    #region Other members

    public bool IsInitialized
    {
      get
      {
        return _isInitialized;
      }
    }

    public void Initialize(double symbolSize)
    {
      _isInitialized = true;
      _symbolSize = symbolSize;
    }

    public double SymbolSize
    {
      get
      {
        return _symbolSize;
      }
    }

    #endregion Other members

    #region Static helpers

    public static void AddExternalGroupStyle(IPlotGroupStyleCollection externalGroups)
    {
      if (PlotGroupStyle.ShouldAddExternalGroupStyle(externalGroups, typeof(SymbolSizeGroupStyle)))
      {
        var gstyle = new SymbolSizeGroupStyle
        {
          IsStepEnabled = true
        };
        externalGroups.Add(gstyle);
      }
    }

    public static void AddLocalGroupStyle(
     IPlotGroupStyleCollection externalGroups,
     IPlotGroupStyleCollection localGroups)
    {
      if (PlotGroupStyle.ShouldAddLocalGroupStyle(externalGroups, localGroups, typeof(SymbolSizeGroupStyle)))
        localGroups.Add(new SymbolSizeGroupStyle());
    }

    public delegate double SymbolSizeGetter();

    public static void PrepareStyle(
      IPlotGroupStyleCollection externalGroups,
      IPlotGroupStyleCollection localGroups,
      SymbolSizeGetter getter)
    {
      if (!externalGroups.ContainsType(typeof(SymbolSizeGroupStyle))
        && localGroups is not null
        && !localGroups.ContainsType(typeof(SymbolSizeGroupStyle)))
      {
        localGroups.Add(new SymbolSizeGroupStyle());
      }

      SymbolSizeGroupStyle? grpStyle = null;
      if (externalGroups.ContainsType(typeof(SymbolSizeGroupStyle)))
        grpStyle = (SymbolSizeGroupStyle)externalGroups.GetPlotGroupStyle(typeof(SymbolSizeGroupStyle));
      else if (localGroups is not null)
        grpStyle = (SymbolSizeGroupStyle)localGroups.GetPlotGroupStyle(typeof(SymbolSizeGroupStyle));

      if (grpStyle is not null && getter is not null && !grpStyle.IsInitialized)
        grpStyle.Initialize(getter());
    }

    public delegate void SymbolSizeSetter(double c);

    /// <summary>
    /// Try to apply the symbol size group style. Returns true if successfull applied.
    /// </summary>
    /// <param name="externalGroups"></param>
    /// <param name="localGroups"></param>
    /// <param name="setter"></param>
    /// <returns></returns>
    public static bool ApplyStyle(
      IPlotGroupStyleCollection externalGroups,
      IPlotGroupStyleCollection localGroups,
      SymbolSizeSetter setter)
    {
      IPlotGroupStyleCollection? grpColl = null;
      if (externalGroups.ContainsType(typeof(SymbolSizeGroupStyle)))
        grpColl = externalGroups;
      else if (localGroups is not null && localGroups.ContainsType(typeof(SymbolSizeGroupStyle)))
        grpColl = localGroups;

      if (grpColl is not null)
      {
        var grpStyle = (SymbolSizeGroupStyle)grpColl.GetPlotGroupStyle(typeof(SymbolSizeGroupStyle));
        grpColl.OnBeforeApplication(typeof(SymbolSizeGroupStyle));
        setter(grpStyle.SymbolSize);
        return true;
      }
      else
      {
        return false;
      }
    }

    #endregion Static helpers
  }
}
