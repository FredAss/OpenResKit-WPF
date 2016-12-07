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
using Caliburn.Micro;
using Ork.Danger.DomainModelService;
using Ork.Danger.Factories;

namespace Ork.Danger.ViewModels
{
  [Export(typeof (AssessmentManagementViewModel))]
  public class AssessmentManagementViewModel : Conductor<IScreen>.Collection.OneActive, IWorkspaceOptions
  {
    private readonly IAssessmentViewModelFactory m_AssessmentViewModelFactory;
    private readonly ICompanyContext m_CompanyContext;
    private readonly string[,] m_Matrix = new string[6, 6];

    private readonly string[] m_MatrixValues =
    {
      "3", "2", "1", "1", "1", "3", "2", "1", "1", "1", "3", "2", "2", "1", "1", "3", "2", "2", "2", "1", "3", "3", "3", "2", "2"
    };

    private readonly Threat m_ModelThreat;

    private readonly IDangerRepository m_Repository;
    private readonly IEnumerable m_Status;
    private int acceptEdit = 0;
    private string color = "";
    private int m_DangerDimItem = 5;
    private int m_PossibilityItem = 5;
    private string m_RiskGroupResult = "";
    private string m_SearchText = "";
    private Activity m_SelectedActivity;
    private AssessmentViewModel m_SelectedAssessmentViewModel;
    private Category m_SelectedCategory;
    private CompanyViewModel m_SelectedCompanyViewModel;
    private Dangerpoint m_SelectedDangerpoint;
    private GFactor m_SelectedGFactor;
    private int m_SelectedStatus;
    private Threat m_SelectedThreat;
    private string m_ThreatDescription = "";
    private string riskgroup = "";

    [ImportingConstructor]
    public AssessmentManagementViewModel(IDangerRepository repository, IAssessmentViewModelFactory assessmentViewModelFactory, ICompanyContext companyContext)
    {
      m_Repository = repository;
      m_CompanyContext = companyContext;
      SelectedCompanyViewModel = m_CompanyContext.companyViewModel;
      m_CompanyContext.CompanyChanged += (s, e) => UpdateSelectedCompanyViewModel();
      m_AssessmentViewModelFactory = assessmentViewModelFactory;
      m_Status = Enum.GetValues(typeof (Status));
    }

    public bool AcceptIsEnabled
    {
      get
      {
        if (m_ThreatDescription != "" &&
            acceptEdit == 1 &&
            m_PossibilityItem != 5 &&
            m_DangerDimItem != 5 &&
            m_SelectedGFactor != null)
        {
          return true;
        }

        return false;
      }
    }

    public bool CancelIsEnabled
    {
      get
      {
        return m_SelectedThreat != null ? true : false;
      }
    }

    public bool RemoveIsEnabled
    {
      get
      {
        return m_SelectedThreat != null ? true : false;
        
      }
    }

    public bool CopyIsEnabled
    {
      get
      {
        return m_SelectedThreat != null ? true : false;

      }
    }

    //ComboBox1
    public ObservableCollection<Activity> CollectionOfActivities
    {
      get
      {
        if (m_SelectedAssessmentViewModel != null)
        {
          return m_SelectedAssessmentViewModel.WorkplaceOfModel.Activities;
        }

        return new ObservableCollection<Activity>();
      }
    }

    //ComboBox2
    public ObservableCollection<Category> CollectionofCategories
    {
      get
      {
        if (m_SelectedAssessmentViewModel != null)
        {
          return m_SelectedAssessmentViewModel.Categories;
        }

        return new ObservableCollection<Category>();
      }
    }

    //ComboBox4
    public IEnumerable<Dangerpoint> CollectionofDangerpoints
    {
      get
      {
        if (m_SelectedAssessmentViewModel != null &&
            m_SelectedGFactor != null)
        {
          return m_SelectedGFactor.Dangerpoints;
        }

        return null;
      }
    }

