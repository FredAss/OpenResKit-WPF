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
using Ork.Energy.Domain.DomainModelService;
using Ork.Energy.Factories;

namespace Ork.Energy.ViewModels
{
  public class MeasurePrintPreviewViewModel : MeasureEditViewModel
  {
    [ImportingConstructor]
    public MeasurePrintPreviewViewModel(EnergyMeasure model, Action removeMeasureAction, ResponsibleSubjectViewModel[] responsibleSubjectViewModels, [Import] IEnergyRepository energyRepository,
      [Import] IViewModelFactory subMeasureViewModelFactory)
      : base(model, removeMeasureAction, responsibleSubjectViewModels, energyRepository, subMeasureViewModelFactory)
    {
      SelectedResponsibleSubject = responsibleSubjectViewModels.Single(rsvm => model.ResponsibleSubject == rsvm.Model);
    }
  }
}