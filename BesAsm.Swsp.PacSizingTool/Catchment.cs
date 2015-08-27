using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BesAsm.Swsp.PacSizingTool
{
  /// <summary>
  /// The impervious drainage area upstream of a facility
  /// </summary>
  public class Catchment
  {
    private string _name;
    private double _imperviousAreaSquareFeet;
    private double _curveNumber = 98; // check if this is appropriate default
    private double _timeOfConcentrationMinutes = 5; // check if this is appropriate default

    private InfiltrationTestType _infiltrationTestType = InfiltrationTestType.OpenPitFallingHead; // check if this is appropriate default

    private double _testedInfiltrationRateInchesPerHour = 5; // check if this is appropriate default

    private bool _acceptableSeparationFromGroundwater = true; // check if this is appropriate default    

    /// <summary>
    /// The maximum allowable value for native soil infiltration rate
    /// </summary>
    public static readonly double MaxNativeInfiltrationInchesPerHour = 20;
    /// <summary>
    /// The default value for imported medium infiltration rate
    /// </summary>
    public static readonly double DefaultImportedMediumInfiltrationInchesPerHour = 2;

    /// <summary>
    /// Constructs a new Catchment object
    /// </summary>
    /// <param name="name">The name of the Catchment</param>
    public Catchment(string name)
    {
      _name = name;
    }
    /// <summary>
    /// Returns the Catchment name
    /// </summary>
    public string Name
    {
      get { return _name; }
    }
    /// <summary>
    /// The total impervious area draining to a catchment
    /// </summary>
    public double ImperviousAreaSquareFeet
    {
      get { return _imperviousAreaSquareFeet; }
      set { _imperviousAreaSquareFeet = value; } // check for valid range
    }
    /// <summary>
    /// The Curve Number of the catchment (a hydrologic parameter)
    /// </summary>
    public double CurveNumber
    {
      get { return _curveNumber; }
      set { _curveNumber = value; } // check for valid range
    }
    /// <summary>
    /// The Time of Concentration of the catchment (a hydrologic parameter)
    /// </summary>
    public double TimeOfConcentrationMinutes
    {
      get { return _timeOfConcentrationMinutes; }
      set { _timeOfConcentrationMinutes = value; } // check for valid range
    }

    /// <summary>
    /// The infiltration test type used to evaluate the infiltration of the native soils.
    /// Current types include:
    /// 1. Open-Pit Falling-Head
    /// 2. Encased Falling-Head
    /// 3. Double-Ring Infiltometer
    /// </summary>
    public InfiltrationTestType InfiltrationTestType
    {
      get { return _infiltrationTestType; }
      set { _infiltrationTestType = value; }
    }

    /// <summary>
    /// The unfactored infiltration rate from the infiltration test, in inches per hour.
    /// </summary>
    public double TestedInfiltrationRateInchesPerHour
    {
      get { return _testedInfiltrationRateInchesPerHour; }
      set { _testedInfiltrationRateInchesPerHour = value; } // check for valid range
    }

    /// <summary>
    /// Field indicating whether the tested area within the catchment meets
    /// separation from groundwater criteria
    /// </summary>
    public bool AcceptableSeparationFromGroundwater
    {
      get { return _acceptableSeparationFromGroundwater; }
      set { _acceptableSeparationFromGroundwater = value; }
    }

    /// <summary>
    /// Correction factor applied to tested infiltration rate
    /// </summary>
    /// <returns></returns>
    public double CorrectionFactor()
    {
      switch (_infiltrationTestType)
      {
        case InfiltrationTestType.OpenPitFallingHead:
          return 2;
        case InfiltrationTestType.EncasedFallingHead:
          return 2;
        case InfiltrationTestType.DoubleRingInfiltometer:
          return 1;
        default:
          throw new ArgumentException("Invalid enumerator");
      }
    }

    /// <summary>
    /// The infiltration rate of the native soil used for design. This is the tested infiltration
    /// rate divided by the correction factor.
    /// </summary>
    public double DesignInfiltrationNativeInchesPerHour
    {
      get
      {
        double correctionFactor = CorrectionFactor();

        return Math.Min(_testedInfiltrationRateInchesPerHour / correctionFactor, MaxNativeInfiltrationInchesPerHour);
      }
    }
    /// <summary>
    /// The infiltration rate of imported growing medium
    /// </summary>
    public double ImportedMediumInfiltrationInchesPerHour
    {
      get
      {
        return DefaultImportedMediumInfiltrationInchesPerHour;
      }
    }

  }

  /// <summary>
  /// Enumerator indicating the testing method used to determine native soil infiltration rate
  /// </summary>
  public enum InfiltrationTestType 
  { 
      /// <summary>
      /// Open-Pit Falling-Head test, a test of the combination of vertical and lateral infiltration.
      /// </summary>
      OpenPitFallingHead,
 
      /// <summary>
      /// The Encased Falling-Head test, a test of the vertical infiltration.
      /// </summary>
      EncasedFallingHead, 

      /// <summary>
      /// The Double-Ring Infiltrometer, a test of the vertical infiltration.
      /// Performed in accordance with ASTM 3385-94
      /// </summary>
      DoubleRingInfiltometer 
  }
}
