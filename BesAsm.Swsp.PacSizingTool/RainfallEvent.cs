using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace BesAsm.Swsp.PacSizingTool
{
  /// <summary>
  /// Defines a rainfall event for use in the hydrology calculations
  /// </summary>
  public class RainfallEvent
  {
    private string _eventName;
    private IList<double> _rainfallInches;
    private double _timeIntervalMinutes;

    internal RainfallEvent(string eventName, IEnumerable<double> rainfallInches, double timeIntervalMinutes)
    {
      _eventName = eventName;
      _rainfallInches = rainfallInches.ToList();
      _timeIntervalMinutes = timeIntervalMinutes;
    }

    internal int Length
    {
      get { return _rainfallInches.Count(); }
    }

    internal string EventName
    {
      get { return _eventName; }
    }

    internal double TimeIntervalMinutes
    {
      get { return _timeIntervalMinutes; }
    }

    internal IList<double> RainfallInches
    {
      get { return _rainfallInches; }
    }

    internal double[] AsArray()
    {
      return _rainfallInches.ToArray();
    }

    internal double TotalInches
    {
      get
      {
        return _rainfallInches.Sum();
      }
    }


    /// <summary>
    /// Provides a 24-hour SCS Type 1A RainfallEvent, given the total depth of the design storm.
    /// </summary>
    /// <param name="name">
    /// The name of the rainfall event. e.g. "2-year storm"
    /// </param>
    /// <param name="totalRainfallInches">
    /// The total rainfall depth for the entire storm, disregarding initial abstraction.
    /// </param>
    /// <returns></returns>
    public static RainfallEvent GetScsOneAEvent(string name, double totalRainfallInches)
    {
      double[] rainfall = new double[RainfallEvent.ScsOneARainfallDistribution.Length];

      for (int i = 0; i < RainfallEvent.ScsOneARainfallDistribution.Length; i++)
        rainfall[i] = RainfallEvent.ScsOneARainfallDistribution[i] * totalRainfallInches;

      return new RainfallEvent(name, ScsOneARainfallDistribution.Select(p => p * totalRainfallInches), 10);
    }

    /// <summary>
    /// The Soil and Conservation Service (NRCS) Type 1A synthetic rainfall hyetograph
    /// provided in 10-minute intervals.
    /// Each element represents the fraction of the total depth of the storm.
    /// </summary>
    public static readonly double[] ScsOneARainfallDistribution = {
        0.000,	0.004,	0.004,	0.004,	0.004,	0.004,	0.004,	0.004,	0.004,	0.004,	0.004,	0.005,	0.005,	0.005,	0.005,
        0.005,	0.005,	0.006,	0.006,	0.006,	0.006,	0.006,	0.006,	0.007,	0.007,	0.007,	0.007,	0.007,	0.007,	0.0082,	
        0.0082,	0.0082,	0.0082,	0.0082,	0.0082,	0.0095,	0.0095,	0.0095,	0.0095,	0.0095,	0.0095,	0.0134,	0.0134,	0.0134,	0.018,	
        0.018,	0.034,	0.054,	0.027,	0.018,	0.0134,	0.0134,	0.0134,	0.0088,	0.0088,	0.0088,	0.0088,	0.0088,	0.0088,	0.0088,	
        0.0088,	0.0088,	0.0088,	0.0088,	0.0088,	0.0072,	0.0072,	0.0072,	0.0072,	0.0072,	0.0072,	0.0072,	0.0072,	0.0072,	0.0072,	
        0.0072,	0.0072,	0.0057,	0.0057,	0.0057,	0.0057,	0.0057,	0.0057,	0.0057,	0.0057,	0.0057,	0.0057,	0.0057,	0.0057,	0.005,	
        0.005,	0.005,	0.005,	0.005,	0.005,	0.005,	0.005,	0.005,	0.005,	0.005,	0.005,	0.004,	0.004,	0.004,	0.004,	
        0.004,	0.004,	0.004,	0.004,	0.004,	0.004,	0.004,	0.004,	0.004,	0.004,	0.004,	0.004,	0.004,	0.004,	0.004,	
        0.004,	0.004,	0.004,	0.004,	0.004,	0.004,	0.004,	0.004,	0.004,	0.004,	0.004,	0.004,	0.004,	0.004,	0.004,	
        0.004,	0.004,	0.004,	0.004,	0.004,	0.004,	0.004,	0.004,	0.004,	0.004,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	
        0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	
        0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,  0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	
        0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	
        0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	
        0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0,	0
      };
  }
}
