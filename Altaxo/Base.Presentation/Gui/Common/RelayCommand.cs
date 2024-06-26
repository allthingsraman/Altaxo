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

#nullable disable warnings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Altaxo.Gui.Common
{
  public class RelayCommand : ICommand
  {
    private Action<object> execute;
    private Func<object, bool> canExecute;

    public event EventHandler CanExecuteChanged
    {
      add { CommandManager.RequerySuggested += value; }
      remove { CommandManager.RequerySuggested -= value; }
    }

    public RelayCommand(Action<object> execute) : this(execute, null)
    {
    }

    public RelayCommand(Action<object> execute, Func<object, bool> canExecute)
    {
      this.execute = execute;
      this.canExecute = canExecute;
    }

    public bool CanExecute(object parameter)
    {
      return canExecute is null || canExecute(parameter);
    }

    public void Execute(object parameter)
    {
      execute(parameter);
    }
  }

  public class RelayCommand<TArg> : ICommand
  {
    private Action<TArg> execute;
    private Func<TArg, bool> canExecute;

    public event EventHandler CanExecuteChanged
    {
      add { CommandManager.RequerySuggested += value; }
      remove { CommandManager.RequerySuggested -= value; }
    }

    public RelayCommand(Action<TArg> execute) : this(execute, null)
    {
    }

    public RelayCommand(Action<TArg> execute, Func<TArg, bool> canExecute)
    {
      this.execute = execute;
      this.canExecute = canExecute;
    }

    public bool CanExecute(TArg parameter)
    {
      return canExecute is null || canExecute(parameter);
    }

    public void Execute(TArg parameter)
    {
      execute(parameter);
    }

    void ICommand.Execute(object parameter)
    {
      if (parameter is TArg)
        Execute((TArg)parameter);
      else
        throw new ArgumentException(string.Format("Type {0} was expected, but it is type {1}", typeof(TArg), parameter?.GetType()), nameof(parameter));
    }

    bool ICommand.CanExecute(object parameter)
    {
      if (parameter is TArg)
        return CanExecute((TArg)parameter);
      else
        return false;
    }
  }
}
