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

using System.Windows;
using Ork.Framework;

namespace Ork.Meter.ViewModels
{
  public class MeterAddViewModel : MeterViewModel
  {
    private readonly DomainModelService.Meter m_Model;

    public MeterAddViewModel(DomainModelService.Meter model)
      : base(model)
    {
      DisplayName = TranslationProvider.Translate("AddMeter");
      m_Model = model;
    }

    public new DomainModelService.Map Map
    {
      get { return base.Map; }
      set
      {
        m_Model.MapPosition.Map = value;
        NotifyOfPropertyChange(() => MeterPosition);
      }
    }

    public double X
    {
      get { return m_Model.MapPosition.XPosition; }
      set
      {
        m_Model.MapPosition.XPosition = value;
        MeterPosition.Location = new Point(value, Y);
        NotifyOfPropertyChange(() => MeterPosition);
      }
    }

    public double Y
    {
      get { return m_Model.MapPosition.YPosition; }
      set
      {
        m_Model.MapPosition.YPosition = value;
        MeterPosition.Location = new Point(X, value);
        NotifyOfPropertyChange(() => MeterPosition);
      }
    }
  }
}