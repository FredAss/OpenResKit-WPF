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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ork.Calendar.Models;
using Ork.Calendar.ViewModels;

namespace Ork.Calendar.Controls
{
  public partial class Calendar : UserControl
  {
    internal static readonly DependencyProperty CalendarRangesProperty = DependencyProperty.Register("CalendarRanges", typeof (IEnumerable<TimeRange>), typeof (Calendar),
      new PropertyMetadata(default(IEnumerable<TimeRange>)));

    internal static readonly DependencyProperty CurrentRangeProperty = DependencyProperty.Register("CurrentRange", typeof (TimeRange), typeof (Calendar),
      new PropertyMetadata(default(TimeRange), CurrentRangeChanged));

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof (IEnumerable<CalendarEntry>), typeof (Calendar),
      new PropertyMetadata(default(IEnumerable<CalendarEntry>), ItemsSourceChanged));


    public static readonly DependencyProperty CurrentDateProperty = DependencyProperty.Register("CurrentDate", typeof (DateTime), typeof (Calendar),
      new PropertyMetadata(default(DateTime), CurrentDateChanged));

    public static readonly DependencyProperty SelectedCalendarEntryProperty = DependencyProperty.Register("SelectedCalendarEntry", typeof (CalendarEntry), typeof (Calendar),
      new PropertyMetadata(default(CalendarEntry), SelectedCalenderEntryProperty));

    public Calendar()
    {
      InitializeComponent();
      CalendarRanges = new TimeRange[]
                       {
                         new DayViewModel(), new WeekViewModel(), new MonthViewModel()
                       };
      CurrentRange = CalendarRanges.ElementAt(1);
      CurrentDate = DateTime.Today;
    }

    public DateTime CurrentDate
    {
      get { return (DateTime) GetValue(CurrentDateProperty); }
      set { SetValue(CurrentDateProperty, value); }
    }

    public IEnumerable<CalendarEntry> ItemsSource
    {
      get { return (IEnumerable<CalendarEntry>) GetValue(ItemsSourceProperty); }
      set { SetValue(ItemsSourceProperty, value); }
    }

    public CalendarEntry SelectedCalendarEntry
    {
      get { return (CalendarEntry) GetValue(SelectedCalendarEntryProperty); }
      set { SetValue(SelectedCalendarEntryProperty, value); }
    }

    public ICommand StepNextCommand
    {
      get { return new ActionCommand(_ => StepNext()); }
    }

    public ICommand StepPreviousCommand
    {
      get { return new ActionCommand(_ => StepPrevious()); }
    }

    internal IEnumerable<TimeRange> CalendarRanges
    {
      get { return (IEnumerable<TimeRange>) GetValue(CalendarRangesProperty); }
      set { SetValue(CalendarRangesProperty, value); }
    }

    internal TimeRange CurrentRange
    {
      get { return (TimeRange) GetValue(CurrentRangeProperty); }
      set { SetValue(CurrentRangeProperty, value); }
    }

    public void Refresh()
    {
      foreach (var range in CalendarRanges)
      {
        range.Refresh();
      }
    }

    public void StepNext()
    {
      CurrentDate += CurrentRange.Range;
    }

    public void StepPrevious()
    {
      CurrentDate -= CurrentRange.Range;
    }

    private static void CurrentDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var calendar = (Calendar) d;
      calendar.CurrentRange.ChangeCurrentDate(calendar.CurrentDate);
    }

    private static void CurrentRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var calendar = (Calendar) d;
      if (calendar.ItemsSource == null)
      {
        return;
      }
      calendar.CurrentRange.ChangeCurrentDate(calendar.CurrentDate);
      calendar.CurrentRange.ChangeSelectedCalendarEntry(calendar.SelectedCalendarEntry);
      calendar.CurrentRange.Items = calendar.ItemsSource;
    }

    private static void ItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var calendar = (Calendar) d;
      calendar.CurrentRange.Items = calendar.ItemsSource;
    }

    private static void SelectedCalenderEntryProperty(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var calendar = (Calendar) d;
      calendar.CurrentRange.ChangeSelectedCalendarEntry(calendar.SelectedCalendarEntry);
    }
  }
}