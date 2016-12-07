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

using System.ComponentModel.Composition;
using Ork.Meter.DomainModelService;
using Ork.Meter.ViewModels;

namespace Ork.Meter.Factories
{
  [Export(typeof (IMeterReadingViewModelFactory))]
  public class MeterReadingViewModelFactory : IMeterReadingViewModelFactory
  {
    private readonly IMeterViewModelFactory m_ContainerViewModelFactory;
    private readonly ISeriesViewModelFactory m_SeriesViewModelFactory;

    [ImportingConstructor]
    public MeterReadingViewModelFactory(IMeterViewModelFactory containerViewModelFactory, ISeriesViewModelFactory seriesViewModelFactory)
    {
      m_ContainerViewModelFactory = containerViewModelFactory;
      m_SeriesViewModelFactory = seriesViewModelFactory;
    }

    public MeterReadingViewModel CreateFromExisting(MeterReading model)
    {
      return new MeterReadingViewModel(model, m_ContainerViewModelFactory.CreateFromExisiting(model.ReadingMeter), m_SeriesViewModelFactory.CreateFromExisting(model.RelatedSeries));
    }
  }
}