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
using Ork.Inventory.DomainModelService;
using Ork.Setting;

namespace Ork.Inventory
{
    [Export(typeof (IInventoryRepository))]
    internal class InventoryRepository : IInventoryRepository
    {
        private readonly Func<DomainModelContext> m_CreateMethod;
        private DomainModelContext m_Context;

        [ImportingConstructor]
        public InventoryRepository([Import] ISettingsProvider settingsContainer, [Import] Func<DomainModelContext> createMethod)
        {
            m_CreateMethod = createMethod;
            settingsContainer.ConnectionStringUpdated += (s, e) => Initialize();
            Initialize();
        }

        public bool HasConnection { get; private set; }

        public DataServiceCollection<BuildingRoom> Rooms { get; private set; }
        public DataServiceCollection<InventoryType> InventoryTypes { get; private set; }
        public DataServiceCollection<DomainModelService.Inventory> Inventories { get; private set; }

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

            ReloadDataFromServer();

            RaiseEvent(ContextChanged);
        }

        public void ReloadDataFromServer()
        {
            try
            {
                LoadRooms();
                LoadInventories();
                LoadInventoryTypes();

                HasConnection = true;
            }
            catch (Exception e)
            {
                HasConnection = false;
            }
        }

        private void LoadRooms()
        {
            Rooms = new DataServiceCollection<BuildingRoom>(m_Context);
            var query = m_Context.BuildingRooms.Expand("Inventories, Building");
            Rooms.Load(query);
        }

        private void LoadInventoryTypes()
        {
            InventoryTypes = new DataServiceCollection<InventoryType>(m_Context);
            var query = m_Context.InventoryTypes.Expand("Inventories");
            InventoryTypes.Load(query);
        }

        private void LoadInventories()
        {
            Inventories = new DataServiceCollection<DomainModelService.Inventory>(m_Context);
            var query = m_Context.Inventories.Expand("Room/Building, InventoryType");
            Inventories.Load(query);
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