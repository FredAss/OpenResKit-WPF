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
using System.ComponentModel.Composition;
using Ork.Meter.DomainModelService;
using Ork.Meter.ViewModels;

namespace Ork.Meter.Factories
{
  [Export(typeof (IMeterViewModelFactory))]
  internal class MeterViewModelFactory : IMeterViewModelFactory
  {
    public MeterAddViewModel CreateAddViewModel(MapViewModel selectedMap)
    {
      return new MeterAddViewModel(CreateNewMeterModel(selectedMap));
    }

    public MeterEditViewModel CreateEditViewModel(DomainModelService.Meter model, Action removeMeterAction)
    {
      return new MeterEditViewModel(model, removeMeterAction);
    }

    public MeterViewModel CreateFromExisiting(DomainModelService.Meter meter)
    {
      return new MeterViewModel(meter);
    }

    private DomainModelService.Meter CreateNewMeterModel(MapViewModel selectedMap)
    {
      var position = new MapPosition
                     {
                       XPosition = 0,
                       YPosition = 0
                     };
      if (selectedMap != null)
      {
        position.Map = selectedMap.Model;
      }
      var meter = new DomainModelService.Meter
                  {
                    MapPosition = position
                  };
      return meter;
    }
  }
}