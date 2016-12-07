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
  public class WeekViewModel : TimeRange
  {
    private IEnumerable<DayViewModel> m_Days;

    public IEnumerable<DayViewModel> Days
    {
      get { return m_Days; }
      set
      {
        if (m_Days == value)
        {
          return;
        }
        m_Days = value;
        NotifyOfPropertyChange(() => Days);
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
      get { return TranslationProvider.Translate("Week"); }
    }

    public override TimeSpan Range
    {
      get { return TimeSpan.FromDays(7); }
    }

    public override string RangeString
    {
      get { return string.Format("{0}. {1} - {2}", RangeStartDate.GetWeekOfYear(), TranslationProvider.Translate("WeekOfYear"), RangeStartDate.ToString("MMMM yyyy")); }
    }

    public override void ChangeCurrentDate(DateTime currentDate)
    {
      var rangeStartDate = currentDate;
      for (var i = 1; i < 8; i++)
      {
        if (rangeStartDate.DayOfWeek == DayOfWeek.Monday)
        {
          RangeStartDate = rangeStartDate;
          break;
        }
        else
        {
          rangeStartDate = rangeStartDate.Subtract(TimeSpan.FromDays(1));
        }
      }
      CreateDays();
    }

    private void CreateDays()
    {
      IList<DayViewModel> list = new List<DayViewModel>();
      var itemsInRange = ItemsInRange.ToArray();
      for (var i = 0; i <= 6; i++)
      {
        list.Add(new DayViewModel(i, itemsInRange, RangeStartDate.AddDays(i)));
      }
      Days = list;
    }

    public override void ChangeSelectedCalendarEntry(CalendarEntry selectedCalendarEntry)
    {
      foreach (var dayEntryViewModel in Days)
      {
        dayEntryViewModel.ChangeSelectedCalendarEntry(selectedCalendarEntry);
      }
    }

    protected override void OnItemsChanged()
    {
      CreateDays();
    }
  }
}