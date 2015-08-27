using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BesAsm.Swsp.PacSizingTool
{
  /// <summary>
  /// Contains a hydrograph, which is a series of points representing a flow rate over time.
  /// </summary>
  public class Hydrograph
  {
    private double _timeStepMinutes;

    private const bool SANITIZEINPUT = false;

    private List<double> _hydrographData;
    private string _name;
    private string _units;
    /// <summary>
    /// Constructs a Hydrograph from an existing list. The name of the Hydrograph will be a random unique identifier.
    /// </summary>
    /// <param name="list">The existing list to be loaded in to the Hydrograph</param>
    /// <param name="timeStepMinutes">The time period for each flow value in the hydrograph</param>
    internal Hydrograph(IEnumerable<double> list, double timeStepMinutes)
    {
      _name = Guid.NewGuid().ToString();
      _units = Guid.NewGuid().ToString();
      _hydrographData = new List<double>(list);
      _timeStepMinutes = timeStepMinutes;
    }
    /// <summary>
    /// Constructs a Hydrograph from an existing list.
    /// </summary>
    /// <param name="name">The name to assign to the hydrograph</param>
    /// <param name="units">The units to assign to the hydrograph</param>
    /// <param name="list">The existing list to be loaded in to the Hydrograph</param>
    /// <param name="timeStepMinutes">The time period for each flow value in the hydrograph</param>
    internal Hydrograph(string name, string units, IEnumerable<double> list, double timeStepMinutes)
      : this(list, timeStepMinutes)
    {
      _name = name;
      _units = units;
    }
    /// <summary>
    /// Constructs a new Hydrograph filled with default data
    /// </summary>
    /// <param name="name">The name to assign to the hydrograph</param>
    /// <param name="units">The units to assign to the hydrograph</param>
    /// <param name="timeStepMinutes">The time period for each flow value in the hydrograph</param>
    /// <param name="length">The number of data points to create in the new Hydrograph</param>
    /// <param name="defaultValue">The default data with which to populate the new Hydrograph</param>
    internal Hydrograph(string name, string units, double timeStepMinutes, int length = 0, double defaultValue = 0)
      : this(name, units, new double[length].Select(p => defaultValue), timeStepMinutes)
    {
    }
    /// <summary>
    /// Gets the name of the Hydrograph
    /// </summary>
    public string Name
    {
      get { return _name; }
    }
    /// <summary>
    /// Gets the units of the Hydrograph
    /// </summary>
    public string Units
    {
        get { return _units; }
    }

    /// <summary>
    /// Gets the Time Step of the hydrograph, in minutes.
    /// </summary>
    public double TimeStepMinutes
    {
      get { return _timeStepMinutes; }
      set
      {
        if (value <= 0 && SANITIZEINPUT) throw new ArgumentException("Time Step must be greater than zero");
        _timeStepMinutes = value;
      }
    }

    internal Hydrograph Select(Func<double, double> selector)
    {
      return new Hydrograph(((IEnumerable<double>)this).Select(p => selector(p)), _timeStepMinutes);
    }

    internal Hydrograph Combine(Hydrograph h2)
    {
      return new Hydrograph(_hydrographData.PairwiseAddition(h2.AsArray()), _timeStepMinutes);
    }
    /// <summary>
    /// Converts data in the Hydrograph to an array of double-precision values.
    /// </summary>
    /// <returns></returns>
    public double[] AsArray()
    {
      return _hydrographData.ToArray();
    }


  }


}
