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
using System.Linq;
using Ork.Approval.DomainModelService;
using Ork.Setting;

namespace Ork.Approval
{
    [Export(typeof(IApprovalRepository))]
    internal class ApprovalRepository : IApprovalRepository
    {
        private readonly Func<DomainModelContext> m_CreateMethod;
        private DomainModelContext m_Context;

        [ImportingConstructor]
        public ApprovalRepository([Import] ISettingsProvider settingsContainer, [Import] Func<DomainModelContext> createMethod)
        {
            m_CreateMethod = createMethod;
            settingsContainer.ConnectionStringUpdated += (s, e) => Initialize();
            Initialize();
        }

        public DataServiceCollection<Plant> Plants { get; private set; }
        public DataServiceCollection<Permission> Permissions { get; set; }
        public DataServiceCollection<ScheduledTask> Inspections { get; set; } 
        public DataServiceCollection<AuxillaryCondition> AuxillaryConditions { get; set; } 
        public DataServiceCollection<ResponsibleSubject> ResponsibleSubjects { get; set; } 
        public DataServiceCollection<ConditionInspection> ConditionInspections { get; set; }
        public DataServiceCollection<Approval_Measure> Measures { get; set; }
        public DomainModelContext Context { get { return m_Context; } }

        public IEnumerable<LinkDescriptor> Links
        {
            get { return m_Context.Links; }
        }

        public IEnumerable<EntityDescriptor> Entities
        {
            get { return m_Context.Entities; }
        }

        public bool HasConnection { get; private set; }

        private void Initialize()
        {
            m_Context = m_CreateMethod();

            try
            {
                LoadPlants();
                LoadPermissions();
                LoadInspections();
                LoadAuxillaryConditions();
                LoadResponsibleSubjects();
                LoadConditionInspections();
                LoadMeasures();
            }
            catch(Exception ex)
            {
                HasConnection = false;
            }

            RaiseEvent(ContextChanged);
        }

        private void LoadPlants()
        {
            Plants = new DataServiceCollection<Plant>(m_Context);
            var query = Context.Plants.Expand("Permissions").Expand("PlantImageSource").Expand("AttachedDocuments/DocumentSource");
            Plants.Load(query);
        }

        private void LoadPermissions()
        {
            Permissions = new DataServiceCollection<Permission>(m_Context);
            var query = Context.Permissions.Expand("AuxillaryConditions").Expand("Plants").Expand("AttachedDocuments").Expand("AuxillaryConditions/ConditionInspections");
            Permissions.Load(query);
        }

        private void LoadInspections()
        {
            Inspections = new DataServiceCollection<ScheduledTask>(m_Context);
            var query = Context.ScheduledTasks
                .Expand("DueDate")
                .Expand("EntryDate")
                .Expand("AppointmentResponsibleSubject")
                .Expand("RelatedSeries/SeriesColor")
                .Expand("RelatedSeries/WeekDays")
                           .Expand("EntryResponsibleSubject")
                           .Expand("OpenResKit.DomainModel.Approval_Inspection/ConditionInspections")
                           .Expand("OpenResKit.DomainModel.Approval_Inspection/ConditionInspections/Measures");

            Inspections.Load(query);
        }

        private void LoadAuxillaryConditions()
        {
            AuxillaryConditions = new DataServiceCollection<AuxillaryCondition>(m_Context);
            var query = Context.AuxillaryConditions.Expand("ConditionInspections");
            AuxillaryConditions.Load(query);
        }

        private void LoadResponsibleSubjects()
        {
            ResponsibleSubjects = new DataServiceCollection<ResponsibleSubject>(m_Context);
            var query = m_Context.ResponsibleSubjects.Expand("OpenResKit.DomainModel.Employee/Groups");
            ResponsibleSubjects.Load(query);
        }

        private void LoadConditionInspections()
        {
            ConditionInspections = new DataServiceCollection<ConditionInspection>(m_Context);
            var query = m_Context.ConditionInspections.Expand("Measures").Expand("EntryDate");
            ConditionInspections.Load(query);
        }

        private void LoadMeasures()
        {
            Measures = new DataServiceCollection<Approval_Measure>(m_Context);
            var query = m_Context.Measures.Expand("AttachedDocuments").Expand("ResponsibleSubject").Expand("DueDate").Expand("EntryDate").OfType<Approval_Measure>(); ;
            Measures.Load(query);
        }

        public void Save()
        {
            if (m_Context.ApplyingChanges)
                return;

            var result = Context.BeginSaveChanges(SaveChangesOptions.Batch, r =>
                                                                        {
                                                                          var dm = (DomainModelContext) r.AsyncState;
                                                                          dm.EndSaveChanges(r);
                                                                          RaiseEvent(SaveCompleted);
                                                                        }, m_Context);
        }

        public void DeleteObject(object objectToDelete)
        {
            m_Context.DeleteObject(objectToDelete);
        }

        private void RaiseEvent(EventHandler eventHandler)
        {
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
        }

        public event EventHandler ContextChanged;

        public event EventHandler SaveCompleted;
    }
}
