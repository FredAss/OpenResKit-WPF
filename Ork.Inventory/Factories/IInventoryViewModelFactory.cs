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

using Ork.Inventory.DomainModelService;
using Ork.Inventory.ViewModels;
using System;
using System.Collections.Generic;

namespace Ork.Inventory.Factories
{
  public interface IInventoryViewModelFactory
  {
      RoomViewModel CreateFromExisting(BuildingRoom room);
      InventoryViewModel CreateFromExisting(DomainModelService.Inventory inventory);
      InventoryTypeViewModel CreateFromExisting(InventoryType inventoryType);
      InventoryTypeAddViewModel CreateInventoryTypeAddViewModel();
      InventoryAddViewModel CreateInventoryAddViewModel(RoomViewModel room, InventoryTypeViewModel inventoryType, IEnumerable<InventoryTypeViewModel> inventoryTypes, IEnumerable<RoomViewModel> rooms);
      InventoryTypeEditViewModel CreateInventoryTypeEditViewModel(InventoryTypeViewModel inventoryType, System.Action removeInventoryTypeAction);
      InventoryEditViewModel CreateInventoryEditViewModel(InventoryViewModel inventory, System.Action removeInventoryAction, IEnumerable<InventoryTypeViewModel> inventoryTypes, IEnumerable<RoomViewModel> rooms);
  }
}