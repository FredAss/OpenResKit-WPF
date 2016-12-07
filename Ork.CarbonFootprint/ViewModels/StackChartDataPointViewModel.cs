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

namespace Ork.CarbonFootprint.ViewModels
{
  public class StackChartDataPointViewModel : PropertyChangedBase
  {
    private readonly Func<string, TagColor> m_GetTagColor;
    private string m_Key;
    private double m_Sum;
    private string m_Tag;
    private TagColor m_TagColor;

    public StackChartDataPointViewModel(string key, double value, string tag, Func<string, TagColor> getTagColor)
    {
      m_GetTagColor = getTagColor;
      Key = key;
      Sum = value;
      Tag = tag;
    }

    public string Key
    {
      get { return m_Key; }
      set
      {
        if (m_Key == value)
        {
          return;
        }
        m_Key = value;
        NotifyOfPropertyChange(() => Key);
      }
    }

    public double Sum
    {
      get { return m_Sum; }
      set
      {
        if (m_Sum == value)
        {
          return;
        }
        m_Sum = value;
        NotifyOfPropertyChange(() => Sum);
      }
    }

    public string Tag
    {
      get { return m_Tag; }
      set
      {
        if (value == m_Tag)
        {
          return;
        }
        m_Tag = value;
        TagColor = m_GetTagColor(m_Tag);
        NotifyOfPropertyChange(() => Tag);
      }
    }

    public TagColor TagColor
    {
      get { return m_TagColor; }
      set
      {
        if (m_TagColor == value)
        {
          return;
        }
        m_TagColor = value;
        NotifyOfPropertyChange(() => TagColor);
      }
    }
  }
}