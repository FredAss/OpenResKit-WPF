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

namespace Ork.Meter.ViewModels
{
  public class MeterEditViewModel : MeterAddViewModel
  {
    private readonly Action m_RemoveMeter;

    public MeterEditViewModel(DomainModelService.Meter model, Action removeMeterAction)
      : base(model)
    {
      m_RemoveMeter = removeMeterAction;
      DisplayName = TranslationProvider.Translate("EditMeter");
    }

    public void RemoveMeter()
    {
      m_RemoveMeter();
    }
  }
}