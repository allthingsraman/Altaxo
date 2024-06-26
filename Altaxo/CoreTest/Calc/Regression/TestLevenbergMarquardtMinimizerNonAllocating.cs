﻿#region Copyright

/////////////////////////////////////////////////////////////////////////////
// Altaxo:  a data processing and data plotting program
// Copyright (C) 2002-2022 Dr. Dirk Lellinger
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//
/////////////////////////////////////////////////////////////////////////////

#endregion Copyright

using System;
using System.Collections.Generic;
using System.Threading;
using Altaxo.Calc.FitFunctions.Peaks;
using Altaxo.Calc.LinearAlgebra;
using Altaxo.Calc.Optimization;
using Altaxo.Calc.Optimization.ObjectiveFunctions;
using Altaxo.Calc.Regression.Nonlinear;
using Xunit;

namespace Altaxo.Calc.Regression
{
  public class TestLevenbergMarquardtMinimizerNonAllocating
  {
    [Fact]
    public void TestGauss()
    {
      var xx = new double[10];
      var yy = new double[10];
      for (int i = 0; i < xx.Length; ++i)
      {
        xx[i] = i;
        double arg = (xx[i] - 5) / 1.5;
        yy[i] = 17 * Math.Exp(-0.5 * arg * arg);
      }

      var initialGuess = new double[3] { 10, 5, 3 };

      var ff = new GaussAmplitude(1, -1);
      var model = new NonlinearObjectiveFunctionNonAllocating(ff.Evaluate, ff.EvaluateDerivative, 1);
      model.SetObserved(xx, yy, null);
      var fit = new LevenbergMarquardtMinimizerNonAllocating();
      var result = fit.FindMinimum(model, initialGuess, null, null, null, null, CancellationToken.None, null);

      AssertEx.GreaterOrEqual(2E-16, result.ModelInfoAtMinimum.Value);
      AssertEx.AreEqual(17, result.MinimizingPoint[0], 1E-16, 1E-8);
      AssertEx.AreEqual(5, result.MinimizingPoint[1], 1E-16, 1E-8);
      AssertEx.AreEqual(1.5, result.MinimizingPoint[2], 1E-16, 1E-8);
    }

    /// <summary>
    /// Create a Gaussian with width=1.5, but limit the fit width to 3.
    /// </summary>
    [Fact]
    public void TestGaussLowerBound()
    {
      var xx = new double[10];
      var yy = new double[10];
      for (int i = 0; i < xx.Length; ++i)
      {
        xx[i] = i;
        double arg = (xx[i] - 5) / 1.5;
        yy[i] = 17 * Math.Exp(-0.5 * arg * arg);
      }

      var initialGuess = new double[3] { 10, 5, 3 };
      var lowerBound = new double?[3] { null, null, 2 };

      var ff = new GaussAmplitude(1, -1);
      var model = new NonlinearObjectiveFunctionNonAllocating(ff.Evaluate, ff.EvaluateDerivative, 1);
      model.SetObserved(xx, yy, null);
      var fit = new LevenbergMarquardtMinimizerNonAllocating();
      var result = fit.FindMinimum(model, initialGuess, lowerBound, null, null, null, CancellationToken.None, null);

      AssertEx.AreEqual(2, result.MinimizingPoint[2], 1E-16, 1E-16);
      AssertEx.GreaterOrEqual(32, result.ModelInfoAtMinimum.Value);
    }

    /// <summary>
    /// Create a Gaussian with height=17, but limit the fit height to 16.
    /// </summary>
    [Fact]
    public void TestGaussUpperBound()
    {
      var xx = new double[10];
      var yy = new double[10];
      for (int i = 0; i < xx.Length; ++i)
      {
        xx[i] = i;
        double arg = (xx[i] - 5) / 1.5;
        yy[i] = 17 * Math.Exp(-0.5 * arg * arg);
      }

      var initialGuess = new double[3] { 10, 5, 3 };
      var upperBound = new double?[3] { 16, null, null };

      var ff = new GaussAmplitude(1, -1);
      var model = new NonlinearObjectiveFunctionNonAllocating(ff.Evaluate, ff.EvaluateDerivative, 1);
      model.SetObserved(xx, yy, null);
      var fit = new LevenbergMarquardtMinimizerNonAllocating();
      var result = fit.FindMinimum(model, initialGuess, null, upperBound, null, null, CancellationToken.None, null);

      AssertEx.AreEqual(16, result.MinimizingPoint[0], 1E-16, 1E-16);
      AssertEx.GreaterOrEqual(1.9, result.ModelInfoAtMinimum.Value);
    }

