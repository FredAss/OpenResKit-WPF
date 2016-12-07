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

using Caliburn.Micro;
using Ork.Waste.DomainModelService;

namespace Ork.Waste.ViewModels
{
  public class WasteCollectionModifyViewModel : PropertyChangedBase
  {
    private readonly WasteCollection m_Model;

    public WasteCollectionModifyViewModel(WasteCollection model)
    {
      m_Model = model;
    }

    public WasteCollection Model
    {
      get { return m_Model; }
      private set { }
    }

    public string ContainerText
    {
      get { return m_Model.Container.Barcode + " " + m_Model.Container.Name; }
    }

    public double DesiredState
    {
      get { return m_Model.DesiredState; }
      set { m_Model.DesiredState = value; }
    }

    public double DesiredPrice
    {
      get { return m_Model.DesiredPrice; }
      set { m_Model.DesiredPrice = value; }
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

    public double ActualPrice
    {
      get { return m_Model.ActualPrice; }
      set
      {
        m_Model.ActualPrice = value;
        NotifyOfPropertyChange(() => ActualPrice);
      }
    }
  }
}