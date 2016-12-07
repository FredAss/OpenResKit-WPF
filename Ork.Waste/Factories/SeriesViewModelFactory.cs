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
using System.ComponentModel.Composition;
using System.Linq;
using Ork.Waste.DomainModelService;
using Ork.Waste.ViewModels;

namespace Ork.Waste.Factories
{
  [Export(typeof (ISeriesViewModelFactory))]
  internal class SeriesViewModelFactory : ISeriesViewModelFactory
  {
    private readonly IContainerViewModelFactory m_ContainerViewModelFactory;
    private readonly Random m_Random;
    private readonly IWasteRepository m_Repository;
    private readonly IResponsibleSubjectViewModelFactory m_ResponsibleSubjectViewModelFactory;
    private readonly ITaskGenerator m_TaskGenerator;


    [ImportingConstructor]
    public SeriesViewModelFactory(IWasteRepository repository, IResponsibleSubjectViewModelFactory responsibleSubjectViewModelFactory, IContainerViewModelFactory containerViewModelFactory,
      ITaskGenerator taskGenerator)
    {
      m_Repository = repository;
      m_ResponsibleSubjectViewModelFactory = responsibleSubjectViewModelFactory;
      m_ContainerViewModelFactory = containerViewModelFactory;
      m_TaskGenerator = taskGenerator;
      m_Random = new Random();
    }

    public SeriesAddViewModel CreateAddViewModel()
    {
      return new SeriesAddViewModel(CreateModel(), m_Repository, m_ContainerViewModelFactory, m_ResponsibleSubjectViewModelFactory, m_TaskGenerator);
    }

    public SeriesEditViewModel CreateEditViewModelFromExisting(FillLevelReadingViewModel fillLevelReadingViewModel, IEnumerable<FillLevelReadingViewModel> allFillLevelReadings)
    {
      return new SeriesEditViewModel(fillLevelReadingViewModel.RelatedSeries, m_Repository, m_ContainerViewModelFactory, m_ResponsibleSubjectViewModelFactory, m_TaskGenerator,
        fillLevelReadingViewModel.Model, allFillLevelReadings.Where(flr => flr.RelatedSeries == fillLevelReadingViewModel.RelatedSeries)
                                                             .Select(flr => flr.Model));
    }

    private Series CreateModel()
    {
      var newSeries = new Series();
      newSeries.Begin = DateTime.Today.AddDays(1)
                                .AddHours(9);
      newSeries.End = DateTime.Today.AddDays(1)
                              .AddHours(10);
      newSeries.EndsWithDate = true;
      newSeries.Cycle = 0;
      newSeries.RecurrenceInterval = 1;
      newSeries.NumberOfRecurrences = 35;
      newSeries.RepeatUntilDate = newSeries.End.Date.AddWeeks(1)
                                           .AddDays(1)
                                           .Subtract(new TimeSpan(0, 0, 1));
      newSeries.IsWeekdayRecurrence = true;


      var colorBytes = new byte[3];
      m_Random.NextBytes(colorBytes);
      var col = new SeriesColor();
      col.R = colorBytes[0];
      col.G = colorBytes[1];
      col.B = colorBytes[2];

      newSeries.SeriesColor = col;

      return newSeries;
    }

    #region ISeriesViewModelFactory Members

    public SeriesViewModel CreateFromExisting(Series series)
    {
      return new SeriesViewModel(series);
    }

    #endregion
  }
}