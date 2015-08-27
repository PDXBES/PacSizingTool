using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BesAsm.Swsp.PacSizingTool
{
  /// <summary>
  /// The discrete segments which make up a sloped facility
  /// </summary>
  [Serializable]
  public class SlopedFacilitySegment
  {
    private double _segmentLengthFt = 6;
    private double _checkDamLengthFt = 1;
    private double _slopeRatio = 0.02;
    private double _bottomWidthFt = 2;
    private double _sideSlopeRightRatio = 3;
    private double _sideSlopeLeftRatio = 3;
    private double _downstreamDepthIn = 9;
    private double _landscapeWidthFt = 8;

    private double _rockStorageWidthFt = 2;

    private SlopedFacility _facility; // Parent

    /// <summary>
    /// Constructor for a sloped facility segment
    /// </summary>
    public SlopedFacilitySegment(SlopedFacility facility)
    {
      _facility = facility;
    }

    /// <summary>
    /// Constructor for a sloped facility segment.
    /// Assigns segment to current sloped facility pointer.
    /// </summary>
    public SlopedFacilitySegment()
    {
      _facility = SlopedFacility.GetSlopedFacility();
    }
    #region Base Parameter Getters and Setters
    /// <summary>
    /// The length of a segment in feet.
    /// </summary>
    public double SegmentLengthFt
    {
      get { return _segmentLengthFt; }
      set { _segmentLengthFt = value; }
    }
    /// <summary>
    /// The length of a check dam in feet. Values less than five should
    /// trigger a warning.
    /// </summary>
    public double CheckDamLengthFt
    {
      get { return _checkDamLengthFt; }
      set { _checkDamLengthFt = value; }
    }
    /// <summary>
    /// The longitudinal slope of the segment expressed as a ratio.    
    /// Should be between zero and 1.
    /// </summary>
    public double SlopeRatio
    {
      get { return _slopeRatio; }
      set { _slopeRatio = value; }
    }
    /// <summary>
    /// The bottom width of a segment in feet.
    /// </summary>
    public double BottomWidthFt
    {
      get { return _bottomWidthFt; }
      set { _bottomWidthFt = value; }
    }
    /// <summary>
    /// The left-bank side slope of the segment, expressed as fraction
    /// of X:1 where X is this parameter. For swales, this paraemter should
    /// be >= 3. For sloped planters, this value must be 0.
    /// </summary>
    public double SideSlopeRightRatio
    {
      get { return _sideSlopeRightRatio; }
      set { _sideSlopeRightRatio = value; }
    }
    /// <summary>
    /// The right-bank side slope of the segment, expressed as fraction
    /// of X:1 where X is this parameter. For swales, this paraemter should
    /// be >= 3. For sloped planters, this value must be 0.
    /// </summary>
    public double SideSlopeLeftRatio
    {
      get { return _sideSlopeLeftRatio; }
      set { _sideSlopeLeftRatio = value; }
    }
    /// <summary>
    /// The downstream depth of a segment in inches. Values less than 6 inches
    /// should trigger a warning.
    /// </summary>
    public double DownstreamDepthIn
    {
      get { return _downstreamDepthIn; }
      set { _downstreamDepthIn = value; }
    }
    /// <summary>
    /// The width of the segment landscaping in feet. If this value is less than
    /// BottomWidthFt a warning should be triggered.
    /// </summary>
    public double LandscapeWidthFt
    {
      get { return _landscapeWidthFt; }
      set { _landscapeWidthFt = value; }
    }
    /// <summary>
    /// The width of the rock storage gallery.
    /// If this value is greater than the LandscapeWidthFt a
    /// warning should be triggered.
    /// </summary>
    public double RockStorageWidthFt
    {
      get { return _rockStorageWidthFt; }
      set { _rockStorageWidthFt = value; }
    }

    #endregion
    /// <summary>
    /// The calculated segment length used for analysis.
    /// Generally this will be the specified segment length minus the check dam
    /// length.
    /// If the facility is too short and/or too steep, the adjusted length will
    /// be the length of the facility at the point the upstream depth is zero.
    /// </summary>
    public double AdjustedSegmentLengthFt
    {
      get
      {
          return AdjustedSegmentLengthAtDepthFt(_downstreamDepthIn);
      }
    }

    /// <summary>
    /// The calculated depth at the upstream end of the facilty. This
    /// parameter is calculated as the DownstreamDepthIn - AdjustedSegmentLengthFt * SlopeRatio.
    /// If the UpstreamDepthIn is zero a warning should be presented to the user.
    /// </summary>
    public double UpstreamDepthIn
    {
      get { return UpstreamDepthAtDepthIn(_downstreamDepthIn); }
    }

    /// <summary>
    /// The calculated width of the facility at the downstream end.
    /// </summary>
    /// 
    public double DownstreamTopWidthFt
    {
      get
      {
          return DownstreamTopWidthAtDepthFt(_downstreamDepthIn);
      }
    }

    /// <summary>
    /// The calculated width of the facility at the upstream end (or the point where the
    /// upstream depth is zero).
    /// </summary>
    public double UpstreamTopWidthFt
    {
      get
      {
          return UpstreamTopWidthAtDepthFt(_downstreamDepthIn);
      }
    }
      
    /// <summary>
    /// The cross-sectional area of the facility at the downstream end.
    /// </summary>
    public double DownstreamAreaSqFt
    {
      get
      {
          return DownstreamAreaAtDepthSqFt(_downstreamDepthIn);
      }
    }

    /// <summary>
    /// The cross-sectional area of the facility at the upstream end (or the
    /// point where the upstream depth is zero).
    /// </summary>
    public double UpstreamAreaSqFt
    {
      get
      {
          return UpstreamAreaAtDepthSqFt(_downstreamDepthIn);
      }
    }

    /// <summary>
    /// The surface storage capacity of the segment in cubic feet.
    /// </summary>
    public double SurfaceCapacityCuFt
    {
      get
      {
          return SurfaceCapacityAtDepthCuFt(_downstreamDepthIn);
      }
    }

    /// <summary>
    /// The surface area of the facility when the facility is full.
    /// </summary>
    public double SurfaceAreaAtDepth1SqFt
    {
        get
        {
            return (UpstreamTopWidthFt + DownstreamTopWidthFt) / 2 * AdjustedSegmentLengthFt;
        }
    }

    /// <summary>
    /// The area of the facility available for infiltration when the facility
    /// is 75% full.
    /// </summary>
    public double InfiltrationArea75PercentSqFt
    {
      get
      {
        double dsDepth75 = 0.75 * _downstreamDepthIn;
        double usTopWidth75 = UpstreamTopWidthAtDepthFt(dsDepth75);
        double dsTopWidth75 = DownstreamTopWidthAtDepthFt(dsDepth75);
        double len75 = AdjustedSegmentLengthAtDepthFt(dsDepth75);

        return (usTopWidth75 + dsTopWidth75) / 2 * len75;
      }
    }

    /// <summary>
    /// The length of the rock storage gallery in a segment in feet
    /// </summary>
    public double RockStorageLengthFt
    {
      get { return SegmentLengthFt; }
    }


    /// <summary>
    /// The storage area of the segment rock gallery in square feet
    /// </summary>
    public double RockStorageBottomAreaSqFt
    {
      get
      {
        switch (SlopedFacility.GetSlopedFacility().Configuration)
        {
          case FacilityConfiguration.A:
            return InfiltrationArea75PercentSqFt;
          case FacilityConfiguration.D:
            return 0;
          default:
            return RockStorageWidthFt * RockStorageLengthFt;
        }
      }
      set
      {

      }
    }

   
    /// <summary>
    /// The rock storage volume of a segment in cubic feet.
    /// </summary>
    public double RockStorageVolumeCuFt
    {
      //A=3 B=4 C=1 D=2 E=5 F=6
      get
      {
        switch (SlopedFacility.GetSlopedFacility().Configuration)
        {
          case FacilityConfiguration.C:
          case FacilityConfiguration.F:
            return RockStorageWidthFt *
              RockStorageLengthFt *
              Math.Min(SlopedFacility.GetSlopedFacility().StorageDepth3In, SlopedFacility.GetSlopedFacility().RockStorageDepthIn) / 12 *
              SlopedFacility.GetSlopedFacility().RockVoidRatio;
          default:
            return RockStorageWidthFt *
              RockStorageLengthFt *
              SlopedFacility.GetSlopedFacility().RockStorageDepthIn / 12 *
              SlopedFacility.GetSlopedFacility().RockVoidRatio;
        }
      }
    }

      /// <summary>
      /// Computes the surface capacity at a specific depth
      /// </summary>
      /// <param name="depth">
      /// The depth of water at the lower end of the facility
      /// </param>
      /// <returns></returns>
    private double AdjustedSegmentLengthAtDepthFt(double depth)
    {
        double len = _segmentLengthFt - (_checkDamLengthFt / 2);
        if (depth - (len * _slopeRatio * 12) > 0)
            return Math.Max(len, 0);
        else
            return Math.Max((depth / 12) / _slopeRatio, 0);
    }
    private double UpstreamDepthAtDepthIn(double depth)
    {
        return Math.Max(depth - (AdjustedSegmentLengthAtDepthFt(depth) * _slopeRatio * 12), 0);
    }
    private double DownstreamTopWidthAtDepthFt(double depth)
    {
        return _bottomWidthFt +
          depth * _sideSlopeRightRatio / 12 +
          depth * _sideSlopeLeftRatio / 12;
    }
    private double UpstreamTopWidthAtDepthFt(double depth)
    {
        return _bottomWidthFt +
                  UpstreamDepthAtDepthIn(depth) * _sideSlopeRightRatio / 12 +
                  UpstreamDepthAtDepthIn(depth) * _sideSlopeLeftRatio / 12;
    }
    private double DownstreamAreaAtDepthSqFt(double depth)
    {
        return ((DownstreamTopWidthAtDepthFt(depth) + _bottomWidthFt) / 2) * depth / 12;
    }
    private double UpstreamAreaAtDepthSqFt(double depth)
    {
        return ((UpstreamTopWidthAtDepthFt(depth) + _bottomWidthFt) / 2) * UpstreamDepthAtDepthIn(depth) / 12;
    }
    internal double SurfaceCapacityAtDepthCuFt(double depth)
    {
        return (DownstreamAreaAtDepthSqFt(depth) + UpstreamAreaAtDepthSqFt(depth)) / 2 * AdjustedSegmentLengthAtDepthFt(depth);
    }
  }
}
