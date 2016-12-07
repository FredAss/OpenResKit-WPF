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
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Services.Client;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Ork.Calendar.Models;
using Ork.Framework;
using Ork.Meter.DomainModelService;
using Ork.Meter.Factories;
using WPFLocalizeExtension.Engine;
using DayOfWeek = Ork.Meter.DomainModelService.DayOfWeek;

namespace Ork.Meter.ViewModels
{
  [Export(typeof (IWorkspace))]
  internal class ReadingPlanningViewModel : DocumentBase, IWorkspace
  {
    private readonly BindableCollection<MeterReadingViewModel> m_MeterReadings = new BindableCollection<MeterReadingViewModel>();
    private readonly IMeterReadingViewModelFactory m_ReadingViewModelFactory;
    private readonly IMeterRepository m_Repository;
    private readonly ISeriesViewModelFactory m_SeriesViewModelFactory;
    private Calendar.Controls.Calendar m_Calendar;
    private SeriesAddViewModel m_EditItem;
    private bool m_FlyoutActivated;
    private bool m_IsEnabled;
    private string m_ReadingSearchText;
    private MeterReadingViewModel m_SelectedReading;

    [ImportingConstructor]
    public ReadingPlanningViewModel(ISeriesViewModelFactory seriesViewModelFactory, IMeterReadingViewModelFactory readingViewModelFactory, IMeterViewModelFactory meterViewModelFactory,
      IResponsibleSubjectViewModelFactory responsibleSubjectViewModelFactory, IMeterRepository contextRepository, ITaskGenerator taskGenerator)
    {
      m_Repository = contextRepository;
      m_SeriesViewModelFactory = seriesViewModelFactory;
      m_ReadingViewModelFactory = readingViewModelFactory;
      m_Repository.ContextChanged += (s, e) => Application.Current.Dispatcher.Invoke(Reload);
      Reload();

      LocalizeDictionary.Instance.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args)
                                                     {
                                                       if (args.PropertyName == "Culture")
                                                       {
                                                         if (m_Calendar != null)
                                                         {
                                                           m_Calendar.Refresh();
                                                         }
                                                       }
                                                     };

      FlyoutActivated = true;
    }

    public IEnumerable<CalendarEntry> CalendarEntries
    {
      get { return FilteredReadings.Select(fr => fr.CalendarEntry); }
    }

    public IEnumerable<MeterReadingViewModel> FilteredReadings
    {
      get
      {
        return SearchInReadingList()
          .ToArray();
      }
    }

    public bool IsEnabledIfResponsibleSubjectsExists
    {
      get
      {
        var a = m_Repository.ResponsibleSubjects.Any();
        return a;
      }
    }

    public bool FlyoutActivated
    {
      get { return m_FlyoutActivated; }
      set
      {
        if (m_FlyoutActivated == value)
        {
          return;
        }
        m_FlyoutActivated = value;
        NotifyOfPropertyChange(() => FlyoutActivated);
      }
    }

    public override bool IsDirty
    {
      get { return m_EditItem != null && m_EditItem.IsActive; }
      set { base.IsDirty = value; }
    }

    public string ReadingSearchText
    {
      get { return m_ReadingSearchText; }
      set
      {
        m_ReadingSearchText = value;
        NotifyOfPropertyChange(() => FilteredReadings);
        NotifyOfPropertyChange(() => CalendarEntries);
      }
    }

    public MeterReadingViewModel SelectedReading
    {
      get { return m_SelectedReading; }
      set
      {
        if (m_SelectedReading == value)
        {
          return;
        }
        m_SelectedReading = value;
        NotifyOfPropertyChange(() => SelectedReading);
      }
    }

    public int Index
    {
      get { return 200; }
    }

    public bool IsEnabled
    {
      get { return m_IsEnabled; }
      private set
      {
        m_IsEnabled = value;
        NotifyOfPropertyChange(() => IsEnabled);
      }
    }

    public string Title
    {
      get { return TranslationProvider.Translate("Planning"); }
    }

    protected override void OnViewLoaded(object view)
    {
      base.OnViewLoaded(view);

      var frameworkElement = view as FrameworkElement;

      if (frameworkElement == null)
      {
        return;
      }

      m_Calendar = frameworkElement.FindName("Calendar") as Calendar.Controls.Calendar;
    }

    private void Reload()
    {
      IsEnabled = m_Repository.HasConnection;
      if (IsEnabled)
      {
        LoadData();
      }
      NotifyOfPropertyChange(() => IsEnabledIfResponsibleSubjectsExists);
    }

    private void CloseEditor()
    {
      m_EditItem.TryClose();
    }

    public void OpenAddDialog()
    {
      OpenEditor(m_SeriesViewModelFactory.CreateAddViewModel());
    }

    public void OpenEditDialog(object dataContext)
    {
      var scheduledTaskViewModel = m_SeriesViewModelFactory.CreateEditViewModelFromExisting((MeterReadingViewModel) dataContext, m_MeterReadings);
      OpenEditor(scheduledTaskViewModel);
    }

    public void OpenEditor(object dataContext)
    {
      m_EditItem = (SeriesAddViewModel) dataContext;
      Dialogs.ShowDialog(m_EditItem);
    }

