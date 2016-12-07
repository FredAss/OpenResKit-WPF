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

using System.ComponentModel.Composition;
using Ork.Inventory.DomainModelService;
using Ork.Inventory.ViewModels;
using System;
using System.Collections.Generic;

namespace Ork.Inventory.Factories
{
    [Export(typeof (IInventoryViewModelFactory))]
    internal class InventoryViewModelFactory : IInventoryViewModelFactory
    {
        private readonly IInventoryRepository m_Repository;

        [ImportingConstructor]
        public InventoryViewModelFactory([Import] IInventoryRepository contextRepository)
        {
            m_Repository = contextRepository;
        }

        public RoomViewModel CreateFromExisting(BuildingRoom room)
        {
            return new RoomViewModel(room);
        }

        public InventoryViewModel CreateFromExisting(DomainModelService.Inventory inventory)
        {
            return new InventoryViewModel(inventory);
        }

        public InventoryTypeViewModel CreateFromExisting(InventoryType inventoryType)
        {
            return new InventoryTypeViewModel(inventoryType);
        }

        public InventoryTypeAddViewModel CreateInventoryTypeAddViewModel()
        {
            return new InventoryTypeAddViewModel(new InventoryType());
        }

        public InventoryAddViewModel CreateInventoryAddViewModel(RoomViewModel room, InventoryTypeViewModel inventoryType, IEnumerable<InventoryTypeViewModel> inventoryTypes, IEnumerable<RoomViewModel> rooms)
        {
            return new InventoryAddViewModel(new DomainModelService.Inventory
            {
                Room = room != null ? room.Model : null,
                InventoryType = inventoryType != null ? inventoryType.Model : null
            }, inventoryTypes, rooms);
        }

        public InventoryTypeEditViewModel CreateInventoryTypeEditViewModel(InventoryTypeViewModel inventoryType, System.Action removeInventoryTypeAction)
        {
            return new InventoryTypeEditViewModel(inventoryType, removeInventoryTypeAction);
        }

        public InventoryEditViewModel CreateInventoryEditViewModel(InventoryViewModel inventory, System.Action removeInventoryAction, IEnumerable<InventoryTypeViewModel> inventoryTypes, IEnumerable<RoomViewModel> rooms)
        {
            return new InventoryEditViewModel(inventory, removeInventoryAction, inventoryTypes, rooms);
        }
    }
}