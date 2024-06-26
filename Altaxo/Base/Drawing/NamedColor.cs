﻿#region Copyright

/////////////////////////////////////////////////////////////////////////////
//    Altaxo:  a data processing and data plotting program
//    Copyright (C) 2002-2012 Dr. Dirk Lellinger
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
using Altaxo.Drawing.ColorManagement;

namespace Altaxo.Drawing
{
  /// <summary>
  ///
  /// </summary>
  [Serializable]
  [System.ComponentModel.ImmutableObject(true)]
  [System.ComponentModel.TypeConverterAttribute(typeof(NamedColorTypeConverter))]
  public struct NamedColor : IEquatable<NamedColor>, IEquatable<AxoColor>, Altaxo.Main.IImmutable
  {
    private AxoColor _color;
    private string? _name;
    private ColorManagement.IColorSet? _parent;

    [NonSerialized]
    private string? _autogeneratedName;

    #region Serialization

    /// <summary>
    /// 2015-11-25 Version 1 NamedColor moved from Altaxo.Graph workspace into Altaxo.Drawing namespace
    /// </summary>
    [Altaxo.Serialization.Xml.XmlSerializationSurrogateFor("AltaxoBase", "Altaxo.Graph.NamedColor", 0)]
    [Altaxo.Serialization.Xml.XmlSerializationSurrogateFor("AltaxoBase", "Altaxo.Drawing.NamedColor", 1)]
    private class XmlSerializationSurrogate0 : Altaxo.Serialization.Xml.IXmlSerializationSurrogate
    {
      public void Serialize(object obj, Altaxo.Serialization.Xml.IXmlSerializationInfo info)
      {
        var s = (NamedColor)obj;
        info.AddValue("Color", s.Color.ToInvariantString());
        info.AddValue("Name", s.Name);
      }

      public object Deserialize(object? o, Altaxo.Serialization.Xml.IXmlDeserializationInfo info, object? parent)
      {
        var color = AxoColor.FromInvariantString(info.GetString("Color"));
        var name = info.GetString("Name");

        return new NamedColor(color, name);
      }
    }

    /// <summary>
    /// 2012-09-10 Extension of NamedColor by _parent member requires storage of color set name and level where this color belongs to.
    /// 2015-11-14 Move to Altaxo.Drawing namespace.
    /// </summary>
    [Altaxo.Serialization.Xml.XmlSerializationSurrogateFor("AltaxoBase", "Altaxo.Graph.NamedColor", 1)]
    [Altaxo.Serialization.Xml.XmlSerializationSurrogateFor("AltaxoBase", "Altaxo.Drawing.NamedColor", 2)]
    private class XmlSerializationSurrogate1 : Altaxo.Serialization.Xml.IXmlSerializationSurrogate
    {
      public void Serialize(object obj, Altaxo.Serialization.Xml.IXmlSerializationInfo info)
      {
        throw new InvalidOperationException("Serialization of an old version is not permitted");
        /*
                NamedColor s = (NamedColor)obj;
                info.AddValue("Color", s.Color.ToInvariantString());
                info.AddValue("Name", s.Name);

                if (null != s._parent)
                {
                    if (s._parent.Level == ColorManagement.ColorSetLevel.Builtin && !object.ReferenceEquals(s._parent, ColorManagement.ColorSetManager.Instance.BuiltinKnownColors))
                    {
                        info.AddValue("Set", s._parent);
                    }
                    else
                    {
                        info.AddValue("SetName", s._parent.Name);
                        info.AddEnum("SetLevel", s._parent.Level);
                    }
                }
                */
      }

      public object Deserialize(object? o, Altaxo.Serialization.Xml.IXmlDeserializationInfo info, object? parent)
      {
        var colorValue = AxoColor.FromInvariantString(info.GetString("Color"));
        var colorName = info.GetString("Name");

        if (info.CurrentElementName == "Set")
        {
          // note: the deserialization of the built-in color set is responsible for creating a temporary project level color set,
          // if it is an older version of a color set
          var colorSet = (ColorManagement.IColorSet)info.GetValue("Set", null);
          ColorManagement.ColorSetManager.Instance.TryRegisterList(info, colorSet, Main.ItemDefinitionLevel.Project, out colorSet);
          return ColorManagement.ColorSetManager.Instance.GetDeserializedColorFromBuiltinSet(colorValue, colorName, colorSet);
        }
        else if (info.CurrentElementName == "SetName")
        {
          string colorSetName = info.GetString("SetName");
          var colorSetLevel = (Altaxo.Main.ItemDefinitionLevel)info.GetEnum("SetLevel", typeof(Altaxo.Main.ItemDefinitionLevel));
          return ColorManagement.ColorSetManager.Instance.GetDeserializedColorFromLevelAndSetName(info, colorValue, colorName, colorSetName);
        }
        else // nothing of both, thus color belongs to nothing or to the standard color set
        {
          return ColorManagement.ColorSetManager.Instance.GetDeserializedColorWithNoSet(colorValue, colorName);
        }
      }
    }

