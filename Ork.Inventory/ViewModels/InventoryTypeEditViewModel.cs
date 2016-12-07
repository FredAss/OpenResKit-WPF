using System;
using Ork.Framework;

namespace Ork.Inventory.ViewModels
{
    public class InventoryTypeEditViewModel : InventoryTypeAddViewModel
    {
        private readonly Action m_RemoveInventoryType;

        public InventoryTypeEditViewModel(InventoryTypeViewModel model, Action removeInventoryTypeAction)
            : base(model.Model)
        {
            DisplayName = TranslationProvider.Translate("EditInventoryType");
            InventoryTypeViewModel = model;
            m_RemoveInventoryType = removeInventoryTypeAction;
        }


        public InventoryTypeViewModel InventoryTypeViewModel { get; set; }

        public void RemoveInventoryType()
        {
            m_RemoveInventoryType();
        }

        
    }
}