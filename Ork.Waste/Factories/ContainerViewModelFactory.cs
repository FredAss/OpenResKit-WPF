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
using Ork.Waste.DomainModelService;
using Ork.Waste.ViewModels;

namespace Ork.Waste.Factories
{
  [Export(typeof (IContainerViewModelFactory))]
  internal class ContainerViewModelFactory : IContainerViewModelFactory
  {
    private readonly IAvvWasteTypeProvider m_AvvWasteTypeProvider;

    [ImportingConstructor]
    public ContainerViewModelFactory([Import] IAvvWasteTypeProvider avvWasteTypeProvider)
    {
      m_AvvWasteTypeProvider = avvWasteTypeProvider;
    }

    public ContainerAddViewModel CreateAddViewModel(MapViewModel selectedMap)
    {
      return new ContainerAddViewModel(CreateNewContainerModel(selectedMap), m_AvvWasteTypeProvider);
    }

    public ContainerEditViewModel CreateEditViewModel(WasteContainer model, Action removeContainerAction)
    {
      return new ContainerEditViewModel(model, removeContainerAction, m_AvvWasteTypeProvider);
    }

    public ContainerViewModel CreateFromExisiting(WasteContainer container)
    {
      return new ContainerViewModel(container, m_AvvWasteTypeProvider);
    }

    private WasteContainer CreateNewContainerModel(MapViewModel selectedMap)
    {
      var position = new MapPosition
                     {
                       XPosition = 0,
                       YPosition = 0
                     };
      if (selectedMap != null)
      {
        position.Map = selectedMap.Model;
      }
      var container = new WasteContainer
                      {
                        MapPosition = position
                      };
      return container;
    }
  }
}