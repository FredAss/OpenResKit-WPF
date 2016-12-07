using System;
using System.Collections.Generic;
using Ork.Framework;

namespace Ork.Inventory.ViewModels
{
    public class InventoryEditViewModel : InventoryAddViewModel
    {
        private readonly Action m_RemoveInventory;

        public InventoryEditViewModel(InventoryViewModel model, Action removeInventoryAction,  IEnumerable<InventoryTypeViewModel> inventoryTypes, IEnumerable<RoomViewModel> rooms)
            : base(model.Model, inventoryTypes, rooms)
        {
            DisplayName = TranslationProvider.Translate("EditInventory");
            InventoryViewModel = model;
            m_RemoveInventory = removeInventoryAction;
        }


        public InventoryViewModel InventoryViewModel { get; set; }

        public void RemoveInventory()
        {
            m_RemoveInventory();
        }

        
    }
}