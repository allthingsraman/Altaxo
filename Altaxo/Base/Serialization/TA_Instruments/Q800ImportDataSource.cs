﻿#region Copyright

/////////////////////////////////////////////////////////////////////////////
//    Altaxo:  a data processing and data plotting program
//    Copyright (C) 2002-2025 Dr. Dirk Lellinger
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Altaxo.Data;

namespace Altaxo.Serialization.TA_Instruments
{
  public class Q800ImportDataSource : TableDataSourceBase, Altaxo.Data.IAltaxoTableDataSource
  {
    private IDataSourceImportOptions _importOptions;

    private Q800ImportOptions _processOptions;

    private List<AbsoluteAndRelativeFileName> _fileNames = new List<AbsoluteAndRelativeFileName>();

    private HashSet<string> _resolvedAsciiFileNames = new HashSet<string>();

    protected bool _isDirty = false;

    protected System.IO.FileSystemWatcher[] _fileSystemWatchers = new System.IO.FileSystemWatcher[0];

    protected Altaxo.Main.TriggerBasedUpdate? _triggerBasedUpdate;

    /// <summary>Indicates that serialization of the whole AltaxoDocument (!) is still in progress. Data sources should not be updated during serialization.</summary>
    [NonSerialized]
    protected bool _isDeserializationInProgress;

    #region Serialization

    #region Version 0

    /// <summary>
    /// 2025-05-25 initial version.
    /// </summary>
    [Altaxo.Serialization.Xml.XmlSerializationSurrogateFor(typeof(Q800ImportDataSource), 0)]
    private class XmlSerializationSurrogate0 : Altaxo.Serialization.Xml.IXmlSerializationSurrogate
    {
      public virtual void Serialize(object obj, Altaxo.Serialization.Xml.IXmlSerializationInfo info)
      {
        var s = (Q800ImportDataSource)obj;

        info.AddValue("ImportOptions", s._importOptions);
        info.AddValue("ProcessOptions", s._processOptions);
        info.AddArray("ProcessData", s._fileNames.ToArray(), s._fileNames.Count);
      }

      public object Deserialize(object? o, Altaxo.Serialization.Xml.IXmlDeserializationInfo info, object? parent)
      {
        if (o is Q800ImportDataSource s)
          s.DeserializeSurrogate0(info);
        else
          s = new Q800ImportDataSource(info, 0);
        return s;
      }
    }

    [MemberNotNull(nameof(_importOptions), nameof(_processOptions))]
    private void DeserializeSurrogate0(Altaxo.Serialization.Xml.IXmlDeserializationInfo info)
    {
      _isDeserializationInProgress = true;
      ChildSetMember(ref _importOptions, (IDataSourceImportOptions)info.GetValue("ImportOptions", this));
      _processOptions = (Q800ImportOptions)info.GetValue("ProcessOptions", this);
      var count = info.OpenArray("ProcessData");
      for (int i = 0; i < count; ++i)
        _fileNames.Add((AbsoluteAndRelativeFileName)info.GetValue("e", this));
      info.CloseArray(count);

      info.AfterDeserializationHasCompletelyFinished += EhAfterDeserializationHasCompletelyFinished;
    }

    #endregion Version 0

    /// <summary>
    /// Deserialization constructor
    /// </summary>
    protected Q800ImportDataSource(Altaxo.Serialization.Xml.IXmlDeserializationInfo info, int version)
    {
      switch (version)
      {
        case 0:
          DeserializeSurrogate0(info);
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(version));
      }
    }

    #endregion Serialization

    #region Construction

    [MemberNotNull(nameof(_fileNames), nameof(_importOptions), nameof(_processOptions))]
    private void CopyFrom(Q800ImportDataSource from)
    {
      using (var token = SuspendGetToken())
      {
        _fileNames = new List<AbsoluteAndRelativeFileName>(CopyHelper.GetEnumerationMembersNotNullCloned(from._fileNames));
        ChildCloneToMember(ref _importOptions, from._importOptions);
        _processOptions = from._processOptions;
        EhSelfChanged(EventArgs.Empty);
        token.Resume();
      }
    }

    public bool CopyFrom(object obj)
    {
      if (ReferenceEquals(this, obj))
        return true;

      var from = obj as Q800ImportDataSource;
      if (from is not null)
      {
        CopyFrom(from);
        return true;
      }
      return false;
    }




    public Q800ImportDataSource(string fileName, Q800ImportOptions options)
      : this(new string[] { fileName }, options)
    {
    }

