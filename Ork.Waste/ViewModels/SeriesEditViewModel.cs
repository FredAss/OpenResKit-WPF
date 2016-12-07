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

using System.Collections.Generic;
using System.Linq;
using Ork.Framework;
using Ork.Waste.DomainModelService;
using Ork.Waste.Factories;

namespace Ork.Waste.ViewModels
{
  public class SeriesEditViewModel : SeriesAddViewModel
  {
    private readonly IEnumerable<FillLevelReading> m_FillLevelReadings;
    private readonly IWasteRepository m_Repository;
    private readonly FillLevelReading m_SelectedReading;

    public SeriesEditViewModel(Series model, IWasteRepository repository, IContainerViewModelFactory containerViewModelFactory, IResponsibleSubjectViewModelFactory responsibleSubjectViewModelFactory,
      ITaskGenerator taskGenerator, FillLevelReading selectedReading, IEnumerable<FillLevelReading> allFillLevelReadings)
      : base(model, repository, containerViewModelFactory, responsibleSubjectViewModelFactory, taskGenerator)
    {
      m_Repository = repository;
      m_SelectedReading = selectedReading;
      m_FillLevelReadings = allFillLevelReadings;

      DisplayName = TranslationProvider.Translate("TitleSeriesEditViewModel");

      SelectContainer();
      SelectWeekDays();
      SelectResponsibleSubject();
    }

    public override bool IsEditable
    {
      get { return false; }
    }

    public override ResponsibleSubjectViewModel SelectedResponsibleSubject
    {
      get { return m_ResponsibleSubjects.Single(rs => rs.Model == m_SelectedReading.AppointmentResponsibleSubject); }
      set
      {
        if (m_SelectedReading.AppointmentResponsibleSubject == value.Model)
        {
          return;
        }
        var editFillLevelReadingResponsibleSubject = m_FillLevelReadings.Where(flr => flr.RelatedSeries == m_SelectedReading.RelatedSeries);
        foreach (var currentFillLevelReading in editFillLevelReadingResponsibleSubject)
        {
          currentFillLevelReading.AppointmentResponsibleSubject = value.Model;
        }
      }
    }

    private void SelectResponsibleSubject()
    {
      SelectedResponsibleSubject = m_ResponsibleSubjects.Single(rs => rs.Model == m_SelectedReading.AppointmentResponsibleSubject);
    }


    public void DeleteFollowingReadings()
    {
      //Alle nachfolgenden Termine finden
      FillLevelReading[] followingReadings;
      if (m_SelectedReading.DueDate.IsAllDay)
      {
        followingReadings = m_FillLevelReadings.Where(flr => flr.DueDate.Begin.Date >= m_SelectedReading.DueDate.Begin.Date)
                                               .ToArray();
      }
      else
      {
        followingReadings = m_FillLevelReadings.Where(flr => flr.DueDate.Begin >= m_SelectedReading.DueDate.Begin)
                                               .ToArray();
      }

      //Serie ggf. löschen
      if (!m_FillLevelReadings.Except(followingReadings)
                              .Any())
      {
        m_Repository.DeleteObject(Model);
      }
      else
      {
        //Ende der alten Serie anpassen
        if (EndsWithDate)
        {
          Model.RepeatUntilDate = m_SelectedReading.DueDate.Begin;
        }
        else
        {
          Model.NumberOfRecurrences = Model.NumberOfRecurrences - followingReadings.Count();
        }
      }

      //Alle folgenden Termine löschen
      foreach (var followingReading in followingReadings)
      {
        DeleteReadingFromRepository(followingReading);
      }
      m_Repository.Save();
      TryClose();
    }

    public void DeleteReading()
    {
      DeleteReadingFromRepository(m_SelectedReading);

      //Serie ggf. löschen
      if (!m_FillLevelReadings.Any())
      {
        m_Repository.DeleteObject(Model);
      }
      m_Repository.Save();
      TryClose();
    }

    public void DeleteSeries()
    {
      foreach (var fillLevelReading in m_FillLevelReadings.ToArray())
      {
        DeleteReadingFromRepository(fillLevelReading);
      }

      //Serie löschen
      m_Repository.DeleteObject(Model);
      m_Repository.Save();
      TryClose();
    }

    private void DeleteReadingFromRepository(FillLevelReading fillLevelReading)
    {
      m_Repository.DeleteObject(fillLevelReading.DueDate);
      if (m_SelectedReading.EntryDate != null)
      {
        m_Repository.DeleteObject(m_SelectedReading.EntryDate);
      }
      m_Repository.FillLevelReadings.Remove(fillLevelReading);
    }

    private void SelectContainer()
    {
      var seriesContainers = m_FillLevelReadings.Select(fr => fr.ReadingContainer)
                                                .Distinct();

      foreach (var selectableContainerViewModel in from container in seriesContainers
                                                   from selectableContainerViewModel in FilteredContainers
                                                   where selectableContainerViewModel.ContainerViewModel.Model == container
                                                   select selectableContainerViewModel)
      {
        selectableContainerViewModel.IsSelected = true;
      }
    }

    private void SelectWeekDays()
    {
      if (!Repeat ||
          Cycle != 1)
      {
        return;
      }
      foreach (var selectableWeekdayViewModel in Model.WeekDays.SelectMany(weekDay => WeekDayList.Where(selectableWeekdayViewModel => selectableWeekdayViewModel.DayOfWeek == weekDay.GetAsDayOfWeek()))
        )
      {
        selectableWeekdayViewModel.IsSelected = true;
      }
    }
  }
}