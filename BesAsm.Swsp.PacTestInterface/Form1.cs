using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;

namespace BesAsm.Swsp.PacSizingTool
{
  public partial class Form1 : Form
  {
    bool _validFacility = true;

    public Form1()
    {
      InitializeComponent();
    }

    private void btnTest_Click(object sender, EventArgs e)
    {
      try
      {
        //Create a new segment
        SlopedFacilitySegment segment = new SlopedFacilitySegment();

        //Assign values to segment. All segments have these fields:
        segment.SegmentLengthFt = 10;
        segment.CheckDamLengthFt = 2;
        segment.SlopeRatio = 0.02;
        segment.SideSlopeRightRatio = 4;
        segment.SideSlopeLeftRatio = 4;
        segment.DownstreamDepthIn = 9;
        segment.LandscapeWidthFt = 2;

        segment.RockStorageWidthFt = 4; //Only facility types with rock galleries use this field; see parameter matrix for details

        //Create a catchment, no change for sloped facilities
        Catchment catchment = new Catchment("Test Catchment"); 

        //Create a SlopedFacility object, constructed the same way as a standard facility
        SlopedFacility facility = new SlopedFacility(FacilityType.Swale, FacilityConfiguration.A, catchment);

        //Segments can be added or deleted from the sloped facility.
        facility.AddSegment(segment);

        facility.DeleteSegment(segment);



        PerformCalculations();
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error: " + ex.Message);
      }
    }

    private void Form1_Load(object sender, EventArgs e)
    {
      cmbFacilityConfiguration.SelectedIndex = 1;
      cmbFacilityShape.SelectedIndex = 0;
      cmbFacilityType.SelectedIndex = 1;
      cmbHierachy.SelectedIndex = 1;
      cmbInfiltrationProcedure.SelectedIndex = 0;
      cmbDischargePoint.SelectedIndex = 0;

      this.cmbHierachy.SelectedIndexChanged += cmbHierachy_SelectedIndexChanged;
      this.cmbFacilityType.SelectedIndexChanged += cmbFacilityType_SelectedIndexChanged;
      this.cmbFacilityConfiguration.SelectedIndexChanged += cmbFacilityConfiguration_SelectedIndexChanged;
      this.cmbFacilityShape.SelectedIndexChanged += cmbFacilityShape_SelectedIndexChanged;

      ValidateFacility(cmbHierachy.Text, cmbFacilityType.Text, cmbFacilityConfiguration.Text);
    }

    private void cmbHierachy_SelectedIndexChanged(object sender, EventArgs e)
    {
      ValidateFacility(cmbHierachy.Text, cmbFacilityType.Text, cmbFacilityConfiguration.Text);
    }

    private void cmbFacilityType_SelectedIndexChanged(object sender, EventArgs e)
    {
      ValidateFacility(cmbHierachy.Text, cmbFacilityType.Text, cmbFacilityConfiguration.Text);
    }

    private void cmbFacilityConfiguration_SelectedIndexChanged(object sender, EventArgs e)
    {
      ValidateFacility(cmbHierachy.Text, cmbFacilityType.Text, cmbFacilityConfiguration.Text);
    }

    private void cmbFacilityShape_SelectedIndexChanged(object sender, EventArgs e)
    {
      ValidateFacility(cmbHierachy.Text, cmbFacilityType.Text, cmbFacilityConfiguration.Text);
    }