    public Q800ImportDataSource(IEnumerable<string> fileNames, Q800ImportOptions options)
    {
      _fileNames = new List<AbsoluteAndRelativeFileName>();
      foreach (var fileName in fileNames)
      {
        _fileNames.Add(new AbsoluteAndRelativeFileName(fileName));
      }
      _processOptions = options;
      _importOptions = new DataSourceImportOptions() { ParentObject = this };
    }

    public Q800ImportDataSource(Q800ImportDataSource from)
    {
      CopyFrom(from);
    }

    public object Clone()
    {
      return new Q800ImportDataSource(this);
    }

    protected override IEnumerable<Main.DocumentNodeAndName> GetDocumentNodeChildrenWithName()
    {
      if (_importOptions is not null)
        yield return new Main.DocumentNodeAndName(_importOptions, () => _importOptions = null!, "ImportOptions");
    }

    #endregion Construction

    protected override void OnResume(int eventCount)
    {
      base.OnResume(eventCount);

      // UpdateWatching should only be called if something concerning the watch (Times etc.) has changed during the suspend phase
      // Otherwise it will cause endless loops because UpdateWatching triggers immediatly an EhUpdateByTimerQueue event, which triggers an UpdateDataSource event, which leads to another Suspend and then Resume, which calls OnResume(). So the loop is closed.
      if (_triggerBasedUpdate is null)
        UpdateWatching(); // Compromise - we update only if the watch is off
    }

    public override void FillData_Unchecked(DataTable destinationTable, IProgressReporter reporter)
    {
      var validFileNames = _fileNames.Select(x => x.GetResolvedFileNameOrNull()).OfType<string>().Where(x => !string.IsNullOrEmpty(x)).ToArray();

      if (validFileNames.Length > 0)
      {
        destinationTable.DataColumns.RemoveColumnsAll();
        destinationTable.PropCols.RemoveColumnsAll();
        new Q800Importer().Import(validFileNames, destinationTable, _processOptions, attachDataSource: false);
      }

      var invalidFileNames = _fileNames.Where(x => string.IsNullOrEmpty(x.GetResolvedFileNameOrNull())).ToArray();
      if (invalidFileNames.Length > 0)
      {
        var stb = new StringBuilder();
        stb.AppendLine("The following file names could not be resolved:");
        foreach (var fn in invalidFileNames)
        {
          stb.AppendLine(fn.AbsoluteFileName);
        }
        stb.AppendLine("(End of file names that could not be resolved)");

        throw new ApplicationException(stb.ToString());
      }
    }

    #region Properties

    public string SourceFileName
    {
      get
      {
        if (_fileNames.Count != 1)
          throw new InvalidOperationException("In order to get the source file name, the number of files has to be one");

        return _fileNames[0].GetResolvedFileNameOrNull() ?? _fileNames[0].AbsoluteFileName;
      }
      set
      {
        string? oldName = null;
        if (_fileNames.Count == 1)
          oldName = SourceFileName;

        _fileNames.Clear();
        _fileNames.Add(new AbsoluteAndRelativeFileName(value));

        if (oldName != SourceFileName)
        {
          UpdateWatching();
        }
      }
    }

    public IEnumerable<string> SourceFileNames
    {
      get
      {
        return _fileNames.Select(x => x.GetResolvedFileNameOrNull() ?? x.AbsoluteFileName);
      }
      set
      {
        _fileNames.Clear();
        foreach (var name in value)
          _fileNames.Add(new AbsoluteAndRelativeFileName(name));

        UpdateWatching();
      }
    }

    public int SourceFileNameCount
    {
      get
      {
        return _fileNames.Count;
      }
    }

    public override IDataSourceImportOptions ImportOptions
    {
      get
      {
        return _importOptions;
      }
      set
      {
        if (value is null)
          throw new ArgumentNullException(nameof(ImportOptions));

        var oldValue = _importOptions;

        _importOptions = value;

        if (!object.ReferenceEquals(oldValue, value))
        {
          UpdateWatching();
        }
      }
    }

    public Q800ImportOptions ProcessOptions
    {
      get
      {
        return _processOptions;
      }
      set
      {
        _processOptions = value;
      }
    }

    object IAltaxoTableDataSource.ProcessOptionsObject
    {
      get => _processOptions;
      set => ProcessOptions = (Q800ImportOptions)value;
    }

    object IAltaxoTableDataSource.ProcessDataObject
    {
      get => SourceFileNames;
      set => SourceFileNames = (IEnumerable<string>)value;
    }

    #endregion Properties

    private void SetAbsoluteRelativeFilePath(AbsoluteAndRelativeFileName value)
    {
      if (value is null)
        throw new ArgumentNullException("value");

      var oldValue = _fileNames.Count == 1 ? _fileNames[0] : null;
      _fileNames.Clear();
      _fileNames.Add(value);

      if (!value.Equals(oldValue))
      {
        UpdateWatching();
      }
    }

