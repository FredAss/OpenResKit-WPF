#region License

// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at
//  
// http://www.apache.org/licenses/LICENSE-2.0.html
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  
// Copyright (c) 2013, HTW Berlin

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using Ork.Danger.DomainModelService;
using Ork.Danger.Factories;
using Ork.Framework;

namespace Ork.Danger.ViewModels
{
  [Export(typeof (IWorkspace))]
  public class OverViewManagementViewModel : DocumentBase, IWorkspace
  {
    private readonly IAssessmentViewModelFactory m_AssessmentViewModelFactory;
    private readonly string[,] m_Matrix = new string[6, 6];

    private readonly string[] m_MatrixValues =
    {
      "3", "2", "1", "1", "1", "3", "2", "1", "1", "1", "3", "2", "2", "1", "1", "3", "2", "2", "2", "1", "3", "3", "3", "2", "2"
    };

    private readonly IDangerRepository m_Repository;
    private readonly IEnumerable m_Status;
    private ObservableCollection<Activity> m_Activities = new ObservableCollection<Activity>();
    private ObservableCollection<Assessment> m_Assessments;
    private ObservableCollection<Activity> m_CollectionOfActivities;
    private bool m_IsEnabled;
    private string m_RiskGroupResult;
    private string m_SearchText = "";
    private Activity m_SelectedActivity;
    private AssessmentViewModel m_SelectedAssessment;
    private Company m_SelectedCompany;
    private CompanyViewModel m_SelectedCompanyViewModel;
    private string m_SelectedOverview = "Tätigkeiten";
    private int m_SelectedStatus;
    private ObservableCollection<GFactor> m_gfactor;

    [ImportingConstructor]
    public OverViewManagementViewModel(IDangerRepository repository, IAssessmentViewModelFactory assessmentViewModelFactory)
    {
      m_AssessmentViewModelFactory = assessmentViewModelFactory;
      m_Repository = repository;
      m_Repository.SaveCompleted += (s, e) => Application.Current.Dispatcher.Invoke(CheckIsEnabled);


      //m_CompanyContext.CompanyChanged += (s, e) => UpdateSelectedCompanyViewModel();
      m_Status = Enum.GetValues(typeof (Status));
      CheckIsEnabled();
      m_Assessments = m_Repository.Assessments;
    }

    public ObservableCollection<Company> Companies
    {
      get { return m_Repository.Companies; }
    }

    public Company SelectedCompany
    {
      get
      {
        if (Companies.Any())
        {
          return m_SelectedCompany = Companies.FirstOrDefault();
        }
        return null;
      }
      set
      {
        m_SelectedCompany = value;

        NotifyOfPropertyChange(() => SelectedCompany);
        NotifyOfPropertyChange(() => GetAssessmentViewModeslsOfWorkpalces);
        NotifyOfPropertyChange(() => Threats);
      }
    }

    public IEnumerable Status
    {
      get { return m_Status; }
    }

    public int SelectedStatus
    {
      get { return m_SelectedStatus; }
      set
      {
        m_SelectedStatus = value;
        NotifyOfPropertyChange(() => GetAssessmentViewModeslsOfWorkpalces);
      }
    }

    //
    public ObservableCollection<Activity> CollectionOfActivities
    {
      get
      {
        if (m_SelectedOverview == "Tätigkeiten")
        {
          return m_CollectionOfActivities;
        }

        return null;
      }
      set
      {
        m_CollectionOfActivities = value;
        NotifyOfPropertyChange(() => CollectionOfActivities);
      }
    }

    public string RiskResult
    {
      get
      {
        CreateRiskGroupMatrix();

        return m_RiskGroupResult;
      }
    }

    public IEnumerable<ThreatViewModel> Threats
    {
      get
      {
        var threats = new List<ThreatViewModel>();
        CreateRiskGroupMatrix();
        if (m_SelectedActivity == null &&
            m_SelectedOverview == "Tätigkeiten" &&
            m_SelectedAssessment != null)
        {
          return m_SelectedAssessment.Threats.Where(t => t.Status != "0" && t.Status != "1")
                                     .Select(t => new ThreatViewModel(t, m_RiskGroupResult));
        }
        if (m_SelectedActivity != null &&
            m_SelectedOverview == "Tätigkeiten")
        {
          var threatlist = m_Repository.Threats.Where(t => t.Activity != null);

          foreach (var threat in threatlist.Where(a => a.Activity.Id.Equals(m_SelectedActivity.Id)))
          {
            m_RiskGroupResult = m_Matrix[threat.RiskPossibility, threat.RiskDimension];
            threats.Add(new ThreatViewModel(threat, m_RiskGroupResult));
          }
          return threats;
        }
        return threats;
      }
    }


