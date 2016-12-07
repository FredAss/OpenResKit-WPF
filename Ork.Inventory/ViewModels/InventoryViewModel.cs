using Ork.Framework;

namespace Ork.Inventory.ViewModels
{
    public class InventoryViewModel : DocumentBase
    {
        private readonly DomainModelService.Inventory m_Model;

        public InventoryViewModel(DomainModelService.Inventory inventory)
        {
            m_Model = inventory;
        }

        public DomainModelService.Inventory Model
        {
            get { return m_Model; }
        }

        public string Name
        {
            get { return m_Model.Name; }
            set { m_Model.Name = value; }
        }

        public string Description
        {
            get { return m_Model.Description; }
            set { m_Model.Description = value; }
        }

        public string InventoryId
        {
            get { return m_Model.InventoryId; }
            set { m_Model.InventoryId = value; }
        }

        public string RoomName
        {
            get { return m_Model.Room != null ? m_Model.Room.Name : TranslationProvider.Translate("NotSelected"); }
        }

        public string TypeName
        {
            get { return m_Model.InventoryType != null ? m_Model.InventoryType.Name : TranslationProvider.Translate("NoType"); }
        }
    }
}