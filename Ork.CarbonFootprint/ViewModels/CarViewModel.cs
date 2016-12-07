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
  public class CarViewModel : PositionViewModel
  {
    public enum Fuel
    {
      Benzin,
      Diesel,
    }

    public CarViewModel(CarbonFootprintPosition cfp, Func<string, TagColor> getColorForTag, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
      : base(cfp, getColorForTag, responsibleSubjects)
    {
      CarModel = (Car) cfp;
    }

    private Car CarModel { get; set; }

    public int Kilometrage
    {
      get { return CarModel.Kilometrage; }
      set
      {
        CarModel.Kilometrage = value;
        NotifyOfPropertyChange(() => Kilometrage);
      }
    }

    public double Consumption
    {
      get { return CarModel.Consumption; }
      set
      {
        CarModel.Consumption = value;
        NotifyOfPropertyChange(() => Consumption);
      }
    }

    public double CarbonProduction
    {
      get { return CarModel.CarbonProduction; }
      set
      {
        CarModel.CarbonProduction = value;
        NotifyOfPropertyChange(() => CarbonProduction);
      }
    }

    public Fuel FuelType
    {
      get { return (Fuel) CarModel.m_Fuel; }
      set
      {
        CarModel.m_Fuel = (int) value;
        NotifyOfPropertyChange(() => FuelType);
      }
    }
  }
}