    /// <summary>
    /// Create a Gaussian with height = 17 and w=1.5, but limit the fit height to 12 and the fit width to 2.
    /// </summary>
    [Fact]
    private void TestGaussLowerAndUpperBound()
    {
      var xx = new double[10];
      var yy = new double[10];
      for (int i = 0; i < xx.Length; ++i)
      {
        xx[i] = i;
        double arg = (xx[i] - 5) / 1.5;
        yy[i] = 17 * Math.Exp(-0.5 * arg * arg);
      }

      var initialGuess = new double[3] { 10, 5, 3 };
      var lowerBound = new double?[3] { null, null, 2 };
      var upperBound = new double?[3] { 12, null, null };

      var ff = new GaussAmplitude(1, -1);
      var model = new NonlinearObjectiveFunctionNonAllocating(ff.Evaluate, ff.EvaluateDerivative, 1);
      model.SetObserved(xx, yy, null);
      var fit = new LevenbergMarquardtMinimizerNonAllocating();
      var result = fit.FindMinimum(model, initialGuess, lowerBound, upperBound, null, null, CancellationToken.None, null);

      AssertEx.AreEqual(12, result.MinimizingPoint[0], 1E-16, 1E-16);
      AssertEx.AreEqual(2, result.MinimizingPoint[2], 1E-16, 1E-16);
      AssertEx.GreaterOrEqual(52, result.ModelInfoAtMinimum.Value);
    }

    /// <summary>
    /// Create a Gaussian with height = 17, pos=5 and w=1.5, but limit the fit height to 12, the pos to 5.125 and the fit width to 2.
    /// </summary>
    [Fact]
    private void TestGaussLowerAndUpperBoundAllFixed()
    {
      var xx = new double[10];
      var yy = new double[10];
      for (int i = 0; i < xx.Length; ++i)
      {
        xx[i] = i;
        double arg = (xx[i] - 5) / 1.5;
        yy[i] = 17 * Math.Exp(-0.5 * arg * arg);
      }

      var initialGuess = new double[3] { 10, 5.5, 3 };
      var lowerBound = new double?[3] { null, 5.125, 2 };
      var upperBound = new double?[3] { 12, null, null };

      var ff = new GaussAmplitude(1, -1);
      var model = new NonlinearObjectiveFunctionNonAllocating(ff.Evaluate, ff.EvaluateDerivative, 1);
      model.SetObserved(xx, yy, null);
      var fit = new LevenbergMarquardtMinimizerNonAllocating();
      var result = fit.FindMinimum(model, initialGuess, lowerBound, upperBound, null, null, CancellationToken.None, null);

      AssertEx.AreEqual(12, result.MinimizingPoint[0], 1E-16, 1E-16);
      AssertEx.AreEqual(5.125, result.MinimizingPoint[1], 1E-16, 1E-16);
      AssertEx.AreEqual(2, result.MinimizingPoint[2], 1E-16, 1E-16);
      Assert.True(result.ReasonForExit == ExitCondition.BoundTolerance);
      AssertEx.GreaterOrEqual(53, result.ModelInfoAtMinimum.Value);
    }

    /// <summary>
    /// FitFunction a² + b*x, but:
    /// in mode = 1: if a is greater than 1E-20 or b is greater than 1E-80, it returns NaN
    /// in mode = 2: if a is greater than 1E-20 or b is greater 0 it returns NaN
    /// </summary>
    /// <seealso cref="Altaxo.Calc.Regression.Nonlinear.IFitFunction" />
    private class FitFunc1 : IFitFunction
    {
      int _mode;

      public FitFunc1(int mode)
      {
        _mode = mode;
      }

      public int NumberOfIndependentVariables => 1;

      public int NumberOfDependentVariables => 1;

      public int NumberOfParameters => 2;

      public event EventHandler Changed;

