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
using Ork.Building.DomainModelService;
using Ork.Building.ViewModels;
using System.Collections.Generic;
using Caliburn.Micro;

namespace Ork.Building.Factories
{
  public interface IBuildingViewModelFactory
  {
      BuildingViewModel CreateFromExisting(DomainModelService.Building building);
      RoomViewModel CreateFromExisting(BuildingRoom room, BindableCollection<BuildingViewModel> buildings);
      AddressViewModel CreateFromExisting(Address address);
      BuildingAddViewModel CreateBuildingAddViewModel(IEnumerable<AddressViewModel> addresses);
      BuildingEditViewModel CreateBuildingEditViewModel(BuildingViewModel buildingViewModel, IEnumerable<AddressViewModel> addresses, System.Action removeBuildingAction);
  }
}