    [Altaxo.Serialization.Xml.XmlSerializationSurrogateFor(typeof(NamedColor), 3)]
    private class XmlSerializationSurrogate3 : Altaxo.Serialization.Xml.IXmlSerializationSurrogate
    {
      public void Serialize(object obj, Altaxo.Serialization.Xml.IXmlSerializationInfo info)
      {
        var s = (NamedColor)obj;
        info.AddValue("Color", s.Color.ToInvariantString());
        info.AddValue("Name", s._name); // use _name instead of Name, to make sure _name is null if name was autogenerated

        if (s._parent is not null)
        {
          var colorSetName = s._parent.Name;
          if (!object.ReferenceEquals(NamedColors.Instance, s._parent) && info.GetProperty(ColorSet.GetSerializationRegistrationKey(s._parent)) is null)
            info.AddValue("Set", s._parent);
          else
            info.AddValue("SetName", s._parent.Name);
        }
      }

      public object Deserialize(object? o, Altaxo.Serialization.Xml.IXmlDeserializationInfo info, object? parent)
      {
        var colorValue = AxoColor.FromInvariantString(info.GetString("Color"));
        var colorName = info.GetString("Name"); // remember that colorName can be null or empty here. In this case, we use an autogenerated name

        if (info.CurrentElementName == "Set")
        {
          var colorSet = (Drawing.ColorManagement.IColorSet)info.GetValue("ColorSet", parent);
          ColorSetManager.Instance.TryRegisterList(info, colorSet, Main.ItemDefinitionLevel.Project, out var registeredColorSet);
          return ColorManagement.ColorSetManager.Instance.GetDeserializedColorFromLevelAndSetName(info, colorValue, colorName, colorSet.Name); // Note: here we use the name of the original color set, not of the registered color set. Because the original name is translated during registering into the registered name
        }
        else if (info.CurrentElementName == "SetName")
        {
          string colorSetName = info.GetString("SetName");
          return ColorManagement.ColorSetManager.Instance.GetDeserializedColorFromLevelAndSetName(info, colorValue, colorName, colorSetName);
        }
        else // nothing of both, thus color belongs to nothing or to the standard color set
        {
          return ColorManagement.ColorSetManager.Instance.GetDeserializedColorWithNoSet(colorValue, colorName);
        }
      }
    }

    #endregion Serialization

    public NamedColor(AxoColor c, string name)
    {
      if (string.IsNullOrEmpty(name))
        throw new ArgumentNullException(nameof(name), "Name must not be null or empty. Otherwise use the constructor without name argument");

      _color = c;
      _name = name;
      _autogeneratedName = null;
      _parent = null;
    }

    public NamedColor(AxoColor c)
    {
      _color = c;
      _name = null;
      _autogeneratedName = null;
      _parent = null;
    }

    public NamedColor(AxoColor c, string name, ColorManagement.IColorSet? parent)
    {
      if (string.IsNullOrEmpty(name))
        throw new ArgumentNullException(nameof(name), "Name must not be null or empty. Otherwise use the constructor without name argument");

      _color = c;
      _name = name;
      _autogeneratedName = null;
      _parent = parent;
    }

    public NamedColor(NamedColor c, ColorManagement.IColorSet parent)
    {
      _color = c.Color;
      _name = c._name;
      _autogeneratedName = c._autogeneratedName;
      _parent = parent;
    }

    public static NamedColor FromArgb(byte a, byte r, byte g, byte b)
    {
      var c = AxoColor.FromArgb(a, r, g, b);
      return new NamedColor(c);
    }

    public static NamedColor FromArgb(byte a, byte r, byte g, byte b, string name)
    {
      return new NamedColor(AxoColor.FromArgb(a, r, g, b), name);
    }

    public static NamedColor FromScRgb(float a, float r, float g, float b)
    {
      var c = AxoColor.FromScRgb(a, r, g, b);
      return new NamedColor(c);
    }

    public static NamedColor FromScRgb(float a, float r, float g, float b, string name)
    {
      return new NamedColor(AxoColor.FromScRgb(a, r, g, b), name);
    }

    public NamedColor NewWithOpacityInPercent(int opacityInPercent)
    {
      if (opacityInPercent < 0 || opacityInPercent > 100)
        throw new ArgumentOutOfRangeException("opacityInPercent should be a value between 0 and 100");

      return NewWithAlphaValue((opacityInPercent * 255) / 100);
    }