    public IEnumerable<GFactor> CollectionofGFactor
    {
      get
      {
        if (m_SelectedAssessmentViewModel != null &&
            m_SelectedCategory != null)
        {
          return m_SelectedCategory.GFactors;
        }

        return null;
      }
    }



    public List<string> FillDimensionList
    {
      get
      {
        var dimensionList = new List<string>
                            {
                              "V - ohne Arbeitsunfall",
                              "IV - mit Arbeitsunfall",
                              "III - leichter bleibender Gesundheitsschaden",
                              "II - schwerer bleibender Gesundheitsschaden",
                              "I - Tod",
                              ""
                            };
        return dimensionList;
      }
    }

    public List<string> FillPossibilityList
    {
      get
      {
        var possibilityList = new List<string>
                              {
                                "A - häufig",
                                "B - gelegentlich",
                                "C - selten",
                                "D - unwahrscheinlich",
                                "E - praktisch unmöglich",
                                ""
                              };
        return possibilityList;
      }
    }

    public IEnumerable<WorkplaceViewModel> FilteredWorkplacesByCompany
    {
      get
      {
        if (m_SelectedCompanyViewModel == null)
        {
          return null;
        }

        var filteredWorkplaces = FilterWorkplaceViewModelsBySearchtext();
        return filteredWorkplaces;
      }
    }

    public IEnumerable<AssessmentViewModel> GetAssessmentViewModeslsOfWorkpalces
    {
      get { return GetAssessmentViewModelsFromFilteredWorkpalces(); }
    }

    public IEnumerable<Picture> Images
    {
      get
      {
        if (m_SelectedAssessmentViewModel != null &&
            m_SelectedThreat != null &&
            acceptEdit == 0)
        {
          return m_SelectedThreat.Pictures;
        }
        return null;
      }
    }

    public string RiskColor
    {
      get
      {
        if (m_RiskGroupResult == "1")
        {
          color = "Red";
        }
        if (m_RiskGroupResult == "2")
        {
          color = "Yellow";
        }
        if (m_RiskGroupResult == "3")
        {
          color = "LawnGreen";
        }
        if (m_RiskGroupResult != "1" &&
            m_RiskGroupResult != "2" &&
            m_RiskGroupResult != "3")
        {
          color = "Transparent";
        }
        return color;
      }
    }

    public string RiskGroupResult
    {
      get
      {
        CreateRiskGroupMatrix();
        m_RiskGroupResult = m_Matrix[m_PossibilityItem, m_DangerDimItem];
        return m_RiskGroupResult;
      }
    }

    public string SearchText
    {
      get { return m_SearchText; }
      set
      {
        if (value != m_SearchText)
        {
          m_SearchText = value;
          NotifyOfPropertyChange(() => FilteredWorkplacesByCompany);
          NotifyOfPropertyChange(() => GetAssessmentViewModeslsOfWorkpalces);
        }
      }
    }

    public Activity SelectedActivity
    {
      get
      {
        if (m_SelectedThreat != null)
        {
          return CollectionOfActivities.Single(a => a == m_SelectedThreat.Activity);
        }
        //return m_SelectedThreat.Activity;

        //if (m_SelectedActivity != null && m_SelectedThreat == null)
        //    return CollectionOfActivities.Single(a => a == m_SelectedThreat.Activity);

        return m_SelectedActivity;
      }
      set
      {
        m_SelectedActivity = value;
        acceptEdit = 1;
        NotifyOfPropertyChange(() => AcceptIsEnabled);
        NotifyOfPropertyChange(() => RemoveIsEnabled);
        NotifyOfPropertyChange(() => CopyIsEnabled);
        NotifyOfPropertyChange(() => CancelIsEnabled);
      }
    }

