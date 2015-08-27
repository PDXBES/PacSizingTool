using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BesAsm.Swsp.PacSizingTool
{
  /// <summary>
  /// Contains SBUH hydrology calcutions
  /// </summary>
  public static class SantaBarbaraUrbanHydrograph
  {
    /// <summary>
    /// Performs SBUH calculations for a Catchment
    /// </summary>
    /// <param name="catchment"></param>
    /// <param name="rainfallEvent"></param>
    /// <returns></returns>
    public static Hydrograph CalculateHydrograph(Catchment catchment, RainfallEvent rainfallEvent)
    {
      double A = catchment.ImperviousAreaSquareFeet / 43560;
      double dt = rainfallEvent.TimeIntervalMinutes;
      double Tc = catchment.TimeOfConcentrationMinutes;
      double CN = catchment.CurveNumber;

      double omega = dt / (2 * Tc + dt);      
      double S = (1000 / CN) - 10;
      

      IList<double> accumulatedRainfall = rainfallEvent.RainfallInches.Accumulate();
      IList<double> accumulatedRunoff = new List<double>().Initialize(rainfallEvent.Length);


      for (int i = 0; i < rainfallEvent.Length; i++)
      {
        if (accumulatedRainfall[i] < 0.2 * S)
          accumulatedRunoff[i] = 0;
        else
          accumulatedRunoff[i] = Math.Pow(accumulatedRainfall[i] - 0.2 * S, 2)
            / (accumulatedRainfall[i] + 0.8 * S);
      }

      IList<double> incrementalRunoff = accumulatedRunoff.Deaccumulate();

      IList<double> instantaneousHydrograph = incrementalRunoff.Select(p => p * 60.5 * A / dt).ToList();

      IList<double> designHydrograph = new List<double>().Initialize(rainfallEvent.Length);
      
      for (int i = 1; i < rainfallEvent.Length - 1; i++)
      {
        designHydrograph[i] = designHydrograph[i-1] + omega*(instantaneousHydrograph[i] + instantaneousHydrograph[i-1]-2*designHydrograph[i-1]);
      }

      string name = string.Format("{0} {1}", catchment.Name, rainfallEvent.EventName);
      return new Hydrograph(name, "cfs", designHydrograph, dt);
    }

  }


}
