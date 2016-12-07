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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Ork.Framework;
using Ork.Map.ViewModels;
using Ork.Waste.AVVCatalog;
using Ork.Waste.DomainModelService;

namespace Ork.Waste.ViewModels
{
  public class ContainerViewModel : Screen
  {
    private readonly IAvvWasteTypeProvider m_AvvWasteTypeProvider;
    private readonly MapItemViewModel m_ContainerPosition;
    private readonly WasteContainer m_Model;
    private int m_MapIndicator;

    public ContainerViewModel(WasteContainer model, IAvvWasteTypeProvider avvWasteTypeProvider)
    {
      m_Model = model;
      m_Model.PropertyChanged += ModelPropertyChanged;
      m_Model.MapPosition.PropertyChanged += PositionChanged;
      m_Model.WasteTypes.CollectionChanged += WasteTypesOnCollectionChanged;
      DisplayName = TranslationProvider.Translate("EditContainer");
      m_AvvWasteTypeProvider = avvWasteTypeProvider;
      SelectedAvvWasteTypes = new ObservableCollection<AvvWasteType>();
      if (m_AvvWasteTypeProvider.AvvWasteTypes != null)
      {
        SetSelectedAvvWasteType();
      }
      else
      {
        m_AvvWasteTypeProvider.Loaded += (s, e) => SetSelectedAvvWasteType();
      }

      m_ContainerPosition = new MapItemViewModel(model.Name, new Point(model.MapPosition.XPosition, model.MapPosition.YPosition), false);
    }

    public IEnumerable<AvvWasteType> AvvWasteTypeList
    {
      get { return m_AvvWasteTypeProvider.AvvWasteTypes; }
    }

    public IAvvWasteTypeProvider AvvWasteTypeProvider
    {
      get { return m_AvvWasteTypeProvider; }
    }

    public string Barcode
    {
      get { return m_Model.Barcode; }
      set { m_Model.Barcode = value; }
    }

    public MapItemViewModel ContainerPosition
    {
      get { return m_ContainerPosition; }
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
      get { return "Container"; }
    }

    public WasteContainer Model
    {
      get { return m_Model; }
    }

    public string Name
    {
      get { return m_Model.Name; }
      set { m_Model.Name = value; }
    }

    public ObservableCollection<AvvWasteType> SelectedAvvWasteTypes { get; private set; }

    public double Size
    {
      get { return m_Model.Size; }
      set { m_Model.Size = Math.Round(value, 2); }
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
        ContainerPosition.Location = new Point(m_Model.MapPosition.XPosition, m_Model.MapPosition.YPosition);
      }
    }

    private void SetSelectedAvvWasteType()
    {
      SelectedAvvWasteTypes.Clear();
      foreach (var wasteType in m_Model.WasteTypes)
      {
        SelectedAvvWasteTypes.Add(AvvWasteTypeList.Single(avvWt => avvWt.Id == wasteType.AvvId));
      }
    }

    private void WasteTypesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
    {
      SetSelectedAvvWasteType();
      NotifyOfPropertyChange(() => SelectedAvvWasteTypes);
    }
  }
}