    private void PerformCalculations()
    {
      Facility facility =  ValidateFacility(cmbHierachy.Text, cmbFacilityType.Text, cmbFacilityConfiguration.Text);
      double preCurveNumber = Convert.ToDouble(txtPreCurveNumber.Text);

      int hierarchyNumber = Convert.ToInt32(cmbHierachy.Text);

      PacResults results = PacExecutor.PerformCalculations(facility.Catchment, facility, hierarchyNumber);

      txtPrPassFail.Text = results.PollutionReductionScore.ToString();
      txt10YrPassFail.Text = results.TenYearScore.ToString();
      txtPrPercentCapacity.Text = string.Format("{0:0.0}%", results.PollutionReductionSurfaceCapacity * 100);
      txt10YrPercentCapacity.Text = string.Format("{0:0.0}%", results.TenYearSurfaceCapacity * 100);
      txt2YrPeakInFlow.Text = string.Format("{0:0.000}", results.TwoYearPeakInflow);
      txt5YrPeakInFlow.Text = string.Format("{0:0.000}", results.FiveYearPeakInflow);
      txt10YrPeakInFlow.Text = string.Format("{0:0.000}", results.TenYearPeakInflow);
      txt25YrPeakInFlow.Text = string.Format("{0:0.000}", results.TwentyfiveYearPeakInflow);
      txt2YrPeakFlow.Text = string.Format("{0:0.000}", results.TwoYearPeakOverflow);
      txt5YrPeakFlow.Text = string.Format("{0:0.000}", results.FiveYearPeakOverflow);
      txt10YrPeakFlow.Text = string.Format("{0:0.000}", results.TenYearPeakOverflow);
      txt25YrPeakFlow.Text = string.Format("{0:0.000}", results.TwentyfiveYearPeakOverflow);

      txtPrRockCap.Text = string.Format("{0:0.0}%", results.PollutionReductionPercentRockCapacity * 100);
      txt10YrRockCap.Text = string.Format("{0:0.0}%", results.TenYearPercentRockCapacity * 100);
      txtTotalFacilityArea.Text = string.Format("{0:0}", facility.TotalFacilityAreaSqFt);
      txtSizingFactor.Text = string.Format("{0:0.0}%", facility.FacilitySizingRatio * 100);

      txt10YrOverflowCuFt.Text = string.Format("{0:0.000}", results.TenYearTotalOverflowVolume);
      txtPROverflowCuFt.Text = string.Format("{0:0.000}", results.PollutionReductionTotalOverflowVolume);

      PlotResults(chartPollutionReductionAboveGrade, results.PollutionReductionResults.AboveGradePrimaryResults, results.PollutionReductionResults.AboveGradeSecondaryResults);
      PlotResults(chartPollutionReductionBelowGrade, results.PollutionReductionResults.BelowGradePrimaryResults, results.PollutionReductionResults.BelowGradeSecondaryResults);
      PlotResults(chartTenYearAboveGrade, results.TenYearResults.AboveGradePrimaryResults, results.TenYearResults.AboveGradeSecondaryResults);
      PlotResults(chartTenYearBelowGrade, results.TenYearResults.BelowGradePrimaryResults, results.TenYearResults.BelowGradeSecondaryResults);

      String selected = this.cmbHierachy.SelectedItem as String;
      InfiltrationTestType infiltrationTestType;

      switch (cmbInfiltrationProcedure.SelectedIndex)
      {
          case(0):
              infiltrationTestType = InfiltrationTestType.OpenPitFallingHead;
              break;
          case(1):
              infiltrationTestType = InfiltrationTestType.EncasedFallingHead;
              break;
          case(2):
              infiltrationTestType = InfiltrationTestType.DoubleRingInfiltometer;
              break;
          default:
              infiltrationTestType = InfiltrationTestType.OpenPitFallingHead;
              break;
      }

      if (selected.Equals("3") || selected.Equals("4"))
      {
          Catchment preCatchment = new Catchment("Pre-Developed Catchment A")
          {
              ImperviousAreaSquareFeet = facility.Catchment.ImperviousAreaSquareFeet,
              AcceptableSeparationFromGroundwater = chkMeetsGroundwaterRequirements.Checked,
              CurveNumber = preCurveNumber,
              TimeOfConcentrationMinutes = facility.Catchment.TimeOfConcentrationMinutes,
              TestedInfiltrationRateInchesPerHour = facility.Catchment.DesignInfiltrationNativeInchesPerHour,
              InfiltrationTestType = infiltrationTestType
          };

          char dischargePoint = 'A';
          switch(this.cmbDischargePoint.SelectedIndex)
          {
              case 0:
                  dischargePoint = 'A';
                  break;
              case 1:
                  dischargePoint = 'B';
                  break;
              case 2:
                  dischargePoint = 'C';
                  break;
              default:
                  dischargePoint = 'A';
                  break;
          }

          results = PacExecutor.PerformCalculations(facility.Catchment, preCatchment, facility, hierarchyNumber, dischargePoint);

          txtPre2YrPeakFlow.Text = string.Format("{0:0.000}", results.PreDevelopedTwoYearPeakInflow);
          txtPre5YrPeakFlow.Text = string.Format("{0:0.000}", results.PreDevelopedFiveYearPeakInflow);
          txtPre10YrPeakFlow.Text = string.Format("{0:0.000}", results.PreDevelopedTenYearPeakInflow);
          txtPre25YrPeakFlow.Text = string.Format("{0:0.000}", results.PreDevelopedTwentyfiveYearPeakInflow);
          tblPeakTable.RowStyles[1].SizeType = SizeType.AutoSize;
          if(selected.Equals("3"))
          {
              switch (this.cmbDischargePoint.SelectedIndex)
              {
                  case 0:
                      lbl2YrPassFail.Text = "N/A";
                      lbl5YrPassFail.Text = "N/A";
                      lbl10YrPassFail.Text = "N/A";
                      lbl25YrPassFail.Text = "N/A";
                      break;
                  case 1:
                      lbl2YrPassFail.Text = results.TwoYearPeakOverflow <= (results.PreDevelopedTwoYearPeakInflow/2) ? "Pass" : "Fail";
                      lbl5YrPassFail.Text = results.FiveYearPeakOverflow <= results.PreDevelopedFiveYearPeakInflow ? "Pass" : "Fail";
                      lbl10YrPassFail.Text = results.TenYearPeakOverflow <= results.PreDevelopedTenYearPeakInflow ? "Pass" : "Fail";
                      lbl25YrPassFail.Text = results.TwentyfiveYearPeakOverflow <= results.PreDevelopedTwentyfiveYearPeakInflow ? "Pass" : "Fail";
                      break;
                  case 2:
                      lbl2YrPassFail.Text = results.TwoYearPeakOverflow <= results.PreDevelopedTwoYearPeakInflow ? "Pass" : "Fail";
                      lbl5YrPassFail.Text = results.FiveYearPeakOverflow <= results.PreDevelopedFiveYearPeakInflow ? "Pass" : "Fail";
                      lbl10YrPassFail.Text = results.TenYearPeakOverflow <= results.PreDevelopedTenYearPeakInflow ? "Pass" : "Fail";
                      lbl25YrPassFail.Text = "N/A";
                      break;
                  default:
                      lbl2YrPassFail.Text = "N/A";
                      lbl5YrPassFail.Text = "N/A";
                      lbl10YrPassFail.Text = "N/A";
                      lbl25YrPassFail.Text = "N/A";
                      break;
              }
          }
          else
          {
              lbl2YrPassFail.Text = "N/A";
              lbl5YrPassFail.Text = "N/A";
              lbl10YrPassFail.Text = "N/A";
              lbl25YrPassFail.Text = results.TwentyfiveYearPeakOverflow <= results.PreDevelopedTenYearPeakInflow ? "Pass" : "Fail";
          }
      }
      else
      {
          tblPeakTable.RowStyles[1].SizeType = SizeType.Absolute;
          tblPeakTable.RowStyles[1].Height = 0;
          lbl2YrPassFail.Text = "N/A";
          lbl5YrPassFail.Text = "N/A";
          lbl10YrPassFail.Text = "N/A";
          lbl25YrPassFail.Text = "N/A";
      }
      lblFlowControlScore.Text = results.FlowControlScore.ToString();
      lbl2YrPassFail.Text = results.TwoYearFlowControlScore.ToString();
      lbl5YrPassFail.Text = results.FiveYearFlowControlScore.ToString();
      lbl10YrPassFail.Text = results.TenYearFlowControlScore.ToString();
      lbl25YrPassFail.Text = results.TwentyfiveYearFlowControlScore.ToString();
    }