    public List<string> Overviews
    {
      get
      {
        var overviewList = new List<string>
                           {
                             "Tätigkeiten",
                             "Maßnahmen"
                           };
        return overviewList;
      }
    }

    public string SelectedOverview
    {
      get { return m_SelectedOverview; }
      set
      {
        m_SelectedOverview = value;
        NotifyOfPropertyChange(() => CollectionOfActivities);
        NotifyOfPropertyChange(() => Threats);
      }
    }

    public ObservableCollection<Threat> ActivityInformationFromThreats
    {
      get { return new ObservableCollection<Threat>(m_Repository.Threats); }
    }

    public Activity GetActivity
    {
      get { return m_SelectedActivity; }
      set
      {
        m_SelectedActivity = value;
        NotifyOfPropertyChange(() => Threats);
        NotifyOfPropertyChange(() => GetActivity);
      }
    }

    #region Expander Listbox

    public IEnumerable<AssessmentViewModel> GetAssessmentViewModeslsOfWorkpalces
    {
      get { return GetAssessmentViewModelsFromFilteredWorkpalces(); }
    }

    public AssessmentViewModel SelectedAssessmentViewModel
    {
      get { return m_SelectedAssessment; }

      set
      {
        m_SelectedAssessment = value;
        if (m_SelectedAssessment == null)
        {
          CollectionOfActivities.Clear();
        }
        else
        {
          CollectionOfActivities = m_SelectedAssessment.WorkplaceOfModel.Activities;
          NotifyOfPropertyChange(() => Threats);
          //GetActivity = CollectionOfActivities.FirstOrDefault();
        }
      }
    }

    public IEnumerable<WorkplaceViewModel> FilteredWorkplacesByCompany
    {
      get
      {
        var filteredWorkplaces = GetFilteredWorkplaceByStatusAndCompany();
        return filteredWorkplaces;
      }
    }

    private IEnumerable<AssessmentViewModel> GetAssessmentViewModelsFromFilteredWorkpalces()
    {
      var assessmentViewModelsOfAllWorkpalces = new List<AssessmentViewModel>();

      if (FilteredWorkplacesByCompany != null)
      {
        foreach (var workplaceViewModel in FilteredWorkplacesByCompany)
        {
          assessmentViewModelsOfAllWorkpalces.AddRange(CreateAssessmentViewModelsFromWorkplace(workplaceViewModel));
        }
      }

      return assessmentViewModelsOfAllWorkpalces;
    }

    private IEnumerable<WorkplaceViewModel> GetFilteredWorkplaceByStatusAndCompany()
    {
      var workplaces = m_SelectedCompany.Workplaces;

      foreach (var workplace in workplaces)
      {
        foreach (var assessement in workplace.Assessments)
        {
          if (assessement.Status == m_SelectedStatus)
          {
            yield return new WorkplaceViewModel(workplace);
          }
        }
      }
    }

    private IEnumerable<AssessmentViewModel> CreateAssessmentViewModelsFromWorkplace(WorkplaceViewModel workplaceViewModel)
    {
      var assessmentViewModelsOfWorkpalce = new Collection<AssessmentViewModel>();

      foreach (var assessment in workplaceViewModel.Assessments.Where(a => a.Status == SelectedStatus))
      {
        assessmentViewModelsOfWorkpalce.Add(m_AssessmentViewModelFactory.CreateAssessmentViewModel(assessment, workplaceViewModel.Model));
      }

      return assessmentViewModelsOfWorkpalce;
    }

    #endregion

    public int Index
    {
      get { return 118; }
    }

    public string Title
    {
      get { return "Übersichten"; }
    }

    public bool IsEnabled
    {
      get { return m_IsEnabled; }
      set
      {
        if (m_Repository.SurveyTypes.Any() &&
            m_Repository.Companies.Any())
        {
          m_IsEnabled = true;
        }
        else
        {
          m_IsEnabled = false;
        }

        NotifyOfPropertyChange(() => IsEnabled);
      }
    }

    public void CheckIsEnabled()
    {
      if (m_Repository.SurveyTypes.Any() &&
          m_Repository.Companies.Any())
      {
        m_IsEnabled = true;
      }
      else
      {
        m_IsEnabled = false;
      }

      NotifyOfPropertyChange(() => IsEnabled);
    }

    public string CreateRiskGroupMatrix()
    {
      for (var i = 0; i < m_MatrixValues.Length; i++)
      {
        // Fill row
        for (var j = 0; j < 5; j++)
        {
          // Fill column of row
          for (var k = 0; k < 5; k++)
          {
            m_Matrix[j, k] = m_MatrixValues[i];
            i++;
          }
        }
      }
      //m_RiskGroupResult = "";
      return m_RiskGroupResult;
    }
  }
}