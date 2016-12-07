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
using Ork.Approval.DomainModelService;
using Ork.Approval.ViewModels;

namespace Ork.Approval.Factories
{
    [Export(typeof(IReadingPlanningViewModelFactory))]
    public class ReadingPlanningViewModelFactory : IReadingPlanningViewModelFactory
    {
        private readonly IPlantManagementViewModelFactory m_PlantManagementViewModelFactory;
        private readonly IResponsibleSubjectViewModelFactory m_ResponsibleSubjectViewModelFactory;
        private readonly ITaskGenerator m_TaskGenerator;
        private readonly IApprovalRepository m_Repository;
        private readonly Random m_Random;


        [ImportingConstructor]
        public ReadingPlanningViewModelFactory(IApprovalRepository repository, IResponsibleSubjectViewModelFactory responsibleSubjectViewModelFactory, IPlantManagementViewModelFactory plantManagementViewModelFactory, ITaskGenerator taskGenerator)
        {
            m_Repository = repository;
            m_ResponsibleSubjectViewModelFactory = responsibleSubjectViewModelFactory;
            m_PlantManagementViewModelFactory = plantManagementViewModelFactory;
            m_TaskGenerator = taskGenerator;
            m_Random = new Random();
        }

        public SeriesViewModel CreateSeriesViewModel(Series series)
        {
            return new SeriesViewModel(series);
        }

        public SeriesAddViewModel CreateSeriesAddViewModel()
        {
            return new SeriesAddViewModel(CreateSeriesModel(), m_Repository, m_PlantManagementViewModelFactory, m_ResponsibleSubjectViewModelFactory, m_TaskGenerator);
        }

        public SeriesEditViewModel CreateSeriesEditViewModel(InspectionViewModel inspectionViewModel, IEnumerable<InspectionViewModel> allInspectionViewModels)
        {
            var allInspections =
                allInspectionViewModels.Where(ivm => ivm.RelatedSeries == inspectionViewModel.RelatedSeries)
                    .Select(ivm => ivm.Inspection);

            return new SeriesEditViewModel(inspectionViewModel.RelatedSeries, m_Repository, m_PlantManagementViewModelFactory, m_ResponsibleSubjectViewModelFactory, m_TaskGenerator, inspectionViewModel.Inspection, allInspections);
        }

        private Series CreateSeriesModel()
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
            newSeries.RepeatUntilDate = newSeries.End.Date.AddDays(7)
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
    }
}
