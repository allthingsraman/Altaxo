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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Altaxo.Graph.Gdi
{
	#region ChangedEventArgs

	/// <summary>
	/// Holds information about what has changed in the table.
	/// </summary>
	public class GraphDocumentCollectionChangedEventArgs : System.EventArgs
	{
		/// <summary>
		/// If true, one or more tables where added.
		/// </summary>
		public bool ItemAdded;

		/// <summary>
		/// If true, one or more table where removed.
		/// </summary>
		public bool ItemRemoved;

		/// <summary>
		/// If true, one or more tables where renamed.
		/// </summary>
		public bool ItemRenamed;

		/// <summary>
		/// Empty constructor.
		/// </summary>
		public GraphDocumentCollectionChangedEventArgs()
		{
		}

		/// <summary>
		/// Returns an empty instance.
		/// </summary>
		public static new GraphDocumentCollectionChangedEventArgs Empty
		{
			get { return new GraphDocumentCollectionChangedEventArgs(); }
		}

		/// <summary>
		/// Returns an instance with TableAdded set to true;.
		/// </summary>
		public static GraphDocumentCollectionChangedEventArgs IfItemAdded
		{
			get
			{
				GraphDocumentCollectionChangedEventArgs e = new GraphDocumentCollectionChangedEventArgs();
				e.ItemAdded = true;
				return e;
			}
		}

		/// <summary>
		/// Returns an instance with TableRemoved set to true.
		/// </summary>
		public static GraphDocumentCollectionChangedEventArgs IfItemRemoved
		{
			get
			{
				GraphDocumentCollectionChangedEventArgs e = new GraphDocumentCollectionChangedEventArgs();
				e.ItemRemoved = true;
				return e;
			}
		}

		/// <summary>
		/// Returns an  instance with TableRenamed set to true.
		/// </summary>
		public static GraphDocumentCollectionChangedEventArgs IfItemRenamed
		{
			get
			{
				GraphDocumentCollectionChangedEventArgs e = new GraphDocumentCollectionChangedEventArgs();
				e.ItemRenamed = true;
				return e;
			}
		}

		/// <summary>
		/// Merges information from another instance in this ChangedEventArg.
		/// </summary>
		/// <param name="from"></param>
		public void Merge(GraphDocumentCollectionChangedEventArgs from)
		{
			this.ItemAdded |= from.ItemAdded;
			this.ItemRemoved |= from.ItemRemoved;
			this.ItemRenamed |= from.ItemRenamed;
		}

		/// <summary>
		/// Returns true when the collection has changed (addition, removal or renaming of tables).
		/// </summary>
		public bool CollectionChanged
		{
			get { return ItemAdded | ItemRemoved | ItemRenamed; }
		}
	}

	#endregion ChangedEventArgs
}