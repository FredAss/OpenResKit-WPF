using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Ork.Framework;

namespace Ork.Inventory.ViewModels
{
    public class InventoryAddViewModel : Screen
    {
        private readonly DomainModelService.Inventory m_Model;
        private InventoryTypeViewModel m_SelectedInventoryType;
        private RoomViewModel m_SelectedRoom;

        public InventoryAddViewModel(DomainModelService.Inventory model,
            IEnumerable<InventoryTypeViewModel> inventoryTypes, IEnumerable<RoomViewModel> rooms)
        {
            DisplayName = TranslationProvider.Translate("AddInventory");
            m_Model = model;
            Rooms = rooms;
            InventoryTypes = inventoryTypes;
            m_SelectedRoom = rooms.FirstOrDefault(i => i.Model == model.Room);
            m_SelectedInventoryType = inventoryTypes.FirstOrDefault(i => i.Model == model.InventoryType);
        }

        public string InventoryId
        {
            get { return m_Model.InventoryId; }
            set { m_Model.InventoryId = value; }
        }

        public string Description
        {
            get { return m_Model.Description; }
            set { m_Model.Description = value; }
        }

        public string Producer
        {
            get { return m_Model.Producer; }
            set { m_Model.Producer = value; }
        }

        public string YearOfManufacture
        {
            get { return m_Model.YearOfManufacture; }
            set { m_Model.YearOfManufacture = value; }
        }

        public IEnumerable<InventoryTypeViewModel> InventoryTypes { get; private set; }

        public DomainModelService.Inventory Model
        {
            get { return m_Model; }
        }

        public string Name
        {
            get { return m_Model.Name; }
            set
            {
                m_Model.Name = value;
                NotifyOfPropertyChange(() => ValidateInventory);
            }
        }

        public IEnumerable<RoomViewModel> Rooms { get; private set; }

        public InventoryTypeViewModel SelectedInventoryType
        {
            get { return m_SelectedInventoryType; }
            set
            {
                m_SelectedInventoryType = value;
                if (value != null)
                {
                    m_Model.InventoryType = value.Model;
                }
                NotifyOfPropertyChange(() => SelectedInventoryType);
            }
        }

        public RoomViewModel SelectedRoom
        {
            get { return m_SelectedRoom; }
            set
            {
                if (value != null)
                {
                    m_SelectedRoom = value;
                    m_Model.Room = value.Model;
                }
                NotifyOfPropertyChange(() => SelectedRoom);
            }
        }

        public bool ValidateInventory
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