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
using System.Data.Services.Client;
using System.Linq;
using System.Windows.Input;
using Caliburn.Micro;
using Ork.Danger.DomainModelService;
using Ork.Danger.Factories;
using Ork.Framework;
using Action = Ork.Danger.DomainModelService.Action;

namespace Ork.Danger.ViewModels
{
  [Export(typeof (ActionManagementViewModel))]
  public class ActionManagementViewModel : Conductor<IScreen>.Collection.OneActive, IShell, IWorkspaceOptions
  {
    private readonly IAssessmentViewModelFactory m_AssessmentViewModelFactory;
    private readonly ICompanyContext m_CompanyContext;
    private readonly IDangerRepository m_Repository;
    private readonly IEnumerable m_Status;
    private readonly BindableCollection<ThreatViewModel> m_ThreatViewModels = new BindableCollection<ThreatViewModel>();
    private readonly ViewModelFactory m_VMFactory;
    private readonly ViewModelFactory vmFactory;
    private string m_ActionDescription;
    private ICollection<Action> m_ActionsFromThreat;
    private ProtectionGoal m_ProtectionGoal;
    private string m_ProtectionGoalText;
    private string m_RiskGroupResult;
    private string m_SearchText = "";
    private Action m_SelectedAction;
    private AssessmentViewModel m_SelectedAssessmentViewModel;
    private CompanyViewModel m_SelectedCompanyViewModel;
    private int m_SelectedStatus;
    private Threat m_SelectedThreat;

    private string m_ThreatDescription = "";
    private DataServiceCollection<Threat> m_Threats = new DataServiceCollection<Threat>();

    [ImportingConstructor]
    public ActionManagementViewModel(IDialogManager dialogManager, IAssessmentViewModelFactory assessmentViewModelFactory, IDangerRepository repository, ICompanyContext companyContext)
    {
      m_Repository = repository;
      m_CompanyContext = companyContext;
      m_VMFactory = new ViewModelFactory();
      m_Status = Enum.GetValues(typeof (Status));
      m_CompanyContext.CompanyChanged += (s, e) => UpdateSelectedCompanyViewModel();
      SelectedCompanyViewModel = m_CompanyContext.companyViewModel;
      m_AssessmentViewModelFactory = assessmentViewModelFactory;
      vmFactory = new ViewModelFactory();
      m_SelectedAction = new Action()
                         {
                           DueDate = DateTime.Now
                         };
      Dialogs = dialogManager;
    }

    public CompanyViewModel SelectedCompanyViewModel
    {
      get { return m_SelectedCompanyViewModel; }
      set
      {
        m_SelectedCompanyViewModel = value;
        NotifyOfPropertyChange(() => SelectedCompanyViewModel);
        NotifyOfPropertyChange(() => FilteredWorkplacesByCompany);
        NotifyOfPropertyChange(() => GetAssessmentViewModelsOfWorkplaces);
      }
    }