    public AssessmentViewModel SelectedAssessmentViewModel
    {
      get { return m_SelectedAssessmentViewModel; }
      set
      {
        m_SelectedAssessmentViewModel = value;
        NotifyOfPropertyChange(() => CollectionOfActivities);
        NotifyOfPropertyChange(() => SelectedAssessmentViewModel);
        NotifyOfPropertyChange(() => CollectionofCategories);
        NotifyOfPropertyChange(() => CollectionofDangerpoints);
        NotifyOfPropertyChange(() => CollectionofGFactor);
        NotifyOfPropertyChange(() => SelectedPossibility);
        NotifyOfPropertyChange(() => SelectedDimension);
        NotifyOfPropertyChange(() => Threatlist);
      }
    }

    public Category SelectedCategory
    {
      get
      {
        if (m_SelectedThreat != null)
        {
          /*m_SelectedCategory = (CollectionofCategories.SelectMany(category => category.GFactors,
                        (category, gfactor) => new { category, gfactor })
                        .Where(t => t.gfactor == m_SelectedThreat.GFactor)
                        .Select(t => t.category)).First();*/

          NotifyOfPropertyChange(() => CollectionofGFactor);

          SelectedGFactor = m_SelectedThreat.GFactor;

          return m_SelectedCategory;
        }
        if (m_SelectedCategory != null &&
            m_SelectedThreat == null)
        {
          return CollectionofCategories.Single(a => a == m_SelectedCategory);
        }

        return m_SelectedCategory;
      }
      set
      {
        acceptEdit = 1;
        m_SelectedCategory = value;
        NotifyOfPropertyChange(() => CollectionofGFactor);
        NotifyOfPropertyChange(() => SelectedAssessmentViewModel);
        NotifyOfPropertyChange(() => AcceptIsEnabled);
      }
    }

    public CompanyViewModel SelectedCompanyViewModel
    {
      get { return m_SelectedCompanyViewModel; }
      set
      {
        m_SelectedCompanyViewModel = value;
        NotifyOfPropertyChange(() => SelectedCompanyViewModel);
        NotifyOfPropertyChange(() => FilteredWorkplacesByCompany);
        NotifyOfPropertyChange(() => GetAssessmentViewModeslsOfWorkpalces);
      }
    }

    public Dangerpoint SelectedDangerpoint
    {
      get
      {
        if (m_SelectedDangerpoint != null &&
            m_SelectedThreat != null &&
            acceptEdit == 0)
        {
          return m_SelectedDangerpoint;
        }
        //return m_SelectedThreat.Dangerpoint;
        if (m_SelectedDangerpoint != null &&
            m_SelectedThreat != null &&
            acceptEdit == 1)
        {
          return CollectionofDangerpoints.Single(a => a == m_SelectedDangerpoint);
        }
        return null;
      }
      set
      {
        m_SelectedDangerpoint = value;
        acceptEdit = 1;
        NotifyOfPropertyChange(() => AcceptIsEnabled);
        NotifyOfPropertyChange(() => RemoveIsEnabled);
        NotifyOfPropertyChange(() => CopyIsEnabled);
        NotifyOfPropertyChange(() => CancelIsEnabled);
      }
    }

    public int SelectedDimension
    {
      get
      {
        if (m_SelectedThreat != null &&
            acceptEdit == 0)
        {
          return m_SelectedThreat.RiskDimension;
        }

        if (m_SelectedThreat == null &&
            m_DangerDimItem != 5 &&
            acceptEdit == 1)
        {
          return m_DangerDimItem;
        }

        if (m_SelectedThreat != null &&
            m_DangerDimItem != 5 &&
            acceptEdit == 1)
        {
          return m_DangerDimItem;
        }
        return 5;
      }
      set
      {
        m_DangerDimItem = value;
        acceptEdit = 1;

        NotifyOfPropertyChange(() => RiskGroupResult);
        NotifyOfPropertyChange(() => RiskColor);
        NotifyOfPropertyChange(() => AcceptIsEnabled);
        NotifyOfPropertyChange(() => RemoveIsEnabled);
        NotifyOfPropertyChange(() => CopyIsEnabled);
        NotifyOfPropertyChange(() => CancelIsEnabled);
      }
    }

