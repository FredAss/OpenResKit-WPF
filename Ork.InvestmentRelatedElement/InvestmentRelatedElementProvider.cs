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
using System.Linq;
using Ork.Framework;
using Ork.InvestmentRelatedElement.DomainModelService;
using Ork.Measure;
using Ork.Measure.ViewModels;

namespace Ork.InvestmentRelatedElement
{
  [Export(typeof (IRelatedElementProvider))]
  internal class InvestmentRelatedElementProvider : IRelatedElementProvider
  {
    private readonly IInvestmentRelatedElementRepository m_Repository;

    [ImportingConstructor]
    public InvestmentRelatedElementProvider([Import] IInvestmentRelatedElementRepository repository)
    {
      m_Repository = repository;
    }

    public RelatedElementProviderViewModel CreateViewModel()
    {
      var elements = CreateRelatedElementViewModels(m_Repository.Comparisons);
      return new RelatedElementProviderViewModel(TranslationProvider.Translate("Comparison"), elements, this);
    }

    public RelatedElementProviderViewModel GetViewModel(int measureId)
    {
      var elements = CreateRelatedElementViewModels(m_Repository.Comparisons)
        .ToArray();
      var rvm = new RelatedElementProviderViewModel(TranslationProvider.Translate("Comparison"), elements, this);
      var existingRelatedElement = m_Repository.RelatedElements.SingleOrDefault(re => re.Measure.Id == measureId);
      if (existingRelatedElement != null)
      {
        elements.Single(vm => vm.Model == existingRelatedElement.Comparison)
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
      var meter = (Comparison) relatedElement;
      var existingRelatedElement = m_Repository.RelatedElements.SingleOrDefault(re => re.Measure.Id == measureId);
      if (existingRelatedElement != null)
      {
        if (existingRelatedElement.Comparison != meter)
        {
          existingRelatedElement.Comparison = meter;
          m_Repository.Save();
        }
      }
      else
      {
        var measure = m_Repository.GetMeasure(measureId);
        var re = new DomainModelService.InvestmentRelatedElement()
                 {
                   Comparison = meter,
                   Measure = measure
                 };
        m_Repository.RelatedElements.Add(re);
        m_Repository.Save();
      }
    }

    private IEnumerable<RelatedElementViewModel> CreateRelatedElementViewModels(IEnumerable<Comparison> meters)
    {
      return meters.Select(CreateRelatedElementViewModel);
    }

    private RelatedElementViewModel CreateRelatedElementViewModel(Comparison element)
    {
      return new RelatedElementViewModel(element, string.Format("{0} ({1} €)", element.ComparisonName, element.InvestmentSum));
    }
  }
}