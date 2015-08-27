using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BesAsm.Swsp.PacSizingTool
{
  /// <summary>
  /// An object containing the results of a PAC calculation.
  /// </summary>
  public class PacResults
  {
    /// <summary>
    /// Results for the Pollution Reduction storm event
    /// </summary>
    public StormEventResults PollutionReductionResults { get; set; }
    /// <summary>
    /// Results for the Two-Year storm event
    /// </summary>
    public StormEventResults TwoYearResults { get; set; }
    /// <summary>
    /// Results for the Five-Year storm event
    /// </summary>
    public StormEventResults FiveYearResults { get; set; }
    /// <summary>
    /// Results for the Ten-Year storm event
    /// </summary>
    public StormEventResults TenYearResults { get; set; }
    /// <summary>
    /// Results for the Twentyfive-Year storm event
    /// </summary>
    public StormEventResults TwentyfiveYearResults { get; set; }

    /// <summary>
    /// The peak overflow from a facility during the Pollution Reduction event, in cfs 
    /// </summary>
    public double PollutionReductionPeakOverflow { get; internal set; }
    /// <summary>
    /// The total overflow voume during the Pollution Reduction event, in ft^3
    /// </summary>
    public double PollutionReductionTotalOverflowVolume { get; internal set; }
    /// <summary>
    /// The peak flow rate from a catchment from the SBUH calcs, in cfs
    /// </summary>
    public double PollutionReductionPeakInflow { get; internal set; }
    /// <summary>
    /// The total volume running off a catchment from the SBUH calcs, in cfs
    /// </summary>
    public double PollutionReductionInflowVolume { get; internal set; }
    /// <summary>
    /// The peak overflow from a facility during the Two Year event, in cfs 
    /// </summary>
    public double TwoYearPeakOverflow { get; internal set; }
    /// <summary>
    /// The total overflow voume during the Two Year event, in ft^3
    /// </summary>
    public double TwoYearTotalOverflowVolume { get; internal set; }
    /// <summary>
    /// The peak flow rate from a catchment from the SBUH calcs, in cfs
    /// </summary>
    public double TwoYearPeakInflow { get; internal set; }
    /// <summary>
    /// The peak flow rate for a two-year design storm from the pre-developed catchment from the SBUH calcs, in cfs
    /// </summary>
    public double PreDevelopedTwoYearPeakInflow { get; internal set; }
    /// <summary>
    /// The total volume running off a catchment from the SBUH calcs, in cfs
    /// </summary>
    public double TwoYearInflowVolume { get; internal set; }
    /// <summary>
    /// The peak overflow from a facility during Five Year event, in cfs 
    /// </summary>
    public double FiveYearPeakOverflow { get; internal set; }
    /// <summary>
    /// The total overflow voume during the Five Year event, in ft^3
    /// </summary>
    public double FiveYearTotalOverflowVolume { get; internal set; }
    /// <summary>
    /// The peak flow rate from a catchment from the SBUH calcs, in cfs
    /// </summary>
    public double FiveYearPeakInflow { get; internal set; }
    /// <summary>
    /// The peak flow rate for a five-year design storm from the pre-developed catchment from the SBUH calcs, in cfs
    /// </summary>
    public double PreDevelopedFiveYearPeakInflow { get; internal set; }
    /// <summary>
    /// The total volume running off a catchment from the SBUH calcs, in cfs
    /// </summary>
    public double FiveYearInflowVolume { get; internal set; }
    /// <summary>
    /// The peak overflow from a facility during the Ten Year event, in cfs 
    /// </summary>
    public double TenYearPeakOverflow { get; internal set; }
    /// <summary>
    /// The total overflow voume during the Ten Year event, in ft^3
    /// </summary>
    public double TenYearTotalOverflowVolume{ get; internal set; }
    /// <summary>
    /// The peak flow rate from a catchment from the SBUH calcs, in cfs
    /// </summary>
    public double TenYearPeakInflow { get; internal set; }
    /// <summary>
    /// The peak flow rate for a ten-year design storm from the pre-developed catchment from the SBUH calcs, in cfs
    /// </summary>
    public double PreDevelopedTenYearPeakInflow { get; internal set; }
    /// <summary>
    /// The total volume running off a catchment from the SBUH calcs, in cfs
    /// </summary>
    public double TenYearInflowVolume { get; internal set; }
    /// <summary>
    /// The peak overflow from a facility during the Twentyfive year event, in cfs 
    /// </summary>
    public double TwentyfiveYearPeakOverflow { get; internal set; }
    /// <summary>
    /// The total overflow voume during the Twentyfive Year event, in ft^3
    /// </summary>
    public double TwentyfiveYearTotalOverflowVolume { get; internal set; }
    /// <summary>
    /// The peak flow rate from a catchment from the SBUH calcs, in cfs
    /// </summary>
    public double TwentyfiveYearPeakInflow { get; internal set; }
    /// <summary>
    /// The peak flow rate for a twenty five-year design storm from the pre-developed catchment from the SBUH calcs, in cfs
    /// </summary>
    public double PreDevelopedTwentyfiveYearPeakInflow { get; internal set; }
    /// <summary>
    /// The total volume running off a catchment from the SBUH calcs, in cfs
    /// </summary>
    public double TwentyfiveYearInflowVolume { get; internal set; }

    /// <summary>
    /// Indicates whether the facility passed or failed the Pollution Reduction criteria
    /// </summary>
    public PacScore PollutionReductionScore { get; internal set; }
    /// <summary>
    /// The peak capacity of surface storage used during the Pollution Reduction event
    /// </summary>
    public double PollutionReductionSurfaceCapacity { get; internal set; }
    /// <summary>
    /// Indicates whether the facility passed or failed the Ten Year management criteria
    /// </summary>
    public PacScore TenYearScore { get; internal set; }
    /// <summary>
    /// The peak capacity of surface storage used during the Ten Year event
    /// </summary>
    public double TenYearSurfaceCapacity { get; internal set; }
    
    /// <summary>
    /// Indicates whether the facility passed or failed the Flow Control management criteria for stormwater hierarchy categories 3 and 4
    /// </summary>
    public PacScore FlowControlScore { get; internal set; }
    /// <summary>
    /// Indicates whether the facility passed or failed the Two Year Flow Control management criteria for stormwater hierarchy categories 3 and 4
    /// </summary>
    public PacScore TwoYearFlowControlScore { get; internal set; }
    /// <summary>
    /// Indicates whether the facility passed or failed the Five Year Flow Control management criteria for stormwater hierarchy categories 3 and 4
    /// </summary>
    public PacScore FiveYearFlowControlScore { get; internal set; }
    /// <summary>
    /// Indicates whether the facility passed or failed the Ten Year Flow Control management criteria for stormwater hierarchy categories 3 and 4
    /// </summary>
    public PacScore TenYearFlowControlScore { get; internal set; }
    /// <summary>
    /// Indicates whether the facility passed or failed the Twenty-five Year Flow Control management criteria for stormwater hierarchy categories 3 and 4
    /// </summary>
    public PacScore TwentyfiveYearFlowControlScore { get; internal set; }

    /// <summary>
    /// The percent of rock capacity used during the Pollution Reduction event
    /// </summary>
    public double PollutionReductionPercentRockCapacity { get; internal set; }

    /// <summary>
    /// The percent of rock capacity used during the Ten Year event
    /// </summary>
    public double TenYearPercentRockCapacity { get; internal set; }
        
    internal PacResults()
    {
      
    }
   
  }

  /// <summary>
  /// Enumerator indicated possible results of a facility
  /// </summary>
  public enum PacScore 
  {
      /// <summary>
      /// Meets the criteria.
      /// </summary>
      Pass,
 
      /// <summary>
      /// Does not meet the criteria.
      /// </summary>
      Fail,
 
      /// <summary>
      /// There is no criteria set.
      /// </summary>
      NotUsed 
  };
}
