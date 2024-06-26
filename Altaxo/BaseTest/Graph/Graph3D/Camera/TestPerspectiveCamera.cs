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
using Altaxo.Geometry;
using Xunit;

namespace Altaxo.Graph.Graph3D.Camera
{

  public class TestPerspectiveCamera
  {
    [Fact]
    public void TestZoom1_XOnly_ZDirection()
    {
      double aspectRatio = 1.0;
      double cameraDistanceOriginal = 1000;
      double zNear = 100;
      double zFar = 10000;
      double widthByZNear = 0.5;
      double distanceFactor = 0.75;
      var targetPosition = new PointD3D(0, 0, 0);
      var targetToEye = new VectorD3D(0, 0, cameraDistanceOriginal);
      var cameraUpVector = new VectorD3D(0, 1, 0);
      var targetToWorldPoint = new VectorD3D(200, 0, 0);

      PointD3D cameraPosition = targetPosition + targetToEye;
      PointD3D worldPoint = targetPosition + targetToWorldPoint;

      Assert.Equal(0, VectorD3D.DotProduct(targetToEye, targetToWorldPoint)); // "The test must be set up in a way that targetToEye and targetToWorldPoint are perpendicular to each other");

      var cameraO = new PerspectiveCamera(cameraUpVector, cameraPosition, targetPosition, zNear, zFar, widthByZNear * zNear);

      var screenO = cameraO.GetViewProjectionMatrix(aspectRatio).Transform(worldPoint);

      Assert.Equal(0.0, screenO.Y); // "Test must be set up so that screen.Y is always zero");

      var cameraN = cameraO.ZoomByGettingCloserToTarget(distanceFactor, screenO.X, screenO.Y, aspectRatio);

      var screenN = cameraN.GetViewProjectionMatrix(aspectRatio).Transform(worldPoint);

      Assert.Equal(screenO.X, screenN.X, 3);
      Assert.Equal(screenO.Y, screenN.Y, 3);
    }

    [Fact]
    public void TestZoom1_XOnly_XDirection()
    {
      double aspectRatio = 1.0;
      double cameraDistanceOriginal = 1000;
      double zNear = 100;
      double zFar = 10000;
      double widthByZNear = 0.5;
      double distanceFactor = 0.75;
      var targetPosition = new PointD3D(0, 0, 0);
      var targetToEye = new VectorD3D(cameraDistanceOriginal, 0, 0);
      var cameraUpVector = new VectorD3D(0, 0, 1);
      var targetToWorldPoint = new VectorD3D(0, 200, 0);

      PointD3D cameraPosition = targetPosition + targetToEye;
      PointD3D worldPoint = targetPosition + targetToWorldPoint;

      Assert.Equal(0, VectorD3D.DotProduct(targetToEye, targetToWorldPoint)); // "The test must be set up in a way that targetToEye and targetToWorldPoint are perpendicular to each other");

      var cameraO = new PerspectiveCamera(cameraUpVector, cameraPosition, targetPosition, zNear, zFar, widthByZNear * zNear);

      var screenO = cameraO.GetViewProjectionMatrix(aspectRatio).Transform(worldPoint);

      Assert.Equal(0.0, screenO.Y); // "Test must be set up so that screen.Y is always zero");

      var cameraN = cameraO.ZoomByGettingCloserToTarget(distanceFactor, screenO.X, screenO.Y, aspectRatio);

      var screenN = cameraN.GetViewProjectionMatrix(aspectRatio).Transform(worldPoint);

      Assert.Equal(screenO.X, screenN.X, 3);
      Assert.Equal(screenO.Y, screenN.Y, 3);
    }

    [Fact]
    public void TestZoom1_XOnly_WithOffset()
    {
      double aspectRatio = 1.0;
      double cameraDistanceOriginal = 1000;
      double zNear = 100;
      double zFar = 10000;
      double widthByZNear = 0.5;
      double distanceFactor = 0.75;
      var cameraPosition = new PointD3D(0, 100, cameraDistanceOriginal);
      var targetPosition = new PointD3D(0, 100, 300);
      var worldPoint = new PointD3D(200, 100, 300);

      var cameraO = new PerspectiveCamera(new VectorD3D(0, 1, 0), cameraPosition, targetPosition, zNear, zFar, widthByZNear * zNear);

      var screenO = cameraO.GetViewProjectionMatrix(aspectRatio).Transform(worldPoint);

      var cameraN = cameraO.ZoomByGettingCloserToTarget(distanceFactor, screenO.X, screenO.Y, aspectRatio);

      var screenN = cameraN.GetViewProjectionMatrix(aspectRatio).Transform(worldPoint);

      Assert.Equal(screenO.X, screenN.X, 3);
      Assert.Equal(screenO.Y, screenN.Y, 3);
    }

    [Fact]
    public void TestZoom1_XY()
    {
      double aspectRatio = 1.0;
      double cameraDistanceOriginal = 1000;
      double zNear = 100;
      double zFar = 10000;
      double widthByZNear = 0.5;
      double distanceFactor = 0.75;
      var cameraPosition = new PointD3D(0, 0, cameraDistanceOriginal);
      var targetPosition = new PointD3D(0, 0, 0);
      var worldPoint = new PointD3D(200, 200, 0);

      var cameraO = new PerspectiveCamera(new VectorD3D(0, 1, 0), cameraPosition, targetPosition, zNear, zFar, widthByZNear * zNear);

      var screenO = cameraO.GetViewProjectionMatrix(aspectRatio).Transform(worldPoint);

      var cameraN = cameraO.ZoomByGettingCloserToTarget(distanceFactor, screenO.X, screenO.Y, aspectRatio);

      var screenN = cameraN.GetViewProjectionMatrix(aspectRatio).Transform(worldPoint);

      Assert.Equal(screenO.X, screenN.X, 3);
      Assert.Equal(screenO.Y, screenN.Y, 3);
    }
  }
}
