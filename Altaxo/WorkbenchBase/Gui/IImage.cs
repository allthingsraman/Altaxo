﻿// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Drawing;
using System.Windows.Media;
using Altaxo.Gui;
using Altaxo.Gui.Workbench;
using Altaxo.Main.Services;

namespace Altaxo.Gui
{
  /// <summary>
  /// Represents an image.
  /// </summary>
  public interface IImage
  {
    /// <summary>
    /// Gets the image as WPF ImageSource.
    /// </summary>
    ImageSource ImageSource { get; }

    /// <summary>
    /// Gets the image as System.Drawing.Bitmap.
    /// </summary>
    Bitmap Bitmap { get; }

    /// <summary>
    /// Gets the image as System.Drawing.Icon.
    /// </summary>
    Icon Icon { get; }
  }

  /// <summary>
  /// Represents an image that gets loaded from a ResourceService.
  /// </summary>
  public class ResourceServiceImage : IImage
  {
    private readonly string resourceName;

    /// <summary>
    /// Creates a new ResourceServiceImage.
    /// </summary>
    /// <param name="resourceName">The name of the image resource.</param>
    [Obsolete("Use SD.ResourceService.GetImage() instead")]
    public ResourceServiceImage(string resourceName)
    {
      if (resourceName is null)
        throw new ArgumentNullException("resourceName");
      this.resourceName = resourceName;
    }

    internal ResourceServiceImage(IResourceService resourceService, string resourceName)
    {
      this.resourceName = resourceName;
    }

    /// <inheritdoc/>
    public ImageSource ImageSource
    {
      get
      {
        return PresentationResourceService.GetBitmapSource(resourceName);
      }
    }

    static Bitmap? _defaultErrorBitmap;

    /// <inheritdoc/>
    public Bitmap Bitmap
    {
      get
      {
        var result = Altaxo.Current.ResourceService.GetBitmap(resourceName);

        if (result is not null)
          return result;

        if (_defaultErrorBitmap is not null)
          return _defaultErrorBitmap;

        var bmp = new Bitmap(16, 16);
        using (var g = Graphics.FromImage(bmp))
        {
          g.DrawLine(Pens.Red, 0, 0, 16, 16);
          g.DrawLine(Pens.Red, 0, 16, 16, 0);
        }

        System.Threading.Interlocked.Exchange(ref _defaultErrorBitmap, bmp);
        return bmp;
      }
    }

    /// <inheritdoc/>
    public Icon Icon
    {
      get
      {
        return Altaxo.Current.ResourceService.GetIcon(resourceName);
      }
    }

    public override bool Equals(object? obj)
    {
      return obj is ResourceServiceImage other && resourceName == other.resourceName;
    }

    public override int GetHashCode()
    {
      return resourceName.GetHashCode();
    }

    public override string ToString()
    {
      return string.Format("[ResourceServiceImage {0}]", resourceName);
    }
  }
}
