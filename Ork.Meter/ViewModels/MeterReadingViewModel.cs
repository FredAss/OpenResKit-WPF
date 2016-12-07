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
using System.ComponentModel;
using System.Windows.Media;
using Caliburn.Micro;
using Ork.Calendar.Models;
using Ork.Framework;
using Ork.Meter.DomainModelService;

namespace Ork.Meter.ViewModels
{
  public class MeterReadingViewModel : PropertyChangedBase
  {
    private readonly CalendarEntry m_CalendarEntry;
    private readonly MeterViewModel m_MeterViewModel;
    private readonly MeterReading m_Model;
    private readonly SeriesViewModel m_RelatedSeriesViewModel;
    private ResponsibleSubject m_AppointmentResponsibleSubject;
    private int m_DateIndicator;
    private Brush m_SeriesColor;

    public MeterReadingViewModel(MeterReading model, MeterViewModel meterViewModel, SeriesViewModel relatedSeriesViewModel)
    {
      m_Model = model;
      m_MeterViewModel = meterViewModel;
      m_RelatedSeriesViewModel = relatedSeriesViewModel;
      m_AppointmentResponsibleSubject = model.AppointmentResponsibleSubject;
      m_Model.PropertyChanged += OnModelPropertyChanged;
      m_Model.RelatedSeries.SeriesColor.PropertyChanged += OnSeriesColorPropertyChanged;
      m_Model.RelatedSeries.PropertyChanged += OnRelatedSeriesPropertyChanged;
      m_Model.DueDate.PropertyChanged += OnDueDatePropertyChanged;
      var color = Color.FromRgb(m_Model.RelatedSeries.SeriesColor.R, m_Model.RelatedSeries.SeriesColor.G, m_Model.RelatedSeries.SeriesColor.B);
      SeriesColor = new SolidColorBrush(color);
      m_CalendarEntry = new CalendarEntry(m_Model.DueDate.Begin, m_Model.DueDate.End, m_Model.ReadingMeter.Number, color, m_Model.DueDate.IsAllDay, this);
    }

    public ResponsibleSubject AppointmentResponsibleSubject
    {
      get { return m_AppointmentResponsibleSubject; }
      set
      {
        if (m_AppointmentResponsibleSubject == value)
        {
          return;
        }
        m_AppointmentResponsibleSubject = value;
        NotifyOfPropertyChange(() => AppointmentResponsibleSubject);
        NotifyOfPropertyChange(() => ResponsibleSubject);
      }
    }

    public string ResponsibleSubject
    {
      get
      {
        if (m_AppointmentResponsibleSubject is Employee)
        {
          var employee = (Employee) m_AppointmentResponsibleSubject;
          return employee.FirstName + " " + employee.LastName;
        }
        if (m_AppointmentResponsibleSubject is EmployeeGroup)
        {
          var employeeGroup = (EmployeeGroup) m_AppointmentResponsibleSubject;
          return employeeGroup.Name;
        }
        return TranslationProvider.Translate("NotAvailable");
      }
    }


    public Brush SeriesColor
    {
      get { return m_SeriesColor; }
      set
      {
        if (m_SeriesColor == value)
        {
          return;
        }
        m_SeriesColor = value;
        NotifyOfPropertyChange(() => SeriesColor);
      }
    }

    public CalendarEntry CalendarEntry
    {
      get { return m_CalendarEntry; }
    }

    public string MeterName
    {
      get { return m_Model.ReadingMeter.Number; }
    }

    public MeterViewModel MeterViewModel
    {
      get { return m_MeterViewModel; }
    }

    public SeriesViewModel RelatedSeriesViewModel
    {
      get { return m_RelatedSeriesViewModel; }
    }

    public int DateIndicator
    {
      get { return m_DateIndicator; }
      set
      {
        if (value.Equals(m_DateIndicator))
        {
          return;
        }
        m_DateIndicator = value;
        NotifyOfPropertyChange(() => DateIndicator);
      }
    }

    public string DueDate
    {
      get { return m_Model.DueDate.Begin.ToString(TranslationProvider.Translate("DateTimeForShortPlanning")); }
    }

    public string DueDateShort
    {
      get { return m_Model.DueDate.Begin.ToString("ddd, ") + m_Model.DueDate.Begin.ToShortDateString(); }
    }

    public string DueDateTimeRange
    {
      get { return m_Model.DueDate.Begin.ToString(TranslationProvider.Translate("DueDateTimeRange")) + " - " + m_Model.DueDate.End.ToString(TranslationProvider.Translate("DueDateTimeRange")); }
    }

    public DateTime DueDateBeginDateTime
    {
      get { return m_Model.DueDate.Begin; }
    }

    public string EntryDateString
    {
      get
      {
        if (m_Model.EntryDate != null)
        {
          return m_Model.EntryDate.Begin.ToString("ddd, ") + m_Model.EntryDate.Begin.ToShortDateString();
        }
        return "";
      }
    }

    public DateTime EntryDate
    {
      get
      {
        if (m_Model.EntryDate != null)
        {
          return m_Model.EntryDate.Begin;
        }
        return DateTime.MaxValue;
      }
    }

    public bool IsAllDay
    {
      get { return m_Model.DueDate.IsAllDay; }
    }

    public MeterReading Model
    {
      get { return m_Model; }
    }

    public DateTime OrderDate
    {
      get { return m_Model.DueDate.Begin; }
    }

    public double Value
    {
      get { return m_Model.CounterReading; }
    }

    public string FormatedValue
    {
      get
      {
        var value = m_Model.CounterReading;
        var unitString = m_Model.ReadingMeter.Unit;

        return value + " " + unitString;
      }
    }

    public Series RelatedSeries
    {
      get { return m_Model.RelatedSeries; }
    }

    public string RelatedSeriesName
    {
      get { return m_Model.RelatedSeries.Name; }
    }

    private void OnSeriesColorPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      var color = Color.FromRgb(m_Model.RelatedSeries.SeriesColor.R, m_Model.RelatedSeries.SeriesColor.G, m_Model.RelatedSeries.SeriesColor.B);
      var brush = new SolidColorBrush(color);
      brush.Freeze();
      SeriesColor = brush;
      m_CalendarEntry.Color = SeriesColor;
    }

    private void OnDueDatePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      NotifyOfPropertyChange(() => CalendarEntry);
      NotifyOfPropertyChange(() => DueDate);
    }

    private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      NotifyOfPropertyChange(e.PropertyName);
      if (e.PropertyName == "RelatedSeries")
      {
        NotifyOfPropertyChange(() => RelatedSeriesName);
      }
      if (e.PropertyName == "ReadingContainer")
      {
        NotifyOfPropertyChange(() => MeterName);
      }
    }

    private void OnRelatedSeriesPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "Name")
      {
        NotifyOfPropertyChange(() => RelatedSeriesName);
      }
    }
  }
}