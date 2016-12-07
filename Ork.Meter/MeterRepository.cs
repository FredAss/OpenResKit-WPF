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
using Ork.Meter.DomainModelService;
using Ork.Setting;

namespace Ork.Meter
{
  [Export(typeof (IMeterRepository))]
  internal class MeterRepository : IMeterRepository
  {
    private readonly Func<DomainModelContext> m_CreateMethod;
    private DomainModelContext m_Context;

    [ImportingConstructor]
    public MeterRepository([Import] ISettingsProvider settingsMeter, [Import] Func<DomainModelContext> createMethod)
    {
      m_CreateMethod = createMethod;
      settingsMeter.ConnectionStringUpdated += (s, e) => Initialize();
      Initialize();
    }

    public DataServiceCollection<ScheduledTask> MeterReadings { get; private set; }

    public DataServiceCollection<DomainModelService.Meter> Meter { get; private set; }

    public IEnumerable<EntityDescriptor> Entities
    {
      get { return m_Context.Entities; }
    }

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
                                                                        }, m_Context);
    }

    private void Initialize()
    {
      m_Context = m_CreateMethod();

      try
      {
        LoadMeters();
        LoadMaps();
        LoadResponsibleSubjects();
        LoadSeries();
        LoadMeterReadings();
        HasConnection = true;
      }
      catch (Exception)
      {
        HasConnection = false;
      }

      RaiseEvent(ContextChanged);
    }

    private void LoadMeters()
    {
      Meter = new DataServiceCollection<DomainModelService.Meter>(m_Context);

      var query = m_Context.Meters.Expand("MapPosition/Map");
      Meter.Load(query);
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

    private void LoadMeterReadings()
    {
      MeterReadings = new DataServiceCollection<ScheduledTask>(m_Context);
      var query = m_Context.ScheduledTasks.Expand("DueDate")
                           .Expand("EntryDate")
                           .Expand("OpenResKit.DomainModel.MeterReading/ReadingMeter/MapPosition/Map")
                           .Expand("AppointmentResponsibleSubject")
                           .Expand("OpenResKit.DomainModel.MeterReading/RelatedSeries/SeriesColor")
                           .Expand("OpenResKit.DomainModel.MeterReading/RelatedSeries/WeekDays")
                           .Expand("EntryResponsibleSubject");

      MeterReadings.Load(query);
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