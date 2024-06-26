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
using System.Text;

namespace Altaxo.Calc.Probability
{
  /// <summary>
  /// Represents the base of all StableDistributions classes in different parametrizations.
  /// </summary>
  /// <remarks>
  /// </remarks>
  public class StableDistributionBase : ContinuousDistribution
  {
    private ExponentialDistribution _expDist = new ExponentialDistribution();
    private ContinuousUniformDistribution _contDist = new ContinuousUniformDistribution();

    /// <summary>Helper variables used for generating the random values.</summary>
    protected double _gen_t, _gen_B, _gen_S, _gen_Scale;

    /// <summary>The highest number x that, when taken Exp(-x), gives a result greater than zero.</summary>
    protected static readonly double MinusLogTiny = -Math.Log(double.Epsilon);

    /// <summary>Default precision used for the integrations.</summary>
    protected static readonly double DefaultPrecision = Math.Sqrt(DoubleConstants.DBL_EPSILON);

    #region Abstract Implementation of ContinuousDistribution

    /// <summary>Initializes a new instance of the <see cref="StableDistributionBase"/> class.</summary>
    /// <param name="generator">The random number generator used.</param>
    protected StableDistributionBase(Generator generator)
      : base(generator)
    {
      _expDist = new ExponentialDistribution(generator);
      _contDist = new ContinuousUniformDistribution(generator);
    }

    /// <summary>Gets the minimum possible value of distributed random numbers.</summary>
    public override double Minimum
    {
      get { throw new Exception("The method or operation is not implemented."); }
    }

    /// <summary>Gets the maximum possible value of distributed random numbers.</summary>
    public override double Maximum
    {
      get { throw new Exception("The method or operation is not implemented."); }
    }

    /// <summary>Gets the mean of distributed random numbers.</summary>
    public override double Mean
    {
      get { throw new Exception("The method or operation is not implemented."); }
    }

    /// <summary>Gets the median of distributed random numbers.</summary>
    public override double Median
    {
      get { throw new Exception("The method or operation is not implemented."); }
    }

    /// <summary>Gets the variance of distributed random numbers.</summary>
    public override double Variance
    {
      get { throw new Exception("The method or operation is not implemented."); }
    }

    /// <summary>Gets the mode of distributed random numbers.</summary>
    public override double[] Mode
    {
      get { throw new Exception("The method or operation is not implemented."); }
    }

    /// <summary>Returns a distributed floating point random number.</summary>
    /// <returns>A distributed double-precision floating point number.</returns>
    public override double NextDouble()
    {
      throw new Exception("The method or operation is not implemented.");
    }

    #endregion Abstract Implementation of ContinuousDistribution

    #region Subdivisions

    /// <summary>
    /// Returns the number of logarithmically spaced subdivisions to span a certain range.
    /// This mean that if span==smallestDivision, the function will return 1.
    /// If span = smallestdivision + smallestdivision*span + smallestdivision*span^2, the function will return 3.
    /// </summary>
    /// <param name="span">The range to space logarithmically.</param>
    /// <param name="smallestDivision">The smallest division.</param>
    /// <param name="scale">Value &gt; 1 which is the ratio of the value of the next subdivision to the current subdivision.</param>
    /// <returns></returns>
    private static double GetNumberOfLogarithmicDivisions(double span, double smallestDivision, double scale)
    {
      double ges = span / smallestDivision;
      return Math.Log(ges * (scale - 1) + scale) / Math.Log(scale) - 1;
    }

    private static double GetScaleOfLogarithmicSubdivision(double span, double smallestDivision, double n)
    {
      double scale = double.MaxValue;
      double OneByN = 1.0 / n;
      double ges = span / smallestDivision;
      if (ges + 1 < double.MaxValue)
      {
        // first guess
        scale = Math.Pow(ges + 1 - ges / scale, OneByN);
        scale = Math.Pow(ges + 1 - ges / scale, OneByN);
        scale = Math.Pow(ges + 1 - ges / scale, OneByN);
      }
      else
      {
        double ln_ges = Math.Log(span) - Math.Log(smallestDivision);
        scale = Math.Exp((ln_ges + Math.Log(1 - 1 / scale)) * OneByN);
        scale = Math.Exp((ln_ges + Math.Log(1 - 1 / scale)) * OneByN);
        scale = Math.Exp((ln_ges + Math.Log(1 - 1 / scale)) * OneByN);
      }
      return scale;
    }

    #endregion Subdivisions

    #region Function value finding

    /// <summary>
    /// Finds the x where func(x)==1±-1E-5 between  x0 &lt; x &lt; x1 for a monoton increasing function func.
    /// </summary>
    /// <param name="func">Function for which to find the argument x where func(x)=1. The function has to be increasing with x.</param>
    /// <param name="x0">Lower bound of the search range.</param>
    /// <param name="x1">Upper bound of the search range.</param>
    /// <returns>The value x at which func(x)=1±-1E-5.</returns>
    protected static double FindIncreasingYEqualToOne(Func<double, double> func, double x0, double x1)
    {
      const double ConsideredAsZero = 2 * double.Epsilon;
      double low = x0;
      double high = x1;
      double xm = 0;
      double xmprev = 0;
      double y;
      for (; ; )
      {
        xm = 0.5 * (low + high);
        if (xm == xmprev)
          break;
        xmprev = xm;

        y = func(xm);
        if (Math.Abs(y - 1) < 1E-5)
          break;
        else if (y < 1)
          low = xm;
        else
          high = xm;
      }

      return (x0 == 0 && xm <= ConsideredAsZero) ? 0 : xm;
    }

    /// <summary>
    /// Finds the x where func(x)==1±-1E-5 between  x0 &lt; x &lt; x1 for a monoton decreasing function func.
    /// </summary>
    /// <param name="func">Function for which to find the argument x where func(x)=1. The function has to be decreasing with x.</param>
    /// <param name="x0">Lower bound of the search range.</param>
    /// <param name="x1">Upper bound of the search range.</param>
    /// <returns>The value x at which func(x)=1±-1E-5.</returns>
    protected static double FindDecreasingYEqualToOne(Func<double, double> func, double x0, double x1)
    {
      double low = x0;
      double high = x1;
      double xmprev = 0;
      double xm = 0;
      double y;
      for (; ; )
      {
        xm = 0.5 * (low + high);
        if (xm == xmprev)
          break;
        xmprev = xm;

        y = func(xm);
        if (Math.Abs(y - 1) < 1E-5)
          break;
        else if (y < 1)
          high = xm;
        else
          low = xm;
      }
      return xm;
    }

    /// <summary>
    /// Finds the x where func(x)==<paramref name="ysearch"/>±<paramref name="tol"/> between  x0 &lt; x &lt; x1 for a monotonic increasing function func.
    /// </summary>
    /// <param name="func">Function for which to find the argument x where func(x)=1. The function has to be increasing with x.</param>
    /// <param name="x0">Lower bound of the search range.</param>
    /// <param name="x1">Upper bound of the search range.</param>
    /// <param name="ysearch">The function value to search.</param>
    /// <param name="tol">Tolerable deviation of the searched function value.</param>
    /// <param name="y">On return, this is the function value that corresponds to the returned x value.</param>
    /// <returns>The value x at which func(x)=<paramref name="ysearch"/>±<paramref name="tol"/>.</returns>
    protected static double FindIncreasingYEqualTo(Func<double, double> func, double x0, double x1, double ysearch, double tol, out double y)
    {
      const double ConsideredAsZero = 2 * double.Epsilon;

      double low = x0;
      double high = x1;
      double xm = 0;
      double xmprev = 0;
      y = double.NaN;

      for (; ; )
      {
        xm = 0.5 * (low + high);
        if (xm == xmprev)
          break;
        xmprev = xm;

        y = func(xm);
        if (Math.Abs(y - ysearch) < tol)
          break;
        else if (y < ysearch)
          low = xm;
        else
          high = xm;
      }
      return (x0 == 0 && xm <= ConsideredAsZero) ? 0 : xm;
    }

    /// <summary>
    /// Finds the x where func(x)==<paramref name="ysearch"/>±<paramref name="tol"/> between  x0 &lt; x &lt; x1 for a monotonic decreasing function func.
    /// </summary>
    /// <param name="func">Function for which to find the argument x where func(x)=1. The function has to be decreasing with x.</param>
    /// <param name="x0">Lower bound of the search range.</param>
    /// <param name="x1">Upper bound of the search range.</param>
    /// <param name="ysearch">The function value to search.</param>
    /// <param name="tol">Tolerable deviation of the searched function value.</param>
    /// <param name="y">On return, this is the function value that corresponds to the returned x value.</param>
    /// <returns>The value x at which func(x)=<paramref name="ysearch"/>±<paramref name="tol"/>.</returns>
    protected static double FindDecreasingYEqualTo(Func<double, double> func, double x0, double x1, double ysearch, double tol, out double y)
    {
      double low = x0;
      double high = x1;
      double xmprev = 0;
      double xm = 0;
      y = double.NaN;
      for (; ; )
      {
        xm = 0.5 * (low + high);
        if (xm == xmprev)
          break;
        xmprev = xm;

        y = func(xm);
        if (Math.Abs(y - ysearch) < tol)
          break;
        else if (y < ysearch)
          high = xm;
        else
          low = xm;
      }
      return xm;
    }

    /// <summary>
    /// Find the bracket of a root, i.e. values for x0 and x1, so that ysearch is inbetween f(x0) and f(x1). This is done be extension of the interval [x0,x1] either
    /// to the left or the right side or both. <c>True</c> is returned when a bracket was found.
    /// </summary>
    /// <param name="func">The function used to evaluate the function values.</param>
    /// <param name="ysearch">The value to find.</param>
    /// <param name="x0">Starting parameter of x0, at the end the lower value of the bracket interval.</param>
    /// <param name="x1">Starting parameter of x1, at the end the upper value of the bracket interval.</param>
    /// <returns>True if a bracket interval was found. If such an interval could not be found, the return value is false.</returns>
    public static bool BracketRootByExtensionOnly(Func<double, double> func, double ysearch, ref double x0, ref double x1)
    {
      if (!(x0 != x1))
        return false;

      if (x0 > x1) // make sure that x0<x1
      {
        double xh = x0;
        x0 = x1;
        x1 = xh;
      }

      double y0 = func(x0);
      if (double.IsNaN(y0))
        return false;

      double y1 = func(x1);
      if (double.IsNaN(y1))
        return false;

      if (y0 == ysearch || y1 == ysearch)
        return true;

      for (; ; )
      {
        if (y0 < y1) // increasing
        {
          if (y0 <= ysearch && ysearch <= y1)
          {
            return true;
          }
          else if (y0 > ysearch)
          {
            // extend the interval in the direction of x0
            double oldx0 = x0;
            x0 -= x1 - x0;
            y0 = func(x0);
            if (!(x0 != oldx0) || double.IsNaN(y0))
              return false;
          }
          else if (y1 < ysearch)
          {
            // extend the interval in the direction of x1
            double oldx1 = x1;
            x1 += x1 - x0;
            y1 = func(x1);
            if (!(x1 != oldx1) || double.IsNaN(y1))
              return false;
          }
          else
          {
            return false; // something else happend, for instance some of the value is infinite
          }
        }
        else if (y0 > y1)
        {
          if (y1 <= ysearch && ysearch <= y0)
          {
            return true;
          }
          else if (y0 < ysearch)
          {
            // extend the interval in the direction of x0
            double oldx0 = x0;
            x0 -= x1 - x0;
            y0 = func(x0);
            if (!(x0 != oldx0) || double.IsNaN(y0))
              return false;
          }
          else if (y1 > ysearch)
          {
            // extend the interval in the direction of x1
            double oldx1 = x1;
            x1 += x1 - x0;
            y1 = func(x1);
            if (!(x1 != oldx1) || double.IsNaN(y1))
              return false;
          }
          else
          {
            return false; // something else happend, for instance some of the value is infinite
          }
        }
        else // both values are equal
        {
          // extend the interval in both directions
          double oldx0 = x0;
          double oldx1 = x1;

          x0 -= oldx1 - oldx0;
          x1 += oldx1 - oldx0;
          y0 = func(x0);
          y1 = func(x1);
          if (!(x0 != oldx0) || !(x1 != oldx1))
            return false;
          if (double.IsNaN(y0) || double.IsNaN(y1))
            return false;
        }
      }
    }

    #endregion Function value finding

    #region Parameter conversion between different parametrizations

    protected static double GammaFromAlphaBetaTanPiA2(double alpha, double beta, double abe, double tan_pi_alpha_2, out double aga)
    {
      // The orginal formula is gamma=(2/Pi)*ArcTan(-beta*Tan(alpha Pi/2))
      double gamma;
      if (0 == beta)
      {
        gamma = 0;
        aga = StableDistributionFeller.GetAgaFromAlphaGamma(alpha, gamma);
      }
      else if (abe == 0) // Avoid roundoff errors when Abs(beta)==1
      {
        if (beta >= 0)
          gamma = Math.IEEERemainder(-alpha, 2);
        else
          gamma = Math.IEEERemainder(alpha, 2);

        aga = StableDistributionFeller.GetAgaFromAlphaGamma(alpha, gamma);
      }
      else if (Math.Abs(beta) < 0.5 && Math.Abs(beta * tan_pi_alpha_2) < 1)
      {
        double arg = -beta * tan_pi_alpha_2;
        gamma = (2 / Math.PI) * Math.Atan(arg);
        aga = StableDistributionFeller.GetAgaFromAlphaGamma(alpha, gamma);
      }
      else
      {
        // here we used arctan((x-y)/(1+x y) = arctan(x)-arctan(y) and set y to Tan(alpha Pi/2)
        double diff = 2 * Math.Atan(abe * SinXPiBy2(2 * alpha) / (2 - abe * (1 - CosXPiBy2(2 * alpha)))) / (Math.PI);
        if (alpha <= 1)
        {
          aga = diff / alpha;
          gamma = StableDistributionFeller.GetGammaFromAlphaAga(alpha, aga, beta > 0);
        }
        else
        {
          aga = -diff;
          gamma = StableDistributionFeller.GetGammaFromAlphaAga(alpha, aga, beta < 0);
        }
      }

      return gamma;
    }

