using System.Data.Services.Client;
using Ork.Building.DomainModelService;
using Ork.Framework;

namespace Ork.Building.ViewModels
{
    public class BuildingViewModel
    {
        private readonly DomainModelService.Building m_Model;

        public BuildingViewModel(DomainModelService.Building building)
        {
            m_Model = building;
        }

        public string Description
        {
            get { return m_Model.Description; }
        }

        public string LongAddress
        {
            get
            {
                return m_Model.Address != null ? m_Model.Address.Street + " " + m_Model.Address.Number + ", " + m_Model.Address.Zip + " " +
                    m_Model.Address.City : TranslationProvider.Translate("NoAddress");
            }
        }

        public DomainModelService.Building Model
        {
            get { return m_Model; }
        }

        public string Name
        {
            get { return m_Model.Name; }
        }

        public DataServiceCollection<BuildingRoom> Rooms
        {
            get { return m_Model.Rooms; }
        }
    }
}