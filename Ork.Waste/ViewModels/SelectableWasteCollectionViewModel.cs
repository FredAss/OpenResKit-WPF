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
using Ork.Framework.ViewModels;
using Ork.Waste.DomainModelService;

namespace Ork.Waste.ViewModels
{
  public class SelectableWasteCollectionViewModel : SelectableItemViewModel
  {
    private readonly WasteCollection m_Model;

    public SelectableWasteCollectionViewModel(WasteCollection wasteCollection)
      : base(false)
    {
      m_Model = wasteCollection;
    }

    public SelectableWasteCollectionViewModel(bool isSelected)
      : base(isSelected)
    {
    }

    public WasteCollection Model
    {
      get { return m_Model; }
    }

    public Disposer Disposer
    {
      get { return m_Model.Disposer; }
    }

    public string ContainerText
    {
      get { return m_Model.Container.Barcode + " " + m_Model.Container.Name; }
    }

    public DateTime ScheduledDate
    {
      get { return m_Model.ScheduledDate; }
    }

    public double DesiredState
    {
      get { return m_Model.DesiredState; }
      set
      {
        m_Model.DesiredState = value;
        NotifyOfPropertyChange(() => DesiredState);
      }
    }

    public double DesiredPrice
    {
      get { return m_Model.DesiredPrice; }
      set
      {
        m_Model.DesiredPrice = value;
        NotifyOfPropertyChange(() => DesiredPrice);
      }
    }

    public double ActualPrice
    {
      get { return m_Model.ActualPrice; }
      set
      {
        m_Model.ActualPrice = value;
        NotifyOfPropertyChange(() => ActualPrice);
      }
    }

    public double ActualState
    {
      get { return m_Model.ActualState; }
      set
      {
        m_Model.ActualState = value;
        NotifyOfPropertyChange(() => ActualState);
      }
    }
  }
}