    public GFactor SelectedGFactor
    {
      get
      {
        //if (m_SelectedThreat != null)
        //return CollectionofGFactor.Single(a => a == m_SelectedThreat.GFactor);
        //return m_SelectedThreat.GFactor;

        if (m_SelectedGFactor != null &&
            m_SelectedThreat == null)
        {
          return CollectionofGFactor.Single(a => a == m_SelectedGFactor);
        }
        return m_SelectedGFactor;
      }
      set
      {
        m_SelectedGFactor = value;
        NotifyOfPropertyChange(() => CollectionofDangerpoints);
        acceptEdit = 1;
        NotifyOfPropertyChange(() => AcceptIsEnabled);
        NotifyOfPropertyChange(() => RemoveIsEnabled);
        NotifyOfPropertyChange(() => CopyIsEnabled);
        NotifyOfPropertyChange(() => CancelIsEnabled);
      }
    }

    public int SelectedPossibility
    {
      get
      {
        if (m_PossibilityItem != null &&
            m_SelectedThreat != null &&
            acceptEdit == 0)
        {
          return m_SelectedThreat.RiskPossibility;
        }

        if (m_PossibilityItem != null &&
            m_SelectedThreat == null &&
            acceptEdit == 1)
        {
          return m_PossibilityItem;
        }

        if (m_PossibilityItem != null &&
            m_SelectedThreat != null &&
            acceptEdit == 1)
        {
          return m_PossibilityItem;
        }
        return 5;
      }
      set
      {
        m_PossibilityItem = value;
        acceptEdit = 1;
        NotifyOfPropertyChange(() => RiskGroupResult);
        NotifyOfPropertyChange(() => RiskColor);
        NotifyOfPropertyChange(() => AcceptIsEnabled);
        NotifyOfPropertyChange(() => RemoveIsEnabled);
        NotifyOfPropertyChange(() => CopyIsEnabled);
        NotifyOfPropertyChange(() => CancelIsEnabled);
      }
    }

    public int SelectedStatus
    {
      get { return m_SelectedStatus; }
      set
      {
        m_SelectedStatus = value;
        NotifyOfPropertyChange(() => GetAssessmentViewModeslsOfWorkpalces);
        NotifyOfPropertyChange(() => SelectedActivity);
        NotifyOfPropertyChange(() => ThreatDescription);
        NotifyOfPropertyChange(() => Images);
        NotifyOfPropertyChange(() => SelectedCategory);
        NotifyOfPropertyChange(() => SelectedDangerpoint);
        NotifyOfPropertyChange(() => SelectedDimension);
        NotifyOfPropertyChange(() => SelectedGFactor);
        NotifyOfPropertyChange(() => SelectedPossibility);
        NotifyOfPropertyChange(() => Threatlist);
        CreateRiskGroupMatrix();
      }
    }

    public Threat SelectedThreat
    {
      get { return m_SelectedThreat; }
      set
      {
        acceptEdit = 0;
        m_SelectedThreat = value;
        if (m_SelectedThreat == null)
        {
          m_ThreatDescription = "";
          m_SelectedActivity = null;
          m_SelectedCategory = null;
        }
        else
        {
          m_ThreatDescription = m_SelectedThreat.Description;
          m_SelectedActivity = m_SelectedThreat.Activity;
          m_SelectedCategory = (CollectionofCategories.SelectMany(category => category.GFactors, (category, gfactor) => new
                                                                                                                        {
                                                                                                                          category,
                                                                                                                          gfactor
                                                                                                                        })
                                                      .Where(t => t.gfactor == m_SelectedThreat.GFactor)
                                                      .Select(t => t.category)).First();
          m_SelectedDangerpoint = m_SelectedThreat.Dangerpoint;

          m_DangerDimItem = m_SelectedThreat.RiskDimension;
          m_PossibilityItem = m_SelectedThreat.RiskPossibility;
        }


        NotifyOfPropertyChange(() => AcceptIsEnabled);
        NotifyOfPropertyChange(() => RemoveIsEnabled);
        NotifyOfPropertyChange(() => CopyIsEnabled);
        NotifyOfPropertyChange(() => CancelIsEnabled);
        NotifyOfPropertyChange(() => Images);
        NotifyOfPropertyChange(() => SelectedActivity);
        NotifyOfPropertyChange(() => SelectedCategory);
        //m_SelectedDangerpoint = m_SelectedThreat.Dangerpoint;
        NotifyOfPropertyChange(() => SelectedDangerpoint);
        NotifyOfPropertyChange(() => SelectedGFactor);
        NotifyOfPropertyChange(() => SelectedStatus);
        NotifyOfPropertyChange(() => ThreatDescription);
        NotifyOfPropertyChange(() => SelectedDimension);
        NotifyOfPropertyChange(() => SelectedPossibility);


        NotifyOfPropertyChange(() => RiskGroupResult);
        NotifyOfPropertyChange(() => RiskColor);
      }
    }

