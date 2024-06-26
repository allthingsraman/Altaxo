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

#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Altaxo.DataConnection
{
  /// <summary>
  /// Contains an connection string (with or without password,
  /// </summary>
  public class AltaxoOleDbConnectionString : Main.IImmutable
  {
    private const string UserIDKey = "User ID";
    private const string PasswordKey = "Password";
    private static AltaxoOleDbConnectionString _emptyInstance = new AltaxoOleDbConnectionString(string.Empty, null);

    private string _originalConnectionString;
    private string _connectionStringWithCredentials;

    public AltaxoOleDbConnectionString(string originalConnectionString, LoginCredentials? credentials)
    {
      _originalConnectionString = originalConnectionString;

      if (credentials is not null && !credentials.AreEmpty)
      {
        var connBuilder = new System.Data.OleDb.OleDbConnectionStringBuilder(_originalConnectionString)
        {
          [UserIDKey] = credentials.UserName,
          [PasswordKey] = credentials.Password
        };
        _connectionStringWithCredentials = connBuilder.ConnectionString;
      }
      else
      {
        _connectionStringWithCredentials = originalConnectionString;
      }
    }

    public bool IsEmpty { get { return string.IsNullOrEmpty(_connectionStringWithCredentials); } }

    public static AltaxoOleDbConnectionString Empty { get { return _emptyInstance; } }

    public string OriginalConnectionString { get { return _originalConnectionString; } }

    public string ConnectionStringWithTemporaryCredentials { get { return _connectionStringWithCredentials; } }

    public LoginCredentials GetCredentials()
    {
      var connBuilder = new System.Data.OleDb.OleDbConnectionStringBuilder(_originalConnectionString);
      string? username = null, password = null;
      if (connBuilder.ContainsKey(UserIDKey))
        username = connBuilder[UserIDKey] as string;
      if (connBuilder.ContainsKey(PasswordKey))
        password = connBuilder[PasswordKey] as string;
      return new LoginCredentials(username ?? string.Empty, password ?? string.Empty);
    }

    public override bool Equals(object? obj)
    {
      if (obj is AltaxoOleDbConnectionString from)
        return _originalConnectionString == from._originalConnectionString && _connectionStringWithCredentials == from._connectionStringWithCredentials;
      else
        return false;
    }

    public override string ToString()
    {
      return _originalConnectionString;
    }

    public override int GetHashCode()
    {
      return _connectionStringWithCredentials.GetHashCode();
    }

    public static bool operator ==(AltaxoOleDbConnectionString x, AltaxoOleDbConnectionString y)
    {
      return ReferenceEquals(x,y) || (x is not null && y is not null && x._originalConnectionString == y._originalConnectionString && x._connectionStringWithCredentials == y._connectionStringWithCredentials);
    }

    public static bool operator !=(AltaxoOleDbConnectionString x, AltaxoOleDbConnectionString y)
    {
      return !(x == y);
    }
  }
}
