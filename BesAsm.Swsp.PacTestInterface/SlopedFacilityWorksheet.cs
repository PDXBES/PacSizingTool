using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;
using BesAsm.Swsp.PacSizingTool;


namespace BesAsm.Swsp.PacSizingTool
{
  public partial class SlopedFacilityWorksheet : Form
  {
    private SlopedFacility _facility;
    private List<SlopedFacilitySegment> _segments;

    public SlopedFacilityWorksheet(SlopedFacility facility)
    {
      InitializeComponent();

      _facility = facility;

      this.slopedFacilityBindingSource.DataSource = _facility;
      this.segmentsBindingSource.DataSource = _facility.Segments;
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
      SaveFileDialog d = new SaveFileDialog();
      d.DefaultExt = "xml";
      d.Filter = "XML (*.xml)|*.xml";

      StreamWriter writer = null;

      if (d.ShowDialog() != DialogResult.OK)
        return;
      try
      {
        string s = d.FileName;
        XmlSerializer x = new XmlSerializer(typeof(List<SlopedFacilitySegment>));
        writer = new StreamWriter(s);
        x.Serialize(writer, _facility.Segments);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
      finally
      {
        if (writer != null)
          writer.Close();
      }

    }

    private void btnLoad_Click(object sender, EventArgs e)
    {
      OpenFileDialog d = new OpenFileDialog();
      d.DefaultExt = "xml";
      d.Filter = "XML (*.xml)|*.xml";
      if (d.ShowDialog() != DialogResult.OK)
        return;

      try
      {
        string s = d.FileName;
        XmlSerializer x = new XmlSerializer(typeof(List<SlopedFacilitySegment>));
        StreamReader reader = new StreamReader(s);

        if (_facility.Segments == null)
          _facility.Segments = new List<SlopedFacilitySegment>();

        _facility.Segments.Clear();
        slopedFacilityBindingSource.ResetBindings(false); 
        segmentsBindingSource.ResetBindings(false);
        _facility.Segments.AddRange(
          (List<SlopedFacilitySegment>)x.Deserialize(reader));

        _segments = _facility.Segments;
        segmentsBindingSource.DataSource = _segments; // reset the pointer

        slopedFacilityBindingSource.ResetBindings(false); // re-read all items in list and refresh displayed values
        segmentsBindingSource.ResetBindings(false);
        reader.Close();
        this.Refresh();
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }

    public List<SlopedFacilitySegment> Segments
    {
      get { return _segments; }
    }

    public SlopedFacility Facility
    {
      get { return _facility; }
      set 
      { 
          _facility = value;
          this.slopedFacilityBindingSource.DataSource = _facility; // reset binding source
          this.segmentsBindingSource.DataSource = _facility.Segments; // reset binding source
      }
    }

    private void btnOk_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
      _segments = new List<SlopedFacilitySegment>(_facility.Segments.AsReadOnly());
      this.Close();
    }

    private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
    {
        reviseCalculatedFields();
    }

    private void dataGridView1_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
    {
        reviseCalculatedFields();
    }

    private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
        reviseCalculatedFields();
    }

    private void reviseCalculatedFields()
    {
        if (_facility != null)
        {
            txtSurfaceVolume.Text = (this.slopedFacilityBindingSource.Current as SlopedFacility).SurfaceCapacityAtDepth1CuFt.ToString();
            txtInfiltrationArea.Text = (this.slopedFacilityBindingSource.Current as SlopedFacility).InfiltAreaAt75PercentDepth1SqFt.ToString();
            txtRockStorageArea.Text = (this.slopedFacilityBindingSource.Current as SlopedFacility).RockStorageBottomAreaSqFt.ToString();
            txtRockStorageVolume.Text = (this.slopedFacilityBindingSource.Current as SlopedFacility).RockStorageCapacityCuFt.ToString();
        }
        this.Refresh();
    }
  }
}