    public void OnAfterDeserialization()
    {
      // Note: it is not neccessary to call UpdateWatching here; UpdateWatching is called when the table connects to this data source via subscription to the DataSourceChanged event
    }

    public void VisitDocumentReferences(Main.DocNodeProxyReporter ReportProxies)
    {
    }

    public void UpdateWatching()
    {
      SwitchOffWatching();

      if (_isDeserializationInProgress)
        return; // in serialization process - wait until serialization has finished

      if (IsSuspended)
        return; // in update operation - wait until finished

      if (_parent is null)
        return; // No listener - no need to watch

      if (_importOptions.ImportTriggerSource != ImportTriggerSource.DataSourceChanged)
        return; // DataSource is updated manually

      var validFileNames = _fileNames.Select(x => x.GetResolvedFileNameOrNull()).OfType<string>().Where(x => !string.IsNullOrEmpty(x)).ToArray();
      if (0 == validFileNames.Length)
        return;  // No file name set

      _resolvedAsciiFileNames = new HashSet<string>(validFileNames);

      SwitchOnWatching(validFileNames);
    }

    private void SwitchOnWatching(string[] validFileNames)
    {
      _triggerBasedUpdate = new Main.TriggerBasedUpdate(Current.TimerQueue)
      {
        MinimumWaitingTimeAfterUpdate = TimeSpanExtensions.FromSecondsAccurate(_importOptions.MinimumWaitingTimeAfterUpdateInSeconds),
        MaximumWaitingTimeAfterUpdate = TimeSpanExtensions.FromSecondsAccurate(Math.Max(_importOptions.MinimumWaitingTimeAfterUpdateInSeconds, _importOptions.MaximumWaitingTimeAfterUpdateInSeconds)),
        MinimumWaitingTimeAfterFirstTrigger = TimeSpanExtensions.FromSecondsAccurate(_importOptions.MinimumWaitingTimeAfterFirstTriggerInSeconds),
        MinimumWaitingTimeAfterLastTrigger = TimeSpanExtensions.FromSecondsAccurate(_importOptions.MinimumWaitingTimeAfterLastTriggerInSeconds),
        MaximumWaitingTimeAfterFirstTrigger = TimeSpanExtensions.FromSecondsAccurate(Math.Max(_importOptions.MaximumWaitingTimeAfterFirstTriggerInSeconds, _importOptions.MinimumWaitingTimeAfterFirstTriggerInSeconds))
      };

      _triggerBasedUpdate.UpdateAction += EhUpdateByTimerQueue;

      var directories = new HashSet<string>(validFileNames.Select(x => System.IO.Path.GetDirectoryName(x)).OfType<string>());
      var watchers = new List<System.IO.FileSystemWatcher>();
      foreach (var directory in directories)
      {
        try
        {
          var watcher = new System.IO.FileSystemWatcher(directory)
          {
            NotifyFilter = System.IO.NotifyFilters.LastWrite | System.IO.NotifyFilters.Size
          };
          watcher.Changed += EhTriggerByFileSystemWatcher;
          watcher.IncludeSubdirectories = false;
          watcher.EnableRaisingEvents = true;
          watchers.Add(watcher);
        }
        catch (Exception)
        {
        }
      }
      _fileSystemWatchers = watchers.ToArray();
    }

    private void SwitchOffWatching()
    {
      IDisposable? disp = null;

      var watchers = _fileSystemWatchers;
      _fileSystemWatchers = new System.IO.FileSystemWatcher[0];

      for (int i = 0; i < watchers.Length; ++i)
      {
        disp = watchers[i];
        if (disp is not null)
          disp.Dispose();
      }

      disp = _triggerBasedUpdate;
      _triggerBasedUpdate = null;
      if (disp is not null)
        disp.Dispose();
    }

    public void EhUpdateByTimerQueue()
    {
      if (_parent is not null)
      {
        if (!IsSuspended) // no events during the suspend phase
        {
          EhChildChanged(this, TableDataSourceChangedEventArgs.Empty);
        }
      }
      else
        SwitchOffWatching();
    }

    private void EhTriggerByFileSystemWatcher(object sender, System.IO.FileSystemEventArgs e)
    {
      if (!_resolvedAsciiFileNames.Contains(e.FullPath))
        return;

      if (_triggerBasedUpdate is not null)
      {
        _triggerBasedUpdate.Trigger();
      }
    }

    private void EhAfterDeserializationHasCompletelyFinished()
    {
      _isDeserializationInProgress = false;
      UpdateWatching();
    }

    protected override void Dispose(bool disposing)
    {
      if (!IsDisposed)
        SwitchOffWatching();

      base.Dispose(disposing);
    }
  }
}
