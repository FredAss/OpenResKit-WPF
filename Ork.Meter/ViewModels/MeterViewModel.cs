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

using System.ComponentModel;
using System.Windows;
using Caliburn.Micro;
using Ork.Framework;
using Ork.Map.ViewModels;

namespace Ork.Meter.ViewModels
{
  public class MeterViewModel : Screen
  {
    private readonly MapItemViewModel m_MeterPosition;
    private readonly DomainModelService.Meter m_Model;
    private bool m_IsChecked;
    private int m_MapIndicator;

    public MeterViewModel(DomainModelService.Meter model)
    {
      m_Model = model;
      m_Model.PropertyChanged += ModelPropertyChanged;
      m_Model.MapPosition.PropertyChanged += PositionChanged;
      DisplayName = TranslationProvider.Translate("EditMeter");
      m_MeterPosition = new MapItemViewModel(model.Number, new Point(model.MapPosition.XPosition, model.MapPosition.YPosition), false);
    }

    public string Barcode
    {
      get { return m_Model.Barcode; }
      set { m_Model.Barcode = value; }
    }

    public string Name
    {
      get { return m_Model.Number; }
      set { m_Model.Number = value; }
    }

    public MapItemViewModel MeterPosition
    {
      get { return m_MeterPosition; }
    }

    public DomainModelService.Map Map
    {
      get { return m_Model.MapPosition.Map; }
    }

    public int MapIndicator
    {
      get { return m_MapIndicator; }
      set
      {
        if (value.Equals(m_MapIndicator))
        {
          return;
        }
        m_MapIndicator = value;
        NotifyOfPropertyChange(() => MapIndicator);
      }
    }

    public string TypeIndicator
    {
      get { return "Meter"; }
    }

    public DomainModelService.Meter Model
    {
      get { return m_Model; }
    }

    public string Number
    {
      get { return m_Model.Number; }
      set { m_Model.Number = value; }
    }

    public string Unit
    {
      get { return m_Model.Unit; }
      set { m_Model.Unit = value; }
    }

    public bool IsChecked
    {
      get { return m_IsChecked; }
      set
      {
        m_IsChecked = value;
        NotifyOfPropertyChange(() => IsChecked);
      }
    }


    private void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      NotifyOfPropertyChange(e.PropertyName);
    }

    private void PositionChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "Map")
      {
        NotifyOfPropertyChange(() => Map);
      }
      else
      {
        MeterPosition.Location = new Point(m_Model.MapPosition.XPosition, m_Model.MapPosition.YPosition);
      }
    }
  }
}