﻿#region Copyright

/////////////////////////////////////////////////////////////////////////////
//    Altaxo:  a data processing and data plotting program
//    Copyright (C) 2002-2023 Dr. Dirk Lellinger
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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Altaxo.Gui.Serialization.Ascii
{
  /// <summary>
  /// Interaction logic for AsciiCharControl.xaml
  /// </summary>
  public partial class AsciiCharControl : UserControl
  {
    public AsciiCharControl()
    {
      InitializeComponent();
    }

    public char Value
    {
      get { return (char)GetValue(ValueProperty); }
      set { SetValue(ValueProperty, value); }
    }

    /// <summary>
    /// Identifies the Value dependency property.
    /// </summary>
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            "Value", typeof(char), typeof(AsciiCharControl),
            new FrameworkPropertyMetadata('\0',
                new PropertyChangedCallback(OnValueChanged)
            )
            { BindsTwoWayByDefault = true }
        );

    private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      var control = (AsciiCharControl)obj;

      var oldValue = (char)args.OldValue;
      var newValue = (char)args.NewValue;

      if (oldValue != newValue)
      {
        control._guiEditBox.Text = string.Empty + newValue;
      }
    }

    public string Text
    {
      get
      {
        return _guiEditBox.Text;
      }
      set
      {
        _guiEditBox.Text = value;
      }
    }

    private void EhTextChanged(object sender, TextChangedEventArgs e)
    {
      var tb = sender as TextBox;
      if (tb is null)
        return;

      string asciiValue;
      if (string.IsNullOrEmpty(tb.Text))
      {
        asciiValue = "[No value]";
      }
      else
      {
        char c = tb.Text[0];

        switch (c)
        {
          case ' ':
            asciiValue = "[Space]";
            break;

          case '\t':
            asciiValue = "[Tabulator]";
            break;

          default:
            asciiValue = string.Format("[0x{0:X}]", (int)c);
            break;
        }
      }
      _guiAsciiValue.Content = asciiValue;

      if (string.IsNullOrEmpty(tb.Text))
      {
        _guiAsciiValue.Foreground = Brushes.Red;
        _guiEmptyIndicator.Visibility = System.Windows.Visibility.Visible;
      }
      else
      {
        _guiAsciiValue.Foreground = _guiEditBox.Foreground;
        _guiEmptyIndicator.Visibility = System.Windows.Visibility.Hidden;
        Value = tb.Text[0];
      }
    }
  }
}
