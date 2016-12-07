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
using System.Windows.Media;
using Caliburn.Micro;

namespace Ork.Calendar.Models
{
  public class CalendarEntry : PropertyChangedBase
  {
    private Brush m_Color;

    public CalendarEntry(DateTime startTime, DateTime endTime, string subject, Color color, bool isAllDay = false, object tag = null)
    {
      StartTime = startTime;
      EndTime = endTime;
      Subject = subject;
      IsAllDay = isAllDay;
      Tag = tag;
      Color = new SolidColorBrush(color);
    }

    public Brush Color
    {
      get { return m_Color; }
      set
      {
        if (m_Color == value)
        {
          return;
        }
        m_Color = value;
        NotifyOfPropertyChange(() => Color);
      }
    }

    public TimeSpan Duration
    {
      get { return EndTime - StartTime; }
    }

    public DateTime EndTime { get; private set; }
    public bool IsAllDay { get; private set; }
    public DateTime StartTime { get; private set; }
    public object Tag { get; private set; }
    public string Subject { get; private set; }
  }
}