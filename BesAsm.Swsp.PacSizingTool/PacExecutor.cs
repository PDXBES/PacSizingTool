using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BesAsm.Swsp.PacSizingTool
{
  /// <summary>
  /// Primary class for executing PAC calculations.
  /// </summary>
  public static class PacExecutor
  {
    /// <summary>
    /// Executes the calculator and returns a PacResults.
    /// </summary>
    /// <param name="catchment">A catchment object defining the hydrologic parameters of the post-developed catchment area to be evaluated.</param>
    /// <param name="preCatchment">A catchment object defining the hydrologic parameters of the pre-developed catchment area to be evaluated.</param>
    /// <param name="facility">A Facility object defining the stormwater management facility to be evaluated.</param>
    /// <param name="category">Identifies the HierarchyCategory the proposed facility will be evaluated against.</param>
    /// <param name="dischargePoint">Identifies the DischargePoint of the proposed facility.</param>
    /// <returns>A PacResults object containing the results of the calculation.</returns>
    internal static PacResults PerformCalculations(Catchment catchment, Catchment preCatchment, Facility facility, HierarchyCategory category, DischargePoint dischargePoint)
    {
      //Define design storms
      RainfallEvent pollutionReduction = RainfallEvent.GetScsOneAEvent("Pollution Reduction", 0.83);
      RainfallEvent twoYear = RainfallEvent.GetScsOneAEvent("Two-Year", 2.4);
      RainfallEvent fiveYear = RainfallEvent.GetScsOneAEvent("Five-Year", 2.9);
      RainfallEvent tenYear = RainfallEvent.GetScsOneAEvent("Ten-Year", 3.4);
      RainfallEvent twentyFiveYear = RainfallEvent.GetScsOneAEvent("Twentyfive-Year", 3.9);
 
      PacResults results = new PacResults();

      //Calculate hydrographs for the most important design storms
      Hydrograph imperviousHydrographPR = SantaBarbaraUrbanHydrograph.CalculateHydrograph
        (catchment, pollutionReduction);
      Hydrograph imperviousHydrographTwoYear = SantaBarbaraUrbanHydrograph.CalculateHydrograph
        (catchment, twoYear);
      Hydrograph imperviousHydrographFiveYear = SantaBarbaraUrbanHydrograph.CalculateHydrograph
        (catchment, fiveYear);
      Hydrograph imperviousHydrographTenYear = SantaBarbaraUrbanHydrograph.CalculateHydrograph
        (catchment, tenYear);
      Hydrograph imperviousHydrographTwentyfiveYear = SantaBarbaraUrbanHydrograph.CalculateHydrograph
        (catchment, twentyFiveYear);

      results.PollutionReductionResults =
        ReservoirRouter.PerformCalculations(facility, category, catchment, imperviousHydrographPR);
      results.PollutionReductionPeakOverflow = results.PollutionReductionResults.PeakOverflow;
      results.PollutionReductionTotalOverflowVolume = results.PollutionReductionResults.OverflowVolume;
      results.PollutionReductionSurfaceCapacity = results.PollutionReductionResults.PercentSurfaceCapacityUsed;
      results.PollutionReductionPercentRockCapacity = results.PollutionReductionResults.PercentRockCapacityUsed;

      results.PollutionReductionInflowVolume = results.PollutionReductionResults.InflowVolume;
      results.PollutionReductionPeakInflow = results.PollutionReductionResults.PeakInflowRate;

      results.TwoYearResults =
        ReservoirRouter.PerformCalculations(facility, category, catchment, imperviousHydrographTwoYear);
      results.TwoYearPeakOverflow = results.TwoYearResults.PeakOverflow;
      results.TwoYearTotalOverflowVolume = results.TwoYearResults.OverflowVolume;

      results.TwoYearInflowVolume = results.TwoYearResults.InflowVolume;
      results.TwoYearPeakInflow = results.TwoYearResults.PeakInflowRate;

      results.FiveYearResults =
        ReservoirRouter.PerformCalculations(facility, category, catchment, imperviousHydrographFiveYear);
      results.FiveYearPeakOverflow = results.FiveYearResults.PeakOverflow;
      results.FiveYearTotalOverflowVolume = results.FiveYearResults.OverflowVolume;

      results.FiveYearInflowVolume = results.FiveYearResults.InflowVolume;
      results.FiveYearPeakInflow = results.FiveYearResults.PeakInflowRate;

      results.TenYearResults =
        ReservoirRouter.PerformCalculations(facility, category, catchment, imperviousHydrographTenYear);
      results.TenYearPeakOverflow = results.TenYearResults.PeakOverflow;
      results.TenYearTotalOverflowVolume = results.TenYearResults.OverflowVolume;
      results.TenYearSurfaceCapacity = results.TenYearResults.PercentSurfaceCapacityUsed;
      results.TenYearPercentRockCapacity = results.TenYearResults.PercentRockCapacityUsed;

      results.TenYearInflowVolume = results.TenYearResults.InflowVolume;
      results.TenYearPeakInflow = results.TenYearResults.PeakInflowRate;

      results.TwentyfiveYearResults =
        ReservoirRouter.PerformCalculations(facility, category, catchment, imperviousHydrographTwentyfiveYear);
      results.TwentyfiveYearPeakOverflow = results.TwentyfiveYearResults.PeakOverflow;
      results.TwentyfiveYearTotalOverflowVolume = results.TwentyfiveYearResults.OverflowVolume;

      results.TwentyfiveYearInflowVolume = results.TwentyfiveYearResults.InflowVolume;
      results.TwentyfiveYearPeakInflow = results.TwentyfiveYearResults.PeakInflowRate;

      results.TenYearScore = PacScore.NotUsed; // Defaults
      results.FlowControlScore = PacScore.NotUsed;
      results.TwoYearFlowControlScore = PacScore.NotUsed;
      results.FiveYearFlowControlScore = PacScore.NotUsed;
      results.TenYearFlowControlScore = PacScore.NotUsed;
      results.TwentyfiveYearFlowControlScore = PacScore.NotUsed;

      switch (category)
      {
          case HierarchyCategory.Category1:
          case HierarchyCategory.Category2:
              results.TenYearScore = results.TenYearPeakOverflow > 0 ? PacScore.Fail : PacScore.Pass;
              break;
          case HierarchyCategory.Category3:
              //Define preliminary catchment runoff results
              results.PreDevelopedTwoYearPeakInflow = ReservoirRouter.PerformCalculations(facility, category, preCatchment, SantaBarbaraUrbanHydrograph.CalculateHydrograph(preCatchment, twoYear)).PeakInflowRate;
              results.PreDevelopedFiveYearPeakInflow = ReservoirRouter.PerformCalculations(facility, category, preCatchment, SantaBarbaraUrbanHydrograph.CalculateHydrograph(preCatchment, fiveYear)).PeakInflowRate;
              results.PreDevelopedTenYearPeakInflow = ReservoirRouter.PerformCalculations(facility, category, preCatchment, SantaBarbaraUrbanHydrograph.CalculateHydrograph(preCatchment, tenYear)).PeakInflowRate;
              results.PreDevelopedTwentyfiveYearPeakInflow = ReservoirRouter.PerformCalculations(facility, category, preCatchment, SantaBarbaraUrbanHydrograph.CalculateHydrograph(preCatchment, twentyFiveYear)).PeakInflowRate;

              switch (dischargePoint)
              {
                  case DischargePoint.A:
                      results.FlowControlScore = PacScore.NotUsed;
                      break;
                  case DischargePoint.B:
                      if (results.TwoYearPeakOverflow <= results.PreDevelopedTwoYearPeakInflow / 2)
                          results.TwoYearFlowControlScore = PacScore.Pass;
                      else
                          results.TwoYearFlowControlScore = PacScore.Fail;
                      if (results.FiveYearPeakOverflow <= results.PreDevelopedFiveYearPeakInflow)
                          results.FiveYearFlowControlScore = PacScore.Pass;
                      else
                          results.FiveYearFlowControlScore = PacScore.Fail;
                      if (results.TenYearPeakOverflow <= results.PreDevelopedTenYearPeakInflow)
                          results.TenYearFlowControlScore = PacScore.Pass;
                      else
                          results.TenYearFlowControlScore = PacScore.Fail;
                      if (results.TwentyfiveYearPeakOverflow <= results.PreDevelopedTwentyfiveYearPeakInflow)
                          results.TwentyfiveYearFlowControlScore = PacScore.Pass;
                      else
                          results.TwentyfiveYearFlowControlScore = PacScore.Fail;
                      if (results.TwoYearPeakOverflow <= results.PreDevelopedTwoYearPeakInflow / 2 &&
                              results.FiveYearPeakOverflow <= results.PreDevelopedFiveYearPeakInflow &&
                              results.TenYearPeakOverflow <= results.PreDevelopedTenYearPeakInflow &&
                              results.TwentyfiveYearPeakOverflow <= results.PreDevelopedTwentyfiveYearPeakInflow)
                          results.FlowControlScore = PacScore.Pass;
                      else
                          results.FlowControlScore = PacScore.Fail;
                      break;
                  case DischargePoint.C:
                      if (results.TwoYearPeakOverflow <= results.PreDevelopedTwoYearPeakInflow)
                          results.TwoYearFlowControlScore = PacScore.Pass;
                      else
                          results.TwoYearFlowControlScore = PacScore.Fail;
                      if (results.FiveYearPeakOverflow <= results.PreDevelopedFiveYearPeakInflow)
                          results.FiveYearFlowControlScore = PacScore.Pass;
                      else
                          results.FiveYearFlowControlScore = PacScore.Fail;
                      if (results.TenYearPeakOverflow <= results.PreDevelopedTenYearPeakInflow)
                          results.TenYearFlowControlScore = PacScore.Pass;
                      else
                          results.TenYearFlowControlScore = PacScore.Fail;

                      if (results.TwoYearPeakOverflow <= results.PreDevelopedTwoYearPeakInflow &&
                              results.FiveYearPeakOverflow <= results.PreDevelopedFiveYearPeakInflow &&
                              results.TenYearPeakOverflow <= results.PreDevelopedTenYearPeakInflow)
                          results.FlowControlScore = PacScore.Pass;
                      else
                          results.FlowControlScore = PacScore.Fail;
                      break;
                  default:
                      break;
              }
              break;
          case HierarchyCategory.Category4:
              //Define preliminary catchment runoff results
              results.PreDevelopedTwoYearPeakInflow = ReservoirRouter.PerformCalculations(facility, category, preCatchment, SantaBarbaraUrbanHydrograph.CalculateHydrograph(preCatchment, twoYear)).PeakInflowRate;
              results.PreDevelopedFiveYearPeakInflow = ReservoirRouter.PerformCalculations(facility, category, preCatchment, SantaBarbaraUrbanHydrograph.CalculateHydrograph(preCatchment, fiveYear)).PeakInflowRate;
              results.PreDevelopedTenYearPeakInflow = ReservoirRouter.PerformCalculations(facility, category, preCatchment, SantaBarbaraUrbanHydrograph.CalculateHydrograph(preCatchment, tenYear)).PeakInflowRate;
              results.PreDevelopedTwentyfiveYearPeakInflow = ReservoirRouter.PerformCalculations(facility, category, preCatchment, SantaBarbaraUrbanHydrograph.CalculateHydrograph(preCatchment, twentyFiveYear)).PeakInflowRate;
              if (results.TwentyfiveYearPeakOverflow <= results.PreDevelopedTenYearPeakInflow)
              {
                  results.FlowControlScore = PacScore.Pass;
                  results.TwentyfiveYearFlowControlScore = PacScore.Pass;
              }
              else
              {
                  results.FlowControlScore = PacScore.Fail;
                  results.TwentyfiveYearFlowControlScore = PacScore.Fail;
              }
              break;
          default:
              break;
      }

      results.PollutionReductionScore = results.PollutionReductionResults.PeakSurfaceOverflow > 0 ?
        PacScore.Fail : PacScore.Pass;

      return results;
    }

    //Category 1 & 2 methods
    /// <summary>
    /// Executes the calculator and returns a PacResults. Supports Category 1 and 2 facilities.
    /// </summary>
    /// <param name="catchment">A catchment object defining the hydrologic parameters of the catchment area to be evaluated.</param>
    /// <param name="facility">A Facility object defining the stormwater management facility to be evaluated.</param>
    /// <param name="category">Identifies the Hierarchy Category the proposed facility will be evaluated against. Must be an integer from 1 to 4.</param>
    /// <returns>A PacResults object containing the results of the calculation.</returns>
    public static PacResults PerformCalculations(Catchment catchment, Facility facility, int category)
    {
        HierarchyCategory categoryEnum;

        switch (category)
        {
            case 1:
                categoryEnum = HierarchyCategory.Category1;
                break;
            case 2:
                categoryEnum = HierarchyCategory.Category2;
                break;
            case 3:
                categoryEnum = HierarchyCategory.Category3;
                break;
            case 4:
                categoryEnum = HierarchyCategory.Category4;
                break;
            default:
                throw new ArgumentException("Invalid hierarchy category specified: Must be integer 1 - 4");
        }
        return PerformCalculations(catchment, catchment, facility, categoryEnum, DischargePoint.A);
    }
    /// <summary>
    /// Executes the calculator and returns a PacResults. Supports Category 1 and 2 facilities.
    /// </summary>
    /// <param name="catchment">A catchment object defining the hydrologic parameters of the catchment area to be evaluated.</param>
    /// <param name="slopedFacility">A SlopedFacility object defining the stormwater management facility to be evaluated.</param>
    /// <param name="category">Identifies the Hierarchy Category the proposed facility will be evaluated against. Must be an integer from 1 to 4.</param>
    /// <returns>A PacResults object containing the results of the calculation.</returns>
    public static PacResults PerformCalculations(Catchment catchment, SlopedFacility slopedFacility, int category)
    {
      return PerformCalculations(catchment, (Facility)slopedFacility, category);
    }

    //Category 3 methods
    /// <summary>
    /// Executes the calculator and returns a PacResults. Supports Category 3 facilities.
    /// </summary>
    /// <param name="catchment">A catchment object defining the hydrologic parameters of the catchment area to be evaluated.</param>
    /// <param name="preCatchment">A catchment object defining the hydrologic parameters of the pre-developed catchment area to be evaluated.</param>
    /// <param name="facility">A Facility object defining the stormwater management facility to be evaluated.</param>
    /// <param name="category">Identifies the Hierarchy Category the proposed facility will be evaluated against. Must be an integer from 1 to 4.</param>
    /// <param name="dischargePoint">Identifies the DischargePoint of the proposed facility.</param>
    /// <returns>A PacResults object containing the results of the calculation.</returns>
    public static PacResults PerformCalculations(Catchment catchment, Catchment preCatchment, Facility facility, int category, char dischargePoint)
    {
        HierarchyCategory categoryEnum;
        DischargePoint dischargeEnum;

        switch (category)
        {
            case 1:
                categoryEnum = HierarchyCategory.Category1;
                break;
            case 2:
                categoryEnum = HierarchyCategory.Category2;
                break;
            case 3:
                categoryEnum = HierarchyCategory.Category3;
                break;
            case 4:
                categoryEnum = HierarchyCategory.Category4;
                break;
            default:
                throw new ArgumentException("Invalid hierarchy category specified: Must be integer 1 - 4");
        }

        switch (char.ToUpper(dischargePoint))
        {
            case 'A':
                dischargeEnum = DischargePoint.A;
                break;
            case 'B':
                dischargeEnum = DischargePoint.B;
                break;
            case 'C':
                dischargeEnum = DischargePoint.C;
                break;
            default:
                throw new ArgumentException("Invalid discharge point specified: Must be character A-C");
        }
        return PerformCalculations(catchment, preCatchment, facility, categoryEnum, dischargeEnum);
    }
    /// <summary>
    /// Executes the calculator and returns a PacResults. Supports Category 3 facilities.
    /// </summary>
    /// <param name="catchment">A catchment object defining the hydrologic parameters of the catchment area to be evaluated.</param>
    /// <param name="preCatchment">A catchment object defining the hydrologic parameters of the pre-developed catchment area to be evaluated.</param>
    /// <param name="slopedFacility">A SlopedFacility object defining the stormwater management facility to be evaluated.</param>
    /// <param name="category">Identifies the Hierarchy Category the proposed facility will be evaluated against. Must be an integer from 1 to 4.</param>
    /// <param name="dischargePoint">Identifies the DischargePoint of the proposed facility.</param>
    /// <returns>A PacResults object containing the results of the calculation.</returns>
    public static PacResults PerformCalculations(Catchment catchment, Catchment preCatchment, SlopedFacility slopedFacility, int category, char dischargePoint)
    {
        return PerformCalculations(catchment, preCatchment, (Facility)slopedFacility, category, dischargePoint);
    }

    //Category 4 methods don't care about the discharge point
    /// <summary>
    /// Executes the calculator and returns a PacResults. Supports Category 4 facilities.
    /// </summary>
    /// <param name="catchment">A catchment object defining the hydrologic parameters of the catchment area to be evaluated.</param>
    /// <param name="preCatchment">A catchment object defining the hydrologic parameters of the pre-developed catchment area to be evaluated.</param>
    /// <param name="facility">A Facility object defining the stormwater management facility to be evaluated.</param>
    /// <param name="category">Identifies the Hierarchy Category the proposed facility will be evaluated against. Must be an integer from 1 to 4.</param>
    /// <returns>A PacResults object containing the results of the calculation.</returns>
    public static PacResults PerformCalculations(Catchment catchment, Catchment preCatchment, Facility facility, int category)
    {
        HierarchyCategory categoryEnum;

        switch (category)
        {
            case 1:
                categoryEnum = HierarchyCategory.Category1;
                break;
            case 2:
                categoryEnum = HierarchyCategory.Category2;
                break;
            case 3:
                categoryEnum = HierarchyCategory.Category3;
                break;
            case 4:
                categoryEnum = HierarchyCategory.Category4;
                break;
            default:
                throw new ArgumentException("Invalid hierarchy category specified: Must be integer 1 - 4");
        }
        return PerformCalculations(catchment, preCatchment, facility, categoryEnum, DischargePoint.A);
    }
    /// <summary>
    /// Executes the calculator and returns a PacResults. Supports Category 4 facilities.
    /// </summary>
    /// <param name="catchment">A catchment object defining the hydrologic parameters of the catchment area to be evaluated.</param>
    /// <param name="preCatchment">A catchment object defining the hydrologic parameters of the pre-developed catchment area to be evaluated.</param>
    /// <param name="slopedFacility">A SlopedFacility object defining the stormwater management facility to be evaluated.</param>
    /// <param name="category">Identifies the Hierarchy Category the proposed facility will be evaluated against. Must be an integer from 1 to 4.</param>
    /// <returns>A PacResults object containing the results of the calculation.</returns>
    public static PacResults PerformCalculations(Catchment catchment, Catchment preCatchment, SlopedFacility slopedFacility, int category)
    {
        return PerformCalculations(catchment, preCatchment, (Facility)slopedFacility, category, 'A');
    }

      
      
    /// <summary>
    /// Executes only the SBUH calculations.
    /// </summary>
    /// <param name="catchment">The catchment to calculate the SBUH results.</param>
    /// <returns>A PacResults object which is only minimally populated with results data.
    /// The *PeakInflow and *InflowVolume fields will be populated for each storm event.</returns>
    public static PacResults PerformSbuhCalcs(Catchment catchment)
    {
      Facility dummyFacility = new Facility(FacilityType.Basin, FacilityConfiguration.A, catchment)
      {
          BottomAreaSqFt = 100,
          BottomWidthFt = 10,
          SideSlopeRatio = 3,
          StorageDepth1In = 9,
          GrowingMediumDepthIn = 18,
          FreeboardIn = 3,
          Shape = FacilityShape.Rectangle
      };
      return PerformCalculations(catchment, dummyFacility, 1);
    }

    /// <summary>
    /// Performs the SBUH calculations for the Pollution Reduction storm event
    /// </summary>
    /// <param name="catchment">A Catchment object</param>
    /// <returns>A Hydrograph object containing the results of the SBUH calculations</returns>
    public static Hydrograph CalculateSbuhPr(Catchment catchment)
    {
      //Define design storms
      RainfallEvent pollutionReduction = RainfallEvent.GetScsOneAEvent("Pollution Reduction", 0.83);

      Hydrograph imperviousHydrographPR = SantaBarbaraUrbanHydrograph.CalculateHydrograph
        (catchment, pollutionReduction);

      return imperviousHydrographPR;
    }

    /// <summary>
    /// Performs the SBUH calculations for the Two-year storm event
    /// </summary>
    /// <param name="catchment">A Catchment object</param>
    /// <returns>A Hydrograph object containing the results of the SBUH calculations</returns>
    public static Hydrograph CalculateSbuh2Year(Catchment catchment)
    {
      //Define design storms
      RainfallEvent rainfallEvent = RainfallEvent.GetScsOneAEvent("Two-Year", 2.4);

      Hydrograph imperviousHydrograph = SantaBarbaraUrbanHydrograph.CalculateHydrograph
        (catchment, rainfallEvent);

      return imperviousHydrograph;
    }

    /// <summary>
    /// Performs the SBUH calculations for the Five-year storm event
    /// </summary>
    /// <param name="catchment">A Catchment object</param>
    /// <returns>A Hydrograph object containing the results of the SBUH calculations</returns>
    public static Hydrograph CalculateSbuh5Year(Catchment catchment)
    {
      //Define design storms
      RainfallEvent rainfallEvent = RainfallEvent.GetScsOneAEvent("Five-Year", 2.9);

      Hydrograph imperviousHydrograph = SantaBarbaraUrbanHydrograph.CalculateHydrograph
        (catchment, rainfallEvent);

      return imperviousHydrograph;
    }

    /// <summary>
    /// Performs the SBUH calculations for the Ten-year storm event
    /// </summary>
    /// <param name="catchment">A Catchment object</param>
    /// <returns>A Hydrograph object containing the results of the SBUH calculations</returns>
    public static Hydrograph CalculateSbuh10Year(Catchment catchment)
    {
      //Define design storms
      RainfallEvent rainfallEvent = RainfallEvent.GetScsOneAEvent("Ten-Year", 3.4);

      Hydrograph imperviousHydrograph = SantaBarbaraUrbanHydrograph.CalculateHydrograph
        (catchment, rainfallEvent);

      return imperviousHydrograph;
    }

    /// <summary>
    /// Performs the SBUH calculations for the Twentyfive-year storm event
    /// </summary>
    /// <param name="catchment">A Catchment object</param>
    /// <returns>A Hydrograph object containing the results of the SBUH calculations</returns>
    public static Hydrograph CalculateSbuh25Year(Catchment catchment)
    {
      //Define design storms
      RainfallEvent rainfallEvent = RainfallEvent.GetScsOneAEvent("Twentyfive-Year", 3.9);

      Hydrograph imperviousHydrograph = SantaBarbaraUrbanHydrograph.CalculateHydrograph
        (catchment, rainfallEvent);

      return imperviousHydrograph;
    }

  }
}
