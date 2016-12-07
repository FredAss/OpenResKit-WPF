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
using System.Data.Services.Client;
using System.Linq;
using Caliburn.Micro;
using Ork.Danger.DomainModelService;

namespace Ork.Danger.ViewModels
{
  public class AssessmentViewModel : PropertyChangedBase
  {
    private readonly Assessment m_Model;
    private readonly Workplace m_WorkplaceOfModel;

    public AssessmentViewModel(Assessment model, Workplace workplace)
    {
      m_Model = model;
      m_WorkplaceOfModel = workplace;
    }

    public Assessment Model
    {
      get { return m_Model; }
    }

    public Workplace WorkplaceOfModel
    {
      get { return m_WorkplaceOfModel; }
    }

    public DateTime AssessmentDate
    {
      get { return m_Model.AssessmentDate; }
    }

    public Person EvaluatingPerson
    {
      get { return m_Model.EvaluatingPerson; }
      set
      {
        m_Model.EvaluatingPerson = value;
        NotifyOfPropertyChange(() => EvaluatingPerson);
      }
    }


    public ObservableCollection<Category> Categories
    {
      get { return m_WorkplaceOfModel.SurveyType.Categories; }
      set { NotifyOfPropertyChange(() => GFactors); }
    }

    public IEnumerable<GFactor> GFactors
    {
      get { return m_WorkplaceOfModel.SurveyType.Categories.SelectMany(category => category.GFactors); }
      set { NotifyOfPropertyChange(() => Dangerpoints); }
    }

    public IEnumerable<Dangerpoint> Dangerpoints
    {
      get { return m_WorkplaceOfModel.SurveyType.Categories.SelectMany(category => category.GFactors.SelectMany(gfactor => gfactor.Dangerpoints)); }
    }

    public DataServiceCollection<Threat> Threats
    {
      get { return m_Model.Threats; }
      set { m_Model.Threats = value; }
    }


    public string AssessmentDateToString
    {
      get { return AssessmentDate.ToShortDateString(); }
    }

    public string NameOfWorkplace
    {
      get { return m_WorkplaceOfModel.Name; }
    }

    public int Status
    {
      get { return m_Model.Status; }
    }

    public string StatusName
    {
      get { return ((Status) Status).ToString(); }
    }
  }
}