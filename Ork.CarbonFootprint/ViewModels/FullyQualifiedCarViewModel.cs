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
using Ork.CarbonFootprint.DomainModelService;
using Ork.CarbonFootprint.PositionDataProvider;

namespace Ork.CarbonFootprint.ViewModels
{
  public class FullyQualifiedCarViewModel : CarViewModel
  {
    private readonly PositionDataAccessor m_PositionDataAccessor;
    private IEnumerable<FullyQualifiedCarData> m_FullyQualifiedCars;
    private string m_SelectedDescription;

    private string m_SelectedManufacturer;

    private string m_SelectedModel;

    private string m_SelectedYear;

    public FullyQualifiedCarViewModel(CarbonFootprintPosition cfp, PositionDataAccessor positionDataAccessor, Func<string, TagColor> getColorForTag,
      IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
      : base(cfp, getColorForTag, responsibleSubjects)
    {
      FullyQualifiedCarModel = (FullyQualifiedCar) cfp;

      m_PositionDataAccessor = positionDataAccessor;
      m_PositionDataAccessor.Loaded += CarDataLoaded;
      if (m_PositionDataAccessor.CarData != null)
      {
        SetEntryData();
      }
    }

    public IEnumerable<string> DescriptionSource
    {
      get
      {
        return m_FullyQualifiedCars.Where(m => m.Manufacturer == m_SelectedManufacturer && m.Model == m_SelectedModel)
                                   .Select(d => d.Description)
                                   .OrderBy(n => n)
                                   .Distinct();
      }
    }

    public IEnumerable<string> ManufacturerSource
    {
      get
      {
        return m_FullyQualifiedCars.Select(m => m.Manufacturer)
                                   .OrderBy(m => m)
                                   .Distinct()
                                   .ToArray();
      }
    }

    public IEnumerable<string> ModelSource
    {
      get
      {
        return m_FullyQualifiedCars.Where(m => m.Manufacturer == m_SelectedManufacturer)
                                   .Select(mod => mod.Model)
                                   .OrderBy(n => n)
                                   .Distinct();
      }
    }

    public string SelectedDescription
    {
      get { return m_SelectedDescription; }
      set
      {
        if (m_SelectedDescription == value)
        {
          return;
        }
        m_SelectedDescription = value;
        NotifyOfPropertyChange(() => SelectedDescription);
        NotifyOfPropertyChange(() => YearSource);
      }
    }

    public string SelectedManufacturer
    {
      get { return m_SelectedManufacturer; }
      set
      {
        if (m_SelectedManufacturer == value)
        {
          return;
        }
        m_SelectedManufacturer = value;
        NotifyOfPropertyChange(() => SelectedManufacturer);
        NotifyOfPropertyChange(() => ModelSource);
      }
    }

    public string SelectedModel
    {
      get { return m_SelectedModel; }
      set
      {
        if (m_SelectedModel == value)
        {
          return;
        }
        m_SelectedModel = value;
        NotifyOfPropertyChange(() => SelectedModel);
        NotifyOfPropertyChange(() => DescriptionSource);
      }
    }

    public string SelectedYear
    {
      get { return m_SelectedYear; }
      set
      {
        if (m_SelectedYear == value)
        {
          return;
        }
        m_SelectedYear = value;
        if (value != null)
        {
          var car = m_FullyQualifiedCars.First(m => m.Manufacturer == m_SelectedManufacturer && m.Model == m_SelectedModel && m.Description == m_SelectedDescription && m.DateOfChange == m_SelectedYear);
          FullyQualifiedCarModel.CarId = car.Id;
        }
        NotifyOfPropertyChange(() => SelectedYear);
      }
    }

    public IEnumerable<string> YearSource
    {
      get
      {
        return m_FullyQualifiedCars.Where(m => m.Manufacturer == m_SelectedManufacturer && m.Model == m_SelectedModel && m.Description == m_SelectedDescription)
                                   .Select(d => d.DateOfChange)
                                   .OrderBy(n => n)
                                   .Distinct();
      }
    }

    private FullyQualifiedCar FullyQualifiedCarModel { get; set; }

    private void CarDataLoaded(object sender, EventArgs e)
    {
      SetEntryData();
    }

    private void SetEntryData()
    {
      m_FullyQualifiedCars = m_PositionDataAccessor.CarData;
      if (FullyQualifiedCarModel.CarId > 0)
      {
        var car = m_FullyQualifiedCars.Single(c => c.Id == FullyQualifiedCarModel.CarId);
        m_SelectedManufacturer = car.Manufacturer;
        m_SelectedModel = car.Model;
        m_SelectedDescription = car.Description;
        m_SelectedYear = car.DateOfChange;
      }
    }
  }
}