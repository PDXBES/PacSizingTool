using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BesAsm.Swsp.PacSizingTool
{
  internal class Project
  {
    private List<Catchment> _catchments = new List<Catchment>();
    private List<RainfallEvent> _rainfallEvents = new List<RainfallEvent>();

    private string _projectName;
    private string _projectNumber;

    private DateTime _projectDate;
    private string _designer;

    private string _projectSummary;

    public Project()
    {
    }

    internal List<Catchment> Catchments
    {
      get { return _catchments; }
    }

    internal List<RainfallEvent> RainfallEvents
    {
      get { return _rainfallEvents; }
    }

    internal string ProjectName
    {
      get { return _projectName; }
      set { _projectName = value; }
    }

    internal string ProjectNumber
    {
      get { return _projectNumber; }
      set { _projectNumber = value; }
    }

    internal DateTime ProjectDate
    {
      get { return _projectDate; }
      set { _projectDate = value; }
    }

    internal string Designer
    {
      get { return _designer; }
      set { _designer = value; }
    }

    internal string ProjectSummary
    {
      get { return _projectSummary; }
      set { _projectSummary = value; }
    }



  }
}
