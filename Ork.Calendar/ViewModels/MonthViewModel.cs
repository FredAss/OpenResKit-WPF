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
  public class MonthViewModel : TimeRange
  {
    public IEnumerable<MonthDayViewModel> Days
    {
      get
      {
        //var totalDays = (new DateTime(CurrentDate.Year, CurrentDate.Month, 1) - RangeStartDate) + Range;
        var totalDays = Range;
        var itemsInRange = ItemsInRange.ToArray();
        for (var i = 0; i < totalDays.TotalDays; i++)
        {
          yield return new MonthDayViewModel(itemsInRange, RangeStartDate.AddDays(i));
        }
      }
    }

    public override string Name
    {
      get { return TranslationProvider.Translate("Month"); }
    }

    public override TimeSpan Range
    {
      get { return TimeSpan.FromDays(DateTime.DaysInMonth(RangeStartDate.Year, RangeStartDate.Month)); }
    }

    public override string RangeString
    {
      get { return RangeStartDate.ToString("MMMM yyyy"); }
    }

    public override void ChangeCurrentDate(DateTime currentDate)
    {
      //var firstDayOfCurrentMonth = new DateTime(CurrentDate.Year, CurrentDate.Month, 1);
      //for (var i = 1; i < 8; i++)
      //{
      //  if (firstDayOfCurrentMonth.DayOfWeek == DayOfWeek.Monday)
      //  {
      //    RangeStartDate = firstDayOfCurrentMonth;
      //    break;
      //  }
      //  firstDayOfCurrentMonth = firstDayOfCurrentMonth.Subtract(TimeSpan.FromDays(1));
      //}
      RangeStartDate = new DateTime(currentDate.Year, currentDate.Month, 1);
      NotifyOfPropertyChange(() => Days);
    }

    public override void ChangeSelectedCalendarEntry(CalendarEntry selectedCalendarEntry)
    {
    }

    protected override void OnItemsChanged()
    {
      NotifyOfPropertyChange(() => Days);
    }
  }
}