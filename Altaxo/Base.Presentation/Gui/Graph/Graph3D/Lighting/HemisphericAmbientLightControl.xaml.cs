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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Altaxo.Gui.Graph.Graph3D.Lighting
{
  /// <summary>
  /// Interaction logic for HemisphericAmbientLightControl.xaml
  /// </summary>
  public partial class HemisphericAmbientLightControl : UserControl
  {
    public event EventHandler? ValueChanged;

    private double _lightAmplitude;

    private GuiChangeLocker _lock;

    public HemisphericAmbientLightControl()
    {
      InitializeComponent();
    }

    protected virtual void OnSelectedValueChanged()
    {
      if (_lock.IsNotLocked)
        ValueChanged?.Invoke(this, EventArgs.Empty);
    }

    private void EhColorChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      OnSelectedValueChanged();
    }

    public Altaxo.Graph.Graph3D.Lighting.HemisphericAmbientLight SelectedValue
    {
      get
      {
        return new Altaxo.Graph.Graph3D.Lighting.HemisphericAmbientLight(
          _lightAmplitude,
          _guiColorBelow.SelectedColor,
          _guiColorAbove.SelectedColor,
          _guiDirection.SelectedValue,
          _guiAttachedToCamera.IsChecked == true
        );
      }
      set
      {
        if (value is null)
          throw new ArgumentNullException(nameof(value));

        _lock.ExecuteLocked(
          () =>
          {
            _lightAmplitude = value.LightAmplitude;
            if (_guiLightAmplitudeSlider.Maximum < _lightAmplitude)
              _guiLightAmplitudeSlider.Maximum = _lightAmplitude;
            _guiLightAmplitudeSlider.Value = _lightAmplitude;
            _guiLightAmplitudeBox.SelectedValue = _lightAmplitude;

            _guiColorBelow.SelectedColor = value.ColorBelow;
            _guiColorAbove.SelectedColor = value.ColorAbove;
            _guiDirection.SelectedValue = value.DirectionBelowToAbove;
          });
      }
    }

    private void EhAttachedToCameraChanged(object sender, RoutedEventArgs e)
    {
      OnSelectedValueChanged();
    }

    private void EhLightAmplitudeBoxChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      _lock.ExecuteLockedButOnlyIfNotLockedBefore(
    () =>
    {
      _lightAmplitude = _guiLightAmplitudeBox.SelectedValue;
      if (_guiLightAmplitudeSlider.Maximum < _lightAmplitude)
        _guiLightAmplitudeSlider.Maximum = _lightAmplitude;
      _guiLightAmplitudeSlider.Value = _lightAmplitude;
    },
    () => ValueChanged?.Invoke(this, EventArgs.Empty)
    );
    }

    private void EhLightAmplitudeSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      _lock.ExecuteLockedButOnlyIfNotLockedBefore(
        () =>
        {
          _lightAmplitude = _guiLightAmplitudeSlider.Value;
          _guiLightAmplitudeBox.SelectedValue = _lightAmplitude;
        },
        () => ValueChanged?.Invoke(this, EventArgs.Empty)
        );
    }

    private void EhDirectionChanged(object sender, EventArgs e)
    {
      OnSelectedValueChanged();
    }
  }
}
