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
using System.Linq;
using Caliburn.Micro;
using Ork.CarbonFootprint.DomainModelService;

namespace Ork.CarbonFootprint.ViewModels
{
  public abstract class PositionViewModel : PropertyChangedBase
  {
    protected readonly Func<string, TagColor> m_GetColorForTag;

    public PositionViewModel(CarbonFootprintPosition cfp, Func<string, TagColor> getColorForTag, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      m_GetColorForTag = getColorForTag;
      ResponsibleSubjects = responsibleSubjects;
      Model = cfp;
    }

    public double CalculatedValue
    {
      get { return Model.Calculation; }
    }

    public string Calculation
    {
      get { return String.Format("{0:0.##}", Model.Calculation / 1000); }
    }

    public string Description
    {
      get { return Model.Description; }
      set
      {
        Model.Description = value;
        NotifyOfPropertyChange(() => Description);
      }
    }

    public DateTime Finish
    {
      get { return Model.Finish; }
      set
      {
        Model.Finish = value;
        NotifyOfPropertyChange(() => Finish);
      }
    }

    public CarbonFootprintPosition Model { get; private set; }

    public string Name
    {
      get { return Model.Name; }
      set
      {
        Model.Name = value;
        NotifyOfPropertyChange(() => Name);
      }
    }

    public DateTime Start
    {
      get { return Model.Start; }
      set
      {
        Model.Start = value;
        NotifyOfPropertyChange(() => Start);
      }
    }

    public string Tag
    {
      get { return Model.Tag; }
      set
      {
        Model.Tag = value;
        NotifyOfPropertyChange(() => Tag);
        NotifyOfPropertyChange(() => TagColor);
      }
    }

    public TagColor TagColor
    {
      get { return m_GetColorForTag(Tag); }
    }

    public ResponsibleSubjectViewModel ResponsibleSubject
    {
      get
      {
        var responsibleSubject = ResponsibleSubjects.SingleOrDefault(rs => rs.Model == Model.ResponsibleSubject);
        return responsibleSubject;
      }
      set
      {
        Model.ResponsibleSubject = value.Model;
        NotifyOfPropertyChange(() => ResponsibleSubject);
      }
    }

    public IEnumerable<ResponsibleSubjectViewModel> ResponsibleSubjects { get; private set; }
  }
}