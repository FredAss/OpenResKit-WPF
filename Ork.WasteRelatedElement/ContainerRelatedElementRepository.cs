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
using System.Data.Services.Client;
using System.Linq;
using Ork.Setting;
using Ork.WasteContainerRelatedElement.DomainModelService;

namespace Ork.WasteContainerRelatedElement
{
  [Export(typeof (IContainerRelatedElementRepository))]
  internal class ContainerRelatedElementRepository : IContainerRelatedElementRepository
  {
    private readonly Func<DomainModelContext> m_CreateMethod;
    private DomainModelContext m_Context;

    [ImportingConstructor]
    public ContainerRelatedElementRepository([Import] ISettingsProvider settingsContainer, [Import] Func<DomainModelContext> createMethod)
    {
      m_CreateMethod = createMethod;
      settingsContainer.ConnectionStringUpdated += (s, e) => Initialize();
      Initialize();
    }

    public DataServiceCollection<ContainerRelatedElement> RelatedElements { get; private set; }

    public DomainModelService.Measure GetMeasure(int measureId)
    {
      var query = m_Context.Measures.Where(m => m.Id == measureId);
      return ((DataServiceQuery<DomainModelService.Measure>) query).Execute()
                                                                   .First();
    }

    public void Save()
    {
      if (m_Context.ApplyingChanges)
      {
        return;
      }

      var result = m_Context.BeginSaveChanges(SaveChangesOptions.Batch, r =>
                                                                        {
                                                                          var dm = (DomainModelContext) r.AsyncState;
                                                                          dm.EndSaveChanges(r);
                                                                        }, m_Context);
    }

    public DataServiceCollection<WasteContainer> WasteContainers { get; private set; }

    private void Initialize()
    {
      m_Context = m_CreateMethod();
      try
      {
        LoadWasteContainerRelatedElements();
        LoadWasteContainers();
      }
      catch (Exception)
      {
      }
    }

    private void LoadWasteContainerRelatedElements()
    {
      RelatedElements = new DataServiceCollection<ContainerRelatedElement>(m_Context);

      var query = m_Context.ContainerRelatedElements.Expand(c => c.Measure)
                           .Expand(c => c.Container);

      RelatedElements.Load(query);
    }

    private void LoadWasteContainers()
    {
      WasteContainers = new DataServiceCollection<WasteContainer>(m_Context);

      var query = m_Context.WasteContainers;

      WasteContainers.Load(query);
    }
  }
}