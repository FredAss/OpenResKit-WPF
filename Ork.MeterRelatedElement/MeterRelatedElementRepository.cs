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
using Ork.MeterRelatedElement.DomainModelService;
using Ork.Setting;

namespace Ork.MeterRelatedElement
{
  [Export(typeof (IMeterRelatedElementRepository))]
  internal class MeterRelatedElementRepository : IMeterRelatedElementRepository
  {
    private readonly Func<DomainModelContext> m_CreateMethod;
    private DomainModelContext m_Context;

    [ImportingConstructor]
    public MeterRelatedElementRepository([Import] ISettingsProvider settingsContainer, [Import] Func<DomainModelContext> createMethod)
    {
      m_CreateMethod = createMethod;
      settingsContainer.ConnectionStringUpdated += (s, e) => Initialize();
      Initialize();
    }

    public DataServiceCollection<DomainModelService.MeterRelatedElement> RelatedElements { get; private set; }

    public DataServiceCollection<Meter> Meters { get; private set; }

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

    public DomainModelService.Measure GetMeasure(int measureId)
    {
      var query = m_Context.Measures.Where(m => m.Id == measureId);
      return ((DataServiceQuery<DomainModelService.Measure>) query).Execute()
                                                                   .First();
    }

    private void Initialize()
    {
      m_Context = m_CreateMethod();
      try
      {
        LoadMeterRelatedElements();
        LoadMeter();
      }
      catch (Exception)
      {
      }
    }

    private void LoadMeterRelatedElements()
    {
      RelatedElements = new DataServiceCollection<DomainModelService.MeterRelatedElement>(m_Context);

      var query = m_Context.MeterRelatedElements.Expand(c => c.Measure)
                           .Expand(c => c.Meter);

      RelatedElements.Load(query);
    }

    private void LoadMeter()
    {
      Meters = new DataServiceCollection<Meter>(m_Context);

      var query = m_Context.Meters;

      Meters.Load(query);
    }
  }
}