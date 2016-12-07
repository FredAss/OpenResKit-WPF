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
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using Ork.Waste.DomainModelService;

namespace Ork.Waste.ViewModels
{
  public class WasteCollectionContainerViewModel : SelectableContainerViewModel
  {
    private readonly ContainerViewModel m_Model;
    private IWasteRepository m_WasteRepository;

    [ImportingConstructor]
    public WasteCollectionContainerViewModel(ContainerViewModel container, bool isSelected, IWasteRepository wasteRepository)
      : base(container, isSelected)
    {
      m_Model = container;
      m_WasteRepository = wasteRepository;

      LoadData();
    }

    public string Name
    {
      get { return m_Model.Name; }
    }

    public string Barcode
    {
      get { return m_Model.Barcode; }
    }

    public int PieChartAngleLastReading
    {
      get { return GetAngleFromFillLevel(ValueLastReading); }
    }

    public int PieChartAngleSecondLastReading
    {
      get { return GetAngleFromFillLevel(ValueSecondLastReading); }
    }

    public Visibility IconWarningVisibility
    {
      get
      {
        if (ValueLastReading == 0)
        {
          return Visibility.Visible;
        }

        return Visibility.Hidden;
      }
    }

    public Disposer Disposer { get; set; }

    public string DateLastWasteCollection { get; private set; }

    public float ValueLastReading { get; private set; }
    public string DateLastReading { get; private set; }
    public float ValueSecondLastReading { get; set; }
    public string DateSecondLastReading { get; private set; }
    public string AssumedWeight { get; set; }

    private int GetAngleFromFillLevel(float fillLevel)
    {
      switch (Convert.ToInt32(fillLevel))
      {
        case 25:
          return 90;
        case 50:
          return 180;
        case 75:
          return 270;
        case 100:
          return 360;
        default:
          return 0;
      }
    }

    private void LoadData()
    {
      var containerFillLevelReadings = m_WasteRepository.FillLevelReadings.Cast<FillLevelReading>()
                                                        .Where(flr => flr.ReadingContainer == ContainerViewModel.Model && flr.EntryDate != null)
                                                        .OrderBy(flr => flr.EntryDate.Begin);
      if (!containerFillLevelReadings.Any())
      {
        return;
      }

      var lastReading = containerFillLevelReadings.Last();


      ValueLastReading = lastReading.FillLevel * 100;
      DateLastReading = lastReading.EntryDate.Begin.ToShortDateString();

      var secondLastReading = containerFillLevelReadings.Reverse()
                                                        .Skip(1)
                                                        .First();

      ValueSecondLastReading = secondLastReading.FillLevel * 100;
      DateSecondLastReading = secondLastReading.EntryDate.Begin.ToShortDateString();

      var wasteCollection = m_WasteRepository.WasteCollections.Where(wc => wc.Container == ContainerViewModel.Model);

      if (!wasteCollection.Any())
      {
        return;
      }

      DateLastWasteCollection = wasteCollection.OrderBy(wc => wc.GenerationDate)
                                               .First()
                                               .GenerationDate.ToShortDateString();
    }
  }
}