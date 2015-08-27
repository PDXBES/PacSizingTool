using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BesAsm.Swsp.PacSizingTool
{
  /// <summary>
  /// Class representing a stormwater mananagement facility
  /// </summary>
  public class Facility
  {
    internal bool _validConfiguration;

    protected double _bottomAreaSqFt;
    protected double _bottomWidthFt;

    protected double _storageDepth1In;    

    protected double _surfaceAreaAtStorageDepth1SqFt; //For User-Specified facility shapes
    protected double _bottomPerimeterLengthFt; //For Amoeba facility shapes
    protected double _surfaceAreaAtStorageDepth2SqFt; //For E-type User-Specified facility shapes

    protected double _growingMediumDepthIn;

    protected double _sideSlopeRatio;
    protected bool _specifySideSlope;

    protected double _freeboardIn;

    protected bool _hasSecondaryOverflow;
    protected double _storageDepth2In;    

    protected bool _hasRockStorage;
    protected double _rockStorageBottomAreaSqFt;
    protected bool _hasCustomRockStorageBottomArea;
    protected double _rockStorageDepthIn;
    protected double _rockVoidRatio;

    protected bool _hasRockInfluencedSurfaceStorage;
    protected double _storageDepth3In;

    internal FacilityShape _facilityShape;
    protected bool _allowShapeSelection;

    protected FacilityType _type;
    protected FacilityConfiguration _configuration;
    protected Catchment _catchment;

    protected bool _isLined;

    protected const double DefaultGrowingMediumPorespace = 0.3;

    const bool SANITIZEINPUT = false;    

    /// <summary>
    /// Constructs a new Facility object of the specified Type and Configuration, located within
    /// the specified Catchment
    /// </summary>
    /// <param name="type">The type of facility</param>
    /// <param name="configuration">The plumbed facility configuration</param>
    /// <param name="catchment">The Catchment containing the facility</param>
    public Facility(FacilityType type, FacilityConfiguration configuration, Catchment catchment)
    {
      _type = type;
      _configuration = configuration;
      _catchment = catchment;

      ConfigureFacility();
    }

    /// <summary>
    /// Returns enumerator indicating the facilities' plumbed connections to existing
    /// storm sewer and/or groundwater
    /// </summary>
    public FacilityConfiguration Configuration
    {
      get { return _configuration; }
    }

    /// <summary>
    /// Returns the catchment of the facility
    /// </summary>
    public Catchment Catchment
    {
        get { return _catchment; }
    }

    /// <summary>
    /// Returns enumerator indicating the type of facility
    /// </summary>
    public FacilityType Type
    {
      get { return _type; }
    }

    /// <summary>
    /// The bottom area of the facility in square feet
    /// </summary>
    public virtual double BottomAreaSqFt
    {
      get { return _bottomAreaSqFt; }
      set { _bottomAreaSqFt = value; }

    }

    /// <summary>
    /// The surface area at storage depth 1 in square feet.
    /// </summary>
    public virtual double SurfaceAreaAtStorageDepth1SqFt
    {
      get {
          switch (_type)
          {
              case FacilityType.Swale:
                  throw new NotImplementedException(); // Overridden in SlopedFacility
              case FacilityType.Basin:
                  switch (_facilityShape)
	                {
                        case FacilityShape.Rectangle:
                            {
                                double l = _bottomAreaSqFt / _bottomWidthFt;
                                double w = _bottomWidthFt;
                                double sideArea;
                                double cornerArea;
                                double spread = _sideSlopeRatio * _storageDepth1In / 12;

                                sideArea = 2 * spread * (l + w);
                                cornerArea = Math.PI * Math.Pow(spread, 2);

                                return (_bottomAreaSqFt + sideArea + cornerArea);
                            }
                        case FacilityShape.Amoeba:
                            return (_bottomAreaSqFt + _bottomPerimeterLengthFt * _storageDepth1In / 12 * _sideSlopeRatio);
                        case FacilityShape.Sloped:
                            throw new NotImplementedException(); // Overridden in SlopedFacility
                        case FacilityShape.UserDefined:
                            return _surfaceAreaAtStorageDepth1SqFt;
                        default:
                            throw new NotImplementedException();
	                }
              case FacilityType.PlanterFlat:
                  return _bottomAreaSqFt;
              case FacilityType.PlanterSloped:
                  throw new NotImplementedException(); // Overridden in SlopedFacility
              default:
                  throw new NotImplementedException();
          }

      }
      set { _surfaceAreaAtStorageDepth1SqFt = value; }
    }

    /// <summary>
    /// The surface area at storage depth 2 in square feet. Used only for E-type User-Defined facility
    /// shape in Basin facility
    /// </summary>
    public double SurfaceAreaAtStorageDepth2SqFt
    {
      get { return _surfaceAreaAtStorageDepth2SqFt; }
      set { _surfaceAreaAtStorageDepth2SqFt = value; }
    }

    /// <summary>
    /// The bottom perimeter length in feet. Used only for Amoeba shaped facility
    /// types in Basin facility
    /// </summary>
    public double BottomPerimeterLengthFt
    {
      get { return _bottomPerimeterLengthFt; }
      set { _bottomPerimeterLengthFt = value; }
    }

    /// <summary>
    /// The width of the facility in square feet
    /// </summary>
    public double BottomWidthFt
    {
      get { return _bottomWidthFt; }
      set { _bottomWidthFt = value; }
    }

    /// <summary>
    /// The storage depth before a facility reaches it's first overflow point
    /// </summary>
    public double StorageDepth1In
    {
      get { return _storageDepth1In; }
      set
      {
        _storageDepth1In = value;        
      }
    }
    /// <summary>
    /// The volume of storage capacity in cubic feet on the surface of the facility 
    /// before reaching the primary overflow
    /// </summary>
    public virtual double SurfaceCapacityAtDepth1CuFt
    {
      get { return CalculateVolume(_storageDepth1In); }
    }

    /// <summary>
    /// The storage depth (in inches) before a facility reaches it's second overflow point, if applicable;
    /// See field <para name="HasSecondaryOverflow">Facility.HasSecondaryOverflow</para> to determine if second overflow point is applicable.
    /// </summary>
    public double StorageDepth2In
    {
      get { return _storageDepth2In; }
      set
      {
        if (_hasSecondaryOverflow)
        {
          _storageDepth2In = value;       
        }
      }
    }

    /// <summary>
    /// The volume of storage capacity in cubic feet on the surface of the facility 
    /// before reaching the secondary overflow, if applicable; See field 
    /// <para name="HasSecondaryOverflow">Facility.HasSecondaryOverflow</para> 
    /// to determine if this field is applicable.
    /// </summary>
    public virtual double SurfaceCapacityAtDepth2CuFt
    {
      get { return CalculateVolume(_storageDepth2In); }
    }

    /// <summary>
    /// The storage depth (in inches) before a facilities' rock gallery overflows offsite, if applicable;
    /// See field <para name="HasRockInfluencedSurfaceStorage">Facility.HasRockInfluencedSurfaceStorage</para> 
    /// will be false if this field is applicable.
    /// </summary>
    public double StorageDepth3In
    {
      get { return _storageDepth3In; }
      set
      {
        if (!_hasRockInfluencedSurfaceStorage)
        {
          _storageDepth3In = value;
        }
      }
    }

    /// <summary>
    /// The facility Freeboard (in inches). Not sure how this parameter is used...
    /// See field <para name="SpecifyFreeboard">Facility.SpecifyFreeboard</para> to
    /// determine if this field is applicable.
    /// </summary>
    public double FreeboardIn
    {
      get { return _freeboardIn; }
      set { _freeboardIn = value; }
    }

    /// <summary>
    /// Indicates whether this facility configuration has a user-specifiable Freeboard
    /// </summary>
    public bool SpecifyFreeboard
    {
      get {
          if (_type == FacilityType.Basin && _facilityShape != FacilityShape.UserDefined)
              return true;
          else
              return false;
      }
    }

    /// <summary>
    /// The depth of the growing medium, in inches
    /// </summary>
    public double GrowingMediumDepthIn
    {
      get { return _growingMediumDepthIn; }
      set { _growingMediumDepthIn = value; }
    }

    /// <summary>
    /// The slope of the facility side-slope, specified in the ratio of X:1
    /// See field <see cref="Facility.SpecifySideSlope"/> to
    /// determine if this field is applicable.
    /// </summary>
    public double SideSlopeRatio
    {
      get { return _sideSlopeRatio; }
      set
      {
        _sideSlopeRatio = value;
      }
    }

    /// <summary>
    /// Indicates whether this facility configuration has a user-specifiable SideSlope
    /// </summary>
    public bool SpecifySideSlope
    {
      get { return _specifySideSlope; }
    }

    /// <summary>
    /// The storage area of the rock gallery in square feet
    /// See field <para name="HasRockStorage">Facility.HasRockStorage</para> to
    /// determine if the facility has a rock gallery, and field
    /// <para name="HasCustomRockStorageBottomArea">Facility.HasCustomRockStorageBottomArea</para>
    /// to determine whether the rock gallery area is customizable
    /// </summary>
    public virtual double RockStorageBottomAreaSqFt
    {
      get 
      {
          if (_hasRockStorage)
          {
              return _hasCustomRockStorageBottomArea ?
              _rockStorageBottomAreaSqFt : _bottomAreaSqFt; ;
          }
          else
          {
              return 0;
          }
      }
      set
      {
        _rockStorageBottomAreaSqFt = value;
      }
    }

    /// <summary>
    /// The depth of the rock storage gallery in inches
    /// See field <see cref="Facility.HasRockStorage"/> to
    /// determine if this field is applicable.
    /// Values greater than 48 inches should trigger a warning.
    /// </summary>
    public double RockStorageDepthIn
    {
      get { return _rockStorageDepthIn; }
      set
      {
        if (SANITIZEINPUT && value <= 0)
          throw new ArgumentException("Rock Storage Depth be greater than zero.");
        _rockStorageDepthIn = value;
      }
    }

    /// <summary>
    /// The void ratio of the rock gallery
    /// See field <see cref="Facility.HasRockStorage"/> to
    /// determine if this field is applicable.
    /// Values greater than 0.4 should trigger a warning.
    /// </summary>
    public double RockVoidRatio
    {
      get { return _rockVoidRatio; }
      set
      {
        if (SANITIZEINPUT && value <= 0)
          throw new ArgumentException("Rock Void Ratio must be greater than zero.");
        _rockVoidRatio = value;
      }
    }

    /// <summary>
    /// Indicates whether the facility has a rock gallery
    /// </summary>
    public bool HasRockStorage
    {
      get { return _hasRockStorage; }
      set { _hasRockStorage = value; }
    }

    /// <summary>
    /// Indicates whether the facility has a customizable rock storage bottom area;
    /// if not, the facility uses the facility bottom area as the rock storage bottom area
    /// </summary>
    public bool HasCustomRockStorageBottomArea
    {
      get { return _hasCustomRockStorageBottomArea; }
    }

    /// <summary>
    /// Indicates whether stormwater stored in the rock gallery can influence the volume
    /// of water stored on the surface of the facility. For facilities where the rock
    /// gallery is plumbed directly to a storm sewer this field will be false.
    /// </summary>
    public bool HasRockInfluencedSurfaceStorage
    {
      get { return _hasRockInfluencedSurfaceStorage; }
    }

    /// <summary>
    /// Enumerator describing the shape of the facility
    /// </summary>
    public FacilityShape Shape
    {
      get { return _facilityShape; }
      set { _facilityShape = value; }
    }

    /// <summary>
    /// Indicates whether the shape of the facility is customizable
    /// </summary>
    public bool AllowShapeSelection
    {
      get { return _allowShapeSelection; }
    }

    /// <summary>
    /// Indicates whether the facility has a secondary overflow pipe
    /// </summary>
    public bool HasSecondaryOverflow
    {
      get { return _hasSecondaryOverflow; }
    }

    /// <summary>
    /// The total area of the facility at the design storage depth in square feet.
    /// </summary>
    public virtual double TotalFacilityAreaSqFt
    {
      get
      {
        switch (this.Type)
        {
          case FacilityType.PlanterFlat:
            return _bottomAreaSqFt;
          case FacilityType.Basin:
            switch (this.Shape)
            {
              case FacilityShape.Rectangle:
                {
                    double l = _bottomAreaSqFt / _bottomWidthFt;
                    double w = _bottomWidthFt;
                    double sideArea;
                    double cornerArea;
                    double d = (_hasSecondaryOverflow) ? _storageDepth2In : _storageDepth1In;
                    double spread = _sideSlopeRatio * (d + _freeboardIn) / 12;

                    sideArea = 2 * spread * (l + w);
                    cornerArea = Math.PI * Math.Pow(spread, 2);

                    return (_bottomAreaSqFt + sideArea + cornerArea);
                }
              case FacilityShape.Amoeba:
                {
                    double d = (_hasSecondaryOverflow) ? _storageDepth2In : _storageDepth1In;
                    return (_bottomAreaSqFt + _bottomPerimeterLengthFt * (d + _freeboardIn) / 12 * _sideSlopeRatio);
                }
              case FacilityShape.UserDefined:
                return ((_hasSecondaryOverflow) ? _surfaceAreaAtStorageDepth2SqFt: _surfaceAreaAtStorageDepth1SqFt);
              default:
                return -1;
            }
          case FacilityType.PlanterSloped:
            return -1; //Overridden by SlopedFacility class
          case FacilityType.Swale:
            return -1; //Overridden by SlopedFacility class
          default: return -1;
        }
      }
    }

    /// <summary>
    /// The ratio of the facility footprint to the catchment impervious area expressed as a fraction.
    /// </summary>
    public double FacilitySizingRatio
    {
      get
      {
        return TotalFacilityAreaSqFt / _catchment.ImperviousAreaSquareFeet;
      }
    }

    /// <summary>
    /// Indicates whether the facility includes an impermeable membrane preventing
    /// infiltration to native soil
    /// </summary>
    public bool IsLined { get { return _isLined; } }

    /// <summary>
    /// The area of stormwater in contact with soil when the facility is
    /// 75% full
    /// </summary>
    public virtual double InfiltAreaAt75PercentDepth1SqFt
    {
      get
      {
        return SurfaceAreaAtDepth(_storageDepth1In*0.75);
      }
    }

    /// <summary>
    /// The infiltration rate through imported soil in cfs
    /// </summary>
    /// <returns></returns>
    public double ImportedMediumInfiltrationCapacityCfs()
    {
          return _catchment.ImportedMediumInfiltrationInchesPerHour / 12 * InfiltAreaAt75PercentDepth1SqFt / 3600;
    }

    /// <summary>
    /// The infiltration rate through the native soil in cfs
    /// </summary>
    /// <returns>
    /// </returns>
    public double NativeSoilInfiltrationCapacityCfs()
    {
      if (_isLined) return 0;

      if (_hasRockStorage && _hasCustomRockStorageBottomArea)
        return (_catchment.DesignInfiltrationNativeInchesPerHour / 12) * RockStorageBottomAreaSqFt / 3600;
      else
        return (_catchment.DesignInfiltrationNativeInchesPerHour / 12) * InfiltAreaAt75PercentDepth1SqFt / 3600;
    }

    /// <summary>
    /// Utility method to calculate the surface area of facilities filled to a certain depth, ignorant of overflows.
    /// </summary>
    /// <param name="depthIn"></param>
    /// <returns></returns>
    private double SurfaceAreaAtDepth(double depthIn)
    {
        switch (this.Type)
        {
            case FacilityType.PlanterFlat:
                return _bottomAreaSqFt;
            case FacilityType.Basin:
                switch (this.Shape)
                {
                    case FacilityShape.Rectangle:
                        {
                            double l = _bottomAreaSqFt / _bottomWidthFt;
                            double w = _bottomWidthFt;
                            double sideArea;
                            double cornerArea;
                            double spread = _sideSlopeRatio * (depthIn) / 12;

                            sideArea = 2 * spread * (l + w);
                            cornerArea = Math.PI * Math.Pow(spread, 2);

                            return (_bottomAreaSqFt + sideArea + cornerArea);
                        }
                    case FacilityShape.Amoeba:
                        {
                            return (_bottomAreaSqFt + _bottomPerimeterLengthFt * (depthIn) / 12 * _sideSlopeRatio);
                        }
                    case FacilityShape.UserDefined:
                        {
                            if (depthIn <= _storageDepth1In)
                            {
                                // Area at depth, linear interpolation
                                return _bottomAreaSqFt + (_surfaceAreaAtStorageDepth1SqFt - _bottomAreaSqFt) * (depthIn / _storageDepth1In);
                            }
                            else
                            {
                                // Area at depth, linear interpolation up to Surface Area At Storage Depth 2
                                return _surfaceAreaAtStorageDepth1SqFt + (_surfaceAreaAtStorageDepth2SqFt - _surfaceAreaAtStorageDepth1SqFt)
                                    * ((depthIn - _storageDepth1In) / (_storageDepth2In - _storageDepth1In));
                            }
                        }
                    default:
                        throw new NotImplementedException();
                }
            case FacilityType.PlanterSloped:
                throw new NotImplementedException();
            case FacilityType.Swale:
                throw new NotImplementedException();
            default:
                throw new NotImplementedException();
        }
    }

      /// <summary>
      /// Utility method to calculate the volume of basins and flat planters
      /// </summary>
      /// <param name="depthIn"></param>
      /// <returns></returns>
    private double CalculateVolume(double depthIn)
    {
      double volume = 0;
      switch (_type)
      {
        case FacilityType.Basin:
          if (_facilityShape == FacilityShape.Rectangle)
          {
            volume = //L*W*D + (L+W)*X*D^2 + (1/3)*pi*D^3*X^2
                (_bottomAreaSqFt * depthIn / 12) //L*W*D
              +
                (_bottomWidthFt + _bottomAreaSqFt / _bottomWidthFt)
                * _sideSlopeRatio
                * Math.Pow(depthIn / 12, 2) //(L+W)*X*D^2
              +
                (1.0 / 3) * Math.PI * Math.Pow(depthIn / 12, 3) * Math.Pow(_sideSlopeRatio, 2) //(1/3) * pi * D^3 * X^2
            ;
            break;
          }
          else if (_facilityShape == FacilityShape.Amoeba)
          {
              volume = ((depthIn / 12) * _sideSlopeRatio * _bottomPerimeterLengthFt) / 2 + (_bottomAreaSqFt * (depthIn / 12));
          }
          else if (_facilityShape == FacilityShape.UserDefined)
          {
              if (depthIn <= _storageDepth1In)
              {
                  // Area at depth, linear interpolation
                  double areaAtDepth = SurfaceAreaAtDepth(depthIn);
                  volume = (depthIn / 12) * (_bottomAreaSqFt + areaAtDepth) / 2;
              }
              else if (_hasSecondaryOverflow)
              {
                  // Area at depth, linear interpolation
                  double areaAtDepth1 = SurfaceAreaAtDepth(_storageDepth1In);
                  volume = (_storageDepth1In / 12) * (_bottomAreaSqFt + areaAtDepth1) / 2; // volume up to depth 1.
                  double areaAtDepth2 = SurfaceAreaAtDepth(depthIn);
                  volume += ((depthIn - _storageDepth1In) / 12) * (areaAtDepth1 + areaAtDepth2) / 2;
              }
              else
              {
                  throw new ArgumentException("Volume calculation at depth '" + depthIn + "' not defined");
              }
          }
          break;
        case FacilityType.PlanterFlat:
          volume = depthIn / 12 * _bottomAreaSqFt;
          break;
        case FacilityType.PlanterSloped: // overridden in SlopedFacility class
          if (SANITIZEINPUT)
            throw new NotImplementedException();
          else
            return 0;
        case FacilityType.Swale: // overridden in SlopedFacility class
          if (SANITIZEINPUT)
            throw new NotImplementedException();
          else
            return 0;
        default:
          if (SANITIZEINPUT)
            throw new ArgumentException("Volume calculation not defined");
          else
            return 0;          
      }
      return volume;
    }

    /// <summary>
    /// The pore space of the growing medium
    /// </summary>
    public double GrowingMediumPorespace
    {
      get { return DefaultGrowingMediumPorespace; }
    }

    /// <summary>
    /// The storage volume within the rock gallery, in cubic feet
    /// </summary>
    public virtual double RockStorageCapacityCuFt
    {
      get
      {
          double rockStorageDepth = _hasRockInfluencedSurfaceStorage ?
          _rockStorageDepthIn : Math.Min(_storageDepth3In, _rockStorageDepthIn);

          return RockStorageBottomAreaSqFt * rockStorageDepth / 12 * _rockVoidRatio;
      }
    }

      /// <summary>
      /// Based on the facility configuration, sets appropriate boolean values.
      /// For instance, Facility Configuration A and D do not have rock storage,
      /// thus, _hasRockStorage is set to "false".
      /// </summary>
    protected virtual void ConfigureFacility()
    {
      if (_configuration != FacilityConfiguration.A && _configuration != FacilityConfiguration.D)
        _hasRockStorage = true;
      else
        _hasRockStorage = false;

      if (_configuration == FacilityConfiguration.E)
        _hasSecondaryOverflow = true;
      else
        _hasSecondaryOverflow = false;

      if (_configuration == FacilityConfiguration.D)
        _isLined = true;
      else
        _isLined = false;

      if (_type == FacilityType.Basin)
      {
        _hasCustomRockStorageBottomArea = true;
        _specifySideSlope = true;
        _allowShapeSelection = true;
      }      
      else
      {
        _hasCustomRockStorageBottomArea = false;
        _specifySideSlope = false;
        _allowShapeSelection = false;
      }

      if (_configuration == FacilityConfiguration.A ||
        _configuration == FacilityConfiguration.B ||
        _configuration == FacilityConfiguration.E)
      {
        _hasRockInfluencedSurfaceStorage = true;
      }
      else
      {
        _hasRockInfluencedSurfaceStorage = false;
      }

    }

    /// <summary>
    /// Indicates whether a facility has passed validation
    /// </summary>
    public bool IsValid
    {
      get { return _validConfiguration; }
    }

    /// <summary>
    /// Performs checks to determine whether a Facility has required parameters assigned, and that
    /// all parameters are valid.
    /// </summary>
    /// <param name="facility">The Facility to validate</param>
    /// <param name="category">The Hierarchy Category the facility will be validated against</param>
    /// <param name="message">A message indicating the first error found in an invalid facility,
    /// or "Valid Configuration" if the facility is valid.</param>
    /// <returns>True if the facility parameters are valid, otherwise false</returns>
    public static bool Validate(Facility facility, int category, out string message)
    {
      facility._validConfiguration = true;

      switch (category)
      {
        case 1:
          if (facility._configuration != FacilityConfiguration.A
            && facility._configuration != FacilityConfiguration.B)
          {
            message = "Only Configuration A or B may be used for Category 1";
            facility._validConfiguration = false;
            return false;
          }
          break;
        case 2:
          break;
        case 3:
          if (facility._configuration == FacilityConfiguration.E
            || facility._configuration == FacilityConfiguration.F)
          {
            message = "Configuration E and F may not be used for Category 3";
            facility._validConfiguration = false;
            return false;
          }
          break;
        case 4:
          if (facility._configuration == FacilityConfiguration.E
            || facility._configuration == FacilityConfiguration.F)
          {
            message = "Configuration E and F may not be used for Category 4";
            facility._validConfiguration = false;
            return false;
          }
          break;
        default:
          message = "Unknown configuration, unable to validate facility";
          facility._validConfiguration = false;
          return false;
      }

      Stack<string> invalidated = new Stack<string>();

      switch (facility.Type)
      {
          case FacilityType.Basin:
              {
                  switch (facility.Shape)
                  {
                      case FacilityShape.Rectangle:
                          if (facility.BottomWidthFt < 0)
                              invalidated.Push("Bottom Width must be greater than or equal to zero.");
                          if (facility.SideSlopeRatio < 0)
                              invalidated.Push("Side Slope Ratio must be greater than or equal to zero.");
                          if (facility.FreeboardIn < 0)
                              invalidated.Push("Freeboard must be greater than or equal to zero.");
                          break;
                      case FacilityShape.Amoeba:
                          if (facility.BottomPerimeterLengthFt < Math.Sqrt(4 * facility.BottomAreaSqFt * Math.PI))
                              invalidated.Push("Bottom Perimeter Length must be greater than or equal to the circumference of a circle with area equal to bottom area.");
                          if (facility.SideSlopeRatio < 0)
                              invalidated.Push("Side Slope Ratio must be greater than or equal to zero.");
                          if (facility.FreeboardIn < 0)
                              invalidated.Push("Freeboard must be greater than or equal to zero.");
                          break;
                      case FacilityShape.UserDefined:
                          if (facility.SurfaceAreaAtStorageDepth1SqFt < facility.BottomAreaSqFt)
                              invalidated.Push("Surface Area at Storage Depth 1 must be greater than or equal to the Bottom Area.");
                          if (facility.FreeboardIn < 0)
                              invalidated.Push("Freeboard must be greater than or equal to zero.");
                          if (facility.HasSecondaryOverflow && (facility.SurfaceAreaAtStorageDepth2SqFt < facility.SurfaceAreaAtStorageDepth1SqFt))
                              invalidated.Push("Surface Area at Storage Depth 2 must be greater than or equal to the Surface Area at Storage Depth 1.");
                          break;
                      default:
                          break;
                  }

                  if (facility.BottomAreaSqFt < 0)
                      invalidated.Push("Bottom Area must be greater than or equal to zero.");
                  if (facility.StorageDepth1In <= 0)
                      invalidated.Push("Storage Depth 1 must be greater than zero.");

                  if (facility.HasRockStorage && (facility.RockStorageBottomAreaSqFt <= 0))
                      invalidated.Push("Rock Storage Bottom Area must be greater than zero.");

                  if (facility.HasSecondaryOverflow && (facility.StorageDepth2In <= facility.StorageDepth1In))
                      invalidated.Push("Storage Depth 2 must be greater than or equal to Storage Depth 1.");
                  break;
              }
          case FacilityType.PlanterFlat:
              {
                  if (facility.BottomAreaSqFt <= 0)
                      invalidated.Push("Bottom Area must be greater than zero.");
                  if (facility.StorageDepth1In <= 0)
                      invalidated.Push("Storage Depth 1 must be greater than zero.");

                  if (facility.HasRockStorage && (facility.RockStorageBottomAreaSqFt < 0))
                      invalidated.Push("Rock Storage Bottom Area must be greater than or equal to zero.");

                  if (facility.HasSecondaryOverflow && (facility.StorageDepth2In <= facility.StorageDepth1In))
                      invalidated.Push("Storage Depth 2 must be greater than Storage Depth 1.");
                  break;
              }
          case FacilityType.PlanterSloped:
          case FacilityType.Swale:
              {
                  if (facility is SlopedFacility)
                  {
                      SlopedFacility slopedFacility = facility as SlopedFacility;
                      foreach (SlopedFacilitySegment segment in slopedFacility.Segments)
                      {
                          if (segment.SegmentLengthFt <= 0)
                              invalidated.Push("Segment " + (slopedFacility.Segments.IndexOf(segment)+1).ToString() 
                                  + " Segment Length must be greater than zero.");
                          if (segment.CheckDamLengthFt < 0)
                              invalidated.Push("Segment " + (slopedFacility.Segments.IndexOf(segment)+1).ToString()
                                  + " Check Dam Length must be greater than or equal to zero.");
                          if (segment.SlopeRatio < 0)
                              invalidated.Push("Segment #" + (slopedFacility.Segments.IndexOf(segment)+1).ToString()
                                  + " Slope must be greater than or equal to zero.");
                          if (segment.BottomWidthFt < 0)
                              invalidated.Push("Segment #" + (slopedFacility.Segments.IndexOf(segment) + 1).ToString()
                                  + " Bottom Width must be greater than or equal to zero.");
                          if (segment.SideSlopeRightRatio < 0)
                              invalidated.Push("Segment #" + (slopedFacility.Segments.IndexOf(segment) + 1).ToString()
                                  + " Side Slope Ratio Right must be greater than or equal to zero.");
                          if (segment.SideSlopeLeftRatio < 0)
                              invalidated.Push("Segment #" + (slopedFacility.Segments.IndexOf(segment) + 1).ToString()
                                  + " Side Slope Ratio Left must be greater than or equal to zero.");
                          if (segment.DownstreamDepthIn <= 0)
                              invalidated.Push("Segment #" + (slopedFacility.Segments.IndexOf(segment) + 1).ToString()
                                  + " Downstream Depth must be greater than zero.");
                          if (segment.LandscapeWidthFt < segment.DownstreamTopWidthFt)
                              invalidated.Push("Segment #" + (slopedFacility.Segments.IndexOf(segment) + 1).ToString()
                                  + " Landscape Width must contain facility segment at Downstream Depth.");
                          if (facility.HasRockStorage && (segment.RockStorageWidthFt <= 0))
                              invalidated.Push("Segment #" + (slopedFacility.Segments.IndexOf(segment) + 1).ToString()
                                  + " Rock Storage Width must be greater than zero.");
                      }

                      if (facility.HasSecondaryOverflow && (facility.StorageDepth2In <= slopedFacility.Segments.Last().DownstreamDepthIn))
                          invalidated.Push("Storage Depth 2 must be greater than the Downstream Depth of the final facility segment.");
                  }
                  break;
              }
          default:
              break;
      }

      if (facility.GrowingMediumDepthIn <= 0)
          invalidated.Push("Growing Medium Depth must be greater than zero.");

      if (facility.HasRockStorage)
      {
          if (facility.RockStorageDepthIn <= 0)
            invalidated.Push("Rock Storage Depth must be greater than zero.");
          if (facility.RockVoidRatio <= 0 || facility.RockVoidRatio >= 1)
              invalidated.Push("Rock Porosity must be between zero and one.");
      }

      if ((facility.Configuration == FacilityConfiguration.F 
          || facility.Configuration == FacilityConfiguration.C)
          && ((facility.StorageDepth3In < 0) || (facility.StorageDepth3In > facility._rockStorageDepthIn)))
          invalidated.Push("Storage Depth 3 must be greater than or equal to zero and less than or equal to the Rock Storage Depth.");
        if(facility.Catchment.DesignInfiltrationNativeInchesPerHour < 0)
            invalidated.Push("Native Infiltration must be greater than or equal to zero.");
        if (facility.Catchment.ImportedMediumInfiltrationInchesPerHour < 0)
            invalidated.Push("Imported Medium Infiltration must be greater than or equal to zero.");


        if(invalidated.Count()>0)
        {
            message = string.Empty;
            while (invalidated.Count() > 0)
            {
                message += invalidated.Pop();
                if (invalidated.Count != 0)
                    message += System.Environment.NewLine;
            }
            facility._validConfiguration = false;
            return false;
        }
      message = "Valid configuration.";

      return facility._validConfiguration;
    }

  }

  /// <summary>
  /// Enumerator list valid facilty types
  /// </summary>
  public enum FacilityType 
  { 
      /// <summary>
      /// A sloped planter with or without side-slopes.
      /// </summary>
      Swale,
 
      /// <summary>
      /// A rectangular, amoeba, or user-defined shaped basin.
      /// </summary>
      Basin,
 
      /// <summary>
      /// A flat planter, with vertical walls.
      /// </summary>
      PlanterFlat,
 
      /// <summary>
      /// A sloped planter with or without side-slopes.
      /// Essentially the same as a Swale.
      /// </summary>
      PlanterSloped 
  };

  /// <summary>
  /// Enumerator listing valid facility shapes
  /// </summary>
  public enum FacilityShape 
  { 
      /// <summary>
      /// A trapezoidal basin with rounded corners and sloped sides.
      /// </summary>
      Rectangle,
 
      /// <summary>
      /// A kidney-shaped basin with sloped sides.
      /// </summary>
      Amoeba,
 
      /// <summary>
      /// Represents a sloped planter or swale.
      /// </summary>
      Sloped,
 
      /// <summary>
      /// A facility defined by its surface area at different depths.
      /// </summary>
      UserDefined 
  };


  /// <summary>
  /// Enumerator listing various facility configurations
  /// </summary>
  public enum FacilityConfiguration 
  { 
      /// <summary>
      /// An infiltration configuration with the following qualities:
      /// 1. No rock storage.
      /// 2. A single surface overflow to an approved route of conveyance.
      /// 3. The native infiltration rate can impact the surface infiltration rate.
      /// </summary>
      A = 'A',

      /// <summary>
      /// An infiltration configuration with the following qualities:
      /// 1. Surface infiltration to rock storage without lag-time calculations.
      /// 2. A single surface overflow to an approved route of conveyance.
      /// 3. The native infiltration rate can impact the surface infiltration rate.
      /// </summary>
      B = 'B',

      /// <summary>
      /// A flow-through configuration with the following qualities:
      /// 1. Surface infiltration to rock storage with lag-time calculations.
      /// 2. A single surface overflow to an approved route of conveyance.
      /// 3. A single rock storage overflow to an approved route of conveyance.
      /// 4. The native infiltration rate cannot impact the surface infiltration rate.
      /// </summary>
      C = 'C',

      /// <summary>
      /// A flow-through configuration with the following qualities:
      /// 1. No rock storage.
      /// 2. Completely lined with zero infiltration and an underdrain to an approved route of conveyance.
      /// 3. A single surface overflow to an approved route of conveyance.
      /// 4. Surface infiltration to an approved route of conveyance, after lag-time through the facility topsoil.
      /// </summary>
      D = 'D',

      /// <summary>
      /// A UIC configuration with the following qualities:
      /// 1. Surface infiltration to rock storage without lag-time calculations.
      /// 2. A primary surface overflow to rock storage.
      /// 3. A secondary surface overflow to an approved route of conveyance.
      /// 4. The native infiltration rate can impact the surface infiltration rate.
      /// </summary>
      E = 'E',

      /// <summary>
      /// A UIC configuration with the following qualities:
      /// 1. Surface infiltration to rock storage with lag-time calculations.
      /// 2. A single surface overflow to rock storage.
      /// 3. A single rock storage overflow to an approved route of conveyance.
      /// 4. The native infiltration rate cannot impact the surface infiltration rate.
      /// </summary>
      F = 'F' 
  };

  /// <summary>
  /// Enumerator listing available stormwater management categories
  /// </summary>
  public enum HierarchyCategory 
  { 
      /// <summary>
      /// Total onsite infiltration.
      /// </summary>
      Category1 = 1,
 
      /// <summary>
      /// Total onsite infiltration that overflows to a rule-authorized UIC.
      /// </summary>
      Category2 = 2,
 
      /// <summary>
      /// Onsite detention that overflows to a drainageway, river, or storm-only pipe.
      /// </summary>
      Category3 = 3, 

      /// <summary>
      /// Onsite detention that overflows to the combined sewer system.
      /// </summary>
      Category4 = 4 
  };

  /// <summary>
  /// Enumerator listing various discharge points
  /// A = Direct discharge to a river of Multnomah County Drainage District facility
  /// B = Discharge to an overland storm drainage system, such as a stream, drainageway or ditch either directly or through a storm pipe system
  /// C = Discharge to a storm sewer or drainage system that does not meet above criteria
  /// </summary>
  public enum DischargePoint 
  { 
      /// <summary>
      /// River of major water body.
      /// </summary>
      A = 'A', 

      /// <summary>
      /// Stream, drainageways, ditches, or storm pipe that eventually discharges to one of these.
      /// </summary>
      B = 'B',
 
      /// <summary>
      /// Any other discharge point (storm pipe that discharges to major water body).
      /// </summary>
      C = 'C' 
  };

}