    private void PlotResults(Chart chart, List<Hydrograph> primaryResults, List<Hydrograph> secondaryResults)
    {
      chart.Series.Clear();

      if (primaryResults.Count == 0 && secondaryResults.Count == 0)
      {
        chart.Enabled = false;
        return;
      }

      foreach (Hydrograph h in primaryResults)
      {
        Series s = chart.Series.Add(h.Name);
        s.Points.DataBindY(h.AsArray());
        s.ChartType = SeriesChartType.Line;
      }

      foreach (Hydrograph h in secondaryResults)
      {
        Series s = chart.Series.Add(h.Name);
        s.Points.DataBindY(h.AsArray());
        s.YAxisType = AxisType.Secondary;
        s.ChartType = SeriesChartType.Line;
      }
    }

    private void btnExport_Click(object sender, EventArgs e)
    {
      SaveFileDialog sfd = new SaveFileDialog();
      sfd.DefaultExt = "csv";
      sfd.Filter = "Comma-separated value (*.csv)|*.csv|All files (*.*)|*.*";

      if (sfd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
        return;

      ExportCharts(sfd.FileName);

      MessageBox.Show("Chart data exported");
    }

    private void ExportCharts(string fileName)
    {
      System.IO.StreamWriter writer = null;
      try
      {
        writer = new StreamWriter(fileName);

        writer.Write("Time Step,");
        foreach (Series series in chartPollutionReductionAboveGrade.Series)
        {
          writer.Write(series.Name + ",");
        }
        foreach (Series series in chartPollutionReductionBelowGrade.Series)
        {
          writer.Write(series.Name + ",");
        }
        writer.WriteLine();

        for (int i = 0; i < RainfallEvent.ScsOneARainfallDistribution.Length; i++)
        {
          writer.Write(i + ",");
          foreach (Series series in chartPollutionReductionAboveGrade.Series)
          {
            writer.Write(series.Points[i].YValues[0] + ",");
          }
          foreach (Series series in chartPollutionReductionBelowGrade.Series)
          {
            writer.Write(series.Points[i].YValues[0] + ",");
          }
          writer.WriteLine();
        }

        writer.Close();
      }
      finally
      {
        if (writer != null)
          writer.Close();
      }
    }

    private Facility ValidateFacility(string hierarchy, string facilityType, string configuration)
    {
      try
      {
          //Read catchment parameters to local variables
          double imperviousArea = Convert.ToDouble(txtImperviousArea.Text);
          double curveNumber = Convert.ToDouble(txtCurveNumber.Text);
          double preCurveNumber = Convert.ToDouble(txtPreCurveNumber.Text);
          double timeOfConcentration = Convert.ToDouble(txtTimeOfConcentration.Text);
          double nativeInfiltrationRate = Convert.ToDouble(txtNativeSoilInfiltrationRate.Text);
          InfiltrationTestType infiltrationTestType;

          switch (cmbInfiltrationProcedure.SelectedIndex)
          {
              case (0):
                  infiltrationTestType = InfiltrationTestType.OpenPitFallingHead;
                  break;
              case (1):
                  infiltrationTestType = InfiltrationTestType.EncasedFallingHead;
                  break;
              case (2):
                  infiltrationTestType = InfiltrationTestType.DoubleRingInfiltometer;
                  break;
              default:
                  infiltrationTestType = InfiltrationTestType.OpenPitFallingHead;
                  break;
          }


          //Create catchment object local variables
          Catchment catchment = new Catchment("Catchment A")
          {
              ImperviousAreaSquareFeet = imperviousArea,
              AcceptableSeparationFromGroundwater = chkMeetsGroundwaterRequirements.Checked,
              CurveNumber = curveNumber,
              TimeOfConcentrationMinutes = timeOfConcentration,
              TestedInfiltrationRateInchesPerHour = nativeInfiltrationRate,
              InfiltrationTestType = infiltrationTestType
          };

          //Read facility parameters to local variables
          double bottomArea = Convert.ToDouble(txtBottomArea.Text);
          double bottomWidth = Convert.ToDouble(txtBottomWidth.Text);
          double sideSlope = Convert.ToDouble(txtSideSlope.Text);
          double storageDepth1 = Convert.ToDouble(txtStorageDepth1.Text);
          double storageDepth2 = Convert.ToDouble(txtStorageDepth2.Text);
          double storageDepth3 = Convert.ToDouble(txtStorageDepth3.Text);
          double growingMediumDepth = Convert.ToDouble(txtGrowingMediumDepth.Text);
          double freeboardDepth = Convert.ToDouble(txtFreeboardDepth.Text);
          double rockStorageDepth = Convert.ToDouble(txtRockStorageDepth.Text);
          double rockStorageVoidRatio = Convert.ToDouble(txtRockVoidRatio.Text);
          double rockStorageBottomArea = Convert.ToDouble(txtRockStorageBottomArea.Text);
          double surfaceAreaAtStorageDepth1 = Convert.ToDouble(txtSurfaceAreaAtDepth1.Text);
          double bottomPerimiterLength = Convert.ToDouble(txtBottomPerimeterLength.Text);
          double surfaceAreaAtStorageDepth2 = Convert.ToDouble(txtSurfaceAreaAtDepth2.Text);
          FacilityShape shape = GetFacilityShape(cmbFacilityShape.Text);

          FacilityConfiguration config;
          config = (FacilityConfiguration)cmbFacilityConfiguration.Text[0];
          FacilityType type = GetFacilityType(cmbFacilityType.Text);


          Facility facility;

          if (type == FacilityType.Basin || type == FacilityType.PlanterFlat)
          {
              facility = new Facility(type, config, catchment)
              {
                  BottomAreaSqFt = bottomArea,
                  BottomWidthFt = bottomWidth,
                  SideSlopeRatio = sideSlope,
                  StorageDepth1In = storageDepth1,
                  StorageDepth2In = storageDepth2,
                  StorageDepth3In = storageDepth3,
                  GrowingMediumDepthIn = growingMediumDepth,
                  FreeboardIn = freeboardDepth,
                  RockStorageDepthIn = rockStorageDepth,
                  RockVoidRatio = rockStorageVoidRatio,
                  RockStorageBottomAreaSqFt = rockStorageBottomArea,
                  SurfaceAreaAtStorageDepth1SqFt = surfaceAreaAtStorageDepth1,
                  BottomPerimeterLengthFt = bottomPerimiterLength,
                  SurfaceAreaAtStorageDepth2SqFt = surfaceAreaAtStorageDepth2,
                  Shape = shape
              };
          }
          else //Write sloped facility parameters from UI to facility object and verify results
          {
              //ShowSlopedFacilityWS();
              List<SlopedFacilitySegment> segments;
              if (_sfws != null)
              {
                  if (_sfws.Segments != null)
                      segments = _sfws.Segments;
                  else
                      segments = new List<SlopedFacilitySegment>();
              }
              else
                  segments = new List<SlopedFacilitySegment>();

              facility = new SlopedFacility(type, config, catchment, segments)
              {
                  BottomAreaSqFt = bottomArea,
                  BottomWidthFt = bottomWidth,
                  SideSlopeRatio = sideSlope,
                  StorageDepth1In = storageDepth1,
                  StorageDepth2In = storageDepth2,
                  StorageDepth3In = storageDepth3,
                  GrowingMediumDepthIn = growingMediumDepth,
                  FreeboardIn = freeboardDepth,
                  RockStorageDepthIn = rockStorageDepth,
                  RockVoidRatio = rockStorageVoidRatio,
                  RockStorageBottomAreaSqFt = rockStorageBottomArea,
                  SurfaceAreaAtStorageDepth1SqFt = surfaceAreaAtStorageDepth1,
                  Shape = shape
              };
          }

        int hierarchyNumber = Convert.ToInt32(hierarchy);
        HierarchyCategory hierarchyCategory = (HierarchyCategory)hierarchyNumber;

        string message;
        _validFacility = Facility.Validate(facility, hierarchyNumber, out message);

        ToggleUIParameters(facility);
        if (!_validFacility)
            DisableUI(message);

        return facility;
      }
      catch (Exception ex)
      {
        MessageBox.Show("Error with facility parameters: " + ex.Message);
        return null;
      }
    }

    private void ToggleUIParameters(Facility facility)
    {
      bool sloped = facility.Type == FacilityType.PlanterSloped ||
        facility.Type == FacilityType.Swale;

      txtRockStorageDepth.Enabled = facility.HasRockStorage;
      txtRockVoidRatio.Enabled = facility.HasRockStorage;

      txtRockStorageBottomArea.Enabled = facility.HasCustomRockStorageBottomArea;
      if (facility.HasCustomRockStorageBottomArea)
        txtRockStorageBottomArea.Text = facility.RockStorageBottomAreaSqFt.ToString();

      txtFreeboardDepth.Enabled = facility.SpecifyFreeboard;

      txtStorageDepth1.Enabled = !sloped; //Added per conversation with Richard Davies 10/2/14

      txtStorageDepth2.Enabled = facility.HasSecondaryOverflow;

      if (facility.HasRockStorage)
      {
          txtStorageDepth3.Enabled = !facility.HasRockInfluencedSurfaceStorage ||
            (sloped && facility.Configuration == FacilityConfiguration.C); //Added per conversation with Richard Davies 10/2/14
      }
      else
          txtStorageDepth3.Enabled = false;

      txtSideSlope.Enabled = facility.SpecifySideSlope;

      cmbFacilityShape.Enabled = facility.AllowShapeSelection;

      txtSurfaceAreaAtDepth1.Enabled = facility.Shape == FacilityShape.UserDefined;
      txtBottomPerimeterLength.Enabled = facility.Shape == FacilityShape.Amoeba;
      txtSurfaceAreaAtDepth2.Enabled = facility.Shape == FacilityShape.UserDefined && facility.Configuration == FacilityConfiguration.E;

      
      btnShowSFWS.Enabled = sloped;      
      txtRockStorageBottomArea.Enabled = !sloped && facility.HasCustomRockStorageBottomArea;
      txtBottomArea.Enabled = !sloped;
      
      txtBottomWidth.Enabled = !sloped && facility.Shape != FacilityShape.Amoeba && facility.Shape != FacilityShape.UserDefined;

      btnCalculate.Enabled = true;
      textBox1.ForeColor = SystemColors.WindowText;
      textBox1.Font = new Font(textBox1.Font, FontStyle.Regular);

      textBox1.Text = "Valid facility configuration";

    }

    private void DisableUI(string message)
    {
      btnCalculate.Enabled = false;
      textBox1.ForeColor = Color.Red;
      textBox1.Font = new Font(textBox1.Font, FontStyle.Bold);

      textBox1.Text = message;
    }

    private FacilityType GetFacilityType(string facilityType)
    {
      FacilityType type;
      switch (facilityType)
      {
        case "Swale":
          type = FacilityType.Swale;
          break;
        case "Basin":
          type = FacilityType.Basin;
          break;
        case "Planter (Flat)":
          type = FacilityType.PlanterFlat;
          break;
        case "Planter (Sloped)":
          type = FacilityType.PlanterSloped;
          break;
        default:
          throw new ArgumentException("Invalid facility type.");
      }
      return type;
    }

    private FacilityShape GetFacilityShape(string facilityShape)
    {
      FacilityShape shape;
      switch (facilityShape)
      {
        case "Amoeba":
          shape = FacilityShape.Amoeba;
          break;
        case "Rectangle":
          shape = FacilityShape.Rectangle;
          break;
        case "Sloped":
          shape = FacilityShape.Sloped;
          break;
        case "UserDefined":
          shape = FacilityShape.UserDefined;
          break;
        default:
          throw new ArgumentException("Invalid facility shape.");
      }

      return shape;
    }

    SlopedFacilityWorksheet _sfws;
    
    private void btnShowSFWS_Click(object sender, EventArgs e)
    {
      ShowSlopedFacilityWS();
      ValidateFacility(cmbHierachy.Text, cmbFacilityType.Text, cmbFacilityConfiguration.Text);
    }

    private void ShowSlopedFacilityWS()
    {
      double storageDepth2 = Convert.ToDouble(txtStorageDepth2.Text);
      double storageDepth3 = Convert.ToDouble(txtStorageDepth3.Text);
      double growingMediumDepth = Convert.ToDouble(txtGrowingMediumDepth.Text);
      double freeboardDepth = Convert.ToDouble(txtFreeboardDepth.Text);
      double rockStorageDepth = Convert.ToDouble(txtRockStorageDepth.Text);
      double rockStorageVoidRatio = Convert.ToDouble(txtRockVoidRatio.Text);
      FacilityShape shape = GetFacilityShape(cmbFacilityShape.Text);
      FacilityConfiguration config = (FacilityConfiguration)cmbFacilityConfiguration.Text[0];
      FacilityType type = GetFacilityType(cmbFacilityType.Text);

      List<SlopedFacilitySegment> segments = new List<SlopedFacilitySegment>();
 
      SlopedFacility facility = new SlopedFacility(type, config, new Catchment("test catchment"), segments)
        {
          StorageDepth2In = storageDepth2,
          StorageDepth3In = storageDepth3,
          GrowingMediumDepthIn = growingMediumDepth,
          FreeboardIn = freeboardDepth,
          RockStorageDepthIn = rockStorageDepth,
          RockVoidRatio = rockStorageVoidRatio,
          Shape = shape
        };

      if (_sfws == null || _sfws.Disposing)
        _sfws = new SlopedFacilityWorksheet(facility);
      else
      {
        facility.Segments = _sfws.Segments; // connect new facility object to existing segments.
        _sfws.Facility = facility; // change worksheet object to new facility object.
      }
      

      try
      {
        _sfws.ShowDialog();
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }

    private void txtNativeSoilInfiltrationRate_TextChanged(object sender, EventArgs e)
    {

    }

    private void label43_Click(object sender, EventArgs e)
    {

    }

    private void cmbHierachy_SelectedIndexChanged_1(object sender, EventArgs e)
    {
        String selected = this.cmbHierachy.SelectedItem as String;
        if (selected.Equals("3") || selected.Equals("4"))
        {
            this.lblPreCN.Enabled = true;
            this.txtPreCurveNumber.Enabled = true;

            if (selected.Equals("3"))
            {
                this.cmbDischargePoint.Visible = true;
                this.lblDischargePoint.Visible = true;
            }
            else
            {
                this.cmbDischargePoint.Visible = false;
                this.lblDischargePoint.Visible = false;
            }
        }
        else
        {
            this.lblPreCN.Enabled = false;
            this.txtPreCurveNumber.Enabled = false;
            this.cmbDischargePoint.Visible = false;
            this.lblDischargePoint.Visible = false;
        }
    }

    private void txtBottomArea_Leave(object sender, EventArgs e)
    {
        ValidateFacility(cmbHierachy.Text, cmbFacilityType.Text, cmbFacilityConfiguration.Text);
    }

    private void txtBottomWidth_Leave(object sender, EventArgs e)
    {
        ValidateFacility(cmbHierachy.Text, cmbFacilityType.Text, cmbFacilityConfiguration.Text);
    }

    private void txtStorageDepth1_Leave(object sender, EventArgs e)
    {
        ValidateFacility(cmbHierachy.Text, cmbFacilityType.Text, cmbFacilityConfiguration.Text);
    }

    private void txtGrowingMediumDepth_Leave(object sender, EventArgs e)
    {
        ValidateFacility(cmbHierachy.Text, cmbFacilityType.Text, cmbFacilityConfiguration.Text);
    }

    private void cmbFacilityShape_Leave(object sender, EventArgs e)
    {
        ValidateFacility(cmbHierachy.Text, cmbFacilityType.Text, cmbFacilityConfiguration.Text);
    }

    private void txtRockStorageBottomArea_Leave(object sender, EventArgs e)
    {
        ValidateFacility(cmbHierachy.Text, cmbFacilityType.Text, cmbFacilityConfiguration.Text);
    }

    private void txtRockStorageDepth_Leave(object sender, EventArgs e)
    {
        ValidateFacility(cmbHierachy.Text, cmbFacilityType.Text, cmbFacilityConfiguration.Text);
    }

    private void txtRockVoidRatio_Leave(object sender, EventArgs e)
    {
        ValidateFacility(cmbHierachy.Text, cmbFacilityType.Text, cmbFacilityConfiguration.Text);
    }

    private void txtFreeboardDepth_Leave(object sender, EventArgs e)
    {
        ValidateFacility(cmbHierachy.Text, cmbFacilityType.Text, cmbFacilityConfiguration.Text);
    }

    private void txtSideSlope_Leave(object sender, EventArgs e)
    {
        ValidateFacility(cmbHierachy.Text, cmbFacilityType.Text, cmbFacilityConfiguration.Text);
    }

    private void txtStorageDepth2_Leave(object sender, EventArgs e)
    {
        ValidateFacility(cmbHierachy.Text, cmbFacilityType.Text, cmbFacilityConfiguration.Text);
    }

    private void txtStorageDepth3_Leave(object sender, EventArgs e)
    {
        ValidateFacility(cmbHierachy.Text, cmbFacilityType.Text, cmbFacilityConfiguration.Text);
    }

    private void txtSurfaceAreaAtDepth1_Leave(object sender, EventArgs e)
    {
        ValidateFacility(cmbHierachy.Text, cmbFacilityType.Text, cmbFacilityConfiguration.Text);
    }

    private void txtBottomPerimeterLength_Leave(object sender, EventArgs e)
    {
        ValidateFacility(cmbHierachy.Text, cmbFacilityType.Text, cmbFacilityConfiguration.Text);
    }

    private void txtSurfaceAreaAtDepth2_Leave(object sender, EventArgs e)
    {
        ValidateFacility(cmbHierachy.Text, cmbFacilityType.Text, cmbFacilityConfiguration.Text);
    }

    private void txtNativeSoilInfiltrationRate_Leave(object sender, EventArgs e)
    {
        ValidateFacility(cmbHierachy.Text, cmbFacilityType.Text, cmbFacilityConfiguration.Text);
    }

    private void cmbInfiltrationProcedure_SelectedIndexChanged(object sender, EventArgs e)
    {
        ValidateFacility(cmbHierachy.Text, cmbFacilityType.Text, cmbFacilityConfiguration.Text);
    }

    private void label33_Click(object sender, EventArgs e)
    {

    }
  }
}
