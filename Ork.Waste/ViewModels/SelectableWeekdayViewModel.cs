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
using Caliburn.Micro;
using Ork.Framework;

namespace Ork.Waste.ViewModels
{
  public class SelectableWeekdayViewModel : PropertyChangedBase
  {
    private DayOfWeek m_DayOfWeek;

    private bool m_IsSelected;


    public SelectableWeekdayViewModel(DayOfWeek dayOfWeek, bool isSelected)
    {
      m_DayOfWeek = dayOfWeek;
      m_IsSelected = isSelected;
    }


    public DayOfWeek DayOfWeek
    {
      get { return m_DayOfWeek; }
      set
      {
        if (m_DayOfWeek == value)
        {
          return;
        }
        m_DayOfWeek = value;
        NotifyOfPropertyChange(() => DayOfWeek);
      }
    }

    public string DayOfWeekString
    {
      get
      {
        switch ((int) m_DayOfWeek)
        {
          case 0:
            return TranslationProvider.Translate("SundayShort");
          case 1:
            return TranslationProvider.Translate("MondayShort");
          case 2:
            return TranslationProvider.Translate("TuesdayShort");
          case 3:
            return TranslationProvider.Translate("WednesdayShort");
          case 4:
            return TranslationProvider.Translate("ThursdayShort");
          case 5:
            return TranslationProvider.Translate("FridayShort");
          default:
            return TranslationProvider.Translate("SaturdayShort");
        }
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
  }
}