    public static double BetaFromAlphaGammaAga(double alpha, double gamma, double aga, out double abe)
    {
      // The original formula is:
      // beta = -Tan(gamma Pi/2)/Tan(alpha Pi/2)
      double beta = -TanXPiBy2(gamma) / TanXPiBy2(alpha);
      if (alpha <= 1)
      {
        // we use the formula Tan(a)-Tan(b)=Sin(a-b)/(Cos(a)*Cos(b))
        // with gamma = alpha(1-aga), so a===alpha*Pi/2 and b===alpha*aga*Pi/2;
        if (aga > 0.5)
        {
          beta = -TanXPiBy2(gamma) / TanXPiBy2(alpha);
          abe = beta >= 0 ? 1 - beta : 1 + beta;
        }
        else if (alpha < 0.5)
        {
          abe = SinXPiBy2(alpha * aga) / (SinXPiBy2(alpha) * CosXPiBy2(gamma));
          beta = gamma >= 0 ? -1 + abe : 1 - abe;
        }
        else
        {
          abe = SinXPiBy2(alpha * aga) / (SinXPiBy2(alpha) * SinXPiBy2((1 - alpha) + alpha * aga));
          beta = gamma >= 0 ? -1 + abe : 1 - abe;
        }
      }
      else
      {
        if (alpha < 1.5)
        {
          abe = SinXPiBy2(aga) / (SinXPiBy2(alpha) * SinXPiBy2((alpha - 1) + aga));
          beta = gamma >= 0 ? 1 - abe : -1 + abe;
        }
        else
        {
          abe = SinXPiBy2(aga) / (SinXPiBy2(alpha) * CosXPiBy2(gamma));
          beta = gamma >= 0 ? 1 - abe : -1 + abe;
        }
      }

      return beta;
    }

    /// <summary>Calculates the sine of <paramref name="x"/> times Pi/2 with increased accuracy.</summary>
    /// <param name="x">Argument x.</param>
    /// <returns>Sin(x*Pi/2) with high accuracy.</returns>
    public static double SinXPiBy2(double x)
    {
      const double PiBy2 = Math.PI / 2;

      double first = Math.IEEERemainder(x, 4);
      double rem = Math.IEEERemainder(first, 1);
      int fix = (int)(first - rem);
      switch (fix)
      {
        case -1:
          return -Math.Cos(PiBy2 * rem);

        case 0:
          return Math.Sin(PiBy2 * rem);

        case 1:
          return Math.Cos(PiBy2 * rem);

        default:
          return -Math.Sin(PiBy2 * rem);
      }
    }

    /// <summary>Calculates the cosine of <paramref name="x"/> times Pi/2 with increased accuracy.</summary>
    /// <param name="x">Argument x.</param>
    /// <returns>Cos(x*Pi/2) with high accuracy.</returns>
    public static double CosXPiBy2(double x)
    {
      const double PiBy2 = Math.PI / 2;

      double first = Math.IEEERemainder(x, 4);
      double rem = Math.IEEERemainder(first, 1);
      int fix = (int)(first - rem);
      switch (fix)
      {
        case -1:
          return Math.Sin(PiBy2 * rem);

        case 0:
          return Math.Cos(PiBy2 * rem);

        case 1:
          return -Math.Sin(PiBy2 * rem);

        default:
          return -Math.Cos(PiBy2 * rem);
      }
    }

    /// <summary>Calculates the tangens of <paramref name="x"/> times Pi/2 with increased accuracy.</summary>
    /// <param name="x">Argument x.</param>
    /// <returns>Tan(x*Pi/2) with high accuracy.</returns>
    protected static double TanXPiBy2(double x)
    {
      const double PiBy2 = Math.PI / 2;

      double first = Math.IEEERemainder(x, 2);
      double rem = Math.IEEERemainder(first, 1);
      int fix = (int)(first - rem);
      switch (fix)
      {
        case 0:
          return Math.Tan(PiBy2 * rem);

        default:
          return -1 / Math.Tan(PiBy2 * rem);
      }
    }

    /// <summary>Calculates the tangent of gamma time Pi/2 with high accuracy. If |gamma| is less than or equal to 0.5, the tangent is calculated directly
    /// by calling <see cref="TanXPiBy2"/> with argument gamma. Else (if |gamma| is greater than 0.5, it is calculated from <paramref name="alpha"/> and <paramref name="aga"/>,
    /// which ensures a higher accuracy.</summary>
    /// <param name="alpha">Characteristic exponent of the stable distribution.</param>
    /// <param name="gamma">Parameter gamma of the stable distribution in Feller's parametrization.</param>
    /// <param name="aga">Specifies the parameter <paramref name="gamma"/> with increased precision when |gamma|≈alpha. For an explanation how aga is defined, see <see cref="M:StableDistributionFeller.GetAgaFromAlphaGamma"/>.</param>
    /// <returns>Tangent of gamma time Pi/2 with high accuracy.</returns>
    protected static double TanGammaPiBy2(double alpha, double gamma, double aga)
    {
      if (Math.Abs(gamma) <= 0.5)
      {
        return TanXPiBy2(gamma);
      }
      else
      {
        if (alpha <= 1)
        {
          if (gamma >= 0)
            return 1 / TanXPiBy2((1 - alpha) + aga * alpha);
          else
            return -1 / TanXPiBy2((1 - alpha) + aga * alpha);
        }
        else
        {
          if (gamma >= 0)
            return 1 / TanXPiBy2((alpha - 1) + aga);
          else
            return -1 / TanXPiBy2((alpha - 1) + aga);
        }
      }
    }

    /// <summary>Calculates the cosine of gamma time Pi/2 with high accuracy. If |gamma| is less than or equal to 0.5, the cosine is calculated directly
    /// by calling <see cref="CosXPiBy2"/> with argument gamma. Else (if |gamma| is greater than 0.5, it is calculated from <paramref name="alpha"/> and <paramref name="aga"/>,
    /// which ensures a higher accuracy.</summary>
    /// <param name="alpha">Characteristic exponent of the stable distribution.</param>
    /// <param name="gamma">Parameter gamma of the stable distribution in Feller's parametrization.</param>
    /// <param name="aga">Specifies the parameter <paramref name="gamma"/> with increased precision when |gamma|≈alpha. For an explanation how aga is defined, see <see cref="StableDistributionFeller.GetAgaFromAlphaGamma"/>.</param>
    /// <returns>Cosine of gamma time Pi/2 with high accuracy.</returns>
    protected static double CosGammaPiBy2(double alpha, double gamma, double aga)
    {
      if (Math.Abs(gamma) <= 0.5)
      {
        return CosXPiBy2(gamma);
      }
      else
      {
        if (alpha <= 1)
        {
          return SinXPiBy2((1 - alpha) + aga * alpha);
        }
        else
        {
          return SinXPiBy2((alpha - 1) + aga);
        }
      }
    }

    /// <summary>Calculates (1+x²)^pow with increased accuracy, for both small and big values of |x|.</summary>
    /// <param name="x">Argument x.</param>
    /// <param name="pow">Power.</param>
    /// <returns>The value (1+x²)^pow with increased accuary.</returns>
    public static double PowerOfOnePlusXSquared(double x, double pow)
    {
      double ax = Math.Abs(x);
      if (ax < 1)
      {
        return Math.Exp(pow * RMath.Log1p(x * x));
      }
      else
      {
        return Math.Pow(ax, 2 * pow) * Math.Exp(pow * RMath.Log1p(1 / (x * x)));
      }
    }

    /// <summary>
    /// Upper bound for x, at which the relative error of the series expansion of (1-Exp(x)) with 9 terms exceeds the accuracy of double values.
    /// </summary>
    private static readonly double OneMinusExp_SmallBound = Math.Pow(DoubleConstants.DBL_EPSILON * 3628800, 1 / 9.0);

    /// <summary>
    /// Calculates 1-Exp(x) with more accuracy around x=0.
    /// </summary>
    /// <param name="x">Function argument</param>
    /// <returns>1-Exp(x)</returns>
    /// <remarks>
    /// <para>If |x| is smaller than <see cref="OneMinusExp_SmallBound"/>, the series expansion of (1-Exp(x)) is used for calculation.</para>
    /// <para>If |x| is greater than or equal to <see cref="OneMinusExp_SmallBound"/>, the standard way of evaluation is used.</para>
    /// </remarks>
    public static double OneMinusExp(double x)
    {
      const double A1 = 1;
      const double A2 = 1 / 2.0;
      const double A3 = 1 / 6.0;
      const double A4 = 1 / 24.0;
      const double A5 = 1 / 120.0;
      const double A6 = 1 / 720.0;
      const double A7 = 1 / 5040.0;
      const double A8 = 1 / 40320.0;
      const double A9 = 1 / 362880.0;

      double ax = Math.Abs(x);
      if (ax < OneMinusExp_SmallBound)
      {
        if (ax < DoubleConstants.DBL_EPSILON)
          return -x;
        else
          return -(((((((((A9 * x) + A8) * x + A7) * x + A6) * x + A5) * x + A4) * x + A3) * x + A2) * x + A1) * x;
      }
      else
      {
        return 1 - Math.Exp(x);
      }
    }

    /// <summary>Calculates the 'alternative beta' value abe from the skewness parameter beta. The value abe helps to specifiy beta with enhanced accuracy especially when |beta| is close to 1.
    /// Of course, this helper function is only intended to give a correct abe value for a given beta. But in order to specify beta with enhanced accuracy around |beta|=1, you should
    /// specify abe first, and then calculate beta from abe, which can be done with <see cref="GetBetaFromAbe"/>.</summary>
    /// <param name="beta">The beta value.</param>
    /// <returns>If beta is &gt;= 0, the return value is (1-beta). Else, if beta is &lt;0, the return value is (1+beta).</returns>
    public static double GetAbeFromBeta(double beta)
    {
      return beta >= 0 ? 1 - beta : 1 + beta;
    }

    /// <summary>Gets the skewness parameter beta (range: [-1,1]) from the 'alternative beta' value <paramref name="abe"/>.</summary>
    /// <param name="abe">'Alternative beta' value. It is defined as (1-beta) for beta&gt;=0, and as (1+beta) for beta&lt;0. Thus, abe is a value in the range [0,1].</param>
    /// <param name="isBetaNegative">Specifies the sign of the original beta value. If false, the original beta is a nonnegative value. Of true, the original beta is negative.</param>
    /// <returns>The skewness parameter beta (range: [-1,1]) as calculated from the 'alternative beta' value <paramref name="abe"/>.</returns>
    public static double GetBetaFromAbe(double abe, bool isBetaNegative)
    {
      return isBetaNegative ? -1 + abe : 1 - abe;
    }

    public static void ParameterConversionS0ToFeller(double alpha, double beta, double abe, double sigma0, double mu0, out double gamma, out double aga, out double sigmaf, out double muf)
    {
      if (alpha != 1)
      {
        double tan_pi_alpha_2 = TanXPiBy2(alpha);
        gamma = GammaFromAlphaBetaTanPiA2(alpha, beta, abe, tan_pi_alpha_2, out aga);
        sigmaf = sigma0 * PowerOfOnePlusXSquared(beta * tan_pi_alpha_2, 0.5 / alpha);
        muf = mu0 - sigma0 * beta * tan_pi_alpha_2;
      }
      else
      {
        if (beta == 0)
        {
          gamma = 0;
          sigmaf = sigma0;
          muf = mu0;
          aga = 1;
        }
        else
        {
          throw new ArgumentException("Alpha is 1 and beta!=0, thus the conversion is undefined");
        }
      }
    }

    public static void ParameterConversionS1ToFeller(double alpha, double beta, double abe, double sigma1, double mu1, out double gamma, out double aga, out double sigmaf, out double muf)
    {
      if (alpha != 1)
      {
        double tan_pi_alpha_2 = TanXPiBy2(alpha);
        gamma = GammaFromAlphaBetaTanPiA2(alpha, beta, abe, tan_pi_alpha_2, out aga);
        sigmaf = sigma1 * PowerOfOnePlusXSquared(beta * tan_pi_alpha_2, 0.5 / alpha);
        muf = mu1;
      }
      else
      {
        if (beta == 0)
        {
          gamma = 0;
          aga = 1;
          sigmaf = sigma1;
          muf = mu1;
        }
        else
        {
          throw new ArgumentException("Alpha is 1 and beta!=0, thus the conversion is undefined");
        }
      }
    }

    public static void ParameterConversionFellerToS0(double alpha, double gamma, double aga, double sigmaf, double muf, out double beta, out double abe, out double sigma0, out double mu0)
    {
      if (alpha != 1 && alpha != 2)
      {
        double tan_pi_alpha_2 = TanXPiBy2(alpha);
        beta = BetaFromAlphaGammaAga(alpha, gamma, aga, out abe);
        sigma0 = sigmaf * PowerOfOnePlusXSquared(beta * tan_pi_alpha_2, -0.5 / alpha);
        mu0 = muf + sigma0 * beta * tan_pi_alpha_2;
      }
      else
      {
        if (gamma == 0)
        {
          beta = 0;
          abe = 1;
          sigma0 = sigmaf;
          mu0 = muf;
        }
        else
        {
          throw new ArgumentException("Alpha is 1 or 2 and gamma!=0, thus the conversion is undefined");
        }
      }
    }

    public static void ParameterConversionFellerToS1(double alpha, double gamma, double aga, double sigmaf, double muf, out double beta, out double abe, out double sigma1, out double mu1)
    {
      if (alpha != 1 && alpha != 2)
      {
        double tan_pi_alpha_2 = TanXPiBy2(alpha);
        beta = BetaFromAlphaGammaAga(alpha, gamma, aga, out abe);
        sigma1 = sigmaf * PowerOfOnePlusXSquared(beta * tan_pi_alpha_2, -0.5 / alpha);
        mu1 = muf;
      }
      else
      {
        if (gamma == 0)
        {
          beta = 0;
          abe = 1;
          sigma1 = sigmaf;
          mu1 = muf;
        }
        else
        {
          throw new ArgumentException("Alpha is 1 or 2 and gamma!=0, thus the conversion is undefined");
        }
      }
    }

    public static void ParameterConversionS0ToS1(double alpha, double beta, double sigma0, double mu0, out double mu1)
    {
      if (alpha != 1)
      {
        mu1 = mu0 - sigma0 * beta * TanXPiBy2(alpha);
      }
      else
      {
        mu1 = mu0 - sigma0 * beta * 2 * Math.Log(sigma0) / Math.PI;
      }
    }

    public static void ParameterConversionS1ToS0(double alpha, double beta, double sigma1, double mu1, out double mu0)
    {
      if (alpha != 1)
      {
        mu0 = mu1 + sigma1 * beta * TanXPiBy2(alpha);
      }
      else
      {
        mu0 = mu1 + sigma1 * beta * 2 * Math.Log(sigma1) / Math.PI;
      }
    }

