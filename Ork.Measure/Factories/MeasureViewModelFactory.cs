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
using Ork.Measure.DomainModelService;
using Ork.Measure.ViewModels;

namespace Ork.Measure.Factories
{
  [Export(typeof (IMeasureViewModelFactory))]
  internal class MeasureViewModelFactory : IMeasureViewModelFactory
  {
    private readonly IMeasureRepository m_MeasureRepository;
    private readonly IRelatedElementProvider[] m_RelatedElementProviders;
    private readonly IResponsibleSubjectViewModelFactory m_ResponsibleSubjectViewModelFactory;

    [ImportingConstructor]
    public MeasureViewModelFactory([Import] IMeasureRepository measureRepository, [Import] IResponsibleSubjectViewModelFactory responsibleSubjectViewModelFactory,
      [ImportMany] IEnumerable<IRelatedElementProvider> relatedElementProvider)
    {
      m_MeasureRepository = measureRepository;
      m_ResponsibleSubjectViewModelFactory = responsibleSubjectViewModelFactory;
      m_RelatedElementProviders = relatedElementProvider.ToArray();
    }

    public MeasureViewModel CreateFromExisting(DomainModelService.Measure measure, CatalogViewModel catalog = null)
    {
      return new MeasureViewModel(measure, m_RelatedElementProviders, catalog);
    }

    public MeasureAddViewModel CreateAddViewModel()
    {
      return new MeasureAddViewModel(new DomainModelService.Measure(), CreateResponsibleSubjects(), m_RelatedElementProviders);
    }

    public MeasureEditViewModel CreateEditViewModel(DomainModelService.Measure measure, Action removeMeasureAction)
    {
      return new MeasureEditViewModel(measure, removeMeasureAction, CreateResponsibleSubjects(), m_RelatedElementProviders);
    }

    public CatalogAddViewModel CreateCatalogAddViewModel()
    {
      return new CatalogAddViewModel(new Catalog());
    }

    public CatalogEditViewModel CreateCatalogEditViewModel(CatalogViewModel catalogViewModel, Action removeCatalogAction)
    {
      return new CatalogEditViewModel(catalogViewModel, removeCatalogAction);
    }

    public CatalogViewModel CreateFromExisting(Catalog catalog)
    {
      return new CatalogViewModel(catalog);
    }

    private ResponsibleSubjectViewModel[] CreateResponsibleSubjects()
    {
      return m_MeasureRepository.ResponsibleSubjects.Select(m_ResponsibleSubjectViewModelFactory.CreateFromExisting)
                                .ToArray();
    }
  }
}