    public IEnumerable<AssessmentViewModel> GetAssessmentViewModelsOfWorkplaces
    {
      get { return GetAssessmentViewModelsFromFilteredWorkplaces(); }
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
        NotifyOfPropertyChange(() => GetAssessmentViewModelsOfWorkplaces);
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
          NotifyOfPropertyChange(() => GetAssessmentViewModelsOfWorkplaces);
        }
      }
    }

    public AssessmentViewModel SelectedAssessmentViewModel
    {
      get { return m_SelectedAssessmentViewModel; }
      set
      {
        m_SelectedAssessmentViewModel = value;

        NotifyOfPropertyChange(() => SelectedAssessmentViewModel);
        NotifyOfPropertyChange(() => Threats);
      }
    }

    public IEnumerable<Threat> Threats
    {
      get
      {
        if (m_SelectedAssessmentViewModel != null)
        {
          return m_SelectedAssessmentViewModel.Model.Threats.Where(t => t.Status != "0" && t.Status != "1");
        }
        else
        {
          return new Collection<Threat>();
        }
      }
    }

    public Threat SelectedThreat
    {
      get
      {
        if (Threats.Any())
        {
          return m_SelectedThreat ?? (m_SelectedThreat = m_SelectedAssessmentViewModel.Model.Threats.First());
        }
        return null;
      }
      set
      {
        m_SelectedThreat = value;

        if (m_SelectedThreat == null)
        {
          m_ThreatDescription = "";
        }
        else
        {
          m_ThreatDescription = m_SelectedThreat.Description;
        }
        NotifyOfPropertyChange(() => RiskGroup);
        NotifyOfPropertyChange(() => RiskColor);
        NotifyOfPropertyChange(() => ActionNeeded);
        NotifyOfPropertyChange(() => NoActionNeeded);
        NotifyOfPropertyChange(() => ProtectionGoals);
        NotifyOfPropertyChange(() => ActionsFromThreat);
        NotifyOfPropertyChange(() => CancelIsEnabled);
      }
    }


    public string RiskColor
    {
      get
      {
        if (RiskGroup == "1")
        {
          return "Red";
        }
        if (RiskGroup == "2")
        {
          return "Yellow";
        }
        if (RiskGroup == "3")
        {
          return "LawnGreen";
        }

        return "Transparent";
      }
    }

    public string RiskGroup
    {
      get
      {
        var m_Matrix = new string[6, 6];
        string[] m_MatrixValues =
        {
          "3", "2", "1", "1", "1", "3", "2", "1", "1", "1", "3", "2", "2", "1", "1", "3", "2", "2", "2", "1", "3", "3", "3", "2", "2"
        };

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
        if (SelectedThreat != null)
        {
          return m_Matrix[SelectedThreat.RiskPossibility, SelectedThreat.RiskDimension];
        }
        return "";
      }
    }

    public bool ActionNeeded
    {
      get
      {
        if (SelectedThreat != null)
        {
          return SelectedThreat.Actionneed;
        }
        return false;
      }
      set { SelectedThreat.Actionneed = value; }
    }

    public bool NoActionNeeded
    {
      get
      {
        if (SelectedThreat != null)
        {
          return !SelectedThreat.Actionneed;
        }
        return false;
      }
      set { SelectedThreat.Actionneed = !value; }
    }

    public string ProtectionGoal
    {
      get { return m_ProtectionGoalText; }
      set
      {
        m_ProtectionGoalText = value;
        NotifyOfPropertyChange(() => ProtectionGoal);
      }
    }

    public IEnumerable<ProtectionGoal> ProtectionGoals
    {
      get
      {
        if (m_SelectedThreat != null)
        {
          return m_SelectedThreat.ProtectionGoals;
        }
        return null;
      }
    }


    public string Action
    {
      get
      {
        if (m_SelectedAction != null)
        {
          return m_SelectedAction.Description;
        }
        return "";
      }
      set { m_SelectedAction.Description = value; }
    }

    public DateTime DueDate
    {
      get
      {
        if (m_SelectedAction != null)
        {
          return m_SelectedAction.DueDate;
        }
        return DateTime.Today;
      }
      set
      {
        m_SelectedAction.DueDate = value;
        NotifyOfPropertyChange(() => DueDate);
      }
    }


    public string Execution
    {
      get
      {
        if (m_SelectedAction != null)
        {
          return m_SelectedAction.Execution;
        }
        return "";
      }
      set { m_SelectedAction.Execution = value; }
    }

    public string Person
    {
      get
      {
        if (m_SelectedAction != null &&
            m_SelectedAction.Person != null)
        {
          return m_SelectedAction.Person.Name;
        }
        return "";
      }
      set
      {
        m_SelectedAction.Person = new Person()
                                  {
                                    Name = value
                                  };
        m_Repository.Companies.First(a => a == m_SelectedCompanyViewModel.Model)
                    .Persons.Add(m_SelectedAction.Person);
      }
    }

    public IEnumerable<ActionViewModel> ActionsFromThreat
    {
      get
      {
        return m_SelectedThreat != null
          ? m_SelectedThreat.Actions.Select(action => m_VMFactory.createActionViewModel(action))
          : null;
      }
    }


    public bool CopyIsEnabled
    {
      get { return false; }
    }

    public bool CancelIsEnabled
    {
      get
      {
        if (m_SelectedThreat != null)
        {
          return true;
        }
        return false;
      }
    }

    public bool RemoveIsEnabled
    {
      get { return false; }
    }

    public bool ExportIsEnabled
    {
      get { return false; }
    }

    public bool AcceptIsEnabled
    {
      get { return true; }
    }

    public IDialogManager Dialogs { get; private set; }

    public void CancelObject(object dataContext)
    {
      m_SelectedThreat = null;
      NotifyOfPropertyChange(() => SelectedThreat);
      NotifyOfPropertyChange(() => RiskColor);
      NotifyOfPropertyChange(() => RiskGroup);
      NotifyOfPropertyChange(() => ActionNeeded);
      NotifyOfPropertyChange(() => NoActionNeeded);
      NotifyOfPropertyChange(() => ProtectionGoals);
      NotifyOfPropertyChange(() => ActionsFromThreat);
      NotifyOfPropertyChange(() => Execution);
    }

    public void AcceptObject(object dataContext)
    {
      m_Repository.Save();
    }

    public void RemoveObject(object dataContext)
    {
      throw new NotImplementedException();
    }

    public void CopyObject(object dataContext)
    {
      throw new NotImplementedException();
    }


    public void UpdateSelectedCompanyViewModel()
    {
      SelectedCompanyViewModel = m_CompanyContext.companyViewModel;
    }

    private IEnumerable<AssessmentViewModel> GetAssessmentViewModelsFromFilteredWorkplaces()
    {
      var assessmentViewModelsOfAllWorkpalces = new List<AssessmentViewModel>();

      if (FilteredWorkplacesByCompany != null)
      {
        foreach (var workplaceViewModel in FilteredWorkplacesByCompany)
        {
          assessmentViewModelsOfAllWorkpalces.AddRange(CreateAssessmentViewModelsFromWorkplace(workplaceViewModel));
        }
        SelectedAssessmentViewModel = assessmentViewModelsOfAllWorkpalces.FirstOrDefault();
      }

      return assessmentViewModelsOfAllWorkpalces;
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

    public IEnumerable<WorkplaceViewModel> FilterWorkplaceViewModelsBySearchtext()
    {
      var searchText = m_SearchText.ToLower();

      return m_SelectedCompanyViewModel.Workplaces.Where(w => w.Name.ToLower()
                                                               .Contains(searchText) || w.NameCompany.ToLower()
                                                                                         .Contains(searchText) || w.Description.ToLower()
                                                                                                                   .Contains(searchText))
                                       .Select(w => m_AssessmentViewModelFactory.CreateWorkplaceViewModel(w));
    }

    public void AddProtectionGoalToThreat(string input)
    {
      if (string.IsNullOrEmpty(input))
      {
        return;
      }
      if (m_SelectedThreat == null)
      {
        Dialogs.ShowMessageBox("Bitte wählen Sie eine Gefährdung aus.", "Fehler");
      }
      else
      {
        var protectionGoal = new ProtectionGoal()
                             {
                               Description = input
                             };
        SelectedThreat.ProtectionGoals.Add(protectionGoal);

        ProtectionGoal = string.Empty;
        NotifyOfPropertyChange(() => ProtectionGoals);
        NotifyOfPropertyChange(() => ProtectionGoal);
      }
    }


    public void AddProtectionGoalToThreat(KeyEventArgs e, string input)
    {
      if (e.Key == Key.Return)
      {
        AddProtectionGoalToThreat(input);
      }
    }

    public void RemoveProtectionGoalFromThreat(object dataContext)
    {
      {
        m_SelectedThreat.ProtectionGoals.Remove((ProtectionGoal) dataContext);
        NotifyOfPropertyChange(() => ProtectionGoals);
      }
    }

    public void RemoveActionFromThreat(object dataContext)
    {
      {
        m_SelectedThreat.Actions.Remove(((ActionViewModel) dataContext).Model);
        NotifyOfPropertyChange(() => ActionsFromThreat);
      }
    }

    public void AddActionToThreat()
    {
      if (m_SelectedThreat == null)
      {
        Dialogs.ShowMessageBox("Bitte wählen Sie eine Gefährdung aus.", "Fehler");
      }
      else
      {
        m_SelectedThreat.Actions.Add(m_SelectedAction);
        m_SelectedAction = new Action()
                           {
                             DueDate = DateTime.Now
                           };
        NotifyOfPropertyChange(() => ActionsFromThreat);
        NotifyOfPropertyChange(() => Person);
        NotifyOfPropertyChange(() => Action);
        NotifyOfPropertyChange(() => Execution);
        NotifyOfPropertyChange(() => DueDate);
      }
    }
  }
}