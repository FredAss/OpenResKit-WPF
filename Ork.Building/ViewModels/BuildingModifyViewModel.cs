namespace Ork.Building.ViewModels
{
    public class BuildingModifyViewModel
    {
        private readonly DomainModelService.Building m_Model;

        public string Description
        {
            get { return m_Model.Description; }
            set { m_Model.Description = value; }
        }

        public DomainModelService.Building Model
        {
            get { return m_Model; }
        }

        public string Name
        {
            get { return m_Model.Name; }
            set { m_Model.Name = value; }
        }

        public BuildingModifyViewModel(DomainModelService.Building building)
        {
            m_Model = building;
        }
    }
}