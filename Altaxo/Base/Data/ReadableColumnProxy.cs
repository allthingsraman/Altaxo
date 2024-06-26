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
using Altaxo.Main;

namespace Altaxo.Data
{
  /// <summary>
  /// Proxy that holds instances of type <see cref="IReadableColumnProxy"/>.
  /// </summary>
  public interface IReadableColumnProxy : IDocumentLeafNode, IProxy, ICloneable
  {
    /// <summary>
    /// Returns the holded object. Null can be returned if the object is no longer available (e.g. disposed).
    /// </summary>
    IReadableColumn? Document();

    /// <summary>
    /// Gets the name of the column that is held by this proxy.
    /// </summary>
    /// <param name="level">The name level.</param>
    /// <returns>The name of the column held by this proxy.</returns>
    string GetName(int level);
  }

  /// <summary>
  /// Static class to create instances of <see cref="IReadableColumnProxy"/>.
  /// </summary>
  public static class ReadableColumnProxyBase
  {
    /// <summary>
    /// Creates an <see cref="IReadableColumnProxy"/> from a given column.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <returns>An instance of <see cref="IReadableColumnProxy"/>. The type of instance returned depends on the type of the provided column (e.g. whether the column is part of the document or not).</returns>
    public static IReadableColumnProxy FromColumn(IReadableColumn? column)
    {
      if (column is ITransformedReadableColumn)
      {
        var tcolumn = (ITransformedReadableColumn)column;
        if (tcolumn.UnderlyingReadableColumn is IDocumentLeafNode)
          return TransformedReadableColumnProxy.FromColumn(tcolumn);
        else
          return TransformedReadableColumnProxyForStandaloneColumns.FromColumn(tcolumn);
      }
      else
      {
        if (column is IDocumentLeafNode)
          return ReadableColumnProxy.FromColumn(column);
        else
          return ReadableColumnProxyForStandaloneColumns.FromColumn(column);
      }
    }
  }

  #region ReadableColumnProxy

  /// <summary>
  /// Summary description for DataColumnPlaceHolder.
  /// </summary>
  [Serializable]
  public class ReadableColumnProxy : DocNodeProxy2ndLevel, IReadableColumnProxy
  {
    #region Serialization

    [Altaxo.Serialization.Xml.XmlSerializationSurrogateFor("AltaxoBase", "Altaxo.Data.ReadableColumnProxy", 0)]
    private class XmlSerializationSurrogate0 : Altaxo.Serialization.Xml.IXmlSerializationSurrogate
    {
      public virtual void Serialize(object obj, Altaxo.Serialization.Xml.IXmlSerializationInfo info)
      {
        info.AddBaseValueEmbedded(obj, typeof(DocNodeProxy)); // serialize the base class
      }

      public virtual object Deserialize(object? o, Altaxo.Serialization.Xml.IXmlDeserializationInfo info, object? parent)
      {
        var s = (ReadableColumnProxy?)o ?? new ReadableColumnProxy(info);

#pragma warning disable CS0618 // Type or member is obsolete
        object? baseobj = info.GetBaseValueEmbeddedOrNull(s, "AltaxoBase,Altaxo.Main.DocNodeProxy,0", parent);         // deserialize the base class
#pragma warning restore CS0618 // Type or member is obsolete

        if (!object.ReferenceEquals(s, baseobj))
        {
          return ReadableColumnProxyForStandaloneColumns.FromColumn((IReadableColumn?)baseobj);
        }
        else
        {
          return s;
        }
      }
    }

    [Altaxo.Serialization.Xml.XmlSerializationSurrogateFor("AltaxoBase", "Altaxo.Data.ReadableColumnProxy", 1)]
    private class XmlSerializationSurrogate1 : Altaxo.Serialization.Xml.IXmlSerializationSurrogate
    {
      public virtual void Serialize(object obj, Altaxo.Serialization.Xml.IXmlSerializationInfo info)
      {
        info.AddBaseValueEmbedded(obj, typeof(DocNodeProxy)); // serialize the base class
      }

      public virtual object Deserialize(object? o, Altaxo.Serialization.Xml.IXmlDeserializationInfo info, object? parent)
      {
        var s = (ReadableColumnProxy?)o ?? new ReadableColumnProxy(info);
        info.GetBaseValueEmbedded(s, typeof(DocNodeProxy), parent);         // deserialize the base class

        return s;
      }
    }

    [Altaxo.Serialization.Xml.XmlSerializationSurrogateFor(typeof(ReadableColumnProxy), 2)]
    private class XmlSerializationSurrogate2 : Altaxo.Serialization.Xml.IXmlSerializationSurrogate
    {
      public virtual void Serialize(object obj, Altaxo.Serialization.Xml.IXmlSerializationInfo info)
      {
        info.AddBaseValueEmbedded(obj, typeof(DocNodeProxy2ndLevel)); // serialize the base class
      }

      public virtual object Deserialize(object? o, Altaxo.Serialization.Xml.IXmlDeserializationInfo info, object? parent)
      {
        var s = (ReadableColumnProxy?)o ?? new ReadableColumnProxy(info);
        info.GetBaseValueEmbedded(s, typeof(DocNodeProxy2ndLevel), parent);         // deserialize the base class

        return s;
      }
    }

    #endregion Serialization

    public static ReadableColumnProxy FromColumn(IReadableColumn column)
    {
      if (column is null)
        throw new ArgumentNullException(nameof(column));
      var colAsDocumentNode = column as IDocumentLeafNode;
      if (colAsDocumentNode is null)
        throw new ArgumentException(string.Format("column does not implement {0}. The actual type of column is {1}", typeof(IDocumentLeafNode), column.GetType()));

      return new ReadableColumnProxy(colAsDocumentNode);
    }

    protected ReadableColumnProxy(IDocumentLeafNode column)
      : base(column)
    {
    }

    /// <summary>
    /// For deserialization purposes only.
    /// </summary>
    protected ReadableColumnProxy(Altaxo.Serialization.Xml.IXmlDeserializationInfo info)
      : base(info)
    {
    }

    /// <summary>
    /// Cloning constructor.
    /// </summary>
    /// <param name="from">Object to clone from.</param>
    public ReadableColumnProxy(ReadableColumnProxy from)
      : base(from)
    {
    }

    protected override bool IsValidDocument(object obj)
    {
      return (obj is IReadableColumn) || obj is null;
    }

    public IReadableColumn? Document()
    {
      return (IReadableColumn?)base.DocumentObject();
    }

    public override object Clone()
    {
      return new ReadableColumnProxy(this);
    }

    public string GetName(int level)
    {
      return GetName(level, Document(), InternalDocumentPath);
    }

    public static string GetName(int level, IReadableColumn? col, AbsoluteDocumentPath InternalDocumentPath)
    {
      if (col is Data.DataColumn datacol)
      {
        var table = Altaxo.Data.DataTable.GetParentDataTableOf(datacol);
        string tablename = table is null ? string.Empty : table.Name + "\\";
        string collectionname = table is null ? string.Empty : (table.PropertyColumns.ContainsColumn(datacol) ? "PropCols\\" : "DataCols\\");
        if (level <= 0)
          return datacol.Name;
        else if (level == 1)
          return tablename + datacol.Name;
        else
          return tablename + collectionname + datacol.Name;
      }
      else
      {
        string path = InternalDocumentPath.ToString();
        int idx = 0;
        if (level <= 0)
        {
          idx = path.LastIndexOf('/');
          if (idx < 0)
            idx = 0;
          else
            idx++;
        }

        return path.Substring(idx);
      }
    }
  }

  #endregion ReadableColumnProxy
}
