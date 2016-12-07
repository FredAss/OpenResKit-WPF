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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Services.Client;
using System.Linq;
using Caliburn.Micro;
using Ork.Danger.DomainModelService;

namespace Ork.Danger.ViewModels
{
  public class WorkplaceViewModel : Screen
  {
    private readonly Workplace m_Model;

    public WorkplaceViewModel(Workplace model)
    {
      m_Model = model;
      m_Model.PropertyChanged += ModelPropertyChanged;
    }

    public Workplace Model
    {
      get { return m_Model; }
    }

    public string Name
    {
      get { return m_Model.Name; }
      set
      {
        m_Model.Name = value;
        NotifyOfPropertyChange(() => Name);
      }
    }

    public string NameInCompany
    {
      get { return m_Model.NameCompany; }
      set
      {
        m_Model.NameCompany = value;
        NotifyOfPropertyChange(() => NameInCompany);
      }
    }

    public string Description
    {
      get { return m_Model.Description; }
      set
      {
        m_Model.Description = value;
        NotifyOfPropertyChange(() => Description);
      }
    }


    public ObservableCollection<Activity> Activities
    {
      get { return m_Model.Activities; }
    }

    public SurveyType SurveyType
    {
      get { return m_Model.SurveyType; }
      set
      {
        m_Model.SurveyType = value;
        NotifyOfPropertyChange(() => SurveyType);
      }
    }

    public DataServiceCollection<Assessment> Assessments
    {
      get { return m_Model.Assessments; }
      set
      {
        m_Model.Assessments = value;
        NotifyOfPropertyChange(() => Assessments);
        NotifyOfPropertyChange(() => LastYearInt);
      }
    }

    public int LastYearInt
    {
      get
      {
        if (!Assessments.Any())
        {
          return 3;
        }

        var descendedAssessments = Assessments.OrderByDescending(a => a.AssessmentDate);
        var latestAssessment = descendedAssessments.First();
        return GetDifferenceOfTodayAndAssessmentDate(latestAssessment);
      }
    }

    private int GetDifferenceOfTodayAndAssessmentDate(Assessment assessment)
    {
      var todayYear = DateTime.Today.Year;
      var assessmentYear = assessment.AssessmentDate.Year;

      if (todayYear - assessmentYear >= 4)
      {
        return 0;
      }
      if (todayYear - assessmentYear >= 2 &&
          todayYear - assessmentYear < 4)
      {
        return 1;
      }
      if (todayYear - assessmentYear >= 0 &&
          todayYear - assessmentYear < 2)
      {
        return 2;
      }
      return 3;
    }

    private void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      NotifyOfPropertyChange(e.PropertyName);
    }
  }
}