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
  public class MachineEnergyConsumptionViewModel : EnergyConsumptionViewModel
  {
    public MachineEnergyConsumptionViewModel(CarbonFootprintPosition cfp, Func<string, TagColor> getColorForTag, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
      : base(cfp, getColorForTag, responsibleSubjects)
    {
      MachineEnergyConsumptionModel = (MachineEnergyConsumption) cfp;
    }

    public double ConsumptionPerHourForProcessing
    {
      get { return MachineEnergyConsumptionModel.ConsumptionPerHourForProcessing; }
      set
      {
        MachineEnergyConsumptionModel.ConsumptionPerHourForProcessing = value;
        NotifyOfPropertyChange(() => ConsumptionPerHourForProcessing);
      }
    }

    public double ConsumptionPerHourForStandby
    {
      get { return MachineEnergyConsumptionModel.ConsumptionPerHourForStandby; }
      set
      {
        MachineEnergyConsumptionModel.ConsumptionPerHourForStandby = value;
        NotifyOfPropertyChange(() => ConsumptionPerHourForStandby);
      }
    }

    public double HoursInProcessingState
    {
      get { return MachineEnergyConsumptionModel.HoursInProcessingState; }
      set
      {
        MachineEnergyConsumptionModel.HoursInProcessingState = value;
        NotifyOfPropertyChange(() => HoursInProcessingState);
      }
    }

    public double HoursInStandbyState
    {
      get { return MachineEnergyConsumptionModel.HoursInStandbyState; }
      set
      {
        MachineEnergyConsumptionModel.HoursInStandbyState = value;
        NotifyOfPropertyChange(() => HoursInStandbyState);
      }
    }

    private MachineEnergyConsumption MachineEnergyConsumptionModel { get; set; }
  }
}