using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BesAsm.Swsp.PacSizingTool
{
  /// <summary>
  /// Contains the reservour routing calculations for analzying stormwater facilty performance
  /// </summary>
  public static class ReservoirRouter
  {
    /// <summary>
    /// Contains algorithims implementing the sizing calculations for various facility configurations
    /// </summary>
    /// <param name="facility"></param>
    /// <param name="category"></param>
    /// <param name="catchment"></param>
    /// <param name="inflowHydrograph"></param>
    /// <returns></returns>
    public static StormEventResults PerformCalculations(Facility facility, HierarchyCategory category, Catchment catchment, Hydrograph inflowHydrograph)
    {
      string message;
      Facility.Validate(facility, Convert.ToInt32(category), out message);

      if (!facility.IsValid)
        throw new ArgumentException("Unable to perform calculations: failed validation with message '" + message + "'");

      double dt = inflowHydrograph.TimeStepMinutes;
      int timeSteps = inflowHydrograph.AsArray().Count();

      double inflowFromRain;
      double inflowVolume;
      double inflowVolumeCummulative = 0;

      double[] surfaceInfiltrationCapacity = new double[timeSteps]; //Column E, aka Percolation Capacity
      double initialInfiltrationToAmendedSoil;
      double[] storedToBeInfiltrated = new double[timeSteps];

      double graphLocator = 0;
      double potentialExtraInfiltration;

      double cumulativeInfiltrationVolume = 0; //Column J

      double additionalInfiltrationFromStorage; //Column K

      double[] totalInfiltrationCapacityToBelowGrade = new double[timeSteps]; //Column L

      double inflowToSurfaceStorageAfterInfiltration; //Column M

      double[] inflowMinusInfiltration = new double[timeSteps]; //Column N

      double surfaceStorageCumulativeVolume = 0; //Column O,P,Q,R combined.

      double[] flowOvertoppingToUnderdrain = new double[timeSteps]; //Column T (or Column W for type E)

      double[] infiltrationToBelowGrade = new double[timeSteps]; //Column Z
      double[] totalFlowToBelowGrade = new double[timeSteps]; //Column AA
      double[] inflowToRockStorage = new double[timeSteps]; //column AB

      double[] totalInfiltrationCapacityToNative = new double[timeSteps]; //Column AE
      double totalInfiltratedToNative = 0;

      double rockStorageCumulativeVolume = 0; //Column AJ

      double[] rockPercentCapacity = new double[timeSteps]; //Column AK/AL for A,B&E facilities; Column AR/AS for C&F;

      double excessRockCumulativeVolume = 0; //Column AM
      double underdrainStorageAreaCumulativeVolume = 0; //Column AR

      double[] aboveGradeStoragePercentCapacity = new double[timeSteps]; //Column AO for A,B facilities; Column S for C,D&F
      double[] aboveGradeSecondaryStoragePercentCapacity = new double[timeSteps]; //Column AO for E facilities

      double[] rockOverflowToEscapeRoute = new double[timeSteps]; //Column AP
      double[] overflowToEscapeRoute = new double[timeSteps]; //Column AQ for A,B,D,E,F facilities; Column AT for C&F

      double nativeInfiltrationRate = facility.NativeSoilInfiltrationCapacityCfs();
      double growingMediumInfiltrationRate = facility.ImportedMediumInfiltrationCapacityCfs();

      //The Lag Index is a property of the growing medium depth and infiltration rate that
      //corresponds to the number of time steps it will take for water to percolate through the
      //growing medium. The infiltration hydrograph to the rock medium is delayed by the lag index.
      //For facility configurations A, B, and E, no lag is applied when the native infiltration rate is less
      //than the growing medium infilration rate.
      double lagFactor =
        (facility.Configuration == FacilityConfiguration.A ||
        facility.Configuration == FacilityConfiguration.B ||
        facility.Configuration == FacilityConfiguration.E) &&
        nativeInfiltrationRate < growingMediumInfiltrationRate ? 0 :
        facility.GrowingMediumPorespace;

      double lagTime = facility.GrowingMediumDepthIn / catchment.ImportedMediumInfiltrationInchesPerHour * 60 * lagFactor;
      int lag = (int)Math.Ceiling(lagTime / inflowHydrograph.TimeStepMinutes); // Rounding up is perfored in the current calculator. This may be unnecessary.

      for (int i = 0; i < timeSteps; i++)
      {
        inflowFromRain = inflowHydrograph.AsArray()[i];
        inflowVolume = inflowFromRain * 600;
        inflowVolumeCummulative += inflowVolume;

        //Facility configurations A,B, and D have rock-influenced surface storage demand. When
        //the rock storage is full, infiltration rates in the growing medium can be limited by
        //infiltration rates of the native soil. 
        if (facility.HasRockInfluencedSurfaceStorage && rockPercentCapacity[Math.Max(1, i - 1)] >= 1)
          surfaceInfiltrationCapacity[i] = Math.Min(growingMediumInfiltrationRate, nativeInfiltrationRate);
        else
          surfaceInfiltrationCapacity[i] = growingMediumInfiltrationRate;

        initialInfiltrationToAmendedSoil =
          Math.Min(inflowFromRain, surfaceInfiltrationCapacity[i]);

        storedToBeInfiltrated[i] = Math.Max(inflowFromRain - initialInfiltrationToAmendedSoil, 0) * 600;
    
        graphLocator =
        storedToBeInfiltrated[i] - storedToBeInfiltrated[Math.Max(1, i - 1)] < 0 ?
        1 : graphLocator;

        potentialExtraInfiltration =
          (surfaceInfiltrationCapacity[i] - initialInfiltrationToAmendedSoil) * graphLocator;

        cumulativeInfiltrationVolume += (potentialExtraInfiltration * 600);

          // Existing spreadsheet rounds up storageToBeInfiltrated to nearest ten, 
          // allowing more potentialExtraInfiltration to fill rock storage. This may be unnecessary...
        double storedToBeInfiltratedRoundedUp = Math.Ceiling(storedToBeInfiltrated.Sum()/10)*10;
        double cumulativeInfiltrationVolumeRounded = Math.Round(cumulativeInfiltrationVolume, 10); // Added 6/24/2015 to deal with binary storage of floating point numbers without changing to decimal data type

        additionalInfiltrationFromStorage =
          cumulativeInfiltrationVolumeRounded > storedToBeInfiltratedRoundedUp || cumulativeInfiltrationVolumeRounded > facility.SurfaceCapacityAtDepth1CuFt ?
          0 : potentialExtraInfiltration;

        totalInfiltrationCapacityToBelowGrade[i] = initialInfiltrationToAmendedSoil + additionalInfiltrationFromStorage;

        inflowToSurfaceStorageAfterInfiltration = Math.Max(inflowFromRain - totalInfiltrationCapacityToBelowGrade[i], 0);

        inflowMinusInfiltration[i] = inflowFromRain - surfaceInfiltrationCapacity[i];

        surfaceStorageCumulativeVolume += (inflowMinusInfiltration[i] * 600);

        surfaceStorageCumulativeVolume = Math.Max(surfaceStorageCumulativeVolume, 0);
        surfaceStorageCumulativeVolume = Math.Min(surfaceStorageCumulativeVolume, facility.SurfaceCapacityAtDepth1CuFt);

        flowOvertoppingToUnderdrain[i] =
          surfaceStorageCumulativeVolume < facility.SurfaceCapacityAtDepth1CuFt ?
          0 : inflowToSurfaceStorageAfterInfiltration;

        if (i - lag < 0)
            infiltrationToBelowGrade[i] = totalInfiltrationCapacityToBelowGrade[0];
        else
            infiltrationToBelowGrade[i] = totalInfiltrationCapacityToBelowGrade[i - lag];

        totalFlowToBelowGrade[i] = infiltrationToBelowGrade[i] + flowOvertoppingToUnderdrain[i];

        if (facility.Configuration == FacilityConfiguration.E || facility.Configuration == FacilityConfiguration.F)
          inflowToRockStorage[i] = totalFlowToBelowGrade[i];
        else
          inflowToRockStorage[i] = infiltrationToBelowGrade[i];

        totalInfiltrationCapacityToNative[i] = nativeInfiltrationRate;

        if (rockStorageCumulativeVolume + inflowToRockStorage[i] - totalInfiltrationCapacityToNative[i] < 0)
            totalInfiltratedToNative += rockStorageCumulativeVolume + inflowToRockStorage[i];
        else
            totalInfiltratedToNative += totalInfiltrationCapacityToNative[i];

        rockStorageCumulativeVolume += ((inflowToRockStorage[i] - totalInfiltrationCapacityToNative[i]) * 600);

        excessRockCumulativeVolume = rockStorageCumulativeVolume < facility.RockStorageCapacityCuFt ?
          0 : rockStorageCumulativeVolume - facility.RockStorageCapacityCuFt;

        rockStorageCumulativeVolume = Math.Max(rockStorageCumulativeVolume, 0);

        if (facility.HasRockInfluencedSurfaceStorage)
        {
          aboveGradeStoragePercentCapacity[i] =
          (surfaceStorageCumulativeVolume + excessRockCumulativeVolume) /
          facility.SurfaceCapacityAtDepth1CuFt;
          //E facilities have a secondary storage volume
          if (facility.HasSecondaryOverflow)
          {
            aboveGradeSecondaryStoragePercentCapacity[i] =
            (surfaceStorageCumulativeVolume + excessRockCumulativeVolume) /
            facility.SurfaceCapacityAtDepth2CuFt;
            aboveGradeSecondaryStoragePercentCapacity[i] = Math.Min(aboveGradeSecondaryStoragePercentCapacity[i], 1);
          }
        }
        //C, D, and F facilities have a direct connection from the rock gallery to an overflow,
        //and therefore above grade storage capacity is independent of rock gallery volume
        else
        {
          aboveGradeStoragePercentCapacity[i] =
            surfaceStorageCumulativeVolume / facility.SurfaceCapacityAtDepth1CuFt;
        }
        aboveGradeStoragePercentCapacity[i] = Math.Min(aboveGradeStoragePercentCapacity[i], 1);

        if (facility.HasRockInfluencedSurfaceStorage && !facility.HasSecondaryOverflow) // A, B
        {
            // Added 6/22/2015 to handle case where surface is full, but should overflow more due to limited rock gallery.
            // This will occur only once during a storm, as the next cycle will limit above grade infiltration to the rock gallery 
            // and overflow the correct amount.
            double belowGradeInflowMinusInfiltration = inflowToRockStorage[i] - totalInfiltrationCapacityToNative[i];  
            if (aboveGradeStoragePercentCapacity[i] == 1 && belowGradeInflowMinusInfiltration > 0 )
                rockOverflowToEscapeRoute[i] = belowGradeInflowMinusInfiltration;
            else
                rockOverflowToEscapeRoute[i] = 0;

            rockPercentCapacity[i] = facility.HasRockStorage ?
              rockStorageCumulativeVolume / facility.RockStorageCapacityCuFt : 1;
            rockPercentCapacity[i] = Math.Min(rockPercentCapacity[i], 1);
        }
        else if (facility.HasSecondaryOverflow) // E
        {
          rockOverflowToEscapeRoute[i] = aboveGradeSecondaryStoragePercentCapacity[i] < 1 ? 0 :
          Math.Max(totalFlowToBelowGrade[i] - facility.NativeSoilInfiltrationCapacityCfs(), 0);

          rockPercentCapacity[i] = facility.HasRockStorage ?
            rockStorageCumulativeVolume / facility.RockStorageCapacityCuFt : 1;
          rockPercentCapacity[i] = Math.Min(rockPercentCapacity[i], 1);
        }
        else if (!facility.HasRockInfluencedSurfaceStorage) // C, D & F
        {
          underdrainStorageAreaCumulativeVolume +=
            ((inflowToRockStorage[i] - totalInfiltrationCapacityToNative[i]) * 600);
          underdrainStorageAreaCumulativeVolume = Math.Max(underdrainStorageAreaCumulativeVolume, 0);
          underdrainStorageAreaCumulativeVolume = Math.Min(underdrainStorageAreaCumulativeVolume, facility.RockStorageCapacityCuFt);

          if ((underdrainStorageAreaCumulativeVolume / facility.RockStorageCapacityCuFt >= 1) || facility.RockStorageCapacityCuFt == 0) // Added facility.RockStorageCapacityCuFt for 0/0 case
            rockOverflowToEscapeRoute[i] = inflowToRockStorage[i] - totalInfiltrationCapacityToNative[i];

          rockPercentCapacity[i] = facility.HasRockStorage ?
            underdrainStorageAreaCumulativeVolume / facility.RockStorageCapacityCuFt : 1;
          rockPercentCapacity[i] = Math.Min(rockPercentCapacity[i], 1);
        }

        if (facility.Configuration == FacilityConfiguration.F // Facility F is different, in that overflow from the surface goes to the subsurface
              || facility.Configuration == FacilityConfiguration.E) // Facility E is strange also, in that the flow overtopping to the underdrain doesn't also go to the escape route.
            overflowToEscapeRoute[i] = excessRockCumulativeVolume > 0 ? rockOverflowToEscapeRoute[i] : 0;
        else
            overflowToEscapeRoute[i] = excessRockCumulativeVolume > 0 ?
                flowOvertoppingToUnderdrain[i] + rockOverflowToEscapeRoute[i] : flowOvertoppingToUnderdrain[i];
      }

      StormEventResults results = new StormEventResults();

      results.PeakSurfaceOverflow = flowOvertoppingToUnderdrain.Max();
      results.PeakOverflow = overflowToEscapeRoute.Max();
      results.OverflowVolume = overflowToEscapeRoute.Sum() * 600;

      results.PercentSurfaceCapacityUsed = aboveGradeStoragePercentCapacity.Max();
      results.PercentRockCapacityUsed = rockPercentCapacity.Max();

      results.InflowVolume = inflowHydrograph.AsArray().Sum() * 600;
      results.PeakInflowRate = inflowHydrograph.AsArray().Max();

      results.AboveGradePrimaryResults.Add(new Hydrograph("Inflow from rain", "cfs", inflowHydrograph.AsArray(), dt));
      results.AboveGradePrimaryResults.Add(new Hydrograph("Infiltration capacity", "cfs", surfaceInfiltrationCapacity, dt));

      switch (facility.Configuration)
      {
          case FacilityConfiguration.A:
              results.AboveGradePrimaryResults.Add(new Hydrograph("Infiltration to native soil", "cfs", infiltrationToBelowGrade, dt));
              results.AboveGradePrimaryResults.Add(new Hydrograph("Overflow to approved discharge", "cfs", overflowToEscapeRoute, dt));
              results.AboveGradeSecondaryResults.Add(new Hydrograph("Percent surface capacity", "%", aboveGradeStoragePercentCapacity, dt));

              results.PercentRockCapacityUsed = -1; // There is no rock gallery.
              break;
          case FacilityConfiguration.B:
              results.AboveGradePrimaryResults.Add(new Hydrograph("Percolation to below grade storage", "cfs", infiltrationToBelowGrade, dt));
              results.AboveGradePrimaryResults.Add(new Hydrograph("Overflow to approved discharge", "cfs", overflowToEscapeRoute, dt));

              results.AboveGradeSecondaryResults.Add(new Hydrograph("Percent surface capacity", "%", aboveGradeStoragePercentCapacity, dt));

              results.BelowGradePrimaryResults.Add(new Hydrograph("Inflow to rock storage", "cfs", infiltrationToBelowGrade, dt));
              results.BelowGradePrimaryResults.Add(new Hydrograph("Infiltration capacity", "cfs", totalInfiltrationCapacityToNative, dt));

              results.BelowGradeSecondaryResults.Add(new Hydrograph("Percent rock capacity", "%", rockPercentCapacity, dt));

              break;
          case FacilityConfiguration.C:
              results.AboveGradePrimaryResults.Add(new Hydrograph("Total flow to below grade storage", "cfs", inflowToRockStorage, dt));
              results.AboveGradePrimaryResults.Add(new Hydrograph("Flow bypassing growing medium", "cfs", flowOvertoppingToUnderdrain, dt));

              results.AboveGradeSecondaryResults.Add(new Hydrograph("Percent surface capacity", "%", aboveGradeStoragePercentCapacity, dt));

              results.BelowGradePrimaryResults.Add(new Hydrograph("Inflow to rock storage", "cfs", inflowToRockStorage, dt));
              results.BelowGradePrimaryResults.Add(new Hydrograph("Infiltration capacity", "cfs", totalInfiltrationCapacityToNative, dt));
              results.BelowGradePrimaryResults.Add(new Hydrograph("Overflow to approved discharge", "cfs", rockOverflowToEscapeRoute, dt));

              results.BelowGradeSecondaryResults.Add(new Hydrograph("Percent rock capacity", "%", rockPercentCapacity, dt));

              break;
          case FacilityConfiguration.D:
              results.AboveGradePrimaryResults.Add(new Hydrograph("Total flow to below grade storage", "cfs", inflowToRockStorage, dt));
              results.AboveGradePrimaryResults.Add(new Hydrograph("Flow bypassing growing medium", "cfs", flowOvertoppingToUnderdrain, dt));

              results.AboveGradeSecondaryResults.Add(new Hydrograph("Percent surface capacity", "%", aboveGradeStoragePercentCapacity, dt));

              break;
          case FacilityConfiguration.E:
              results.AboveGradePrimaryResults.Add(new Hydrograph("Overflow to approved discharge", "cfs", rockOverflowToEscapeRoute, dt));
              results.AboveGradePrimaryResults.Add(new Hydrograph("Total flow to below grade storage", "cfs", inflowToRockStorage, dt));

              results.AboveGradeSecondaryResults.Add(new Hydrograph("Percent surface capacity", "%", aboveGradeSecondaryStoragePercentCapacity, dt));

              results.BelowGradePrimaryResults.Add(new Hydrograph("Inflow to rock storage", "cfs", inflowToRockStorage, dt));
              results.BelowGradePrimaryResults.Add(new Hydrograph("Infiltration capacity", "cfs", totalInfiltrationCapacityToNative, dt));

              results.BelowGradeSecondaryResults.Add(new Hydrograph("Percent rock capacity", "%", rockPercentCapacity, dt));

              results.PercentSurfaceCapacityUsed = aboveGradeSecondaryStoragePercentCapacity.Max();
              break;
          case FacilityConfiguration.F:
              results.AboveGradePrimaryResults.Add(new Hydrograph("Total flow to below grade storage", "cfs", inflowToRockStorage, dt));
              results.AboveGradePrimaryResults.Add(new Hydrograph("Flow bypassing growing medium", "cfs", flowOvertoppingToUnderdrain, dt));

              results.AboveGradeSecondaryResults.Add(new Hydrograph("Percent surface capacity", "%", aboveGradeStoragePercentCapacity, dt));

              results.BelowGradePrimaryResults.Add(new Hydrograph("Inflow to rock storage", "cfs", inflowToRockStorage, dt));
              results.BelowGradePrimaryResults.Add(new Hydrograph("Infiltration capacity", "cfs", totalInfiltrationCapacityToNative, dt));
              results.BelowGradePrimaryResults.Add(new Hydrograph("Overflow to approved discharge", "cfs", rockOverflowToEscapeRoute, dt));

              results.BelowGradeSecondaryResults.Add(new Hydrograph("Percent rock capacity", "%", rockPercentCapacity, dt));

              break;
      }
      return results;
    }
  }

  /// <summary>
  /// Contains PAC results for a single storm event
  /// </summary>
  public class StormEventResults
  {
    /// <summary>
    /// Contains analysis results for the above-grade portion of a stormwater
    /// facility which should be plotted on the primary axis of a chart. 
    /// </summary>
    public List<Hydrograph> AboveGradePrimaryResults;
    /// <summary>
    /// Contains analysis results for the above-grade portion of a stormwater
    /// facility which should be plotted on the secondary axis of a chart.
    /// </summary>
    public List<Hydrograph> AboveGradeSecondaryResults;
    /// <summary>
    /// Contains analysis results for the below-grade portion of a stormwater
    /// facility which should be plotted on the primary axis of a chart. 
    /// </summary>
    public List<Hydrograph> BelowGradePrimaryResults;
    /// <summary>
    /// Contains analysis results for the below-grade portion of a stormwater
    /// facility which should be plotted on the secondary axis of a chart. 
    /// </summary>
    public List<Hydrograph> BelowGradeSecondaryResults;

    

    /// <summary>
    /// The peak overflow from a facility to offsite conveyance or overtopping the facility, in cfs
    /// </summary>
    public double PeakOverflow;

    /// <summary>
    /// The peak primary surface overflow, in cfs.
    /// </summary>
    public double PeakSurfaceOverflow;

    /// <summary>
    /// The total overflow vole from a facility to offsite conveyance or overtopping the facility, in ft^3
    /// </summary>
    public double OverflowVolume;

    /// <summary>
    /// The peak percent volume of surface which is filled during a storm event.
    /// </summary>
    public double PercentSurfaceCapacityUsed;

    /// <summary>
    /// The peak percentage of rock capacity used during a storm event.
    /// </summary>
    public double PercentRockCapacityUsed;

    /// <summary>
    /// The peak flow rate from a catchment as calculated via the SBUH, in cfs
    /// </summary>
    public double PeakInflowRate;
    /// <summary>
    /// The total volume from a catchment as calculated via the SBUH, in ft^3
    /// </summary>
    public double InflowVolume;
    
    internal StormEventResults()
    {
      AboveGradePrimaryResults = new List<Hydrograph>();
      AboveGradeSecondaryResults = new List<Hydrograph>();

      BelowGradePrimaryResults = new List<Hydrograph>();
      BelowGradeSecondaryResults = new List<Hydrograph>();
    }
  }


}
