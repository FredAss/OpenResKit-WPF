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
using OxyPlot;

namespace Ork.CarbonFootprint
{
  public class TagColor : PropertyChangedBase
  {
    [NonSerialized]
    private SolidColorBrush m_Brush;

    private Color m_Color;

    [NonSerialized]
    private OxyColor m_OxyColor;

    private string m_Tag;

    public TagColor()
    {
      //has to stay here
    }

    public TagColor(string tag, Color color)
    {
      Tag = tag;
      Color = color;
    }

    public Color Color
    {
      get { return m_Color; }
      set
      {
        if (value.Equals(m_Color))
        {
          return;
        }
        m_Color = value;

        NotifyOfPropertyChange(() => Color);
        m_OxyColor = OxyColor.Parse(m_Color.ToString());
        NotifyOfPropertyChange(() => OxyColor);
        m_Brush = new SolidColorBrush(m_Color);
        NotifyOfPropertyChange(() => ColorBrush);
      }
    }

    public OxyColor OxyColor
    {
      get
      {
        if (m_OxyColor.IsUndefined())
        {
          m_OxyColor = OxyColor.Parse(m_Color.ToString());
        }
        return m_OxyColor;
      }
    }

    public SolidColorBrush ColorBrush
    {
      get
      {
        if (m_Brush == null)
        {
          m_Brush = new SolidColorBrush(m_Color);
        }
        return m_Brush;
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
        NotifyOfPropertyChange(() => Tag);
      }
    }
  }
}