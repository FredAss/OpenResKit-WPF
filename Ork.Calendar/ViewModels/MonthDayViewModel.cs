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
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Ork.Calendar.Models;

namespace Ork.Calendar.ViewModels
{
  public class MonthDayViewModel : TimeRange
  {
    private const int InitialFontSize = 24;
    private IEnumerable<DayEntryViewModel> m_Entries;

    public MonthDayViewModel(IEnumerable<CalendarEntry> items, DateTime currentDate)
    {
      m_Items = items;
      ChangeCurrentDate(currentDate);
    }

    public int Column
    {
      get
      {
        var day = Day.Day;

        while (day > 7)
        {
          day -= 7;
        }

        return day - 1;
      }
    }

    public DateTime Day
    {
      get { return RangeStartDate; }
    }

    public string LocalizableDayLong
    {
      get { return Day.ToString("ddd"); }
    }

    public string LocalizableDayShort
    {
      get { return Day.ToString("dd"); }
    }

    public IEnumerable<DayEntryViewModel> Entries
    {
      get { return m_Entries; }
      set
      {
        if (m_Entries == value)
        {
          return;
        }
        m_Entries = value;
        NotifyOfPropertyChange(() => Entries);
      }
    }

    public int FontSize
    {
      get
      {
        if (Entries.Any())
        {
          return InitialFontSize + 4 * Entries.Count();
        }

        return InitialFontSize;
      }
    }

    public override string Name
    {
      get { return ""; }
    }

    public override TimeSpan Range
    {
      get { return TimeSpan.FromDays(1); }
    }

    public override string RangeString
    {
      get { return string.Empty; }
    }

    public int Row
    {
      get
      {
        var day = Day.Day;
        var iterator = 0;

        while (day > 7)
        {
          iterator++;
          day -= 7;
        }

        return iterator;
      }
    }

    public Visibility StackBarVisibility
    {
      get
      {
        if (Entries.Any())
        {
          return Visibility.Visible;
        }
        return Visibility.Collapsed;
      }
    }

    public IEnumerable<StackItem> StackItems
    {
      get
      {
        var stackItems = from dayEntryViewModel in Entries
                         let top = dayEntryViewModel.Top * 100 / 1440
                         let height = dayEntryViewModel.Height * 100 / 1440
                         select new StackItem(top, height, dayEntryViewModel.Color);

        return stackItems;
      }
    }

    public override void ChangeCurrentDate(DateTime currentDate)
    {
      RangeStartDate = currentDate;
      NotifyOfPropertyChange(() => Day);
      NotifyOfPropertyChange(() => Column);
      NotifyOfPropertyChange(() => Row);
      UpdateEntries();
    }

    public override void ChangeSelectedCalendarEntry(CalendarEntry selectedCalendarEntry)
    {
    }

    protected override void OnItemsChanged()
    {
      UpdateEntries();
    }

    private void UpdateEntries()
    {
      var entries = new Collection<DayEntryViewModel>();

      foreach (var calendarEntry in ItemsInRange.OrderByDescending(d => d.Duration)
                                                .ThenBy(item => item.StartTime))
      {
        entries.Add(new DayEntryViewModel(calendarEntry, RangeStartDate, 0, 1));
      }

      Entries = entries;
    }
  }
}