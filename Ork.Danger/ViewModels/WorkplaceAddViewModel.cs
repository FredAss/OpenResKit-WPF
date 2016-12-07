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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Caliburn.Micro;
using Ork.Danger.DomainModelService;

namespace Ork.Danger.ViewModels
{
  public class WorkplaceAddViewModel : Screen
  {
    private readonly Company m_Company;
    private readonly IEnumerable<SurveyTypeViewModel> m_SurveyTypeViewModels;
    private string m_ActivityName;
    private DateTime m_AssessmentDate;
    private string m_PersonName = "";
    private SurveyTypeViewModel m_SelectedSurveyTypeViewModel;
    private Workplace m_Workplace;

    public WorkplaceAddViewModel(Workplace workplace, IEnumerable<SurveyTypeViewModel> surveytypeViewModels, Company company)
    {
      m_Workplace = workplace;
      m_Company = company;
      m_SurveyTypeViewModels = surveytypeViewModels;
      m_AssessmentDate = DateTime.Today;
      SetSurveyTypeToFirstIfNotNull();
    }

    public Workplace Workplace
    {
      get { return m_Workplace; }
      set
      {
        m_Workplace = value;
        NotifyOfPropertyChange(() => Workplace);
      }
    }

    public string Name
    {
      get { return m_Workplace.Name; }
      set
      {
        m_Workplace.Name = value;
        NotifyOfPropertyChange(() => Name);
        NotifyOfPropertyChange(() => AcceptIsEnabled);
      }
    }

    public string NameInCompany
    {
      get { return m_Workplace.NameCompany; }
      set
      {
        m_Workplace.NameCompany = value;
        NotifyOfPropertyChange(() => NameInCompany);
        NotifyOfPropertyChange(() => AcceptIsEnabled);
      }
    }

    public string Description
    {
      get { return m_Workplace.Description; }
      set
      {
        m_Workplace.Description = value;
        NotifyOfPropertyChange(() => Description);
      }
    }

    public string EvaluatingPerson
    {
      get { return m_PersonName; }
      set
      {
        m_PersonName = value;
        NotifyOfPropertyChange(() => EvaluatingPerson);
      }
    }

    public DateTime AssessmentDate
    {
      get { return m_AssessmentDate; }
      set
      {
        m_AssessmentDate = value;
        NotifyOfPropertyChange(() => AssessmentDate);
      }
    }

    public ObservableCollection<Activity> Activities
    {
      get { return m_Workplace.Activities; }
    }

    public string Activity
    {
      get { return m_ActivityName; }
      set
      {
        m_ActivityName = value;
        NotifyOfPropertyChange(() => Activity);
      }
    }

    public IEnumerable<AssessmentViewModel> AssessmentViewModels
    {
      get { return m_Workplace.Assessments.Select(a => new AssessmentViewModel(a, m_Workplace)); }
    }

    public SurveyTypeViewModel SelectedSurveyTypeViewModel
    {
      get
      {
        if (m_Workplace.SurveyType != null)
        {
          return SurveyTypeViewModels.Single(st => st.Model == m_Workplace.SurveyType);
        }
        else
        {
          return null;
        }
      }
      set
      {
        m_Workplace.SurveyType = value.Model;
        NotifyOfPropertyChange(() => Workplace.SurveyType);
        NotifyOfPropertyChange(() => SelectedSurveyTypeViewModel);
      }
    }


    public IEnumerable<SurveyTypeViewModel> SurveyTypeViewModels
    {
      get { return m_SurveyTypeViewModels; }
    }

    public bool CancelIsEnabled
    {
      get { return true; }
    }

    public bool AcceptIsEnabled
    {
      get { return !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(NameInCompany); }
    }

    public bool RemoveIsEnabled
    {
      get { return false; }
    }

    public bool ChangeIsEnabled
    {
      get { return false; }
    }

    public bool CopyIsEnabled
    {
      get { return false; }
    }

    public bool ExportIsEnabled
    {
      get { return false; }
    }

    private void SetSurveyTypeToFirstIfNotNull()
    {
      if (m_Workplace.SurveyType != null &&
          !SurveyTypeViewModels.Any())
      {
        SelectedSurveyTypeViewModel = m_SurveyTypeViewModels.First();
      }
    }


    public void AddActivityToWorkplace(string input)
    {
      if (string.IsNullOrEmpty(input))
      {
        return;
      }

      var activity = new Activity()
                     {
                       Name = input
                     };
      m_Workplace.Activities.Add(activity);

      Activity = string.Empty;
      NotifyOfPropertyChange(() => Activities);
      NotifyOfPropertyChange(() => Activity);
    }

    public void AddActivityToWorkplace(KeyEventArgs e, string input)
    {
      if (e.Key == Key.Return)
      {
        AddActivityToWorkplace(input);
      }
    }

    public void RemoveActivityFromWorkplace(object dataContext)
    {
      {
        m_Workplace.Activities.Remove((Activity) dataContext);
        NotifyOfPropertyChange(() => Activities);
      }
    }

    public void AddAssessmentToWorkplace()
    {
      var assessment = new Assessment
                       {
                         AssessmentDate = AssessmentDate,
                         Status = 0,
                         EvaluatingPerson = new Person()
                                            {
                                              Name = m_PersonName
                                            }
                       };

      m_Workplace.Assessments.Add(assessment);
      m_Company.Persons.Add(assessment.EvaluatingPerson);
      InsertThreatsOfWorklplace(assessment);

      NotifyOfPropertyChange(() => AssessmentViewModels);
    }

    private void InsertThreatsOfWorklplace(Assessment assessment)
    {
      var a = SelectedSurveyTypeViewModel;

      var b = new Collection<GFactor>();
      foreach (var gFactor in a.Categories.SelectMany(category => category.GFactors))
      {
        assessment.Threats.Add(new Threat()
                               {
                                 Status = "0",
                                 GFactor = gFactor,
                                 RiskDimension = 5,
                                 RiskPossibility = 5
                               });
      }
    }
  }
}