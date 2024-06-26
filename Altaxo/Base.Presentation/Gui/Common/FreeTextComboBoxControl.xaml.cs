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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Altaxo.Gui.Common
{
  /// <summary>
  /// Interaction logic for FreeTextComboBoxControl.xaml
  /// </summary>
  public partial class FreeTextComboBoxControl : UserControl, IFreeTextChoiceView
  {
    private BindingExpressionBase _bindingExpression;

    public FreeTextComboBoxControl()
    {
      InitializeComponent();

      var binding = new Binding
      {
        Source = this,
        Path = new PropertyPath("ValidatedText")
      };
      var validator = new MyValidationRule(this);
      binding.ValidationRules.Add(validator);
      binding.Mode = BindingMode.TwoWay;
      binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
      _bindingExpression = _cbChoice.SetBinding(ComboBox.TextProperty, binding);
    }

    private void EhSelectionChangeCommitted(object sender, SelectionChangedEventArgs e)
    {
      if (SelectionChangeCommitted is not null)
        SelectionChangeCommitted(_cbChoice.SelectedIndex);
    }

    #region Dependency property

    public string ValidatedText
    {
      get { return (string)GetValue(ValidatedTextProperty); }
      set { SetValue(ValidatedTextProperty, value); }
    }

    public static readonly DependencyProperty ValidatedTextProperty =
        DependencyProperty.Register("ValidatedText", typeof(string), typeof(FreeTextComboBoxControl),
        new FrameworkPropertyMetadata(EhValidatedTextChanged));

    private static void EhValidatedTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
    }

    #endregion Dependency property

    private class MyValidationRule : ValidationRule, IValueConverter
    {
      private FreeTextComboBoxControl _parent;

      public MyValidationRule(FreeTextComboBoxControl parent)
      {
        _parent = parent;
      }

      public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
      {
        if (_parent.TextValidating is not null)
        {
          var cea = new System.ComponentModel.CancelEventArgs();
          _parent.TextValidating((string)value, cea);
          if (cea.Cancel)
            return new ValidationResult(false, "The entered text is not valid");
        }
        return ValidationResult.ValidResult;
      }

      public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
        return value;
      }

      public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
        return value;
      }
    }

    #region IFreeTextChoiceView

    public event Action<int>? SelectionChangeCommitted;

    public event Action<string, System.ComponentModel.CancelEventArgs>? TextValidating;

    public void SetDescription(string value)
    {
      _lblDescription.Content = value;
    }

    public void SetChoices(string[] values, int initialselection, bool allowFreeText)
    {
      _cbChoice.ItemsSource = null;
      _cbChoice.IsEditable = allowFreeText;
      _cbChoice.ItemsSource = values;
      if (initialselection >= 0 && initialselection < values.Length)
        _cbChoice.SelectedIndex = initialselection;
    }

    #endregion IFreeTextChoiceView
  }
}
