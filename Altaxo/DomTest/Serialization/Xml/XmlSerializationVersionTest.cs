﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

namespace Altaxo.Serialization.Xml
{

  public class XmlSerializationVersionTest
  {
    public static IEnumerable<Assembly> GetAssembliesToTest()
    {
      yield return typeof(Altaxo.Geometry.PointD2D).Assembly; // AltaxoCore
      yield return typeof(Altaxo.Data.DataTable).Assembly; // AltaxoBase
      yield return typeof(Altaxo.Main.GraphExportBindingDoozer).Assembly; // AltaxoDom

    }

    private record AssemblyNameTypeName(string AssemblyName, string TypeName)
    {
      public override string ToString()
      {
        return $"{AssemblyName}, {TypeName}";
      }
    }

    /// <summary>
    /// Tests if only one serialization attribute exists in which the type is set explicitly (by typeof(...)).
    /// It is neccessary to make this tests for all assemblies simultaneously.
    /// </summary>
    [Fact]
    public void TestAllSerializationAttributes()
    {
      var stb = new StringBuilder();
      var dict = new Dictionary<AssemblyNameTypeName, List<(XmlSerializationSurrogateForAttribute Attr, System.Type AppliedType)>>();
      var allTypesSet = new Dictionary<AssemblyNameTypeName, System.Type>(); // set of all types in the 3 Altaxo assemblies

      // the names of the Altaxo assemblies to test
      var altaxoAssemblyNames = new HashSet<string>(GetAssembliesToTest().Select(ass => ass.GetName().Name));

      foreach (var ass in GetAssembliesToTest())
      {
        var allTypes = ass.GetTypes();
        foreach (var type in allTypes)
        {
          allTypesSet.Add(new AssemblyNameTypeName(type.Assembly.GetName().Name, type.FullName), type);
        }
        foreach (var type in allTypes)
        {
          var attributes = type.GetCustomAttributes().OfType<XmlSerializationSurrogateForAttribute>();

          foreach (var attribute in attributes)
          {
            if (!(attribute.Version >= 0))
              stb.AppendLine($"Attribute version must be >=0: {attribute} applied to {type.FullName}");
            if (string.IsNullOrEmpty(attribute.AssemblyName))
              stb.AppendLine($"Assembly name must not be empty: {attribute} applied to {type.FullName}");
            if (string.IsNullOrEmpty(attribute.TypeName))
              stb.AppendLine($"Type name must not be empty: {attribute} applied to {type.FullName}");

            if (!dict.TryGetValue(new AssemblyNameTypeName(attribute.AssemblyName, attribute.TypeName), out var list))
            {
              list = new List<(XmlSerializationSurrogateForAttribute Attr, System.Type AppliedType)>();
              dict.Add(new AssemblyNameTypeName(attribute.AssemblyName, attribute.TypeName), list);
            }
            list.Add((attribute, type));
          }
        }
      }

      foreach (var entry in dict)
      {
        var list = entry.Value;

        // true if the type that is to be serialized is contained in an Altaxo library
        var isTypeToSerializeAnAltaxoType = altaxoAssemblyNames.Contains(entry.Key.AssemblyName);

        // the list must contain either no (0) or one (1) attribute for which the serializationType is set
        var count = list.Count(x => x.Attr.SerializationType is not null);
        if (!(count == 0 || count == 1))
        {
          stb.AppendLine($"Only one attribute must have set the type directly, but there are {count}: {entry.Key}");
          stb.AppendLine("Those attributes are applied to the following types:");
          foreach (var x in list.Where(x => x.Attr.SerializationType is not null))
          {
            stb.AppendLine($"\t{x.AppliedType.Assembly.GetName().Name}, {x.AppliedType}");
          }
        }

        if (count == 1) // if there is an attribute with the serialization type set ...
        {
          // ... the other attributes in the list must have version numbers below that version
          var onlyAttr = list.Single(x => x.Attr.SerializationType is not null);
          var maxVersion = onlyAttr.Attr.Version;

          // for Altaxo assemblies, the attribute with the direct type must be that with the highest version
          // for other assemblies (System types), the attribute with the direct type must have a version that is greater than or equal to the other attributes
          var attrsWithTooHighVersion = isTypeToSerializeAnAltaxoType ?
                                        list.Where(x => x.Attr.SerializationType is null && x.Attr.Version >= maxVersion).ToArray() :
                                        list.Where(x => x.Attr.SerializationType is null && x.Attr.Version > maxVersion).ToArray();

          if (!(0 == attrsWithTooHighVersion.Length))
          {
            stb.AppendLine($"There is at least one attribute that has a higher or equal version than the attribute that has set the type directly: {entry.Key}");
            stb.AppendLine("Those attributes are applied to the following types:");
            foreach (var x in attrsWithTooHighVersion)
            {
              stb.AppendLine($"\t{x.AppliedType.Assembly.GetName().Name}, {x.AppliedType}: Version:{x.Attr.Version}>={maxVersion}");
            }

          }
        }

        if (isTypeToSerializeAnAltaxoType)
        {
          // all attributes in the list must have different version numbers
          var numberOfVersions = list.Select(x => x.Attr.Version).Distinct().Count();
          if (!(list.Count == numberOfVersions))
          {
            stb.AppendLine($"The version numbers are not all distinct: {entry.Key}");
            var attrs = list.GroupBy(x => x.Attr.Version).ToArray();
            foreach (var group in attrs)
            {
              stb.AppendLine($"Those attributes are applied to the following types (Version={group.Key}):");
              foreach (var x in group)
              {
                stb.AppendLine($"\t{x.AppliedType.Assembly.GetName().Name}, {x.AppliedType}");
              }
            }
          }
        }
        else // if it is not an Altaxo type
        {
          // all indirect attributes in the list must have different version numbers
          var numberOfIndirectAttr = list.Where(x => x.Attr.SerializationType is null).Count();
          var numberOfIndirectAttrVersions = list.Where(x => x.Attr.SerializationType is null).Select(x => x.Attr.Version).Distinct().Count();
          if (!(numberOfIndirectAttr == numberOfIndirectAttrVersions))
          {
            stb.AppendLine($"The version numbers of the indirect attributes are not all distinct: {entry.Key}");
            var attrs = list.Where(x => x.Attr.SerializationType is null).GroupBy(x => x.Attr.Version).ToArray();
            foreach (var group in attrs)
            {
              stb.AppendLine($"Those attributes are applied to the following types (Version={group.Key}):");
              foreach (var x in group)
              {
                stb.AppendLine($"\t{x.AppliedType.Assembly.GetName().Name}, {x.AppliedType}");
              }
            }
          }
        }

        // if the type the attributes refers to still exists in Altaxo, then there
        // must be exactly one attribute which have this type directly set
        // but note (1): it is not an error if the type to serialize is marked as obsolete
        // but note (2): it is not an error if the existing indirect attributes apply to a serialization surrogate which has at least one direct attribute
        if (allTypesSet.TryGetValue(entry.Key, out var serializationType))
        {
          bool isAnError = 0 == count; // no direct attribute exist for that type

          if (isAnError)
          {
            isAnError &= serializationType.GetCustomAttributes().OfType<System.ObsoleteAttribute>().FirstOrDefault() is null; // serialization type is not marked as obsolete
          }

          if (isAnError)
          {
            // See Note 2 above:
            // if there is no direct attribute for that type
            // then the indirect attributes for that type must be applied to a serialization surrogate which
            // has other attributes attached (although not for the same type), and one of those other attributes is a direct one
            // as example see Altaxo.Worksheet.WorksheetViewLayout+XmlSerializationSurrogate0
            bool allIndirectAttributesHaveOneDirectSiblingInSerializationSurrogate = true;
            foreach (var entr in list)
            {
              bool atLeastOneAttributeIsDirect = entr.AppliedType.GetCustomAttributes().OfType<XmlSerializationSurrogateForAttribute>().Any(x => x.SerializationType is not null); ;
              allIndirectAttributesHaveOneDirectSiblingInSerializationSurrogate &= atLeastOneAttributeIsDirect;
            }
            isAnError &= !allIndirectAttributesHaveOneDirectSiblingInSerializationSurrogate;
          }

          if (isAnError)
          {
            stb.AppendLine($"There is no serialization attribute for which serialization type is set directly: {entry.Key}");
            stb.AppendLine($"Those attributes are applied to the following types:");
            foreach (var x in list)
            {
              stb.AppendLine($"\t{x.AppliedType.Assembly.GetName().Name}, {x.AppliedType}: Version:{x.Attr.Version}");
            }
          }
        }
      }
      // if there are any messages, then throw
      Assert.True(stb.Length == 0, stb.ToString());
    }
  }
}
