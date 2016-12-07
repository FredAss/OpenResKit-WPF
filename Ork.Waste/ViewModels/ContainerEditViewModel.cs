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
using Ork.Framework;
using Ork.Waste.DomainModelService;

namespace Ork.Waste.ViewModels
{
  public class ContainerEditViewModel : ContainerAddViewModel
  {
    private readonly Action m_RemoveContainer;

    public ContainerEditViewModel(WasteContainer model, Action removeContainerAction, IAvvWasteTypeProvider avvWasteTypeProvider)
      : base(model, avvWasteTypeProvider)
    {
      m_RemoveContainer = removeContainerAction;
      DisplayName = TranslationProvider.Translate("TitleContainerEditViewModel");
    }

    public void RemoveContainer()
    {
      m_RemoveContainer();
    }
  }
}