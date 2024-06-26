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

#nullable disable warnings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Altaxo.Units;

namespace Altaxo.Gui.Common.Drawing
{
  /// <summary>
  /// Base class for a combobox in that the user can input a dimensionful quantity. This class will accept dimensionless quantities, since the default value
  /// for <see cref="SelectedQuantity"/> is registered with a dimensionless quantity. To make the box accept quantities in other units (for instance length),
  /// derive from this class and override the metadata of the <see cref="SelectedQuantityProperty"/> to use a default value in the destination unit.
  /// </summary>
  public class DimensionfulQuantityImageComboBox : EditableImageComboBox
  {
    protected QuantityWithUnitConverter _converter;

    public event DependencyPropertyChangedEventHandler? SelectedQuantityChanged;

    static DimensionfulQuantityImageComboBox()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(DimensionfulQuantityImageComboBox), new FrameworkPropertyMetadata(typeof(DimensionfulQuantityImageComboBox)));
    }

    public DimensionfulQuantityImageComboBox()
    {
      SetBinding("SelectedQuantity");
      IsTextSearchEnabled = false; // switch text search off since this interferes with the unit system
      _converter.UnitEnvironment = UnitEnvironment;
    }

    protected void SetBinding(string nameOfValueProperty)
    {
      var binding = new Binding
      {
        Source = this,
        Path = new PropertyPath(nameOfValueProperty),
        Mode = BindingMode.TwoWay
      };
      _converter = new QuantityWithUnitConverter(this, SelectedQuantityProperty);
      binding.Converter = _converter;
      binding.ValidationRules.Add(_converter);
      _converter.BindingExpression = SetBinding(ComboBox.TextProperty, binding);

      var dpd = System.ComponentModel.DependencyPropertyDescriptor.FromProperty(ComboBox.TextProperty, typeof(DimensionfulQuantityImageComboBox));
      dpd.AddValueChanged(this, QuantityWithUnitTextBox_TextChanged);

      var childs = LogicalChildren;
    }

    private void QuantityWithUnitTextBox_TextChanged(object? sender, EventArgs e)
    {
      _converter.BindingExpression.ValidateWithoutUpdate();
    }

    protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
    {
      base.OnIsKeyboardFocusWithinChanged(e);

      if (true == (bool)e.OldValue && false == (bool)e.NewValue)
      {
        if (!_converter.BindingExpression.HasError) // if text was successfully interpreted
        {
          _converter.ClearIntermediateConversionResults(); // clear the previous conversion, so that a full new conversion from quantity to string is done when UpdateTarget is called
          _converter.BindingExpression.UpdateTarget(); // update the text with the full quanity including the unit
        }
      }
    }

    protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
    {
      if (e.Key == System.Windows.Input.Key.F5) // interpret the text and update the quantity
      {
        e.Handled = true;
        _converter.BindingExpression.UpdateSource(); // interpret the text
        if (!_converter.BindingExpression.HasError) // if text was successfully interpreted
        {
          _converter.ClearIntermediateConversionResults(); // clear the previous conversion, so that a full new conversion from quantity to string is done when UpdateTarget is called
          _converter.BindingExpression.UpdateTarget(); // update the text with the full quanity including the unit
        }
      }

      base.OnKeyDown(e);
    }

    protected override void OnContextMenuOpening(ContextMenuEventArgs e)
    {
      _converter.OnContextMenuOpening();
      base.OnContextMenuOpening(e);
    }

    #region Dependency property

    /// <summary>
    /// Gets/sets the quantity. The quantity consist of a numeric value together with a unit.
    /// </summary>
    public DimensionfulQuantity SelectedQuantity
    {
      get { return (DimensionfulQuantity)GetValue(SelectedQuantityProperty); }
      set { SetValue(SelectedQuantityProperty, value); }
    }

    public static readonly DependencyProperty SelectedQuantityProperty =
        DependencyProperty.Register("SelectedQuantity", typeof(DimensionfulQuantity), typeof(DimensionfulQuantityImageComboBox),
        new FrameworkPropertyMetadata(EhSelectedQuantityChanged) { BindsTwoWayByDefault=true});

    private static void EhSelectedQuantityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      ((DimensionfulQuantityImageComboBox)obj).OnSelectedQuantityChanged(obj, args);
    }

    /// <summary>
    /// Triggers the <see cref="SelectedQuantityChanged"/> event.
    /// </summary>
    /// <param name="obj">Dependency object (here: the control).</param>
    /// <param name="args">Property changed event arguments.</param>
    protected virtual void OnSelectedQuantityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      if (SelectedQuantityChanged is not null)
        SelectedQuantityChanged(obj, args);
    }

    #region Dependency property UnitEnvironment

    public static readonly DependencyProperty UnitEnvironmentProperty =
        DependencyProperty.Register(nameof(UnitEnvironment), typeof(QuantityWithUnitGuiEnvironment), typeof(DimensionfulQuantityImageComboBox),
         new FrameworkPropertyMetadata(EhUnitEnvironmentChanged));

    /// <summary>
    /// Sets the unit environment. The unit environment determines the units the user is able to enter.
    /// </summary>
    public QuantityWithUnitGuiEnvironment UnitEnvironment
    {
      get
      {
        return (QuantityWithUnitGuiEnvironment)GetValue(UnitEnvironmentProperty);
      }

      set
      {
        SetValue(UnitEnvironmentProperty, value);
      }
    }

    private static void EhUnitEnvironmentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
      var thiss = (DimensionfulQuantityImageComboBox)obj;
      if (args.NewValue is not null)
        thiss._converter.UnitEnvironment = (QuantityWithUnitGuiEnvironment)args.NewValue;
    }

    #endregion Dependency property UnitEnvironment


    #endregion Dependency property

    public double SelectedQuantityInSIUnits
    {
      get { return SelectedQuantity.AsValueInSIUnits; }
      set
      {
        if (UnitEnvironment is null)
          throw new InvalidOperationException("The value can not be set because the unit environment is not initialized yet");

        var quant = new DimensionfulQuantity(value, UnitEnvironment.DefaultUnit.Unit.SIUnit);
        quant = quant.AsQuantityIn(UnitEnvironment.DefaultUnit);
        SelectedQuantity = quant;
      }
    }
  }
}
