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

using System.Collections.Generic;
using System.ComponentModel.Composition;
using Caliburn.Micro;
using Ork.Building.DomainModelService;
using Ork.Building.ViewModels;

namespace Ork.Building.Factories
{
    [Export(typeof (IBuildingViewModelFactory))]
    internal class BuildingViewModelFactory : IBuildingViewModelFactory
    {
        private readonly IBuildingRepository m_Repository;

        [ImportingConstructor]
        public BuildingViewModelFactory([Import] IBuildingRepository contextRepository)
        {
            m_Repository = contextRepository;
        }

        public RoomViewModel CreateFromExisting(BuildingRoom room, BindableCollection<BuildingViewModel> buildings)
        {
            return new RoomViewModel(room, buildings);
        }

        public AddressViewModel CreateFromExisting(Address address)
        {
            return new AddressViewModel(address);
        }

        public BuildingViewModel CreateFromExisting(DomainModelService.Building building)
        {
            return new BuildingViewModel(building);
        }

        public BuildingAddViewModel CreateBuildingAddViewModel(IEnumerable<AddressViewModel> addresses)
        {
            return new BuildingAddViewModel(new DomainModelService.Building(), addresses);
        }

        public BuildingEditViewModel CreateBuildingEditViewModel(BuildingViewModel buildingViewModel, IEnumerable<AddressViewModel> addresses, System.Action removeBuildingAction)
        {
            return new BuildingEditViewModel(buildingViewModel, addresses, removeBuildingAction);
        }

    }
}