    public void OpenEditor(object dataContext, MouseButtonEventArgs e)
    {
      if (e.ClickCount >= 2)
      {
        OpenEditDialog(dataContext);
      }
    }

    public void Accept()
    {
      Save();
      CloseEditor();
    }

    public void Cancel()
    {
      CloseEditor();
    }

    private void Save()
    {
      if (m_Repository.Entities.Where(ed => ed.Entity is MeterReading || ed.Entity is Series || ed.Entity is Appointment || ed.Entity is DayOfWeek || ed.Entity is SeriesColor)
                      .Any(ed => ed.State != EntityStates.Unchanged) ||
          m_Repository.Links.Where(l => l.Source is MeterReading || l.Source is Series || l.Source is Appointment || l.Source is DayOfWeek || l.Source is SeriesColor)
                      .Any(ed => ed.State != EntityStates.Unchanged))
      {
        m_Repository.Save();
      }
    }

    private void AlterMeterReadingCollection(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.Action == NotifyCollectionChangedAction.Add)
      {
        foreach (var newItem in e.NewItems.OfType<MeterReading>())
        {
          m_MeterReadings.Add(CreateReadingViewModel(newItem));
        }
      }
      else
      {
        foreach (var oldItem in e.OldItems.OfType<MeterReading>())
        {
          var readingViewModel = m_MeterReadings.Single(r => r.Model == oldItem);
          m_MeterReadings.Remove(readingViewModel);
        }
      }

      NotifyOfPropertyChange(() => FilteredReadings);
      NotifyOfPropertyChange(() => CalendarEntries);
    }

    private MeterReadingViewModel CreateReadingViewModel(MeterReading newItem)
    {
      var readingViewModel = m_ReadingViewModelFactory.CreateFromExisting(newItem);

      var now = DateTime.Now;
      var dueDate = readingViewModel.Model.DueDate.Begin;

      if (dueDate.Date >= now.GetFirstDayOfWeek() &&
          dueDate.Date <= now.GetLastDayOfWeek() &&
          dueDate >= now)
      {
        //diese Woche
        readingViewModel.DateIndicator = 0;
      }
      else if (dueDate < now)
      {
        if (dueDate.Date == now.Date &&
            readingViewModel.Model.DueDate.IsAllDay)
        {
          //ganztägig und heute = diese Woche (duedate-zeit ignorieren)
          readingViewModel.DateIndicator = 0;
        }
        else
        {
          //abgelaufen
          readingViewModel.DateIndicator = 1;
        }
      }
      else
      {
        //alle anderen
        readingViewModel.DateIndicator = 2;
      }

      return readingViewModel;
    }

    private void LoadData()
    {
      m_MeterReadings.Clear();
      LoadMeterReadings();
    }

    private void LoadMeterReadings()
    {
      m_Repository.MeterReadings.CollectionChanged += AlterMeterReadingCollection;
      foreach (var reading in m_Repository.MeterReadings.OfType<MeterReading>())
      {
        m_MeterReadings.Add(CreateReadingViewModel(reading));
      }
      NotifyOfPropertyChange(() => FilteredReadings);
    }

    private IEnumerable<MeterReadingViewModel> SearchInReadingList()
    {
      if (string.IsNullOrEmpty(ReadingSearchText))
      {
        return m_MeterReadings;
      }
      var searchText = ReadingSearchText.ToLower();

      //TODO: kein Name???
      var searchResult = m_MeterReadings.Where(r => (((r.MeterViewModel.Barcode != null) && (r.MeterViewModel.Barcode.ToLower()
                                                                                              .Contains(searchText))) || ((r.MeterViewModel.Name != null) && (r.MeterViewModel.Name.ToLower()
                                                                                                                                                               .Contains(searchText))) ||
                                                     ((r.AppointmentResponsibleSubject != null) && (r.AppointmentResponsibleSubject.IsOfType<Employee>()) &&
                                                      ((((Employee) r.AppointmentResponsibleSubject).FirstName != null) && ((Employee) r.AppointmentResponsibleSubject).FirstName.ToLower()
                                                                                                                                                                       .Contains(searchText)) ||
                                                      ((((Employee) r.AppointmentResponsibleSubject).LastName != null) && ((Employee) r.AppointmentResponsibleSubject).LastName.ToLower()
                                                                                                                                                                      .Contains(searchText)) ||
                                                      (((Employee) r.AppointmentResponsibleSubject).Number != null) && ((Employee) r.AppointmentResponsibleSubject).Number.ToLower()
                                                                                                                                                                   .Contains(searchText))) ||
                                                    ((r.AppointmentResponsibleSubject != null) && (r.AppointmentResponsibleSubject.IsOfType<EmployeeGroup>()) &&
                                                     ((((EmployeeGroup) r.AppointmentResponsibleSubject).Name != null) && ((EmployeeGroup) r.AppointmentResponsibleSubject).Name.ToLower()
                                                                                                                                                                           .Contains(searchText))) ||
                                                    ((r.RelatedSeriesName != null) && r.RelatedSeriesName.ToLower()
                                                                                       .Contains(searchText)));

      return searchResult;
    }
  }
}