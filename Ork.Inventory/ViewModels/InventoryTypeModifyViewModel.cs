using Ork.Inventory.DomainModelService;

namespace Ork.Inventory.ViewModels
{
    public class InventoryTypeModifyViewModel
    {
        private readonly InventoryType m_Model;


        public InventoryType Model
        {
            get { return m_Model; }
        }

        public string Name
        {
            get { return m_Model.Name; }
            set { m_Model.Name = value; }
        }

        public InventoryTypeModifyViewModel(InventoryType inventoryType)
        {
            m_Model = inventoryType;
        }
    }
}