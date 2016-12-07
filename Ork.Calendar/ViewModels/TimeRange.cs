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
using Caliburn.Micro;
using Ork.Calendar.Models;

namespace Ork.Calendar.ViewModels
{
  public abstract class TimeRange : PropertyChangedBase
  {
    protected IEnumerable<CalendarEntry> m_Items = new List<CalendarEntry>();
    private DateTime m_RangeStartDate = DateTime.Now;
    private CalendarEntry m_SelectedCalendarEntry;

    public IEnumerable<CalendarEntry> Items
    {
      get { return m_Items; }
      set
      {
        if (m_Items == value)
        {
          return;
        }
        m_Items = value;
        OnItemsChanged();
        NotifyOfPropertyChange(() => ItemsInRange);
      }
    }

    public IEnumerable<CalendarEntry> ItemsInRange
    {
      get
      {
        var end = RangeStartDate.Add(Range);
        return Items.Where(i => i.EndTime >= RangeStartDate && i.StartTime < end);
      }
    }

    public abstract string Name { get; }
    public abstract TimeSpan Range { get; }

    public DateTime RangeStartDate
    {
      get { return m_RangeStartDate; }
      protected set
      {
        if (m_RangeStartDate == value)
        {
          return;
        }
        m_RangeStartDate = value;
        NotifyOfPropertyChange(() => RangeStartDate);
        NotifyOfPropertyChange(() => RangeString);
        NotifyOfPropertyChange(() => ItemsInRange);
        NotifyOfPropertyChange(() => RangeStartDateString);
      }
    }

    public string RangeStartDateString
    {
      get { return m_RangeStartDate.ToString("ddd, dd.MM."); }
    }

    public abstract string RangeString { get; }

    public CalendarEntry SelectedCalendarEntry
    {
      get { return m_SelectedCalendarEntry; }
      protected set
      {
        if (m_SelectedCalendarEntry == value)
        {
          return;
        }
        m_SelectedCalendarEntry = value;
        NotifyOfPropertyChange(() => SelectedCalendarEntry);
      }
    }

    public abstract void ChangeCurrentDate(DateTime currentDate);
    public abstract void ChangeSelectedCalendarEntry(CalendarEntry selectedCalendarEntry);
    protected abstract void OnItemsChanged();
  }
}