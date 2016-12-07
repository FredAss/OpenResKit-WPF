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

namespace Ork.Calendar.ViewModels
{
  public class DayEntryViewModel : PropertyChangedBase
  {
    private const double EntryWidth = 150;
    private const double HorizontalStart = 40;
    private const double AllDayEventHeight = 24;
    public const double HorizontalDistanceStackedEntry = EntryWidth / 2;
    private readonly CalendarEntry m_Entry;
    private double m_Height;
    private int m_Intersection;
    private bool m_IsSelected;
    private int m_StackLevel;
    private double m_Top;
    private double m_Width;

    public DayEntryViewModel(CalendarEntry entry, DateTime displayedDay, int stackLevel, int intersection)
    {
      m_Entry = entry;
      m_Entry.PropertyChanged += ModelChange;
      m_StackLevel = stackLevel;
      m_Intersection = intersection;

      var displayedDayStart = displayedDay.Date;

      if (m_Entry.StartTime < displayedDayStart)
      {
        Top = 0;
      }
      else
      {
        Top = m_Entry.StartTime.Hour * 60 + m_Entry.StartTime.Minute;
      }

      var displayDayEnd = displayedDayStart.AddDays(1);
      if (m_Entry.EndTime < displayDayEnd)
      {
        if (m_Entry.StartTime < displayedDayStart)
        {
          Height = (m_Entry.EndTime - m_Entry.EndTime.Date).TotalMinutes;
        }
        else
        {
          Height = m_Entry.Duration.TotalMinutes;
        }
      }
      else
      {
        Height = 20 * 60 - Top;
      }

      if (entry.IsAllDay)
      {
        Height = AllDayEventHeight;
      }

      Width = EntryWidth;
    }

    public Brush Color
    {
      get { return m_Entry.Color; }
    }

    public CalendarEntry Entry
    {
      get { return m_Entry; }
    }

    public double Height
    {
      get { return m_Height; }
      set
      {
        if (Math.Abs(m_Height - value) < double.Epsilon)
        {
          return;
        }
        m_Height = value;
        NotifyOfPropertyChange(() => Height);
      }
    }


    public int Intersection
    {
      get { return m_Intersection; }
      set
      {
        if (m_Intersection == value)
        {
          return;
        }
        m_Intersection = value;
        NotifyOfPropertyChange(() => Intersection);
      }
    }

    public bool IsSelected
    {
      get { return m_IsSelected; }
      set
      {
        if (m_IsSelected == value)
        {
          return;
        }
        m_IsSelected = value;
        NotifyOfPropertyChange(() => IsSelected);
      }
    }

    public double Left
    {
      get { return HorizontalStart + HorizontalDistanceStackedEntry * m_StackLevel; }
    }


    public int StackLevel
    {
      get { return m_StackLevel; }
      set
      {
        if (m_StackLevel == value)
        {
          return;
        }
        m_StackLevel = value;
        NotifyOfPropertyChange(() => StackLevel);
      }
    }

    public string Subject
    {
      get { return m_Entry.Subject; }
    }

    public double Top
    {
      get { return m_Top; }
      set
      {
        if (Math.Abs(m_Top - value) < double.Epsilon)
        {
          return;
        }
        m_Top = value;
        NotifyOfPropertyChange(() => Top);
      }
    }

    public double Width
    {
      get { return m_Width; }
      set
      {
        if (Math.Abs(m_Width - value) < double.Epsilon)
        {
          return;
        }
        m_Width = value;
        NotifyOfPropertyChange(() => Width);
      }
    }

    private void ModelChange(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "Color")
      {
        NotifyOfPropertyChange(() => Color);
      }
    }
  }
}