    #endregion Parameter conversion between different parametrizations

    #region Generation

    protected double GenerateSymmetricCase(double alpha)
    {
      double u, v, t, s;

      u = Math.PI * (_contDist.NextDouble() - 0.5);

      if (alpha == 1)               /* cauchy case */
      {
        t = Math.Tan(u);
        return t;
      }

      do
      {
        v = _expDist.NextDouble();
      }
      while (v == 0);

      if (alpha == 2)             /* gaussian case */
      {
        t = 2 * Math.Sin(u) * Math.Sqrt(v);
        return t;
      }

      /* general case */

      t = Math.Sin(alpha * u) / Math.Pow(Math.Cos(u), 1 / alpha);
      s = Math.Pow(Math.Cos((1 - alpha) * u) / v, (1 - alpha) / alpha);

      return t * s;
    }

    /// <summary>
    /// Generates random variates in S1 Parametrization
    /// </summary>
    /// <param name="alpha"></param>
    /// <param name="beta"></param>
    /// <param name="c"></param>
    /// <returns></returns>
    protected double GenerateAsymmetricCaseS1(double alpha, double beta, double c)
    {
      double V, W, X;
      const double M_PI = Math.PI;
      const double M_PI_2 = Math.PI / 2;

      V = M_PI * (_contDist.NextDouble() - 0.5);

      do
      {
        W = _expDist.NextDouble();
      }
      while (W == 0);

      if (alpha == 1)
      {
        X = ((M_PI_2 + beta * V) * Math.Tan(V) -
             beta * Math.Log(M_PI_2 * W * Math.Cos(V) / (M_PI_2 + beta * V))) / M_PI_2;
        // org return c * (X + beta * Math.Log(c) / M_PI_2);
        return c * X;
      }
      else
      {
        double t = beta * Math.Tan(M_PI_2 * alpha);
        double B = Math.Atan(t) / alpha;
        double S = Math.Pow(1 + t * t, 1 / (2 * alpha));

        X = S * Math.Sin(alpha * (V + B)) / Math.Pow(Math.Cos(V), 1 / alpha)
          * Math.Pow(Math.Cos(V - alpha * (V + B)) / W, (1 - alpha) / alpha);
        return c * X;
      }
    }

    protected double GenerateAsymmetricCaseS1_AEq1(double alpha, double beta)
    {
      double V, W, X;
      const double M_PI = Math.PI;
      const double M_PI_2 = Math.PI / 2;

      V = M_PI * (_contDist.NextDouble() - 0.5);

      do
      {
        W = _expDist.NextDouble();
      }
      while (W == 0);

      X = ((M_PI_2 + beta * V) * Math.Tan(V) - beta * Math.Log(M_PI_2 * W * Math.Cos(V) / (M_PI_2 + beta * V))) / M_PI_2;
      // org return c * (X + beta * Math.Log(c) / M_PI_2);
      return X;
    }

    protected double GenerateAsymmetricCaseS1_ANe1(double alpha, double t, double B, double S, double c)
    {
      double V, W, X;
      const double M_PI = Math.PI;

      V = M_PI * (_contDist.NextDouble() - 0.5);

      do
      {
        W = _expDist.NextDouble();
      }
      while (W == 0);
      //  double t = beta * Math.Tan(M_PI_2 * alpha);
      //  double B = Math.Atan(t) / alpha;
      //  double S = Math.Pow(1 + t * t, 1 / (2 * alpha));

      X = S * Math.Sin(alpha * (V + B)) / Math.Pow(Math.Cos(V), 1 / alpha)
        * Math.Pow(Math.Cos(V - alpha * (V + B)) / W, (1 - alpha) / alpha);
      return c * X;
    }

    #endregion Generation

    #region Classes for integration (alpha<1)

    #region Gamma negative

    #region Function increasing

    /// <summary>
    /// Helper class for calculation used if alpha&lt;1, gamma&lt;0 and for increasing integration direction.
    /// </summary>
    public class Alt1GnI
    {
      protected double factorp;
      protected double facdiv;
      protected double alpha;
      protected double dev;
      protected double logPdfPrefactor;

      protected double _x0;

      protected Func<double, double> pdfCore;
      protected Func<double, double> pdfFunc;

      public Alt1GnI(double factorp, double facdiv, double logPdfPrefactor, double alpha, double dev)
      {
        this.factorp = factorp;
        this.facdiv = facdiv;
        this.logPdfPrefactor = logPdfPrefactor;
        this.alpha = alpha;
        this.dev = dev;
        pdfCore = new Func<double, double>(PDFCore);
        pdfFunc = new Func<double, double>(PDFFunc);
      }

      public void Initialize()
      {
        pdfCore = new Func<double, double>(PDFCore);
        pdfFunc = new Func<double, double>(PDFFunc);
      }

      public double PDFCore(double thetas)
      {
        double r1;
        double r2;
        if (dev == 0 && thetas < 1E-10)
        {
          r1 = Math.Pow(alpha / factorp, alpha / (1 - alpha));
          r2 = (1 - alpha) / facdiv;
        }
        else
        {
          r1 = Math.Pow(Math.Sin(alpha * thetas) / (factorp * Math.Sin(dev + thetas)), alpha / (1 - alpha));
          r2 = Math.Sin(dev + thetas * (1 - alpha)) / (facdiv * Math.Sin(dev + thetas));
        }
        double result = r1 * r2;
        if (!(result >= 0))
          result = double.MaxValue;

        //System.Diagnostics.Debug.WriteLine(string.Format("CorAlt1GnI theta={0}, result={1}", thetas, result));
        return result;
      }

      public double PDFCoreDerivative(double thetas)
      {
        if (thetas == 0)
          return dev == 0 ? 0 : double.PositiveInfinity;

        double r1, r1s;

        r1 = Math.Pow(Math.Sin(alpha * thetas) / (factorp * Math.Sin(dev + thetas)), alpha / (1 - alpha));
        r1s = r1 * alpha / (1 - alpha);
        r1s *= alpha / Math.Tan(alpha * thetas) - 1 / Math.Tan(dev + thetas);

        double r2, r2s;
        r2 = Math.Sin(dev + thetas * (1 - alpha)) / (facdiv * Math.Sin(dev + thetas));
        r2s = r2;
        r2s *= (1 - alpha) / Math.Tan(dev + thetas * (1 - alpha)) - 1 / Math.Tan(dev + thetas);

        return r1 * r2s + r1s * r2;
      }

