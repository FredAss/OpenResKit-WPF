using System.Collections.Generic;
using Caliburn.Micro;
using Ork.Building.DomainModelService;
using Ork.Framework;
using Action = System.Action;

namespace Ork.Building.ViewModels
{
    public class BuildingAddViewModel : Screen
    {
        private readonly BuildingModifyViewModel m_Model;
        private readonly IEnumerable<AddressViewModel> m_Addresses;
        private AddressViewModel m_SelectedAddress;

        public BuildingAddViewModel(DomainModelService.Building model, IEnumerable<AddressViewModel> addresses)
        {
            DisplayName = TranslationProvider.Translate("AddBuilding");
            m_Model = new BuildingModifyViewModel(model);
            m_Addresses = addresses;
        }

        public DomainModelService.Building Model
        {
            get { return m_Model.Model; }
        }

        public IEnumerable<AddressViewModel> Addresses
        {
            get { return m_Addresses; }
        }

        public AddressViewModel SelectedAddress
        {
            get { return m_SelectedAddress; }
            set
            {
                Model.Address = value != null ? value.Model : null;
                m_SelectedAddress = value;
            }
        }

        public string Name
        {
            get { return m_Model.Name; }
            set
            {
                m_Model.Name = value;
                NotifyOfPropertyChange(() => ValidateBuilding);
            }
        }

        public bool ValidateBuilding
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

        public string Description
        {
            get { return m_Model.Description; }
            set { m_Model.Description = value; }
        }

    }
}
