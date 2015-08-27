using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BesAsm.Swsp.PacSizingTool
{
  /// <summary>
  /// Class for specifying a sloped facility (Swale or Sloped Planter).
  /// A sloped facility is defined by a number of discrete segments.
  /// </summary>
  [Serializable]
  public class SlopedFacility : Facility
  {
    private List<SlopedFacilitySegment> _segments;

    private static SlopedFacility _this;    

    /// <summary>
    /// Constructor for a sloped facility
    /// </summary>
    /// <param name="type">The FacilityType; should be Swale or SlopedPlanter</param>
    /// <param name="configuration">The Configuration of the facility</param>
    /// <param name="catchment">The catchment for the facility</param>
    public SlopedFacility(FacilityType type, FacilityConfiguration configuration, Catchment catchment)
      : base(type, configuration, catchment)
    {
      _segments = new List<SlopedFacilitySegment>();
      SlopedFacility._this = this;
    }

    /// <summary>
    /// Constructor for a sloped facility
    /// </summary>
    /// <param name="type">The FacilityType; should be Swale or SlopedPlanter</param>
    /// <param name="configuration">The Configuration of the facility</param>
    /// <param name="catchment">The catchment for the facility</param>
    /// <param name="segments">A list of Segments to load the facility with</param>
    public SlopedFacility(FacilityType type, FacilityConfiguration configuration, Catchment catchment, List<SlopedFacilitySegment> segments)
      : base(type, configuration, catchment)
    {
      _segments = segments;
      SlopedFacility._this = this;
    }

    /// <summary>
    /// The collection of segments which make up the sloped facility.
    /// Segments can be added or deleted by manipulating this list directly,
    /// or via SlopedFacility.AddSegment() and SlopedFacility.DeleteSegment().
    /// </summary>
    public List<SlopedFacilitySegment> Segments
    {
      get { return _segments; }
      set { _segments = value; }
    }

    internal static SlopedFacility GetSlopedFacility()
    {
      return SlopedFacility._this;
    }

    /// <summary>
    /// Adds the a segment to the facility.
    /// </summary>
    /// <param name="segment">The segment to add.</param>
    public void AddSegment(SlopedFacilitySegment segment)
    {
      _segments.Add(segment);
    }
    /// <summary>
    /// Removes a segment from the facility.
    /// </summary>
    /// <param name="segment">The segment to delete.</param>
    public void DeleteSegment(SlopedFacilitySegment segment)
    {
      if (_segments.Contains(segment))
        _segments.Remove(segment);
    }

    /// <summary>
    /// Gets the total facility volume, which is a primary input
    /// to the sizing calculations.
    /// </summary>
    public override double SurfaceCapacityAtDepth1CuFt
    {
      get
      {
        double vol = 0;
        foreach (SlopedFacilitySegment segment in _segments)
        {
          vol += segment.SurfaceCapacityCuFt;
        }
        return vol;
      }
    }

    /// <summary>
    /// The surface storage capacity in a sloped facility in cubic feet before overflow to
    /// a drain. It is the sum of the surface capacity of each segment in the sloped facility.
    /// </summary>
    public override double SurfaceCapacityAtDepth2CuFt
    {
      get
      {
          if (this.HasSecondaryOverflow)
          {

              double vol = 0;
              // Add up all of the segment surface volumes at depth 1
              foreach (SlopedFacilitySegment segment in _segments)
              {
                  vol += segment.SurfaceCapacityCuFt;
              }
              // Subtract the volume of the last segment
              vol -= _segments.Last().SurfaceCapacityCuFt;
              // Add the volume of the last segment at the secondary overflow volume
              vol += _segments.Last().SurfaceCapacityAtDepthCuFt(this._storageDepth2In);
              return vol;
          }
          else
              return 0;
      }
    }
    /// <summary>
    /// Gets the total infiltrating area available when
    /// the facility is 75% full, which is a primary input
    /// to the sizing calculations.
    /// </summary>
    /// public double InfiltAreaAt75PercentDepth1SqFt
    public override double InfiltAreaAt75PercentDepth1SqFt
    {
      get
      {
        double area = 0;
        foreach (SlopedFacilitySegment segment in _segments)
        {
          area += segment.InfiltrationArea75PercentSqFt;
        }
        return area;
      }
    }
   
    /// <summary>
    /// Gets the total rock storage bottom area available when
    /// the facility is 75% full, which is a primary input
    /// to the sizing calculations.
    /// </summary>
    public override double RockStorageBottomAreaSqFt
    {
      get
      {
        double area = 0;
        foreach (SlopedFacilitySegment segment in _segments)
        {
          area += segment.RockStorageBottomAreaSqFt;
        }
        return area;
      }
      set
      {
        
      }
    }
    /// <summary>
    /// Gets the total rock storage volume, which is a primary input
    /// to the sizing calculations.
    /// </summary>
    public override double RockStorageCapacityCuFt
    {
      get
      {
        double vol = 0;
        foreach (SlopedFacilitySegment segment in _segments)
        {
          vol += segment.RockStorageVolumeCuFt;
        }
        return vol;
      }      
    }

    /// <summary>
    /// The bottom area of the sloped facility in square feet.
    /// </summary>
    public override double BottomAreaSqFt
    {
      get
      {
        return InfiltAreaAt75PercentDepth1SqFt;
      }
      set
      {
        base.BottomAreaSqFt = value;
      }
    }

      /// <summary>
    /// The surface area at storage depth 1 in square feet.
    /// </summary>
    public override double SurfaceAreaAtStorageDepth1SqFt
    {
        get
        {
            double area = 0;
            foreach (SlopedFacilitySegment segment in _segments)
            {
                area += segment.SurfaceAreaAtDepth1SqFt;
            }
            return area;
        }
        set { } // Null setter, to allow simplified UI logic and maintain contract.
    }

    /// <summary>
    /// The total facility area of a sloped facility in square feet; it is the sum of the area of each individual segment
    /// </summary>
    public override double TotalFacilityAreaSqFt
    {
      get
      {
        double area = 0;
        foreach (SlopedFacilitySegment s in _segments)
          area += s.SegmentLengthFt * s.LandscapeWidthFt;

        return area;
      }
    }

    /// <summary>
    /// Internal method to configure facility properties.
    /// </summary>
    protected override void ConfigureFacility()
    {
      base.ConfigureFacility();

      _hasCustomRockStorageBottomArea = true;

      if (Type == FacilityType.Swale)
        _specifySideSlope = true;
      else
        _specifySideSlope = false;

    }
  }
  
}
