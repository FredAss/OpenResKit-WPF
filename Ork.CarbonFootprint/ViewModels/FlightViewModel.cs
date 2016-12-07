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
using Ork.CarbonFootprint.DomainModelService;

namespace Ork.CarbonFootprint.ViewModels
{
  public class FlightViewModel : PositionViewModel
  {
    public enum FlightRange
    {
      Langstrecke,
      Mittelstrecke,
      Kurzstrecke,
      Umgebungsstrecke,
    }

    public FlightViewModel(CarbonFootprintPosition cfp, Func<string, TagColor> getColorForTag, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
      : base(cfp, getColorForTag, responsibleSubjects)
    {
      FlightModel = (Flight) cfp;
    }

    private Flight FlightModel { get; set; }

    public bool RadiativeForcing
    {
      get { return FlightModel.RadiativeForcing; }
      set
      {
        FlightModel.RadiativeForcing = value;
        NotifyOfPropertyChange(() => RadiativeForcing);
      }
    }

    public int Kilometrage
    {
      get { return FlightModel.Kilometrage; }
      set
      {
        FlightModel.Kilometrage = value;
        NotifyOfPropertyChange(() => Kilometrage);
      }
    }

    public FlightRange FlightType
    {
      get { return (FlightRange) FlightModel.m_FlightType; }
      set
      {
        FlightModel.m_FlightType = (int) value;
        NotifyOfPropertyChange(() => FlightType);
      }
    }
  }
}