      public double PDFFunc(double x)
      {
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(-f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double PDFFuncLogInt(double z)
      {
        double x = Math.Exp(z);
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(z - f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double PDFFuncLogIntToLeft(double z)
      {
        double x = _x0 - Math.Exp(z);
        if (x < 0)
          x = 0;

        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(z - f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double PDFFuncLogIntToRight(double z)
      {
        double x = _x0 + Math.Exp(z);
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(z - f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double PDFIntegrate(ref object? tempStorage, double precision)
      {
        double integrand;
        if (dev == 0)
        {
          //integrand = IntegrateFuncExpMFuncIncAdvanced(pdfCore, pdfFunc, 0, 0, Math.PI - dev, ref tempStorage, precision);
          integrand = PDFIntegrateZeroDev(ref tempStorage, precision);
        }
        else if (dev < 1e-2)
        {
          //double xm = FindIncreasingYEqualToOne(pdfCore, 0, Math.PI - dev);
          //integrand = IntegrateFuncExpMFuncIncAdvanced(pdfCore, pdfFunc, 0, xm, Math.PI - dev, ref tempStorage, precision);
          integrand = PDFIntegrateSmallDev(ref tempStorage, precision);
        }
        else
        {
          integrand = PDFIntegrateNormalDev(ref tempStorage, precision);
        }
        return integrand;
      }

      protected double PDFIntegrateZeroDev(ref object? tempStorage, double precision)
      {
        const double logPrefactorOffset = 100;
        const double OneByPrefactorOffset = 3.720075976020835962959696e-44;

        // for zero dev we know that the core is constant until x=1E-10
        // so the first part

        double y0 = pdfCore(0);

        if (y0 > (MinusLogTiny + 2 + logPdfPrefactor))
          return 0;
        bool prefactorApplied = false;
        if (y0 > (MinusLogTiny + logPdfPrefactor - logPrefactorOffset))
        {
          prefactorApplied = true;
          logPdfPrefactor += logPrefactorOffset;
        }

        double xm = 1E-10;

        double resultLeft = xm * pdfFunc(0); // note that logPdfPrefactor is already included in pdfFunc(0), so it is not neccessary to include it here

        // now integrate logarithmically
        var error1 = Calc.Integration.QagpIntegration.Integration(
                    PDFFuncLogInt,
                    new double[] { Math.Log(xm), Math.Log(Math.PI) }, 2,
                    0, precision, 100, out var resultRight, out var abserrRight, ref tempStorage);

        if (error1 is not null)
          resultRight = double.NaN;

        return prefactorApplied ? OneByPrefactorOffset * (resultLeft + resultRight) : (resultLeft + resultRight);
      }

      private double PDFIntegrateSmallDev(ref object? tempStorage, double precision)
      {
        GSL_ERROR? error;
        double resultLeft = 0, resultRight;
        double abserrLeft;
        double xm = FindIncreasingYEqualToOne(pdfCore, 0, Math.PI - dev);

        if (xm > 0)
        {
          double yh = pdfCore(xm / 2);
          if (yh < 0.5)
          {
            double fac = 1e5;
            // then the area is more or less concentrated at xm
            _x0 = xm * (1 + fac * DoubleConstants.DBL_EPSILON);
            error = Calc.Integration.QagpIntegration.Integration(
                       PDFFuncLogIntToLeft,
                       new double[] { Math.Log(xm * fac * DoubleConstants.DBL_EPSILON), Math.Log(_x0) }, 2,
                       0, precision, 100, out resultLeft, out abserrLeft, ref tempStorage);
          }
          else
          {
            // First integrate the left side
            error = Calc.Integration.QagpIntegration.Integration(
                      pdfFunc,
                      new double[] { 0, xm }, 2,
                      0, precision, 100, out resultLeft, out abserrLeft, ref tempStorage);
          }
          if (error is not null)
          {
            resultLeft = double.NaN;
          }
        }
        // now the right side
        resultRight = PDFIntegrateUnknownRightSideInc(xm, Math.PI - dev, ref tempStorage, precision);

        return resultLeft + resultRight;
      }

      private double PDFIntegrateNormalDev(ref object? tempStorage, double precision)
      {
        double xm = FindIncreasingYEqualToOne(pdfCore, 0, UpperIntegrationLimit);
        try
        {
          if (xm * 10 < UpperIntegrationLimit) // then a logarithmic integration from left to right
          {
            var error1 = Calc.Integration.QagpIntegration.Integration(
            pdfFunc,
            new double[] { 0, xm }, 2,
            0, precision, 200, out var result1, out var abserr1, ref tempStorage);
            double xincrement = xm / 10;
            _x0 = xm - xincrement;
            var error2 = Calc.Integration.QagpIntegration.Integration(
                        PDFFuncLogIntToRight,
                        new double[] { Math.Log(xincrement), Math.Log(UpperIntegrationLimit - _x0) }, 2,
                        0, precision, 200, out var result2, out var abserr2, ref tempStorage);

            if (error1 is null && error2 is null)
              return result1 + result2;
            else
              return double.NaN;
          }
          else
          {
            var error = Calc.Integration.QagpIntegration.Integration(
                        pdfFunc,
                        new double[] { 0, xm, UpperIntegrationLimit }, 3,
                        0, precision, 200, out var result, out var abserr, ref tempStorage);

            if (error is null)
              return result;
            else
              return double.NaN;
          }
        }
        catch (Exception)
        {
          return double.NaN;
        }
      }

      private double PDFIntegrateUnknownRightSideInc(double x0, double x1a, ref object? tempStorage, double precision)
      {
        const double diffOneDecade = 7;
        GSL_ERROR? error1 = null;
        GSL_ERROR? error2 = null;
        double result1 = 0;
        double result2 = 0;
        double abserr1, abserr2;

        double x1 = FindIncreasingYEqualTo(pdfCore, x0, x1a, MinusLogTiny + 1, 1, out var y1);
        double y0 = pdfCore(x0);

        // When the difference of y values results in a difference able to handle by the algorithm, then return immediately
        if ((y0 >= MinusLogTiny) || ((y1 - y0) < diffOneDecade))
        {
          error1 = Calc.Integration.QagpIntegration.Integration(
                      pdfFunc,
                      new double[] { x0, x1 }, 2,
                      0, precision, 100, out result1, out abserr1, ref tempStorage);
        }
        else
        {
          // now take the overall derivative
          double s01 = (y1 - y0) / (x1 - x0); // overall derivative

          // Take the values in the vicinity of x0 and x1, respectively, but make sure
          // not to use too big differences

          double s0, s1;

          s0 = PDFCoreDerivative(x0);
          s1 = PDFCoreDerivative(x1);

          if (s0 > s01 && s1 < s01) // very steep at the beginning, but shallow at the right
          {
            // so we do a logarithmic integration from the left to the right
            error1 = Calc.Integration.QagpIntegration.Integration(
                        PDFFuncLogInt,
                        new double[] { Math.Log(x0), Math.Log(x1a) }, 2,
                        0, precision, 100, out result1, out abserr1, ref tempStorage);

            /*
                        if (null != error1)
                        {
                            // increasing fast at x0 and slow at x1, so part from the left
                            count = PartFromTheLeft(x0, x1, diffOneDecade / s0, intgrenzen, 0);
                            error1 = Calc.Integration.QagpIntegration.Integration(
                            pdfFunc, intgrenzen, count, 0, precision, 100, out result1, out abserr1, ref tempStorage);
                        }
                        */
          }
          else if (s0 < s01 && s1 > s01) // shallow at the beginning, but steep at the right side
          {
            // we do a logarithmic integration from right to left
            _x0 = x1 + diffOneDecade / s1;
            error1 = Calc.Integration.QagpIntegration.Integration(
                        PDFFuncLogIntToLeft,
                        new double[] { Math.Log(diffOneDecade / s1), Math.Log(_x0 - x0) }, 2,
                        0, precision, 100, out result1, out abserr1, ref tempStorage);
            /*
                        if (null != error1)
                        {
                            // increasing slow at x0 and fast at x1, so part from the right
                            double[] intgrenzen = new double[100];
                            count = PartFromTheRight(x0, x1, diffOneDecade / s1, intgrenzen, 0);
                            error1 = Calc.Integration.QagpIntegration.Integration(
                            pdfFunc, intgrenzen, count, 0, precision, 100, out result1, out abserr1, ref tempStorage);
                        }
                        */
          }
          else if (s0 < s01 && s1 < s01)
          {
            // in this case, there is the fast transition somewhere inbetween the interval, so we have to search for it
            double xm = FindIncreasingYEqualTo(pdfCore, x0, x1, 0.5 * (y0 + y1), 0.1, out var ym); // Point where the fast transtion is (hopefully)
            double sm = PDFCoreDerivative(xm);

            double xinterval = sm > 0 ? diffOneDecade / sm : xm * DoubleConstants.DBL_EPSILON;

            // we do a logarithmic integration from right to left
            _x0 = xm + xinterval;
            error1 = Calc.Integration.QagpIntegration.Integration(
                        PDFFuncLogIntToLeft,
                        new double[] { Math.Log(xinterval), Math.Log(_x0 - x0) }, 2,
                        0, precision, 100, out result1, out abserr1, ref tempStorage);
            /*
                        count = PartFromTheRight(x0, xm, xinterval, intgrenzen, 0);
                        error1 = Calc.Integration.QagpIntegration.Integration(
                        pdfFunc, intgrenzen, count, 0, precision, 100, out result1, out abserr1, ref tempStorage);
                        */

            _x0 = xm - xinterval;
            error2 = Calc.Integration.QagpIntegration.Integration(
                        PDFFuncLogIntToRight,
                        new double[] { Math.Log(xinterval), Math.Log(x1a - _x0) }, 2,
                        0, precision, 100, out result2, out abserr2, ref tempStorage);

            /*
                        count = PartFromTheLeft(xm, x1, xinterval, intgrenzen, 0);
                        error2 = Calc.Integration.QagpIntegration.Integration(
                            pdfFunc, intgrenzen, count, 0, precision, 100, out result2, out abserr2, ref tempStorage);
                        */
          }
          else if (s0 > s01 && s1 > s01)
          {
            // then we have fast increases both on x0 and x1, so we must have a plateau inbetween
            double xm = 0.5 * (x0 + x1);

            error1 = Calc.Integration.QagpIntegration.Integration(
                        PDFFuncLogInt,
                        new double[] { Math.Log(x0), Math.Log(xm) }, 2,
                        0, precision, 100, out result1, out abserr1, ref tempStorage);

            /*
                        if (null != error1)
                        {
                            count = PartFromTheLeft(x0, xm, diffOneDecade / s0, intgrenzen, 0);
                            error1 = Calc.Integration.QagpIntegration.Integration(
                            pdfFunc, intgrenzen, count, 0, precision, 100, out result1, out abserr1, ref tempStorage);
                        }
                        */

            error2 = Calc.Integration.QagpIntegration.Integration(
                        pdfFunc,
                        new double[] { xm, x1 }, 2,
                        0, precision, 100, out result2, out abserr2, ref tempStorage);
            /*
                        if (null != error2)
                        {
                            count = PartFromTheRight(xm, x1, diffOneDecade / s1, intgrenzen, 0);
                            error2 = Calc.Integration.QagpIntegration.Integration(
                                pdfFunc, intgrenzen, count, 0, precision, 100, out result2, out abserr2, ref tempStorage);
                        }
                        */
          }
          else
          {
            // part linearly spaced between x0 and x1
            /*
                        * double xinc = diffOneDecade / s01;
                        double xs = x0;
                        for (count = 0; count < intgrenzen.Length; count++)
                        {
                            if (xs >= x1)
                            {
                                intgrenzen[count] = x1;
                                count++;
                                break;
                            }
                            intgrenzen[count] = xs;
                            xs += xinc;
                        }
                        */

            error1 = Calc.Integration.QagpIntegration.Integration(
              pdfFunc,
              new double[] { x0, x1 }, 2,
              0, precision, 100, out result1, out abserr1, ref tempStorage);
          }
        }

        if (error1 is null && error2 is null)
          return result1 + result2;
        else
          return double.NaN;
      }

      public bool IsMaximumLeftHandSide()
      {
        return PDFCore(0.5 * UpperIntegrationLimit) > 1;
      }

      public double UpperIntegrationLimit
      {
        get
        {
          return Math.PI - dev;
        }
      }

      public double FindXOfPDFCoreY(double ysearch, double tolerance)
      {
        double result;
        result = FindIncreasingYEqualTo(pdfCore, 0, UpperIntegrationLimit, ysearch, tolerance, out var yfound);
        return result;
      }

      #region CDF

      public double CDFFunc(double x)
      {
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : Math.Exp(-f);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double CDFFuncLogInt(double z)
      {
        double x = Math.Exp(z) + _x0;
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : Math.Exp(z - f);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double CDFIntegrate(ref object? tempStorage, double precision)
      {
        GSL_ERROR? error1;
        double resultRight, abserrRight;

        double xm;
        if (dev == 0)
          xm = FindIncreasingYEqualTo(PDFCore, 0, UpperIntegrationLimit, PDFCore(0) + 1, 0.1, out var yfound);
        else
          xm = FindIncreasingYEqualToOne(PDFCore, 0, UpperIntegrationLimit);

        if ((xm * 10) < UpperIntegrationLimit)
        {
          // logarithmical integration
          _x0 = -xm;
          // now integrate logarithmically
          error1 = Calc.Integration.QagpIntegration.Integration(
                      CDFFuncLogInt,
                      new double[] { Math.Log(xm), Math.Log(UpperIntegrationLimit + xm) }, 2,
                      0, precision, 100, out resultRight, out abserrRight, ref tempStorage);
        }
        else // linear integration
        {
          error1 = Calc.Integration.QagpIntegration.Integration(
                      CDFFunc,
                      new double[] { 0, UpperIntegrationLimit }, 2,
                      0, precision, 100, out resultRight, out abserrRight, ref tempStorage);
        }

        if (error1 is not null)
          return double.NaN;
        else
          return resultRight;
      }

      #endregion CDF
    }

    public class Alt1GnIA1 : Alt1GnI
    {
      /// <summary>X-value where the core function is equal to 1.</summary>
      private double _xm;

      /// <summary>Equal to alpha/(1-alpha).</summary>
      private double _n;

      /// <summary>Prefactor, equal to the derivative of the r1 part of the core function by the r1 value at xm..</summary>
      private double _a;

      /// <summary>Distance from _xm, for which points we use the derivative approximation.</summary>
      private double _xmax;

      public Alt1GnIA1(double factorp, double facdiv, double logPdfPrefactor, double alpha, double dev)
        : base(factorp, facdiv, logPdfPrefactor, alpha, dev)
      {
      }

      public double PDFCoreMod(double dx)
      {
        if (Math.Abs(dx) < _xmax)
        {
          //System.Diagnostics.Debug.Write("DN-");
          return Math.Exp(_n * RMath.Log1p(_a * dx));
        }
        else
        {
          //System.Diagnostics.Debug.Write("CO-");
          return PDFCoreSmallDev(_xm + dx);
        }
      }

      /// <summary>
      /// Special calculation of the core function if dev==0. Especially accurate when alpha is very close to 1.
      /// </summary>
      /// <param name="thetas"></param>
      /// <returns></returns>
      public double PDFCoreZeroDev(double thetas)
      {
        double ala = 1 - alpha;
        double n = alpha / ala;

        double r1;
        double r2;
        if (thetas == 0)
        {
          r1 = Math.Pow(alpha / factorp, n);
          r2 = ala / facdiv;
        }
        else
        {
          // The problem is, that r1 is for a wide range of thetas very close to 1,
          // but in connection with the high power small changes can have a dramatic effect on the result
          // original: r1 = Math.Pow(Math.Sin(alpha * thetas) / (factorp * Math.Sin(thetas)), alpha / (1 - alpha));

          // here we calculate the value y as follows: Sin(alpha x)/Sin(x) == alpha*(1+y)
          double y = Math.Sin(alpha * thetas) / n - 2 * Math.Cos((1 - 0.5 * ala) * thetas) * Math.Sin(0.5 * ala * thetas);
          y /= Math.Sin(thetas);

          r1 = Math.Pow(alpha / factorp, n) * Math.Exp(n * RMath.Log1p(y));
          r2 = Math.Sin(thetas * ala) / (facdiv * Math.Sin(thetas));
        }
        double result = r1 * r2;
        if (!(result >= 0))
          result = double.MaxValue;

        //System.Diagnostics.Debug.WriteLine(string.Format("CorAlt1GnI theta={0}, result={1}", thetas, result));
        return result;
      }

      /// <summary>
      /// Special calculation of the core function if dev==0. Especially accurate when alpha is very close to 1.
      /// </summary>
      /// <param name="thetas"></param>
      /// <returns></returns>
      public double PDFCoreSmallDev(double thetas)
      {
        double ala = 1 - alpha;
        double n = alpha / ala;

        double r1;
        double r2;
        if (dev == 0 && thetas == 0)
        {
          r1 = Math.Pow(alpha / factorp, n);
          r2 = ala / facdiv;
        }
        else
        {
          if (thetas < dev)
          {
            r1 = Math.Pow(Math.Sin(alpha * thetas) / (factorp * Math.Sin(dev + thetas)), alpha / (1 - alpha));
          }
          else
          {
            // The problem is, that r1 is for a wide range of thetas very close to 1,
            // but in connection with the high power small changes can have a dramatic effect on the result
            // here we calculate the value y as follows: Sin(alpha x)/Sin(x+dev) == alpha*(1+y)
            double dev2 = dev / 2;
            double y = Math.Sin(alpha * thetas) / n - 2 * Math.Cos((1 - 0.5 * ala) * thetas + dev2) * Math.Sin(0.5 * ala * thetas + dev2);
            y /= Math.Sin(thetas + dev);
            r1 = Math.Pow(alpha / factorp, n) * Math.Exp(n * RMath.Log1p(y));
          }
          r2 = Math.Sin(dev + ala * thetas) / (facdiv * Math.Sin(dev + thetas));
        }
        double result = r1 * r2;
        if (!(result >= 0))
          result = double.MaxValue;

        //System.Diagnostics.Debug.WriteLine(string.Format("CorAlt1GnI theta={0}, result={1}", thetas, result));
        return result;
      }

      public double PDFFuncZeroDev(double x)
      {
        double f = PDFCoreZeroDev(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(-f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double PDFFuncLogIntZeroDev(double z)
      {
        double x = Math.Exp(z);
        double f = PDFCoreZeroDev(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(z - f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public new double PDFFuncLogIntToLeft(double z)
      {
        double x = Math.Max(-_xm, _x0 - Math.Exp(z));
        double f = PDFCoreMod(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(z - f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("z={0}, x={1}, f={2}, r={3}", z, x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public new double PDFFuncLogIntToRight(double z)
      {
        double x = _x0 + Math.Exp(z);
        double f = PDFCoreMod(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(z - f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double PDFIntegrateAlphaNearOne(ref object? tempStorage, double precision)
      {
        if (dev == 0)
          return PDFIntegrateZeroDevAlphaNearOne(ref tempStorage, precision);

        GSL_ERROR? error;
        // we want to find xm now with very high accuracy
        _xm = FindIncreasingYEqualTo(pdfCore, 0, Math.PI - dev, 1, 0, out var yfound);

        double r1c = PDF_R1Core(_xm);
        _a = PDF_R1CoreDerivativeByR1Core(_xm);
        // r1 is now approximated by r1c + r1der*(x-xm)
        // the logarithm of r1 is log(r1c) + r1cder*(x-xm)/r1c - (r1cder*(x-xm))^2/(2*r1c^2) + O((x-xm)^3)
        // in order to keep as much precision as possible, Abs(x-xm) must be smaller than precision*2*r1c/r1cder

        // now we calculate the integration boundaries
        _n = alpha / (1 - alpha);

        double xinc = 0.125 / (_n * _a); // this is the smallest interval to use for the logarithmic integration
        _xmax = 0.125 / _a; // this is the maximum interval to use using the derivative approximation

        if (_xmax < 1e5 * _xm * DoubleConstants.DBL_EPSILON)
          return Math.Exp(logPdfPrefactor) * GammaRelated.Gamma(1 / _n) / (_a * _n * _n); // then we use the analytic solution of the integral
        else
          _xmax = 1e5 * _xm * DoubleConstants.DBL_EPSILON;

        // now we integrate

        double r2 = Math.Sin(dev + _xm * (1 - alpha)) / (facdiv * Math.Sin(dev + _xm));

        // oder wir versuchen dies
        if (double.IsInfinity(xinc)) // this can happen if the derivative (_a) is zero
          xinc = _xmax / _n;

        _x0 = xinc;
        error = Calc.Integration.QagpIntegration.Integration(
                    PDFFuncLogIntToLeft,
                    new double[] { Math.Log(_x0), Math.Log(_xm + _x0) }, 2,
                    0, precision, 200, out var resultLeft, out var abserrLeft, ref tempStorage);

        _x0 = -xinc;
        error = Calc.Integration.QagpIntegration.Integration(
                    new Func<double, double>(PDFFuncLogIntToRight),
                    new double[] { Math.Log(-_x0), Math.Log(Math.PI - dev - _xm - _x0) }, 2,
                    0, precision, 100, out var resultRight, out var abserrRight, ref tempStorage);

        return (resultLeft + resultRight);
      }

      protected double PDFIntegrateZeroDevAlphaNearOne(ref object? tempStorage, double precision)
      {
        const double logPrefactorOffset = 100;
        const double OneByPrefactorOffset = 3.720075976020835962959696e-44;

        // for zero dev we know that the core is constant until x=1E-10
        // so the first part

        double y0 = pdfCore(0);

        if (y0 > (MinusLogTiny + 2 + logPdfPrefactor))
          return 0;

        bool prefactorApplied = false;
        double orgPrefactor = logPdfPrefactor;
        if (y0 > (MinusLogTiny + logPdfPrefactor - logPrefactorOffset))
        {
          prefactorApplied = true;
          logPdfPrefactor += logPrefactorOffset;
        }

        //double xe = FindIncreasingYEqualTo(pdfCore, 0, UpperIntegrationLimit, y0 + 9, 1, out ye);
        double xe = FindIncreasingYEqualTo(PDFCoreZeroDev, 0, UpperIntegrationLimit, y0 + 9, 1, out var ye);

        GSL_ERROR? error1;
        double result, abserr;
        if (xe * 10 < UpperIntegrationLimit) // then: logarithmic integration
        {
          double xm = 1E-10;
          double resultLeft = xm * pdfFunc(0); // note that the logPdfPrefactor is already included in pdfFunc(0), so it is unneccessary to have it here again

          // now integrate logarithmically
          error1 = Calc.Integration.QagpIntegration.Integration(
                      PDFFuncLogIntZeroDev,
                      new double[] { Math.Log(xm), Math.Log(Math.PI) }, 2,
                      0, precision, 100, out result, out abserr, ref tempStorage);

          result += resultLeft;
        }
        else // linear integration
        {
          error1 = Calc.Integration.QagpIntegration.Integration(
                      PDFFuncZeroDev,
                      new double[] { 0, xe, Math.PI }, 3,
                      0, precision, 100, out result, out abserr, ref tempStorage);
        }

        if (error1 is not null && error1.Number != GSL_ERR.GSL_EROUND) // ignore rounding errors here because rounding errors are possible due to the high power alpha/(1-alpha)
          result = double.NaN;

        logPdfPrefactor = orgPrefactor;

        return prefactorApplied ? OneByPrefactorOffset * result : result;
      }

      private double PDF_R1Core(double thetas)
      {
        return Math.Sin(alpha * thetas) / (factorp * Math.Sin(dev + thetas));
      }

      private double PDF_R1CoreDerivativeByR1Core(double thetas)
      {
        return alpha / Math.Tan(alpha * thetas) - 1 / Math.Tan(dev + thetas);
      }
    }

    #endregion Function increasing

    #region Function decreasing

    /// <summary>
    /// Helper class for calculation used if alpha&lt;1, gamma&lt;0 and for decreasing integration direction.
    /// </summary>
    public class Alt1GnD
    {
      protected double factorp;
      protected double facdiv;
      protected double alpha;
      protected double dev;
      protected double logPdfPrefactor;
      protected double _x0;
      protected Func<double, double> pdfCore;
      private Func<double, double> pdfFunc;

      public Alt1GnD(double factorp, double facdiv, double logPdfPrefactor, double alpha, double dev)
      {
        this.factorp = factorp;
        this.facdiv = facdiv;
        this.logPdfPrefactor = logPdfPrefactor;
        this.alpha = alpha;
        this.dev = dev;
        pdfCore = new Func<double, double>(PDFCore);
        pdfFunc = new Func<double, double>(PDFFunc);
      }

      public void Initialize()
      {
        pdfCore = new Func<double, double>(PDFCore);
        pdfFunc = new Func<double, double>(PDFFunc);
      }

      public double PDFCore(double thetas)
      {
        double r1;
        double r2;
        r1 = Math.Pow(Math.Sin(alpha * (Math.PI - dev - thetas)) / (factorp * Math.Sin(thetas)), alpha / (1 - alpha));
        r2 = Math.Sin(alpha * (Math.PI - dev) + thetas * (1 - alpha)) / (facdiv * Math.Sin(thetas));
        double result = r1 * r2;
        if (!(result >= 0))
          result = double.MaxValue;

        //System.Diagnostics.Debug.WriteLine(string.Format("Cor1e theta={0}, result={1}", thetas, result));
        return result;
      }

      public double PDFFunc(double x)
      {
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(-f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double PDFFuncLogInt(double z)
      {
        double x = Math.Exp(z);
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(z - f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double UpperIntegrationLimit
      {
        get
        {
          return Math.PI - dev;
        }
      }

      public double Integrate(ref object? tempStorage, double precision)
      {
        GSL_ERROR? error;
        double abserr;
        double result;
        double x1 = UpperIntegrationLimit;
        double xm = FindDecreasingYEqualToOne(pdfCore, 0, x1);
        try
        {
          if (xm < (x1 - xm))
          {
            error = Calc.Integration.QagpIntegration.Integration(
               PDFFunc,
               new double[] { 0, xm }, 2,
               0, precision, 100, out result, out abserr, ref tempStorage);
            if (error is null)
            {
              error = Calc.Integration.QagpIntegration.Integration(
               PDFFuncLogInt,
               new double[] { Math.Log(xm), Math.Log(x1) }, 2,
               0, precision, 100, out var result1, out abserr, ref tempStorage);

              result += result1;
            }
          }
          else
          {
            error = Calc.Integration.QagpIntegration.Integration(
             pdfFunc,
             new double[] { 0, xm, x1 }, 3,
             0, precision, 100, out result, out abserr, ref tempStorage);
          }
          if (error is not null)
            result = double.NaN;

          return result;
        }
        catch (Exception)
        {
          return double.NaN;
        }
      }

      #region CDF

      public double CDFFunc(double x)
      {
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : OneMinusExp(-f);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double CDFFuncLogInt(double z)
      {
        double x = Math.Exp(z) + _x0;
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : Math.Exp(z) * OneMinusExp(-f);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double CDFIntegrate(ref object? tempStorage, double precision)
      {
        GSL_ERROR? error1;
        double resultRight, abserrRight;

        double xm;

        xm = FindDecreasingYEqualToOne(PDFCore, 0, UpperIntegrationLimit);

        if ((xm * 10) < UpperIntegrationLimit)
        {
          // logarithmical integration
          _x0 = -xm;
          // now integrate logarithmically
          error1 = Calc.Integration.QagpIntegration.Integration(
                      CDFFuncLogInt,
                      new double[] { Math.Log(xm), Math.Log(UpperIntegrationLimit + xm) }, 2,
                      0, precision, 100, out resultRight, out abserrRight, ref tempStorage);
        }
        else // linear integration
        {
          error1 = Calc.Integration.QagpIntegration.Integration(
                      CDFFunc,
                      new double[] { 0, UpperIntegrationLimit }, 2,
                      0, precision, 100, out resultRight, out abserrRight, ref tempStorage);
        }

        if (error1 is not null)
          return double.NaN;
        else
          return resultRight;
      }

      #endregion CDF
    }

    public class Alt1GnDA1 : Alt1GnD
    {
      /// <summary>X-value where the core function is equal to 1.</summary>
      private double _xm;

      /// <summary>Equal to alpha/(1-alpha).</summary>
      private double _n;

      /// <summary>Prefactor, equal to the derivative of the r1 part of the core function by the r1 value at xm..</summary>
      private double _a;

      /// <summary>Distance from _xm, for which points we use the derivative approximation.</summary>
      private double _xmax;

      public Alt1GnDA1(double factorp, double facdiv, double logPdfPrefactor, double alpha, double dev)
        : base(factorp, facdiv, logPdfPrefactor, alpha, dev)
      {
      }

      public double PDFCoreMod(double dx)
      {
        if (Math.Abs(dx) < _xmax)
        {
          //System.Diagnostics.Debug.Write("DN-");
          return Math.Exp(_n * RMath.Log1p(_a * dx));
        }
        else
        {
          //System.Diagnostics.Debug.Write("CO-");
          return PDFCore(_xm + dx);
        }
      }

      public double PDFFuncLogIntToLeft(double z)
      {
        double x = Math.Max(-_xm, _x0 - Math.Exp(z));
        double f = PDFCoreMod(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(z - f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("z={0}, x={1}, f={2}, r={3}", z, x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double PDFFuncLogIntToRight(double z)
      {
        double x = _x0 + Math.Exp(z);
        double f = PDFCoreMod(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(z - f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double PDFIntegrateAlphaNearOne(ref object? tempStorage, double precision)
      {
        GSL_ERROR? error;
        // we want to find xm now with very high accuracy
        _xm = FindDecreasingYEqualTo(pdfCore, 0, Math.PI - dev, 1, 0, out var yfound);

        double r1c = PDF_R1Core(_xm);
        _a = PDF_R1CoreDerivativeByR1Core(_xm);
        // r1 is now approximated by r1c + r1der*(x-xm)
        // the logarithm of r1 is log(r1c) + r1cder*(x-xm)/r1c - (r1cder*(x-xm))^2/(2*r1c^2) + O((x-xm)^3)
        // in order to keep as much precision as possible, Abs(x-xm) must be smaller than precision*2*r1c/r1cder

        // now we calculate the integration boundaries
        _n = alpha / (1 - alpha);

        double xinc = 0.125 / (_n * Math.Abs(_a)); // this is the smallest interval to use for the logarithmic integration
        _xmax = 0.125 / Math.Abs(_a); // this is the maximum interval to use using the derivative approximation

        if (_xmax < 1e5 * _xm * DoubleConstants.DBL_EPSILON)
          return Math.Exp(logPdfPrefactor) * GammaRelated.Gamma(1 / _n) / (-_a * _n * _n); // then we use the analytic solution of the integral
        else
          _xmax = 1e5 * _xm * DoubleConstants.DBL_EPSILON;

        if (double.IsInfinity(xinc))
          xinc = _xmax / _n;

        // now we integrate
        _x0 = xinc;
        error = Calc.Integration.QagpIntegration.Integration(
                  PDFFuncLogIntToLeft,
                  new double[] { Math.Log(_x0), Math.Log(_xm + _x0) }, 2,
                  0, precision, 100, out var resultLeft, out var abserrLeft, ref tempStorage);

        _x0 = -xinc;
        error = Calc.Integration.QagpIntegration.Integration(
                    PDFFuncLogIntToRight,
                    new double[] { Math.Log(-_x0), Math.Log(Math.PI - dev - _xm - _x0) }, 2,
                    0, precision, 100, out var resultRight, out var abserrRight, ref tempStorage);

        return (resultLeft + resultRight);
      }

      private double PDF_R1Core(double thetas)
      {
        return Math.Sin(alpha * (Math.PI - dev - thetas)) / (factorp * Math.Sin(thetas));
      }

      private double PDF_R1CoreDerivativeByR1Core(double thetas)
      {
        return alpha / Math.Tan(alpha * (-Math.PI + dev + thetas)) - 1 / Math.Tan(thetas);
      }
    }

    #endregion Function decreasing

    #endregion Gamma negative

    #region Gamma positive

    #region Function increasing

    public class Alt1GpI
    {
      protected double factorp;
      protected double facdiv;
      protected double alpha;
      protected double dev;
      protected double logPdfPrefactor;
      protected double _x0;
      protected Func<double, double> pdfCore;
      private Func<double, double> pdfFunc;

      public Alt1GpI(double factorp, double facdiv, double logPdfPrefactor, double alpha, double dev)
      {
        this.factorp = factorp;
        this.facdiv = facdiv;
        this.logPdfPrefactor = logPdfPrefactor;
        this.alpha = alpha;
        this.dev = dev;
        pdfCore = new Func<double, double>(PDFCore);
        pdfFunc = new Func<double, double>(PDFFunc);
      }

      public void Initialize()
      {
        pdfCore = new Func<double, double>(PDFCore);
        pdfFunc = new Func<double, double>(PDFFunc);
      }

      // Note that in the moment we dont have a directional change here, but one can easily change
      // the integration direction by replacing thetas with d-thetas (since the integration goes from 0 to dev).
      public double PDFCore(double thetas)
      {
        double r1;
        double r2;

        r1 = Math.Pow(Math.Sin(alpha * thetas) / (factorp * Math.Sin(dev - thetas)), alpha / (1 - alpha));
        r2 = Math.Sin(dev - thetas * (1 - alpha)) / (facdiv * Math.Sin(dev - thetas));

        double result = r1 * r2;
        if (!(result >= 0))
          result = double.MaxValue;

        //System.Diagnostics.Debug.WriteLine(string.Format("Cor1f theta={0}, result={1}", thetas, result));
        return result;
      }

      public double PDFFunc(double x)
      {
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(-f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double PDFFuncLogInt(double z)
      {
        double x = Math.Exp(z);
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(z - f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double Integrate(ref object? tempStorage, double precision)
      {
        GSL_ERROR? error;
        double abserr;
        double result;
        double x1 = dev;
        double xm = FindIncreasingYEqualToOne(pdfCore, 0, x1);
        try
        {
          if (xm < (x1 - xm))
          {
            error = Calc.Integration.QagpIntegration.Integration(
               PDFFunc,
               new double[] { 0, xm }, 2,
               0, precision, 100, out result, out abserr, ref tempStorage);
            if (error is null)
            {
              error = Calc.Integration.QagpIntegration.Integration(
               PDFFuncLogInt,
               new double[] { Math.Log(xm), Math.Log(x1) }, 2,
               0, precision, 100, out var result1, out abserr, ref tempStorage);

              result += result1;
            }
          }
          else
          {
            error = Calc.Integration.QagpIntegration.Integration(
             pdfFunc,
             new double[] { 0, xm, x1 }, 3,
             0, precision, 100, out result, out abserr, ref tempStorage);
          }
          if (error is not null)
            result = double.NaN;

          return result;
        }
        catch (Exception)
        {
          return double.NaN;
        }
      }

      public double UpperIntegrationLimit
      {
        get
        {
          return dev;
        }
      }

      public bool IsMaximumLeftHandSide()
      {
        return PDFCore(0.5 * dev) > 1;
      }

      #region CDF

      public double CDFFunc(double x)
      {
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : Math.Exp(-f);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double CDFFuncLogInt(double z)
      {
        double x = Math.Exp(z) + _x0;
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : Math.Exp(z - f);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double CDFIntegrate(ref object? tempStorage, double precision)
      {
        GSL_ERROR? error1;
        double resultRight, abserrRight;

        double xm;
        if (dev == 0)
          xm = FindIncreasingYEqualTo(PDFCore, 0, UpperIntegrationLimit, PDFCore(0) + 1, 0.1, out var yfound);
        else
          xm = FindIncreasingYEqualToOne(PDFCore, 0, UpperIntegrationLimit);

        if ((xm * 10) < UpperIntegrationLimit)
        {
          // logarithmical integration
          _x0 = -xm;
          // now integrate logarithmically
          error1 = Calc.Integration.QagpIntegration.Integration(
                      CDFFuncLogInt,
                      new double[] { Math.Log(xm), Math.Log(UpperIntegrationLimit + xm) }, 2,
                      0, precision, 100, out resultRight, out abserrRight, ref tempStorage);
        }
        else // linear integration
        {
          error1 = Calc.Integration.QagpIntegration.Integration(
                      CDFFunc,
                      new double[] { 0, UpperIntegrationLimit }, 2,
                      0, precision, 100, out resultRight, out abserrRight, ref tempStorage);
        }

        if (error1 is not null)
          return double.NaN;
        else
          return resultRight;
      }

      #endregion CDF
    }

    public class Alt1GpIA1 : Alt1GpI
    {
      /// <summary>X-value where the core function is equal to 1.</summary>
      private double _xm;

      /// <summary>Equal to alpha/(1-alpha).</summary>
      private double _n;

      /// <summary>Prefactor, equal to the derivative of the r1 part of the core function by the r1 value at xm..</summary>
      private double _a;

      /// <summary>Distance from _xm, for which points we use the derivative approximation.</summary>
      private double _xmax;

      public Alt1GpIA1(double factorp, double facdiv, double logPdfPrefactor, double alpha, double dev)
        : base(factorp, facdiv, logPdfPrefactor, alpha, dev)
      {
      }

      public double PDFCoreMod(double dx)
      {
        if (Math.Abs(dx) < _xmax)
        {
          //System.Diagnostics.Debug.Write("DN-");
          return Math.Exp(_n * RMath.Log1p(_a * dx));
        }
        else
        {
          //System.Diagnostics.Debug.Write("CO-");
          return PDFCore(_xm + dx);
        }
      }

      public double PDFFuncLogIntToLeft(double z)
      {
        double x = Math.Max(-_xm, _x0 - Math.Exp(z));
        double f = PDFCoreMod(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(z - f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("z={0}, x={1}, f={2}, r={3}", z, x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double PDFFuncLogIntToRight(double z)
      {
        double x = _x0 + Math.Exp(z);
        double f = PDFCoreMod(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(z - f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double PDFIntegrateAlphaNearOne(ref object? tempStorage, double precision)
      {
        GSL_ERROR? error;
        // we want to find xm now with very high accuracy
        _xm = FindIncreasingYEqualTo(pdfCore, 0, dev, 1, 0, out var yfound);

        double r1c = PDF_R1Core(_xm);
        _a = PDF_R1CoreDerivativeByR1Core(_xm);
        // r1 is now approximated by r1c + r1der*(x-xm)
        // the logarithm of r1 is log(r1c) + r1cder*(x-xm)/r1c - (r1cder*(x-xm))^2/(2*r1c^2) + O((x-xm)^3)
        // in order to keep as much precision as possible, Abs(x-xm) must be smaller than precision*2*r1c/r1cder

        // now we calculate the integration boundaries
        _n = alpha / (1 - alpha);

        double xinc = 0.125 / (_n * Math.Abs(_a)); // this is the smallest interval to use for the logarithmic integration
        _xmax = 0.125 / Math.Abs(_a); // this is the maximum interval to use using the derivative approximation

        if (_xmax < 1e5 * _xm * DoubleConstants.DBL_EPSILON)
          return Math.Exp(logPdfPrefactor) * GammaRelated.Gamma(1 / _n) / (_a * _n * _n); // then we use the analytic solution of the integral
        else
          _xmax = 1e5 * _xm * DoubleConstants.DBL_EPSILON;

        // now we integrate
        _x0 = xinc;
        error = Calc.Integration.QagpIntegration.Integration(
                  PDFFuncLogIntToLeft,
                  new double[] { Math.Log(_x0), Math.Log(_xm + _x0) }, 2,
                  0, precision, 100, out var resultLeft, out var abserrLeft, ref tempStorage);

        _x0 = -xinc;
        error = Calc.Integration.QagpIntegration.Integration(
                    PDFFuncLogIntToRight,
                    new double[] { Math.Log(-_x0), Math.Log(dev - _xm - _x0) }, 2,
                    0, precision, 100, out var resultRight, out var abserrRight, ref tempStorage);

        return (resultLeft + resultRight);
      }

      private double PDF_R1Core(double thetas)
      {
        return Math.Sin(alpha * thetas) / (factorp * Math.Sin(dev - thetas));
      }

      private double PDF_R1CoreDerivativeByR1Core(double thetas)
      {
        return 1 / Math.Tan(dev - thetas) + alpha / Math.Tan(alpha * thetas);
      }
    }

    #endregion Function increasing

    #region Function decreasing

    public class Alt1GpD
    {
      protected double factorp;
      protected double facdiv;
      protected double alpha;
      protected double dev;
      protected double logPdfPrefactor;
      protected double _x0;
      protected Func<double, double> pdfCore;
      private Func<double, double> pdfFunc;

      public Alt1GpD(double factorp, double facdiv, double logPdfPrefactor, double alpha, double dev)
      {
        this.factorp = factorp;
        this.facdiv = facdiv;
        this.logPdfPrefactor = logPdfPrefactor;
        this.alpha = alpha;
        this.dev = dev;
        pdfCore = new Func<double, double>(PDFCore);
        pdfFunc = new Func<double, double>(PDFFunc);
      }

      public void Initialize()
      {
        pdfCore = new Func<double, double>(PDFCore);
        pdfFunc = new Func<double, double>(PDFFunc);
      }

      public double PDFCore(double thetas)
      {
        double r1;
        double r2;

        r1 = Math.Pow(Math.Sin(alpha * (dev - thetas)) / (factorp * Math.Sin(thetas)), alpha / (1 - alpha));
        r2 = Math.Sin(alpha * dev + thetas * (1 - alpha)) / (facdiv * Math.Sin(thetas));

        double result = r1 * r2;
        if (!(result >= 0))
          result = double.MaxValue;

        //System.Diagnostics.Debug.WriteLine(string.Format("Cor1f theta={0}, result={1}", thetas, result));
        return result;
      }

      public double PDFFunc(double x)
      {
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(-f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double PDFFuncLogInt(double z)
      {
        double x = Math.Exp(z);
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(z - f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double UpperIntegrationLimit
      {
        get
        {
          return dev;
        }
      }

      public double Integrate(ref object? tempStorage, double precision)
      {
        GSL_ERROR? error;
        double abserr;
        double result;
        double x1 = dev;
        double xm = FindDecreasingYEqualToOne(pdfCore, 0, x1);
        try
        {
          if (xm < (x1 - xm))
          {
            error = Calc.Integration.QagpIntegration.Integration(
               PDFFunc,
               new double[] { 0, xm }, 2,
               0, precision, 100, out result, out abserr, ref tempStorage);
            if (error is null)
            {
              error = Calc.Integration.QagpIntegration.Integration(
                        PDFFuncLogInt,
                        new double[] { Math.Log(xm), Math.Log(x1) }, 2,
                        0, precision, 100, out var result1, out abserr, ref tempStorage);

              result += result1;
            }
          }
          else
          {
            error = Calc.Integration.QagpIntegration.Integration(
                      pdfFunc,
                      new double[] { 0, xm, x1 }, 3,
                      0, precision, 100, out result, out abserr, ref tempStorage);
          }
          if (error is not null)
            result = double.NaN;

          return result;
        }
        catch (Exception)
        {
          return double.NaN;
        }
      }

      #region CDF

      public double CDFFunc(double x)
      {
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : OneMinusExp(-f);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double CDFFuncLogInt(double z)
      {
        double x = Math.Exp(z) + _x0;
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : Math.Exp(z) * OneMinusExp(-f);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double CDFIntegrate(ref object? tempStorage, double precision)
      {
        GSL_ERROR? error1;
        double resultRight, abserrRight;

        double xm;

        xm = FindDecreasingYEqualToOne(PDFCore, 0, UpperIntegrationLimit);

        if ((xm * 10) < UpperIntegrationLimit)
        {
          // logarithmical integration
          _x0 = -xm;
          // now integrate logarithmically
          error1 = Calc.Integration.QagpIntegration.Integration(
                      CDFFuncLogInt,
                      new double[] { Math.Log(xm), Math.Log(UpperIntegrationLimit + xm) }, 2,
                      0, precision, 100, out resultRight, out abserrRight, ref tempStorage);
        }
        else // linear integration
        {
          error1 = Calc.Integration.QagpIntegration.Integration(
                      CDFFunc,
                      new double[] { 0, UpperIntegrationLimit }, 2,
                      0, precision, 100, out resultRight, out abserrRight, ref tempStorage);
        }

        if (error1 is not null)
          return double.NaN;
        else
          return resultRight;
      }

      #endregion CDF
    }

    public class Alt1GpDA1 : Alt1GpD
    {
      /// <summary>X-value where the core function is equal to 1.</summary>
      private double _xm;

      /// <summary>Equal to alpha/(1-alpha).</summary>
      private double _n;

      /// <summary>Prefactor, equal to the derivative of the r1 part of the core function by the r1 value at xm..</summary>
      private double _a;

      /// <summary>Distance from _xm, for which points we use the derivative approximation.</summary>
      private double _xmax;

      public Alt1GpDA1(double factorp, double facdiv, double logPdfPrefactor, double alpha, double dev)
        : base(factorp, facdiv, logPdfPrefactor, alpha, dev)
      {
      }

      public double PDFCoreMod(double dx)
      {
        if (Math.Abs(dx) < _xmax)
        {
          //System.Diagnostics.Debug.Write("DN-");
          return Math.Exp(_n * RMath.Log1p(_a * dx));
        }
        else
        {
          //System.Diagnostics.Debug.Write("CO-");
          return PDFCore(_xm + dx);
        }
      }

      public double PDFFuncLogIntToLeft(double z)
      {
        double x = Math.Max(-_xm, _x0 - Math.Exp(z));
        double f = PDFCoreMod(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(z - f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("z={0}, x={1}, f={2}, r={3}", z, x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double PDFFuncLogIntToRight(double z)
      {
        double x = _x0 + Math.Exp(z);
        double f = PDFCoreMod(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(z - f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double PDFIntegrateAlphaNearOne(ref object? tempStorage, double precision)
      {
        GSL_ERROR? error;
        // we want to find xm now with very high accuracy
        _xm = FindDecreasingYEqualTo(pdfCore, 0, dev, 1, 0, out var yfound);

        double r1c = PDF_R1Core(_xm);
        _a = PDF_R1CoreDerivativeByR1Core(_xm);
        // r1 is now approximated by r1c + r1der*(x-xm)
        // the logarithm of r1 is log(r1c) + r1cder*(x-xm)/r1c - (r1cder*(x-xm))^2/(2*r1c^2) + O((x-xm)^3)
        // in order to keep as much precision as possible, Abs(x-xm) must be smaller than precision*2*r1c/r1cder

        // now we calculate the integration boundaries
        _n = alpha / (1 - alpha);

        double xinc = 0.125 / (_n * Math.Abs(_a)); // this is the smallest interval to use for the logarithmic integration
        _xmax = 0.125 / Math.Abs(_a); // this is the maximum interval to use using the derivative approximation

        if (_xmax < 1e5 * _xm * DoubleConstants.DBL_EPSILON)
          return Math.Exp(logPdfPrefactor) * GammaRelated.Gamma(1 / _n) / (-_a * _n * _n); // then we use the analytic solution of the integral
        else
          _xmax = 1e5 * _xm * DoubleConstants.DBL_EPSILON;

        // now we integrate
        _x0 = xinc;
        error = Calc.Integration.QagpIntegration.Integration(
                  PDFFuncLogIntToLeft,
                  new double[] { Math.Log(_x0), Math.Log(_xm + _x0) }, 2,
                  0, precision, 100, out var resultLeft, out var abserrLeft, ref tempStorage);

        _x0 = -xinc;
        error = Calc.Integration.QagpIntegration.Integration(
                    PDFFuncLogIntToRight,
                    new double[] { Math.Log(-_x0), Math.Log(dev - _xm - _x0) }, 2,
                    0, precision, 100, out var resultRight, out var abserrRight, ref tempStorage);

        return (resultLeft + resultRight);
      }

      private double PDF_R1Core(double thetas)
      {
        return Math.Sin(alpha * (dev - thetas)) / (factorp * Math.Sin(thetas));
      }

      private double PDF_R1CoreDerivativeByR1Core(double thetas)
      {
        return -(alpha / Math.Tan(alpha * (dev - thetas))) - 1 / Math.Tan(thetas);
      }
    }

    #endregion Function decreasing

    #endregion Gamma positive

    #endregion Classes for integration (alpha<1)

    #region Classes for integration (alpha==1)

    public class Aeq1I
    {
      private double beta;
      private double abe;
      private double logFactorp;
      private double logPdfPrefactor;
      private double _x0;

      public Aeq1I(double x, double beta, double abe)
      {
        this.beta = beta;
        this.abe = abe;
        logFactorp = -0.5 * Math.PI * x / beta;
        logPdfPrefactor = -Math.Log(2 * Math.Abs(beta));
      }

      public double PDFCore(double thetas)
      {
        double r1;
        double r2;
        if (abe == 0 && thetas < 1E-10)
        {
          r1 = 2 * Math.Exp(logFactorp) / (Math.PI * Math.E);
          r2 = 1;
        }
        else
        {
          double abeta = Math.Abs(beta);
          double h = Math.PI * abe + 2 * abeta * thetas;
          r1 = Math.Exp(logFactorp - h / (2 * abeta * Math.Tan(thetas)));
          r2 = h / (Math.PI * Math.Sin(thetas));
        }
        double result = r1 * r2;
        if (!(result >= 0))
          result = double.MaxValue;

        //System.Diagnostics.Debug.WriteLine(string.Format("CorAlt1GnI theta={0}, result={1}", thetas, result));
        return result;
      }

      public double PDFCoreDerivativeByCore(double thetas)
      {
        double abeta = Math.Abs(beta);
        double c1 = (abe * Math.PI / (2 * abeta) + thetas);

        double s1 = 1 / c1;
        double s2 = -2 / Math.Tan(thetas);
        double s3 = c1 * RMath.Pow(Math.Sin(thetas), -2);

        return s1 + s2 + s3;
      }

      public double PDFFunc(double x)
      {
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(-f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double PDFFuncLogInt(double z)
      {
        double x = Math.Exp(z);
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(z - f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double PDFFuncLogIntToLeft(double z)
      {
        double x = _x0 - Math.Exp(z);
        if (x < 0)
          x = 0;

        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(z - f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double PDFFuncLogIntToRight(double z)
      {
        double x = _x0 + Math.Exp(z);
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(z - f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double Integrate(ref object? tempStorage, double precision)
      {
        if (abe == 0)
          return IntegrateAbeEqZero(ref tempStorage, precision);
        else
          return IntegrateAbeNotEqZero(ref tempStorage, precision);
      }

      public double IntegrateAbeNotEqZero(ref object? tempStorage, double precision)
      {
        GSL_ERROR? error;
        double abserr;
        double result;
        double x1 = UpperIntegrationLimit;

        double xm = FindIncreasingYEqualTo(PDFCore, 0, x1, 1, 0.125, out var ym);

        try
        {
          double xwidth = 1 / (PDFCoreDerivativeByCore(xm) * ym);
          if (xwidth * 8 < xm)
          {
            _x0 = xm + xwidth;
            error = Calc.Integration.QagpIntegration.Integration(
               PDFFuncLogIntToLeft,
               new double[] { Math.Log(xwidth), Math.Log(xm + xwidth) }, 2,
               0, precision, 100, out result, out abserr, ref tempStorage);

            if (error is null)
            {
              _x0 = xm - xwidth;
              error = Calc.Integration.QagpIntegration.Integration(
               PDFFuncLogIntToRight,
               new double[] { Math.Log(xwidth), Math.Log(x1 - _x0) }, 2,
               0, precision, 100, out var result1, out abserr, ref tempStorage);

              result += result1;
            }
          }
          else if (xm * 8 < (x1 - xm))
          {
            error = Calc.Integration.QagpIntegration.Integration(
               PDFFunc,
               new double[] { 0, xm }, 2,
               0, precision, 100, out result, out abserr, ref tempStorage);

            if (error is null)
            {
              error = Calc.Integration.QagpIntegration.Integration(
               PDFFuncLogInt,
               new double[] { Math.Log(xm), Math.Log(x1) }, 2,
               0, precision, 100, out var result1, out abserr, ref tempStorage);

              result += result1;
            }
          }
          else
          {
            error = Calc.Integration.QagpIntegration.Integration(
             PDFFunc,
             new double[] { 0, xm, x1 }, 3,
             0, precision, 100, out result, out abserr, ref tempStorage);
          }
          if (error is not null)
            result = double.NaN;

          return result;
        }
        catch (Exception)
        {
          return double.NaN;
        }
      }

      public double IntegrateAbeEqZero(ref object? tempStorage, double precision)
      {
        GSL_ERROR? error;
        double x1 = UpperIntegrationLimit;
        double xm = 1e-10;

        try
        {
          error = Calc.Integration.QagpIntegration.Integration(
              PDFFunc,
              new double[] { 0, xm }, 2,
              0, precision, 100, out var result, out var abserr, ref tempStorage);

          if (error is null)
          {
            error = Calc.Integration.QagpIntegration.Integration(
             PDFFuncLogInt,
             new double[] { Math.Log(xm), Math.Log(x1) }, 2,
             0, precision, 100, out var result1, out abserr, ref tempStorage);

            result += result1;
          }

          if (error is not null)
            result = double.NaN;

          return result;
        }
        catch (Exception)
        {
          return double.NaN;
        }
      }

      public double UpperIntegrationLimit
      {
        get
        {
          return Math.PI;
        }
      }

      public bool IsMaximumLeftHandSide()
      {
        return PDFCore(0.5 * UpperIntegrationLimit) > 1;
      }

      #region CDF

      public double CDFFunc(double x)
      {
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : Math.Exp(-f);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double CDFFuncLogInt(double z)
      {
        double x = Math.Exp(z) + _x0;
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : Math.Exp(z - f);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double CDFIntegrate(ref object? tempStorage, double precision)
      {
        GSL_ERROR? error1;
        double resultRight, abserrRight;

        double xm;
        if (abe == 0)
          xm = FindIncreasingYEqualTo(PDFCore, 0, UpperIntegrationLimit, PDFCore(0) + 1, 0.1, out var yfound);
        else
          xm = FindIncreasingYEqualToOne(PDFCore, 0, UpperIntegrationLimit);

        if ((xm * 10) < UpperIntegrationLimit)
        {
          // logarithmical integration
          _x0 = -xm;
          // now integrate logarithmically
          error1 = Calc.Integration.QagpIntegration.Integration(
   CDFFuncLogInt,
   new double[] { Math.Log(xm), Math.Log(UpperIntegrationLimit + xm) }, 2, 0, precision, 100, out resultRight, out abserrRight, ref tempStorage);
        }
        else // linear integration
        {
          error1 = Calc.Integration.QagpIntegration.Integration(
   CDFFunc,
   new double[] { 0, UpperIntegrationLimit }, 2, 0, precision, 100, out resultRight, out abserrRight, ref tempStorage);
        }

        if (error1 is not null)
          return double.NaN;
        else
          return resultRight;
      }

      #endregion CDF
    }

    public class Aeq1D
    {
      private double beta;
      private double abe;
      private double logFactorp;
      private double logPdfPrefactor;
      private double _x0;

      public Aeq1D(double x, double beta, double abe)
      {
        this.beta = beta;
        this.abe = abe;
        logFactorp = -0.5 * Math.PI * x / beta;
        logPdfPrefactor = -Math.Log(2 * Math.Abs(beta));
      }

      public double PDFCore(double thetas)
      {
        double r1;
        double r2;

        double abeta = Math.Abs(beta);
        double h = Math.PI * (2 - abe) - 2 * abeta * thetas;
        r1 = Math.Exp(logFactorp + h / (2 * abeta * Math.Tan(thetas)));
        r2 = h / (Math.PI * Math.Sin(thetas));
        double result = r1 * r2;
        if (!(result >= 0))
          result = double.MaxValue;

        //System.Diagnostics.Debug.WriteLine(string.Format("CorAlt1GnI theta={0}, result={1}", thetas, result));
        return result;
      }

      public double PDFCoreDerivativeByCore(double thetas)
      {
        double abeta = Math.Abs(beta);
        double c1 = 2 * abeta / (Math.PI * (1 + abeta) - abeta * thetas);

        double s1 = c1;
        double s2 = 2 / Math.Tan(thetas);
        double s3 = 1 / (c1 * RMath.Pow(Math.Sin(thetas), 2));
        return s1 + s2 + s3;
      }

      public double PDFFunc(double x)
      {
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(-f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double PDFFuncLogInt(double z)
      {
        double x = Math.Exp(z);
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(z - f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double PDFFuncLogIntToLeft(double z)
      {
        double x = _x0 - Math.Exp(z);
        if (x < 0)
          x = 0;

        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(z - f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double PDFFuncLogIntToRight(double z)
      {
        double x = _x0 + Math.Exp(z);
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(z - f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double Integrate(ref object? tempStorage, double precision)
      {
        GSL_ERROR? error;
        double abserr;
        double result;
        double x1 = UpperIntegrationLimit;
        double xm = FindDecreasingYEqualTo(PDFCore, 0, x1, 1, 0.125, out var ym);
        try
        {
          double xwidth = 1 / (PDFCoreDerivativeByCore(xm) * ym);
          if (xwidth * 8 < xm)
          {
            _x0 = xm + xwidth;
            error = Calc.Integration.QagpIntegration.Integration(
               PDFFuncLogIntToLeft,
               new double[] { Math.Log(xwidth), Math.Log(xm + xwidth) }, 2,
               0, precision, 100, out result, out abserr, ref tempStorage);

            if (error is null)
            {
              _x0 = xm - xwidth;
              error = Calc.Integration.QagpIntegration.Integration(
               PDFFuncLogIntToRight,
               new double[] { Math.Log(xwidth), Math.Log(x1 - _x0) }, 2,
               0, precision, 100, out var result1, out abserr, ref tempStorage);

              result += result1;
            }
          }
          else if (xm * 8 < (x1 - xm))
          {
            error = Calc.Integration.QagpIntegration.Integration(
               PDFFunc,
               new double[] { 0, xm }, 2,
               0, precision, 100, out result, out abserr, ref tempStorage);
            if (error is null)
            {
              error = Calc.Integration.QagpIntegration.Integration(
               PDFFuncLogInt,
               new double[] { Math.Log(xm), Math.Log(x1) }, 2,
               0, precision, 100, out var result1, out abserr, ref tempStorage);

              result += result1;
            }
          }
          else
          {
            error = Calc.Integration.QagpIntegration.Integration(
             PDFFunc,
             new double[] { 0, xm, x1 }, 3,
             0, precision, 100, out result, out abserr, ref tempStorage);
          }
          if (error is not null)
            result = double.NaN;

          return result;
        }
        catch (Exception)
        {
          return double.NaN;
        }
      }

      public double UpperIntegrationLimit
      {
        get
        {
          return Math.PI;
        }
      }

      public bool IsMaximumLeftHandSide()
      {
        return PDFCore(0.5 * UpperIntegrationLimit) > 1;
      }

      #region CDF

      public double CDFFunc(double x)
      {
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : OneMinusExp(-f);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double CDFFuncLogInt(double z)
      {
        double x = Math.Exp(z) + _x0;
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : Math.Exp(z) * OneMinusExp(-f);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double CDFIntegrate(ref object? tempStorage, double precision)
      {
        GSL_ERROR? error1;
        double resultRight, abserrRight;

        double xm;
        xm = FindDecreasingYEqualToOne(PDFCore, 0, UpperIntegrationLimit);

        if ((xm * 10) < UpperIntegrationLimit)
        {
          // logarithmical integration
          _x0 = -xm;
          // now integrate logarithmically
          error1 = Calc.Integration.QagpIntegration.Integration(
   CDFFuncLogInt,
   new double[] { Math.Log(xm), Math.Log(UpperIntegrationLimit + xm) }, 2, 0, precision, 100, out resultRight, out abserrRight, ref tempStorage);
        }
        else // linear integration
        {
          error1 = Calc.Integration.QagpIntegration.Integration(
   CDFFunc,
   new double[] { 0, UpperIntegrationLimit }, 2, 0, precision, 100, out resultRight, out abserrRight, ref tempStorage);
        }

        if (error1 is not null)
          return double.NaN;
        else
          return resultRight;
      }

      #endregion CDF
    }

    #endregion Classes for integration (alpha==1)

    #region Classes for integration (alpha>1)

    /// <summary>
    /// Helper class for calculation used if alpha&gt;1, gamma&lt;0 and for increasing integration direction.
    /// </summary>
    public class Agt1GnI
    {
      private double factorp;
      private double factorw;
      private double alpha;
      private double dev;
      private double logPdfPrefactor;
      private double _x0;
      private Func<double, double> pdfCore;
      private Func<double, double> pdfFunc;

      public Agt1GnI(double factorp, double factorw, double logPdfPrefactor, double alpha, double dev)
      {
        this.factorp = factorp;
        this.factorw = factorw;
        this.logPdfPrefactor = logPdfPrefactor;
        this.alpha = alpha;
        this.dev = dev;
        pdfCore = new Func<double, double>(PDFCore);
        pdfFunc = new Func<double, double>(PDFFunc);
      }

      public void Initialize()
      {
        pdfCore = new Func<double, double>(PDFCore);
        pdfFunc = new Func<double, double>(PDFFunc);
      }

      public double PDFCore(double thetas)
      {
        double r1;
        double r2;
        if (dev == 0 && thetas < 1e-9)
        {
          r1 = Math.Pow(factorp / alpha, 1 / (alpha - 1));
          r2 = factorw * (alpha - 1) / alpha;
        }
        else
        {
          double sin_al_theta_dev = Math.Sin(alpha * thetas + dev);
          r1 = Math.Pow(factorp * Math.Sin(thetas) / sin_al_theta_dev, 1 / (alpha - 1));
          r2 = (factorw * Math.Sin(thetas * (alpha - 1) + dev)) / sin_al_theta_dev;
        }
        double result = r1 * r2;
        if (!(result >= 0))
          result = double.MaxValue;

        //System.Diagnostics.Debug.WriteLine(string.Format("Cor1d theta={0}, result={1}", thetas, result));
        return result;
      }

      public double PDFFunc(double x)
      {
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(-f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double PDFFuncLogIntToRight(double z)
      {
        double x = _x0 + Math.Exp(z);
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(z - f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double Integrate(ref object? tempStorage, double precision)
      {
        double xm = FindIncreasingYEqualToOne(pdfCore, 0, UpperIntegrationLimit);
        try
        {
          if (xm * 10 < UpperIntegrationLimit) // then a logarithmic integration from left to right
          {
            var error1 = Calc.Integration.QagpIntegration.Integration(
            pdfFunc,
            new double[] { 0, xm }, 2,
            0, precision, 200, out var result1, out var abserr1, ref tempStorage);
            double xincrement = xm / 10;
            _x0 = xm - xincrement;
            var error2 = Calc.Integration.QagpIntegration.Integration(
                        PDFFuncLogIntToRight,
                        new double[] { Math.Log(xincrement), Math.Log(UpperIntegrationLimit - _x0) }, 2,
                        0, precision, 200, out var result2, out var abserr2, ref tempStorage);

            if (error1 is null && error2 is null)
              return result1 + result2;
            else
              return double.NaN;
          }
          else
          {
            var error = Calc.Integration.QagpIntegration.Integration(
                        pdfFunc,
                        new double[] { 0, xm, UpperIntegrationLimit }, 3,
                        0, precision, 200, out var result, out var abserr, ref tempStorage);

            if (error is null)
              return result;
            else
              return double.NaN;
          }
        }
        catch (Exception)
        {
          return double.NaN;
        }
      }

      public double UpperIntegrationLimit
      {
        get
        {
          return (Math.PI - dev) / alpha;
        }
      }

      public bool IsMaximumLeftHandSide()
      {
        return PDFCore(0.5 * UpperIntegrationLimit) > 1;
      }

      #region CDF

      public double CDFFunc(double x)
      {
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : Math.Exp(-f);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double CDFFuncLogInt(double z)
      {
        double x = Math.Exp(z) + _x0;
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : Math.Exp(z - f);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double CDFIntegrate(ref object? tempStorage, double precision)
      {
        GSL_ERROR? error1;
        double resultRight, abserrRight;

        double xm;
        if (dev == 0)
          xm = FindIncreasingYEqualTo(PDFCore, 0, UpperIntegrationLimit, PDFCore(0) + 1, 0.1, out var yfound);
        else
          xm = FindIncreasingYEqualToOne(PDFCore, 0, UpperIntegrationLimit);

        if ((xm * 10) < UpperIntegrationLimit)
        {
          // logarithmical integration
          _x0 = -xm;
          // now integrate logarithmically
          error1 = Calc.Integration.QagpIntegration.Integration(
                      CDFFuncLogInt,
                      new double[] { Math.Log(xm), Math.Log(UpperIntegrationLimit + xm) }, 2,
                      0, precision, 100, out resultRight, out abserrRight, ref tempStorage);
        }
        else // linear integration
        {
          error1 = Calc.Integration.QagpIntegration.Integration(
                      CDFFunc,
                      new double[] { 0, UpperIntegrationLimit }, 2,
                      0, precision, 100, out resultRight, out abserrRight, ref tempStorage);
        }

        if (error1 is not null)
          return double.NaN;
        else
          return resultRight;
      }

      #endregion CDF
    }

    /// <summary>
    /// Helper class for calculation used if alpha&gt;1, gamma&lt;0 and for decreasing integration direction.
    /// </summary>
    public class Agt1GnD
    {
      private double factorp;
      private double factorw;
      private double alpha;
      private double dev;
      private double logPdfPrefactor;
      private double _x0;

      private Func<double, double> pdfCore;
      private Func<double, double> pdfFunc;

      public Agt1GnD(double factorp, double factorw, double logPdfPrefactor, double alpha, double dev)
      {
        this.factorp = factorp;
        this.factorw = factorw;
        this.logPdfPrefactor = logPdfPrefactor;
        this.alpha = alpha;
        this.dev = dev;
        pdfCore = new Func<double, double>(PDFCore);
        pdfFunc = new Func<double, double>(PDFFunc);
      }

      public void Initialize()
      {
        pdfCore = new Func<double, double>(PDFCore);
        pdfFunc = new Func<double, double>(PDFFunc);
      }

      public double PDFCore(double thetas)
      {
        double r1;
        double r2;
        double sin_al_theta = Math.Sin(alpha * thetas);
        double pi_mdev_byalpha = (Math.PI - dev) / alpha;

        {
          r1 = Math.Pow(factorp * Math.Sin(pi_mdev_byalpha - thetas) / sin_al_theta, 1 / (alpha - 1));
          r2 = (factorw * Math.Sin((dev + (alpha - 1) * (Math.PI - alpha * thetas)) / alpha)) / sin_al_theta;
        }
        double result = r1 * r2;
        if (!(result >= 0))
          result = double.MaxValue;

        //System.Diagnostics.Debug.WriteLine(string.Format("Cor1d theta={0}, result={1}", thetas, result));
        return result;
      }

      public double PDFFunc(double x)
      {
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(-f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double PDFFuncLogIntToRight(double z)
      {
        double x = _x0 + Math.Exp(z);
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(z - f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double Integrate(ref object? tempStorage, double precision)
      {
        double xm = FindDecreasingYEqualToOne(pdfCore, 0, UpperIntegrationLimit);
        try
        {
          if (xm * 10 < UpperIntegrationLimit) // then a logarithmic integration from left to right
          {
            var error1 = Calc.Integration.QagpIntegration.Integration(
            pdfFunc,
            new double[] { 0, xm }, 2,
            0, precision, 200, out var result1, out var abserr1, ref tempStorage);
            double xincrement = xm / 10;
            _x0 = xm - xincrement;
            var error2 = Calc.Integration.QagpIntegration.Integration(
                        PDFFuncLogIntToRight,
                        new double[] { Math.Log(xincrement), Math.Log(UpperIntegrationLimit - _x0) }, 2,
                        0, precision, 200, out var result2, out var abserr2, ref tempStorage);

            if (error1 is null && error2 is null)
              return result1 + result2;
            else
              return double.NaN;
          }
          else
          {
            var error = Calc.Integration.QagpIntegration.Integration(
                        pdfFunc,
                        new double[] { 0, xm, UpperIntegrationLimit }, 3,
                        0, precision, 200, out var result, out var abserr, ref tempStorage);

            if (error is null)
              return result;
            else
              return double.NaN;
          }
        }
        catch (Exception)
        {
          return double.NaN;
        }
      }

      public double UpperIntegrationLimit
      {
        get
        {
          return (Math.PI - dev) / alpha;
        }
      }

      #region CDF

      public double CDFFunc(double x)
      {
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : OneMinusExp(-f);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double CDFFuncLogInt(double z)
      {
        double x = Math.Exp(z) + _x0;
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : Math.Exp(z) * OneMinusExp(-f);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double CDFIntegrate(ref object? tempStorage, double precision)
      {
        GSL_ERROR? error1;
        double resultRight, abserrRight;

        double xm;

        xm = FindDecreasingYEqualToOne(PDFCore, 0, UpperIntegrationLimit);

        if ((xm * 10) < UpperIntegrationLimit)
        {
          // logarithmical integration
          _x0 = -xm;
          // now integrate logarithmically
          error1 = Calc.Integration.QagpIntegration.Integration(
                      CDFFuncLogInt,
                      new double[] { Math.Log(xm), Math.Log(UpperIntegrationLimit + xm) }, 2,
                      0, precision, 100, out resultRight, out abserrRight, ref tempStorage);
        }
        else // linear integration
        {
          error1 = Calc.Integration.QagpIntegration.Integration(
                      CDFFunc,
                      new double[] { 0, UpperIntegrationLimit }, 2,
                      0, precision, 100, out resultRight, out abserrRight, ref tempStorage);
        }

        if (error1 is not null)
          return double.NaN;
        else
          return resultRight;
      }

      #endregion CDF
    }

    /// <summary>
    /// Helper class for calculation used if alpha&gt;1, gamma&gt;=0 and for increasing integration direction.
    /// </summary>
    public class Agt1GpI
    {
      private double factorp;
      private double factorw;
      private double alpha;
      private double dev;
      private double logPdfPrefactor;
      private double _x0;

      private Func<double, double> pdfCore;
      private Func<double, double> pdfFunc;

      public Agt1GpI(double factorp, double factorw, double logPdfPrefactor, double alpha, double dev)
      {
        this.factorp = factorp;
        this.factorw = factorw;
        this.logPdfPrefactor = logPdfPrefactor;
        this.alpha = alpha;
        this.dev = dev;
        pdfCore = new Func<double, double>(PDFCore);
        pdfFunc = new Func<double, double>(PDFFunc);
      }

      public void Initialize()
      {
        pdfCore = new Func<double, double>(PDFCore);
        pdfFunc = new Func<double, double>(PDFFunc);
      }

      public double PDFCore(double thetas)
      {
        double r1;
        double r2;
        double sin_al_devMthetas = Math.Sin(alpha * (dev - thetas));
        {
          r1 = Math.Pow(factorp * Math.Sin(thetas) / sin_al_devMthetas, 1 / (alpha - 1));
          r2 = (factorw * Math.Sin(alpha * dev - (alpha - 1) * thetas)) / sin_al_devMthetas;
        }
        double result = r1 * r2;
        if (!(result >= 0))
          result = double.MaxValue;

        //System.Diagnostics.Debug.WriteLine(string.Format("Cor1d theta={0}, result={1}", thetas, result));
        return result;
      }

      public double PDFFunc(double x)
      {
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(-f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double PDFFuncLogIntToRight(double z)
      {
        double x = _x0 + Math.Exp(z);
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(z - f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double Integrate(ref object? tempStorage, double precision)
      {
        double xm = FindIncreasingYEqualToOne(pdfCore, 0, UpperIntegrationLimit);
        try
        {
          if (xm * 10 < UpperIntegrationLimit) // then a logarithmic integration from left to right
          {
            var error1 = Calc.Integration.QagpIntegration.Integration(
            pdfFunc,
            new double[] { 0, xm }, 2,
            0, precision, 200, out var result1, out var abserr1, ref tempStorage);
            double xincrement = xm / 10;
            _x0 = xm - xincrement;
            var error2 = Calc.Integration.QagpIntegration.Integration(
                        PDFFuncLogIntToRight,
                        new double[] { Math.Log(xincrement), Math.Log(UpperIntegrationLimit - _x0) }, 2,
                        0, precision, 200, out var result2, out var abserr2, ref tempStorage);

            if (error1 is null && error2 is null)
              return result1 + result2;
            else
              return double.NaN;
          }
          else
          {
            var error = Calc.Integration.QagpIntegration.Integration(
                        pdfFunc,
                        new double[] { 0, xm, UpperIntegrationLimit }, 3,
                        0, precision, 200, out var result, out var abserr, ref tempStorage);

            if (error is null)
              return result;
            else
              return double.NaN;
          }
        }
        catch (Exception)
        {
          return double.NaN;
        }
      }

      public double UpperIntegrationLimit
      {
        get
        {
          return dev;
        }
      }

      #region CDF

      public double CDFFunc(double x)
      {
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : OneMinusExp(-f);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double CDFFuncLogInt(double z)
      {
        double x = Math.Exp(z) + _x0;
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : Math.Exp(z) * OneMinusExp(-f);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double CDFIntegrate(ref object? tempStorage, double precision)
      {
        GSL_ERROR? error1;
        double resultRight, abserrRight;

        double xm;

        xm = FindIncreasingYEqualToOne(PDFCore, 0, UpperIntegrationLimit);

        if ((xm * 10) < UpperIntegrationLimit)
        {
          // logarithmical integration
          _x0 = -xm;
          // now integrate logarithmically
          error1 = Calc.Integration.QagpIntegration.Integration(
                      CDFFuncLogInt,
                      new double[] { Math.Log(xm), Math.Log(UpperIntegrationLimit + xm) }, 2,
                      0, precision, 100, out resultRight, out abserrRight, ref tempStorage);
        }
        else // linear integration
        {
          error1 = Calc.Integration.QagpIntegration.Integration(
                      CDFFunc,
                      new double[] { 0, UpperIntegrationLimit }, 2,
                      0, precision, 100, out resultRight, out abserrRight, ref tempStorage);
        }

        if (error1 is not null)
          return double.NaN;
        else
          return resultRight;
      }

      #endregion CDF
    }

    /// <summary>
    /// Helper class for calculation used if alpha&gt;1, gamma&gt;=0 and for decreasing integration direction.
    /// </summary>
    public class Agt1GpD
    {
      private double factorp;
      private double factorw;
      private double alpha;
      private double dev;
      private double logPdfPrefactor;
      private double _x0;

      private Func<double, double> pdfCore;
      private Func<double, double> pdfFunc;

      public Agt1GpD(double factorp, double factorw, double logPdfPrefactor, double alpha, double dev)
      {
        this.factorp = factorp;
        this.factorw = factorw;
        this.logPdfPrefactor = logPdfPrefactor;
        this.alpha = alpha;
        this.dev = dev;
        pdfCore = new Func<double, double>(PDFCore);
        pdfFunc = new Func<double, double>(PDFFunc);
      }

      public void Initialize()
      {
        pdfCore = new Func<double, double>(PDFCore);
        pdfFunc = new Func<double, double>(PDFFunc);
      }

      public double PDFCore(double thetas)
      {
        double r1;
        double r2;
        double sin_al_thetas = Math.Sin(alpha * thetas);
        {
          r1 = Math.Pow(factorp * Math.Sin(dev - thetas) / sin_al_thetas, 1 / (alpha - 1));
          r2 = (factorw * Math.Sin(dev + (alpha - 1) * thetas)) / sin_al_thetas;
        }
        double result = r1 * r2;
        if (!(result >= 0))
          result = double.MaxValue;

        //System.Diagnostics.Debug.WriteLine(string.Format("Cor1d theta={0}, result={1}", thetas, result));
        return result;
      }

      public double PDFFunc(double x)
      {
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(-f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double PDFFuncLogIntToRight(double z)
      {
        double x = _x0 + Math.Exp(z);
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : f * Math.Exp(z - f + logPdfPrefactor);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double Integrate(ref object? tempStorage, double precision)
      {
        double xm = FindDecreasingYEqualToOne(pdfCore, 0, UpperIntegrationLimit);
        try
        {
          if (xm * 10 < UpperIntegrationLimit) // then a logarithmic integration from left to right
          {
            var error1 = Calc.Integration.QagpIntegration.Integration(
            pdfFunc,
            new double[] { 0, xm }, 2,
            0, precision, 200, out var result1, out var abserr1, ref tempStorage);
            double xincrement = xm / 10;
            _x0 = xm - xincrement;
            var error2 = Calc.Integration.QagpIntegration.Integration(
                        PDFFuncLogIntToRight,
                        new double[] { Math.Log(xincrement), Math.Log(UpperIntegrationLimit - _x0) }, 2,
                        0, precision, 200, out var result2, out var abserr2, ref tempStorage);

            if (error1 is null && error2 is null)
              return result1 + result2;
            else
              return double.NaN;
          }
          else
          {
            var error = Calc.Integration.QagpIntegration.Integration(
                        pdfFunc,
                        new double[] { 0, xm, UpperIntegrationLimit }, 3,
                        0, precision, 200, out var result, out var abserr, ref tempStorage);

            if (error is null)
              return result;
            else
              return double.NaN;
          }
        }
        catch (Exception)
        {
          return double.NaN;
        }
      }

      public double UpperIntegrationLimit
      {
        get
        {
          return dev;
        }
      }

      #region CDF

      public double CDFFunc(double x)
      {
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : OneMinusExp(-f);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double CDFFuncLogInt(double z)
      {
        double x = Math.Exp(z) + _x0;
        double f = PDFCore(x);
        double r = double.IsInfinity(f) ? 0 : Math.Exp(z) * OneMinusExp(-f);
        //System.Diagnostics.Debug.WriteLine(string.Format("x={0}, f={1}, r={2}", x, f, r));
        //Current.Console.WriteLine("x={0}, f={1}, r={2}", x, f, r);
        return r;
      }

      public double CDFIntegrate(ref object? tempStorage, double precision)
      {
        GSL_ERROR? error1;
        double resultRight, abserrRight;

        double xm;

        xm = FindDecreasingYEqualToOne(PDFCore, 0, UpperIntegrationLimit);

        if ((xm * 10) < UpperIntegrationLimit)
        {
          // logarithmical integration
          _x0 = -xm;
          // now integrate logarithmically
          error1 = Calc.Integration.QagpIntegration.Integration(
                      CDFFuncLogInt,
                      new double[] { Math.Log(xm), Math.Log(UpperIntegrationLimit + xm) }, 2,
                      0, precision, 100, out resultRight, out abserrRight, ref tempStorage);
        }
        else // linear integration
        {
          error1 = Calc.Integration.QagpIntegration.Integration(
                      CDFFunc,
                      new double[] { 0, UpperIntegrationLimit }, 2,
                      0, precision, 100, out resultRight, out abserrRight, ref tempStorage);
        }

        if (error1 is not null)
          return double.NaN;
        else
          return resultRight;
      }

      #endregion CDF
    }

    #endregion Classes for integration (alpha>1)
  }
}
