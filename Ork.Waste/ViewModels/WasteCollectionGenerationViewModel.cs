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

using System.Collections.Generic;
using System.ComponentModel.Composition;
using Ork.Framework;
using Ork.Waste.DomainModelService;
using Ork.Waste.Factories;

namespace Ork.Waste.ViewModels
{
  public class WasteCollectionGenerationViewModel : LocalizableScreen
  {
    private readonly IWasteCollectionViewModelFactory m_WasteCollectionViewModelFactory;
    private readonly List<WasteCollectionModifyViewModel> m_WasteCollectionViewModels;

    [ImportingConstructor]
    public WasteCollectionGenerationViewModel(IEnumerable<WasteCollection> wasteCollections, [Import] IWasteCollectionViewModelFactory wasteCollectionViewModelFactory)
    {
      m_WasteCollectionViewModels = new List<WasteCollectionModifyViewModel>();
      m_WasteCollectionViewModelFactory = wasteCollectionViewModelFactory;
      foreach (var wasteCollection in wasteCollections)
      {
        m_WasteCollectionViewModels.Add(m_WasteCollectionViewModelFactory.CreateWasteCollectionViewModel(wasteCollection));
      }

      DisplayName = TranslationProvider.Translate("TitleWasteCollectionGenerationViewModel");
    }

    public IEnumerable<WasteCollectionModifyViewModel> WasteCollections
    {
      get { return m_WasteCollectionViewModels; }
      private set { }
    }
  }
}