using System.Collections.Generic;
using Ork.Building.DomainModelService;
using Ork.Framework;

namespace Ork.Building.ViewModels
{
    public class AddressViewModel : DocumentBase
    {
        private readonly Address m_Model;

        public AddressViewModel(Address address)
        {
            m_Model = address;
        }

        public Address Model
        {
            get { return m_Model; }
        }

        public string Street
        {
            get { return m_Model.Street; }
            set { m_Model.Street = value; }
        }

        public string Number
        {
            get { return m_Model.Number; }
            set { m_Model.Number = value; }
        }

        public string Zip
        {
            get { return m_Model.Zip; }
            set { m_Model.Zip = value; }
        }

        public string City
        {
            get { return m_Model.City; }
            set { m_Model.City = value; }
        }

    }
}