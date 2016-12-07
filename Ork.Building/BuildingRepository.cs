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
using Ork.Building.DomainModelService;
using Ork.Setting;

namespace Ork.Building
{
    [Export(typeof (IBuildingRepository))]
    internal class BuildingRepository : IBuildingRepository
    {
        private readonly Func<DomainModelContext> m_CreateMethod;
        private DomainModelContext m_Context;

        [ImportingConstructor]
        public BuildingRepository([Import] ISettingsProvider settingsContainer, [Import] Func<DomainModelContext> createMethod)
        {
            m_CreateMethod = createMethod;
            settingsContainer.ConnectionStringUpdated += (s, e) => Initialize();
            Initialize();
        }

        public bool HasConnection { get; private set; }

        public DataServiceCollection<DomainModelService.Building> Buildings { get; private set; }
        public DataServiceCollection<BuildingRoom> Rooms { get; private set; }
        public DataServiceCollection<Address> Addresses { get; private set; }

        public event EventHandler ContextChanged;
        public event EventHandler SaveCompleted;

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
                LoadBuildings();
                LoadRooms();
                LoadAddresses();

                HasConnection = true;
            }
            catch (Exception e)
            {
                HasConnection = false;
            }

            RaiseEvent(ContextChanged);
        }

        private void LoadBuildings()
        {
            Buildings = new DataServiceCollection<DomainModelService.Building>(m_Context);
            var query = m_Context.Buildings.Expand("Rooms, Address");

            Buildings.Load(query);
        }

        private void LoadRooms()
        {
            Rooms = new DataServiceCollection<BuildingRoom>(m_Context);
            var query = m_Context.BuildingRooms.Expand("Building");
            Rooms.Load(query);
        }

        private void LoadAddresses()
        {
            Addresses = new DataServiceCollection<Address>(m_Context);
            var query = m_Context.Addresses.Expand(i => i.Buildings);
            Addresses.Load(query);
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