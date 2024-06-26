﻿#region Copyright

/////////////////////////////////////////////////////////////////////////////
//    Altaxo:  a data processing and data plotting program
//    Copyright (C) 2002-2018 Dr. Dirk Lellinger
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

namespace Altaxo.Analysis.Statistics.Histograms
{
  /// <summary>
  /// Maintains information for user interaction during binning.
  /// </summary>
  public class HistogramCreationInformation : ICloneable
  {
    private System.Collections.ObjectModel.ObservableCollection<string> _warnings = new System.Collections.ObjectModel.ObservableCollection<string>();
    private System.Collections.ObjectModel.ObservableCollection<string> _errors = new System.Collections.ObjectModel.ObservableCollection<string>();
    protected HistogramCreationOptions _creationOptions = new HistogramCreationOptions();

    /// <summary>
    /// Gets the list of errors that occured during the histogram creation.
    /// </summary>
    /// <value>
    /// List of errors.
    /// </value>
    public IList<string> Errors { get { return _errors; } }

    /// <summary>
    /// Gets the list of warnings that occured during the histogram creation.
    /// </summary>
    /// <value>
    /// List of warnings.
    /// </value>
    public IList<string> Warnings { get { return _warnings; } }

    /// <summary>
    /// Gets or sets the number of original values (before filtering).
    /// </summary>
    /// <value>
    /// The number of ensemble values before filtering.
    /// </value>
    public int NumberOfValuesOriginal { get; set; }

    /// <summary>
    /// Gets or sets the number of ensemble values.
    /// </summary>
    /// <value>
    /// The number of ensemble values.
    /// </value>
    public int NumberOfValuesFiltered { get; set; }

    /// <summary>
    /// Gets or sets the number of ensemble values containing NaN (not-a-number).
    /// </summary>
    /// <value>
    /// The number of NaN values.
    /// </value>
    public int NumberOfNaNValues { get; set; }

    /// <summary>
    /// Gets or sets the number of ensemble values containing Infinity.
    /// </summary>
    /// <value>
    /// The number of infinite values.
    /// </value>
    public int NumberOfInfiniteValues { get; set; }

    /// <summary>
    /// Gets or sets the minimum of the ensemble values.
    /// </summary>
    /// <value>
    /// The minimum of the ensemble values.
    /// </value>
    public double MinimumValue { get; set; }

    /// <summary>
    /// Gets or sets the maximum of the ensemble values.
    /// </summary>
    /// <value>
    /// The maximum of the ensemble values.
    ///</value>
    public double MaximumValue { get; set; }

    /// <summary>
    /// Gets the options used to create the histogram data.
    /// </summary>
    /// <value>
    /// The histogram data creation options.
    /// </value>
    public HistogramCreationOptions CreationOptions { get { return _creationOptions; } }

    /// <summary>
    /// Gets or sets the original data ensemble.
    /// </summary>
    /// <value>
    /// The original data ensemble.
    /// </value>
    public IEnumerator<double>? OriginalDataEnsemble { get; set; }

    /// <summary>
    /// Gets or sets the filtered and sorted data ensemble. The filter options can be found in <see cref="CreationOptions"/>.
    /// </summary>
    /// <value>
    /// The filtered and sorted data ensemble.
    /// </value>
    public IReadOnlyList<double>? FilteredAndSortedDataEnsemble { get; set; }

    /// <summary>
    /// Gets or sets the level of user interaction.
    /// </summary>
    /// <value>
    /// The user interaction level.
    /// </value>
    public Gui.UserInteractionLevel UserInteractionLevel { get; set; }

    /// <inheritdoc />
    public object Clone()
    {
      var result = (HistogramCreationInformation)MemberwiseClone();
      result._warnings = new System.Collections.ObjectModel.ObservableCollection<string>(_warnings);
      result._errors = new System.Collections.ObjectModel.ObservableCollection<string>(_errors);
      result._creationOptions = (HistogramCreationOptions)CreationOptions.Clone();
      return result;
    }
  }
}