    public IEnumerable Status
    {
      get { return m_Status; }
    }

    public string ThreatDescription
    {
      get
      {
        if (m_SelectedThreat == null &&
            m_ThreatDescription == "")
        {
          return null;
        }
        if (m_SelectedThreat != null &&
            m_ThreatDescription == m_SelectedThreat.Description)
        {
          return m_SelectedThreat.Description;
        }
        if (m_SelectedThreat != null &&
            m_ThreatDescription != m_SelectedThreat.Description)
        {
          return m_ThreatDescription;
        }

        return m_ThreatDescription;
      }
      set
      {
        m_ThreatDescription = value;

        NotifyOfPropertyChange(() => AcceptIsEnabled);
        NotifyOfPropertyChange(() => RemoveIsEnabled);
        NotifyOfPropertyChange(() => CopyIsEnabled);
        NotifyOfPropertyChange(() => CancelIsEnabled);
      }
    }

    public IEnumerable<Threat> Threatlist
    {
      get
      {
        if (m_SelectedAssessmentViewModel != null)
        {
          return m_SelectedAssessmentViewModel.Threats.Where(t => t.Status != "0" && t.Status != "1");
        }
        return null;
      }
    }

    public void AcceptObject(object dataContext)
    {
      if (m_SelectedThreat == null)
      {
        var a = SelectedAssessmentViewModel.Threats.Single(t => t.GFactor == m_SelectedGFactor);
        a.Activity = m_SelectedActivity;
        a.Description = m_ThreatDescription;
        a.GFactor = m_SelectedGFactor;
        a.RiskPossibility = SelectedPossibility;
        a.RiskDimension = SelectedDimension;
        a.Status = "3";
        a.Dangerpoint = m_SelectedDangerpoint;


        //var a = new Threat();
        //a.Pictures = m_SelectedThreat.Pictures;
        //m_SelectedAssessmentViewModel.Threats.Add(a);
      }
      else
      {
        m_SelectedThreat.Activity = m_SelectedActivity;
        m_SelectedThreat.Description = m_ThreatDescription;
        m_SelectedThreat.GFactor = m_SelectedGFactor;
        m_SelectedThreat.Dangerpoint = SelectedDangerpoint;
        m_SelectedThreat.RiskPossibility = SelectedPossibility;
        m_SelectedThreat.RiskDimension = SelectedDimension;
        m_SelectedThreat.Status = "3";
      }
      m_Repository.Save();
      acceptEdit = 0;
      EmptyFields();
      m_SelectedThreat = null;

      NotifyOfPropertyChange(() => SelectedActivity);
      NotifyOfPropertyChange(() => CollectionofCategories);
      NotifyOfPropertyChange(() => SelectedCategory);
      NotifyOfPropertyChange(() => CollectionofGFactor);
      NotifyOfPropertyChange(() => SelectedGFactor);
      NotifyOfPropertyChange(() => CollectionofDangerpoints);
      NotifyOfPropertyChange(() => SelectedDangerpoint);
      NotifyOfPropertyChange(() => SelectedDimension);
      NotifyOfPropertyChange(() => SelectedPossibility);
      NotifyOfPropertyChange(() => ThreatDescription);
      NotifyOfPropertyChange(() => Threatlist);
      NotifyOfPropertyChange(() => SelectedThreat);
      NotifyOfPropertyChange(() => AcceptIsEnabled);
    }

