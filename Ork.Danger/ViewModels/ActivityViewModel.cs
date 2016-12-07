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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using Ork.Danger.DomainModelService;

namespace Ork.Danger.ViewModels
{
  public class ActivityViewModel : PropertyChangedBase
  {
    private readonly Activity m_ModelActivity;
    private readonly Workplace m_WorkplaceOfModel;
    private Threat m_Threat;

    public ActivityViewModel(Activity activity, Workplace workplace, Threat threat)
    {
      m_ModelActivity = activity;
      m_WorkplaceOfModel = workplace;
      m_Threat = threat;
    }

    public IEnumerable<Assessment> Assessments
    {
      get
      {
        var test2 = (m_WorkplaceOfModel.Assessments.SelectMany(ass => ass.Threats, (assessment, threat) => new
                                                                                                           {
                                                                                                             assessment,
                                                                                                             threat
                                                                                                           })
                                       .Where(th => th.threat.Activity == m_ModelActivity)
                                       .Select(th => th.assessment));

        return test2;
      }
    }

    public IEnumerable<GFactor> GFactors
    {
      get { return m_WorkplaceOfModel.SurveyType.Categories.SelectMany(category => category.GFactors); }
    }

    public ObservableCollection<Category> Categories
    {
      get { return m_WorkplaceOfModel.SurveyType.Categories; }
      set { NotifyOfPropertyChange(() => GFactors); }
    }
  }
}