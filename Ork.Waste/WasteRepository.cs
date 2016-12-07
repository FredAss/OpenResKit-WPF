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
using System.Data.Services.Client;
using Ork.Setting;
using Ork.Waste.DomainModelService;

namespace Ork.Waste
{
  [Export(typeof (IWasteRepository))]
  internal class WasteRepository : IWasteRepository
  {
    private readonly Func<DomainModelContext> m_CreateMethod;
    private DomainModelContext m_Context;

    [ImportingConstructor]
    public WasteRepository([Import] ISettingsProvider settingsContainer, [Import] Func<DomainModelContext> createMethod)
    {
      m_CreateMethod = createMethod;
      settingsContainer.ConnectionStringUpdated += (s, e) => Initialize();
      Initialize();
    }

    public DataServiceCollection<WasteContainer> Container { get; private set; }
    public DataServiceCollection<Disposer> Disposer { get; private set; }
    public DataServiceCollection<WasteCollection> WasteCollections { get; private set; }

    public IEnumerable<EntityDescriptor> Entities
    {
      get { return m_Context.Entities; }
    }

    public DataServiceCollection<ScheduledTask> FillLevelReadings { get; private set; }
    public bool HasConnection { get; private set; }

    public IEnumerable<LinkDescriptor> Links
    {
      get { return m_Context.Links; }
    }

    public DataServiceCollection<DomainModelService.Map> Maps { get; private set; }
    public DataServiceCollection<ResponsibleSubject> ResponsibleSubjects { get; private set; }
    public DataServiceCollection<Series> Series { get; private set; }
    public event EventHandler ContextChanged;
    public event EventHandler SaveCompleted;

    public void DeleteObject(object objectToDelete)
    {
      m_Context.DeleteObject(objectToDelete);
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
                                                                          RaiseEvent(SaveCompleted);
                                                                          Initialize();
                                                                        }, m_Context);
    }

    public DomainModelContext Context
    {
      get { return m_Context; }
      private set { }
    }

    private void Initialize()
    {
      m_Context = m_CreateMethod();

      try
      {
        LoadContainers();
        LoadDisposer();
        LoadWasteCollections();
        LoadMaps();
        LoadResponsibleSubjects();
        LoadSeries();
        LoadFillLevelReadings();
        HasConnection = true;
      }
      catch (Exception)
      {
        HasConnection = false;
      }

      RaiseEvent(ContextChanged);
    }

    private void LoadDisposer()
    {
      Disposer = new DataServiceCollection<Disposer>(m_Context);

      var query = m_Context.Disposers.Expand("Containers")
                           .Expand("Documents");
      Disposer.Load(query);
    }


    private void LoadWasteCollections()
    {
      WasteCollections = new DataServiceCollection<WasteCollection>(m_Context);

      var query = m_Context.WasteCollections.Expand("Container")
                           .Expand("Disposer");
      WasteCollections.Load(query);
    }

    private void LoadContainers()
    {
      Container = new DataServiceCollection<WasteContainer>(m_Context);

      var query = m_Context.WasteContainers.Expand("WasteTypes")
                           .Expand("MapPosition/Map");
      Container.Load(query);
    }

    private void LoadFillLevelReadings()
    {
      FillLevelReadings = new DataServiceCollection<ScheduledTask>(m_Context);
      var query = m_Context.ScheduledTasks.Expand("DueDate")
                           .Expand("EntryDate")
                           .Expand("OpenResKit.DomainModel.FillLevelReading/ReadingContainer/MapPosition/Map")
                           .Expand("OpenResKit.DomainModel.FillLevelReading/ReadingContainer/WasteTypes")
                           .Expand("AppointmentResponsibleSubject")
                           .Expand("OpenResKit.DomainModel.FillLevelReading/RelatedSeries/SeriesColor")
                           .Expand("OpenResKit.DomainModel.FillLevelReading/RelatedSeries/WeekDays")
                           .Expand("EntryResponsibleSubject");
      FillLevelReadings.Load(query);
    }

    private void LoadMaps()
    {
      Maps = new DataServiceCollection<DomainModelService.Map>(m_Context);

      var query = m_Context.Maps.Expand("MapSource");
      Maps.Load(query);
    }

    private void LoadResponsibleSubjects()
    {
      ResponsibleSubjects = new DataServiceCollection<ResponsibleSubject>(m_Context);

      var query = m_Context.ResponsibleSubjects.Expand("OpenResKit.DomainModel.Employee/Groups");
      ResponsibleSubjects.Load(query);
    }

    private void LoadSeries()
    {
      Series = new DataServiceCollection<Series>(m_Context);

      var query = m_Context.Series;

      Series.Load(query);
    }


    private void RaiseEvent(EventHandler eventHandler)
    {
      if (eventHandler != null)
      {
        eventHandler(this, new EventArgs());
      }
    }
  }
}