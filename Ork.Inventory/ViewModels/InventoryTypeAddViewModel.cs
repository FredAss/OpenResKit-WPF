using System.Collections.Generic;
using Caliburn.Micro;
using Ork.Framework;
using Ork.Inventory.DomainModelService;

namespace Ork.Inventory.ViewModels
{
    public class InventoryTypeAddViewModel : Screen
    {
        private readonly InventoryTypeModifyViewModel m_Model;

        public InventoryTypeAddViewModel(InventoryType model)
        {
            DisplayName = TranslationProvider.Translate("AddInventoryType");
            m_Model = new InventoryTypeModifyViewModel(model);
        }

        public InventoryType Model
        {
            get { return m_Model.Model; }
        }

        public string Name
        {
            get { return m_Model.Name; }
            set
            {
                m_Model.Name = value;
                NotifyOfPropertyChange(() => ValidateInventoryType);
            }
        }

        public bool ValidateInventoryType
        {
            get
            {
                if (!string.IsNullOrEmpty(Name))
                {
                    return true;
                }

                return false;
            }
        }

    }
}
