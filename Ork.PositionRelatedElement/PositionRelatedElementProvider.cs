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
using System.ComponentModel.Composition;
using System.Linq;
using Ork.Framework;
using Ork.Measure;
using Ork.Measure.ViewModels;
using Ork.PositionRelatedElement.DomainModelService;

namespace Ork.PositionRelatedElement
{
  [Export(typeof (IRelatedElementProvider))]
  internal class PositionRelatedElementProvider : IRelatedElementProvider
  {
    private readonly IPositionRelatedElementRepository m_Repository;

    [ImportingConstructor]
    public PositionRelatedElementProvider([Import] IPositionRelatedElementRepository repository)
    {
      m_Repository = repository;
    }

    public RelatedElementProviderViewModel CreateViewModel()
    {
      var elements = CreateRelatedElementViewModels(m_Repository.Positions);
      return new RelatedElementProviderViewModel(TranslationProvider.Translate("CarbonFootprintPosition"), elements, this);
    }

    public RelatedElementProviderViewModel GetViewModel(int measureId)
    {
      var elements = CreateRelatedElementViewModels(m_Repository.Positions)
        .ToArray();
      var rvm = new RelatedElementProviderViewModel(TranslationProvider.Translate("CarbonFootprintPosition"), elements, this);
      var existingRelatedElement = m_Repository.RelatedElements.SingleOrDefault(re => re.Measure.Id == measureId);
      if (existingRelatedElement != null)
      {
        elements.Single(vm => vm.Model == existingRelatedElement.Position)
                .IsSelected = true;
        rvm.IsExpanded = true;
      }

      return rvm;
    }

    public void DeleteRelatedElement(int measureId)
    {
      var existingRelatedElement = m_Repository.RelatedElements.SingleOrDefault(re => re.Measure.Id == measureId);
      if (existingRelatedElement != null)
      {
        m_Repository.RelatedElements.Remove(existingRelatedElement);
        m_Repository.Save();
      }
    }

    public void CreateRelatedElement(int measureId, object relatedElement)
    {
      var meter = (CarbonFootprintPosition) relatedElement;
      var existingRelatedElement = m_Repository.RelatedElements.SingleOrDefault(re => re.Measure.Id == measureId);
      if (existingRelatedElement != null)
      {
        if (existingRelatedElement.Position != meter)
        {
          existingRelatedElement.Position = meter;
          m_Repository.Save();
        }
      }
      else
      {
        var measure = m_Repository.GetMeasure(measureId);
        var re = new DomainModelService.PositionRelatedElement
                 {
                   Position = meter,
                   Measure = measure
                 };
        m_Repository.RelatedElements.Add(re);
        m_Repository.Save();
      }
    }

    private IEnumerable<RelatedElementViewModel> CreateRelatedElementViewModels(IEnumerable<CarbonFootprintPosition> meters)
    {
      return meters.Select(CreateRelatedElementViewModel);
    }


    private RelatedElementViewModel CreateRelatedElementViewModel(CarbonFootprintPosition element)
    {
      var formatedValueinKg = Math.Round(element.Calculation / 1000);
      return new RelatedElementViewModel(element, string.Format("{0} - {1} ({2} kg CO₂)", element.Name, element.Description, formatedValueinKg));
    }
  }
}