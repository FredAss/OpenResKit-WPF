using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Caliburn.Micro;
using Ork.Danger.DomainModelService;
using System.ComponentModel;



namespace Ork.Danger.ViewModels
{
    public class AssViewModel : Screen
    {
        private readonly Assessment m_Model;

        public AssViewModel(Assessment model)
        {
            m_Model = model;
            m_Model.PropertyChanged += ModelPropertyChanged;
        }

        public Assessment Model
        {
            get { return m_Model; }
        }

        public int Id
        {
            get { return m_Model.Id; }
            set
            {
                m_Model.Id = value;
                NotifyOfPropertyChange(() => Id);
            }
        }


        public DateTime AssessmentDate
        {
            get { return m_Model.AssessmentDate; }
            set
            {
                m_Model.AssessmentDate = value;
                NotifyOfPropertyChange(() => AssessmentDate);
            }
        }


        private void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyOfPropertyChange(e.PropertyName);
        }
    }
}
