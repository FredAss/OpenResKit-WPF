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
  public class EnergyConsumptionViewModel : PositionViewModel
  {
    public EnergyConsumptionViewModel(CarbonFootprintPosition cfp, Func<string, TagColor> getColorForTag, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
      : base(cfp, getColorForTag, responsibleSubjects)
    {
      EnergyConsumptionModel = (EnergyConsumption) cfp;
    }

    public double CarbonProduction
    {
      get { return EnergyConsumptionModel.CarbonProduction; }
      set
      {
        EnergyConsumptionModel.CarbonProduction = value;
        NotifyOfPropertyChange(() => CarbonProduction);
      }
    }

    public double Consumption
    {
      get { return EnergyConsumptionModel.Consumption; }
      set
      {
        EnergyConsumptionModel.Consumption = value;
        NotifyOfPropertyChange(() => Consumption);
      }
    }

    public EnergySource EnergySource
    {
      get { return (EnergySource) EnergyConsumptionModel.m_EnergySource; }
      set
      {
        EnergyConsumptionModel.m_EnergySource = (int) value;
        NotifyOfPropertyChange(() => EnergySource);
      }
    }

    private EnergyConsumption EnergyConsumptionModel { get; set; }
  }
}