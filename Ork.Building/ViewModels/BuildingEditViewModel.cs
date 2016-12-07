using System;
using System.Collections.Generic;
using System.Linq;
using Ork.Framework;

namespace Ork.Building.ViewModels
{
    public class BuildingEditViewModel : BuildingAddViewModel
    {
        private readonly Action m_RemoveBuilding;
        
        public BuildingEditViewModel(BuildingViewModel model, IEnumerable<AddressViewModel> addresses, Action removeBuildingAction)
            : base(model.Model, addresses)
        {
            DisplayName = TranslationProvider.Translate("EditBuilding");
            BuildingViewModel = model;
            m_RemoveBuilding = removeBuildingAction;
            
            SelectedAddress = addresses.FirstOrDefault(avm => avm.Model == model.Model.Address);
            //SelectedAddress = addresses.FirstOrDefault(avm => avm.Model.Buildings.FirstOrDefault(b => b == model.Model) != null);
        }


        public BuildingViewModel BuildingViewModel { get; set; }

        public void RemoveBuilding()
        {
            m_RemoveBuilding();
        }

        
    }
}