    public void CancelObject(object dataContext)
    {
      acceptEdit = 0;
      m_ThreatDescription = "";
      EmptyFields();
      m_SelectedThreat = null;
      NotifyOfPropertyChange(() => SelectedActivity);
      NotifyOfPropertyChange(() => CollectionofCategories);
      NotifyOfPropertyChange(() => SelectedCategory);
      NotifyOfPropertyChange(() => CollectionofGFactor);
      NotifyOfPropertyChange(() => SelectedGFactor);
      NotifyOfPropertyChange(() => CollectionofDangerpoints);
      NotifyOfPropertyChange(() => SelectedDangerpoint);
      NotifyOfPropertyChange(() => SelectedDimension);
      NotifyOfPropertyChange(() => SelectedPossibility);
      NotifyOfPropertyChange(() => ThreatDescription);
      NotifyOfPropertyChange(() => Threatlist);
      NotifyOfPropertyChange(() => SelectedThreat);
    }

    public void CopyObject(object dataContext)
    {
      var threat = new Threat();
      threat.Description = "Kopie von " + m_SelectedThreat.Description;
      threat.GFactor = m_SelectedThreat.GFactor;
      threat.Activity = m_SelectedThreat.Activity;

      threat.Status = "3";

      m_SelectedAssessmentViewModel.Threats.Add(threat);
      m_Repository.Threats.Add(threat);
      m_Repository.Save();

      NotifyOfPropertyChange(() => Threatlist);
    }


    public void RemoveObject(object dataContext)
    {
      m_Repository.Threats.Remove(m_SelectedThreat);
      m_SelectedAssessmentViewModel.Threats.Remove(m_SelectedThreat);
      m_Repository.Save();
      acceptEdit = 0;
      EmptyFields();
      m_SelectedThreat = null;

      NotifyOfPropertyChange(() => SelectedActivity);
      NotifyOfPropertyChange(() => CollectionofCategories);
      NotifyOfPropertyChange(() => SelectedCategory);
      NotifyOfPropertyChange(() => CollectionofGFactor);
      NotifyOfPropertyChange(() => SelectedGFactor);
      NotifyOfPropertyChange(() => CollectionofDangerpoints);
      NotifyOfPropertyChange(() => SelectedDangerpoint);
      NotifyOfPropertyChange(() => SelectedDimension);
      NotifyOfPropertyChange(() => SelectedPossibility);
      NotifyOfPropertyChange(() => ThreatDescription);
      NotifyOfPropertyChange(() => Threatlist);
      NotifyOfPropertyChange(() => SelectedThreat);
      NotifyOfPropertyChange(() => AcceptIsEnabled);
    }

    private void EmptyFields()
    {
      m_SelectedActivity = null;
      m_SelectedGFactor = null;
      m_SelectedCategory = null;
      m_SelectedDangerpoint = null;
      m_ThreatDescription = "";
      SelectedPossibility = 5;
      SelectedDimension = 5;
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

      m_RiskGroupResult = "";
      return m_RiskGroupResult;
    }

    public IEnumerable<WorkplaceViewModel> FilterWorkplaceViewModelsBySearchtext()
    {
      var searchText = m_SearchText.ToLower();

      return m_SelectedCompanyViewModel.Workplaces.Where(w => w.Name.ToLower()
                                                               .Contains(searchText) || w.NameCompany.ToLower()
                                                                                         .Contains(searchText) || w.Description.ToLower()
                                                                                                                   .Contains(searchText))
                                       .Select(w => m_AssessmentViewModelFactory.CreateWorkplaceViewModel(w));
    }

    public void UpdateSelectedCompanyViewModel()
    {
      SelectedCompanyViewModel = m_CompanyContext.companyViewModel;
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
  }
}