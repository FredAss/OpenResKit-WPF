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
  [Export(typeof (IDisposerViewModelFactory))]
  public class DisposerViewModelFactory : IDisposerViewModelFactory
  {
    private readonly IContainerViewModelFactory m_ContainerViewModelFactory;
    private readonly IWasteRepository m_WasteRepository;


    [ImportingConstructor]
    public DisposerViewModelFactory([Import] IWasteRepository wasteRepository, [Import] IContainerViewModelFactory containerViewModelFactory)
    {
      m_WasteRepository = wasteRepository;
      m_ContainerViewModelFactory = containerViewModelFactory;
    }

    public DisposerViewModel CreateDisposerViewModel(Disposer disposer)
    {
      return new DisposerViewModel(disposer);
    }

    public DisposerAddViewModel CreateDisposerAddViewModel(Disposer disposer)
    {
      return new DisposerAddViewModel(disposer, m_WasteRepository, m_ContainerViewModelFactory);
    }

    public DisposerEditViewModel CreateDisposerEditViewModel(Disposer disposer, Action removeContainerAction)
    {
      return new DisposerEditViewModel(disposer, m_WasteRepository, m_ContainerViewModelFactory, removeContainerAction);
    }
  }
}