    public NamedColor NewWithAlphaValue(int alphaValue)
    {
      if (alphaValue < 0 || alphaValue > 255)
        throw new ArgumentOutOfRangeException("alphaValue should be a value between 0 and 255");

      string name = Name;
      if (Color.A != 255)
      {
        // Try to retrieve the original name
        int percIdx = name.LastIndexOf('%');
        int spaceIdx = name.LastIndexOf(' ');
        if (percIdx == name.Length - 1 && spaceIdx > 0 && double.TryParse(name.Substring(spaceIdx + 1, percIdx - spaceIdx - 1), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var dummyNumber))
          name = name.Substring(0, spaceIdx);
      }

      if (255 == alphaValue)
        return new NamedColor(Color.ToAlphaValue(255), name);
      else
        return new NamedColor(Color.ToAlphaValue((byte)alphaValue), string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} {1:F1}%", name, Math.Round(alphaValue / 2.55, 0)));
    }

    public AxoColor Color { get { return _color; } }

    public string Name
    {
      get
      {
        return _name ?? (_autogeneratedName ??= GetColorName(Color));
      }
    }

    public ColorManagement.IColorSet? ParentColorSet { get { return _parent; } }

    /// <summary>
    /// Tests if this color is still a member of the parent color set, and sets the parent property to <c>null</c> if it is no longer a member of the parent color set.
    /// </summary>
    /// <returns>If the parent color set is null, or the color is still a member of the parent color set, the color is returned unchanged.
    /// Otherwise, a <see cref="NamedColor"/> with the same color and name but with <see cref="ParentColorSet"/> set to <c>null</c> will be returned.
    /// </returns>
    public NamedColor CoerceParentColorSetToNullIfNotMember()
    {
      if (_parent is null)
        return this;

      if (_name is not null && _parent.TryGetValue(_color, _name, out var result))
        return result;
      if (_parent.TryGetValue(_color, out result))
        return result;
      return string.IsNullOrEmpty(_name) ? new NamedColor(_color) : new NamedColor(_color, _name);
    }

    public static string GetColorName(AxoColor c)
    {
      if (NamedColors.Instance.TryGetValue(c, out var nc))
        return nc.Name;
      if (NamedColors.Instance.TryGetValue(c.ToFullyOpaque(), out nc))
      {
        var name = nc.Name;
        int opaqness = (c.A * 100) / 255;
        if (opaqness != 100)
          name += string.Format(System.Globalization.CultureInfo.InvariantCulture, " {0}%", opaqness);
        return name;
      }

      return c.ToString();
    }

    public override int GetHashCode()
    {
      return _color.GetHashCode() + (_name is not null ? _name.GetHashCode() : 0) + (_parent is not null ? _parent.GetHashCode() : 0);
    }

    public bool Equals(NamedColor from)
    {
      return _color.Equals(from._color) && 0 == string.Compare(_name, from._name) && object.ReferenceEquals(_parent, from._parent);
    }

    public bool EqualsInNameAndColor(NamedColor from)
    {
      return _color.Equals(from._color) && 0 == string.Compare(_name, from._name);
    }

    public bool Equals(AxoColor from)
    {
      return _color.Equals(from);
    }

    public override bool Equals(object? obj)
    {
      if (obj is NamedColor other)
      {
        return Equals(other);
      }
      else if (obj is AxoColor axoColor)
      {
        return Equals(axoColor);
      }
      else
      {
        return false;
      }
    }

    public static bool operator ==(NamedColor x, NamedColor y)
    {
      return x.Equals(y);
    }

    public static bool operator !=(NamedColor x, NamedColor y)
    {
      return !x.Equals(y);
    }

    #region Name

    private static string GetLevelString(Main.ItemDefinitionLevel level)
    {
      switch (level)
      {
        case Main.ItemDefinitionLevel.Builtin:
          return "Builtin";

        case Main.ItemDefinitionLevel.Application:
          return "App";

        case Main.ItemDefinitionLevel.UserDefined:
          return "User";

        case Main.ItemDefinitionLevel.Project:
          return "Project";

        default:
          throw new NotImplementedException();
      }
    }

    public override string ToString()
    {
      if (ParentColorSet is not null && ColorManagement.ColorSetManager.Instance.TryGetList(ParentColorSet.Name, out var value))
      {
        return string.Format("{0} ({1}/{2})", Name, GetLevelString(value.Level), ParentColorSet.Name);
      }
      else
        return string.Format("{0} ({1})", Name, "<<no color set>>");
    }

    #endregion Name

    #region Conversion

    public static implicit operator AxoColor(NamedColor c)
    {
      return c.Color;
    }

    public static implicit operator System.Drawing.Color(NamedColor c)
    {
      return c.Color;
    }

    #endregion Conversion
  }
}
