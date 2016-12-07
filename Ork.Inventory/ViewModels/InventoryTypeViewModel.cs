using System.Data.Services.Client;
using Ork.Inventory.DomainModelService;

namespace Ork.Inventory.ViewModels
{
    public class InventoryTypeViewModel
    {
        private readonly InventoryType m_Model;

        public InventoryTypeViewModel(InventoryType inventoryType)
        {
            m_Model = inventoryType;
        }

        public InventoryType Model
        {
            get { return m_Model; }
        }

        public string Name
        {
            get { return m_Model.Name; }
            set { m_Model.Name = value; }
        }

        public DataServiceCollection<DomainModelService.Inventory> Inventories
        {
            get { return m_Model.Inventories; }
        }

    }
}