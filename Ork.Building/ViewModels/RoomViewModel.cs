using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Ork.Building.DomainModelService;
using Ork.Framework;

namespace Ork.Building.ViewModels
{
    public class RoomViewModel : DocumentBase
    {
        private readonly BuildingRoom m_Model;
        private static BindableCollection<BuildingViewModel> m_Buildings;
        private BuildingViewModel m_SelectedBuilding;

        public RoomViewModel(BuildingRoom room, BindableCollection<BuildingViewModel> buildings)
        {
            m_Model = room;
            m_Buildings = buildings;
            m_SelectedBuilding = buildings.FirstOrDefault(i => i.Rooms.Contains(room));
        }

        public BuildingRoom Model
        {
            get { return m_Model; }
        }

        public static BindableCollection<BuildingViewModel> Buildings
        {
            get { return m_Buildings; }
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

        public double Area
        {
            get { return m_Model.Area; }
            set
            {
                m_Model.Area = value;
                NotifyOfPropertyChange(() => Volume);
            }
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

        public string Volume
        {
            get { return (m_Model.Area * m_Model.Height).ToString(); }
        }

        public BuildingViewModel Building
        {
            get
            {
                return m_Buildings.FirstOrDefault(i => i.Model == m_Model.Building);
            }
            set
            {
                if (value != null)
                {
                    m_SelectedBuilding = value;
                    m_Model.Building = value.Model;
                }
                NotifyOfPropertyChange(()=> Building);
            }
        }
    }
}