      public double DefaultParameterValue(int i)
      {
        return 0;
      }

      public IVarianceScaling DefaultVarianceScaling(int i)
      {
        return new ConstantVarianceScaling();
      }

      public string DependentVariableName(int i)
      {
        throw new NotImplementedException();
      }

      public void Evaluate(double[] independent, double[] parameters, double[] FV)
      {
        throw new NotImplementedException();
      }

      public void Evaluate(IROMatrix<double> independent, IReadOnlyList<double> parameters, IVector<double> FV, IReadOnlyList<bool> dependentVariableChoice)
      {
        for (int i = 0; i < independent.RowCount; i++)
        {
          if (_mode == 1 && (Math.Abs(parameters[0]) > 1E-20 || Math.Abs(parameters[1]) > 1E-80))
            FV[i] = double.NaN;
          else if (_mode == 2 && (Math.Abs(parameters[0]) > 1E-20 || Math.Abs(parameters[1]) > 0))
            FV[i] = double.NaN;
          else
            FV[i] = parameters[0] * parameters[0] + parameters[1] * independent[i, 0];
        }
      }

      public (IReadOnlyList<double?> LowerBounds, IReadOnlyList<double?> UpperBounds) GetParameterBoundariesHardLimit()
      {
        return (null, null);
      }

      public (IReadOnlyList<double?> LowerBounds, IReadOnlyList<double?> UpperBounds) GetParameterBoundariesSoftLimit()
      {
        return (null, null);
      }

      public string IndependentVariableName(int i)
      {
        throw new NotImplementedException();
      }

      public string ParameterName(int i)
      {
        throw new NotImplementedException();
      }
    }

    private class FitFunc2 : FitFunc1, IFitFunctionWithDerivative
    {
      public FitFunc2(int mode) : base(mode) { }

      public void EvaluateDerivative(IROMatrix<double> independent, IReadOnlyList<double> parameters, IReadOnlyList<bool> isFixed, IMatrix<double> DF, IReadOnlyList<bool> dependentVariableChoice)
      {
        for (int i = 0; i < independent.RowCount; i++)
        {
          DF[i, 0] = 2 * parameters[0];
          DF[i, 1] = independent[i, 0];
        }
      }
    }

    [Fact]
    private void TestFitFunctionWithoutDerivative_SmallParameters()
    {
      var a = 1E-50;
      var b = 1E-100;

      var xx = new double[10];
      var yy = new double[10];
      for (int i = 0; i < xx.Length; ++i)
      {
        xx[i] = i;
        yy[i] = a * a + b * xx[i];
      }

      var initialGuess = new double[2] { a / 10d, 0 };

      var ff = new FitFunc1(1);
      var model = new NonlinearObjectiveFunctionNonAllocating(ff.Evaluate, null, 1);
      model.SetObserved(xx, yy, null);
      var fit = new LevenbergMarquardtMinimizerNonAllocating();
      var result = fit.FindMinimum(model, initialGuess, null, null, null, null, CancellationToken.None, null);

      AssertEx.AreEqual(a, result.MinimizingPoint[0], 0, 1E-8);
      AssertEx.AreEqual(b, result.MinimizingPoint[1], 0, 1E-8);
    }

    [Fact]
    private void TestFitFunctionWithoutDerivative_SmallParametersNegative()
    {
      var a = 1E-50;
      var b = -1E-100;

      var xx = new double[10];
      var yy = new double[10];
      for (int i = 0; i < xx.Length; ++i)
      {
        xx[i] = i;
        yy[i] = a * a + b * xx[i];
      }

      var initialGuess = new double[2] { a / 10d, 0 };

      var ff = new FitFunc1(1);
      var model = new NonlinearObjectiveFunctionNonAllocating(ff.Evaluate, null, 1);
      model.SetObserved(xx, yy, null);
      var fit = new LevenbergMarquardtMinimizerNonAllocating();
      var result = fit.FindMinimum(model, initialGuess, null, null, null, null, CancellationToken.None, null);

      AssertEx.AreEqual(a, result.MinimizingPoint[0], 0, 1E-8);
      AssertEx.AreEqual(b, result.MinimizingPoint[1], 0, 1E-8);
    }
  }
}
