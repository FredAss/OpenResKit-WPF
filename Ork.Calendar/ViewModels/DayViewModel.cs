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
using Ork.Calendar.Models;
using Ork.Framework;

namespace Ork.Calendar.ViewModels
{
  public class DayViewModel : TimeRange
  {
    private IEnumerable<DayEntryViewModel> m_AllDayEntries;
    private int m_Column;
    private IEnumerable<DayEntryViewModel> m_Entries;

    public DayViewModel()
    {
    }

    public DayViewModel(int column, IEnumerable<CalendarEntry> items, DateTime currentDate)
    {
      m_Column = column;
      m_Items = items;
      ChangeCurrentDate(currentDate);
    }

    public IEnumerable<DayEntryViewModel> AllDayEntries
    {
      get { return m_AllDayEntries; }
      set
      {
        if (m_AllDayEntries == value)
        {
          return;
        }
        m_AllDayEntries = value;
        NotifyOfPropertyChange(() => AllDayEntries);
      }
    }

    public int Column
    {
      get { return m_Column; }
      set
      {
        if (m_Column == value)
        {
          return;
        }
        m_Column = value;
        NotifyOfPropertyChange(() => Column);
      }
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

    public IEnumerable<int> Hours
    {
      get
      {
        return new[]
               {
                 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23
               };
      }
    }

    public override string Name
    {
      get { return TranslationProvider.Translate("Day"); }
    }

    public string RangeString2
    {
      get
      {
        var a = RangeStartDate.ToString(TranslationProvider.Translate("RangeDateStart"));
        return a;
      }
    }

    public override TimeSpan Range
    {
      get { return TimeSpan.FromDays(1); }
    }

    public override string RangeString
    {
      get { return string.Empty; }
    }

    public override void ChangeCurrentDate(DateTime currentDate)
    {
      RangeStartDate = currentDate;
      UpdateEntries();
    }

    public override void ChangeSelectedCalendarEntry(CalendarEntry selectedCalendarEntry)
    {
      SelectedCalendarEntry = selectedCalendarEntry;
      if (selectedCalendarEntry == null)
      {
        return;
      }
      foreach (var dayEntryViewModel in AllDayEntries.Concat(Entries))
      {
        dayEntryViewModel.IsSelected = dayEntryViewModel.Entry == selectedCalendarEntry;
      }
    }

    protected override void OnItemsChanged()
    {
      UpdateEntries();
    }

    private void UpdateEntries()
    {
      var entries = ItemsInRange.OrderBy(item => item.StartTime)
                                .ThenByDescending(d => d.Duration)
                                .Select(ce => new DayEntryViewModel(ce, RangeStartDate, 1, 1))
                                .ToArray();

      var dayEntries = entries.Where(x => !x.Entry.IsAllDay)
                              .ToArray();


      for (var index = 0; index < dayEntries.Length; index++)
      {
        var item = dayEntries[index];
        var itemsStartingBefore = dayEntries.Take(index)
                                            .Where(e => e.Entry.StartTime <= item.Entry.StartTime && e.Entry.EndTime > item.Entry.StartTime)
                                            .ToArray();
        if (itemsStartingBefore.Any())
        {
          var factor = itemsStartingBefore.Max(d => d.StackLevel);
          if (factor > itemsStartingBefore.Count())
          {
            for (var i = 0; i < itemsStartingBefore.Length; i++)
            {
              if (i + 1 < itemsStartingBefore[i].StackLevel)
              {
                item.StackLevel = i + 1;
                break;
              }
              item.Intersection = factor;
            }
          }
          else
          {
            item.StackLevel = factor + 1;
            item.Intersection = item.StackLevel;
            foreach (var dayEntry in itemsStartingBefore)
            {
              dayEntry.Intersection = item.StackLevel;
            }
          }
        }
        else
        {
          item.StackLevel = 1;
        }
      }

      AllDayEntries = entries.Except(dayEntries)
                             .ToArray();
      Entries = dayEntries;
    }
  }
}