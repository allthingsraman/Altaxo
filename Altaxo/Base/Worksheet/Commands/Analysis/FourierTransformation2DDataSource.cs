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

using Altaxo.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Altaxo.Worksheet.Commands.Analysis
{
	/// <summary>
	/// Data source for a table that was created by a two-dimensional Fourier transformation.
	/// </summary>
	public class FourierTransformation2DDataSource : Altaxo.Data.IAltaxoTableDataSource
	{
		private RealFourierTransformation2DOptions _transformationOptions;
		private DataTableMatrixProxy _inputData;
		private IDataSourceImportOptions _importOptions;

		public event Action<IAltaxoTableDataSource> DataSourceChanged;

		protected Main.EventSuppressor _eventSuppressor;

		#region Serialization

		#region Version 0

		/// <summary>
		/// 2014-07-08 initial version.
		/// </summary>
		[Altaxo.Serialization.Xml.XmlSerializationSurrogateFor(typeof(FourierTransformation2DDataSource), 0)]
		private class XmlSerializationSurrogate0 : Altaxo.Serialization.Xml.IXmlSerializationSurrogate
		{
			public virtual void Serialize(object obj, Altaxo.Serialization.Xml.IXmlSerializationInfo info)
			{
				var s = (FourierTransformation2DDataSource)obj;

				info.AddValue("InputData", s._inputData);
				info.AddValue("TransformationOptions", s._transformationOptions);
				info.AddValue("ImportOptions", s._importOptions);
			}

			protected virtual FourierTransformation2DDataSource SDeserialize(object o, Altaxo.Serialization.Xml.IXmlDeserializationInfo info, object parent)
			{
				var s = (o == null ? new FourierTransformation2DDataSource() : (FourierTransformation2DDataSource)o);

				s._inputData = (DataTableMatrixProxy)info.GetValue("InputData");
				s._transformationOptions = (RealFourierTransformation2DOptions)info.GetValue("TransformationOptions");
				s._importOptions = (IDataSourceImportOptions)info.GetValue("ImportOptions");

				s.InputData = s._inputData;

				return s;
			}

			public object Deserialize(object o, Altaxo.Serialization.Xml.IXmlDeserializationInfo info, object parent)
			{
				var s = SDeserialize(o, info, parent);
				return s;
			}
		}

		#endregion Version 0

		#endregion Serialization

		protected FourierTransformation2DDataSource()
		{
			_eventSuppressor = new Main.EventSuppressor(EhResumeSuppressedEvents);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FourierTransformation2DDataSource"/> class.
		/// </summary>
		/// <param name="inputData">The input data designates the original source of data (used then for the Fourier transformation).</param>
		/// <param name="transformationOptions">The Fourier transformation options.</param>
		/// <param name="importOptions">The data source import options.</param>
		/// <exception cref="System.ArgumentNullException">
		/// inputData
		/// or
		/// transformationOptions
		/// or
		/// importOptions
		/// </exception>
		public FourierTransformation2DDataSource(DataTableMatrixProxy inputData, RealFourierTransformation2DOptions transformationOptions, IDataSourceImportOptions importOptions)
		{
			_eventSuppressor = new Main.EventSuppressor(EhResumeSuppressedEvents);

			if (null == inputData)
				throw new ArgumentNullException("inputData");
			if (null == transformationOptions)
				throw new ArgumentNullException("transformationOptions");
			if (null == importOptions)
				throw new ArgumentNullException("importOptions");

			using (var token = SuppressEventsGettingToken())
			{
				this.FourierTransformation2DOptions = transformationOptions;
				this.ImportOptions = importOptions;
				this.InputData = inputData;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FourierTransformation2DDataSource"/> class.
		/// </summary>
		/// <param name="from">Another instance to copy from.</param>
		public FourierTransformation2DDataSource(FourierTransformation2DDataSource from)
		{
			_eventSuppressor = new Main.EventSuppressor(EhResumeSuppressedEvents);

			CopyFrom(from);
		}

		/// <summary>
		/// Copies from another instance.
		/// </summary>
		/// <param name="obj">The object to copy from.</param>
		/// <returns><c>True</c> if anything could be copied from the object, otherwise <c>false</c>.</returns>
		public bool CopyFrom(object obj)
		{
			if (object.ReferenceEquals(this, obj))
				return true;

			var from = obj as FourierTransformation2DDataSource;
			if (null != from)
			{
				using (var token = SuppressEventsGettingToken())
				{
					RealFourierTransformation2DOptions transformationOptions = null;
					DataTableMatrixProxy inputData = null;
					IDataSourceImportOptions importOptions = null;

					CopyHelper.Copy(ref importOptions, from._importOptions);
					CopyHelper.Copy(ref transformationOptions, from._transformationOptions);
					CopyHelper.Copy(ref inputData, from._inputData);

					this.FourierTransformation2DOptions = transformationOptions;
					this.ImportOptions = importOptions;
					this.InputData = inputData;

					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>
		/// A new object that is a copy of this instance.
		/// </returns>
		public object Clone()
		{
			return new FourierTransformation2DDataSource(this);
		}

		/// <summary>
		/// Fills (or refills) the data table with the 2D-Fourier transformation of the original data.. The data source is represented by this instance, the destination table is provided in the argument <paramref name="destinationTable" />.
		/// </summary>
		/// <param name="destinationTable">The destination table.</param>
		public void FillData(DataTable destinationTable)
		{
			try
			{
				FourierCommands.ExecuteFouriertransformation2D(_inputData, _transformationOptions, destinationTable);
			}
			catch (Exception ex)
			{
				destinationTable.Notes.WriteLine("Error during execution of data source ({0}): {1}", this.GetType().Name, ex.Message);
			}
		}

		/// <summary>
		/// Gets or sets the input data.
		/// </summary>
		/// <value>
		/// The input data. This data is the input for the 2D-Fourier transformation.
		/// </value>
		public DataTableMatrixProxy InputData
		{
			get
			{
				return _inputData;
			}
			set
			{
				if (null != _inputData)
				{
					_inputData.Changed -= EhInputDataChanged;
				}

				_inputData = value;

				if (null != _inputData)
				{
					_inputData.Changed += EhInputDataChanged;
					EhInputDataChanged(this, EventArgs.Empty);
				}
			}
		}

		/// <summary>
		/// Gets or sets the data source import options.
		/// </summary>
		/// <value>
		/// The import options.
		/// </value>
		/// <exception cref="System.ArgumentNullException">ImportOptions</exception>
		public Data.IDataSourceImportOptions ImportOptions
		{
			get
			{
				return _importOptions;
			}
			set
			{
				if (null == value)
					throw new ArgumentNullException("ImportOptions");

				var oldValue = _importOptions;

				_importOptions = value;
			}
		}

		/// <summary>
		/// Gets or sets the options for the 2D Fourier transformation.
		/// </summary>
		/// <value>
		/// The 2D Fourier transformation options.
		/// </value>
		/// <exception cref="System.ArgumentNullException">FourierTransformation2DOptions</exception>
		public RealFourierTransformation2DOptions FourierTransformation2DOptions
		{
			get
			{
				return _transformationOptions;
			}
			set
			{
				if (null == value)
					throw new ArgumentNullException("FourierTransformation2DOptions");

				var oldValue = _transformationOptions;

				_transformationOptions = value;
			}
		}

		/// <summary>
		/// Called when the input data have changed. Depending on the <see cref="ImportOptions"/>, the input data may be reprocessed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void EhInputDataChanged(object sender, EventArgs e)
		{
			if (_importOptions.ImportTriggerSource == ImportTriggerSource.DataSourceChanged)
			{
				if (_eventSuppressor.GetEnabledWithCounting())
				{
					var ev = DataSourceChanged;
					if (null != ev)
						ev(this);
				}
			}
		}

		/// <summary>
		/// Called when the event suppressor has resumed events, and any events have been fired in the time of suppression.
		/// </summary>
		private void EhResumeSuppressedEvents()
		{
			var ev = DataSourceChanged;
			if (null != ev)
				ev(this);
		}

		/// <summary>
		/// Called after deserization of a data source instance, when it is already associated with a data table.
		/// </summary>
		public void OnAfterDeserialization()
		{
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
		}

		/// <summary>
		/// Suppresses the events by getting a token. When the token is disposed, events will be resumed again.
		/// </summary>
		/// <returns>Suppress token.</returns>
		public Main.ISuppressToken SuppressEventsGettingToken()
		{
			return _eventSuppressor.Suspend();
		}

		/// <summary>
		/// Resumes the events.
		/// </summary>
		/// <param name="token">The suppress token.</param>
		public void ResumeEvents(ref Main.ISuppressToken token)
		{
			_eventSuppressor.Resume(ref token);
		}

		/// <summary>
		/// Visits all document references.
		/// </summary>
		/// <param name="ReportProxies">The report proxies.</param>
		public void VisitDocumentReferences(Main.DocNodeProxyReporter ReportProxies)
		{
			if (_inputData != null)
				_inputData.VisitDocumentReferences(ReportProxies);
		}
	}
}