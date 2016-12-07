using System.Data.Services.Client;
using Ork.Framework;
using Ork.Inventory.DomainModelService;

namespace Ork.Inventory.ViewModels
{
    public class RoomViewModel : DocumentBase
    {
        private readonly BuildingRoom m_Model;

        public RoomViewModel(BuildingRoom room)
        {
            m_Model = room;
        }

        public double Area
        {
            get { return m_Model.Area; }
            set
            {
                m_Model.Area = value;
                NotifyOfPropertyChange(() => Volume);
            }
        }

        public string BuildingName
        {
            get
            {
                return m_Model.Building != null ? m_Model.Building.Name : TranslationProvider.Translate("NoBuilding");
            }
        }

        public string Descriptione
        {
            get { return m_Model.Description; }
            set { m_Model.Description = value; }
        }

        public double Height
        {
            get { return m_Model.Height; }
            set
            {
                m_Model.Height = value;
                NotifyOfPropertyChange(() => Volume);
            }
        }

        public DataServiceCollection<DomainModelService.Inventory> Inventories
        {
            get { return m_Model.Inventories; }
        }

        public BuildingRoom Model
        {
            get { return m_Model; }
        }

        public string Name
        {
            get { return m_Model.Name; }
            set { m_Model.Name = value; }
        }

        public string NameWithBuilding
        {
            get
            {
                return m_Model.Building != null
                    ? m_Model.Building.Name + "." + m_Model.Name
                    : TranslationProvider.Translate("NotSelected");
            }
        }

        public string Volume
        {
            get { return (m_Model.Area*m_Model.Height).ToString(); }
        }
    }
}