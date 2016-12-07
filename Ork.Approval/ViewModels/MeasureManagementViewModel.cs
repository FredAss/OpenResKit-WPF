#region License

// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at
//  
// http://www.apache.org/licenses/LICENSE-2.0.html
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  
// Copyright (c) 2013, HTW Berlin

#endregion

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Ork.Approval.DomainModelService;
using Ork.Approval.Factories;
using Ork.Framework;

namespace Ork.Approval.ViewModels
{
    [Export(typeof(IWorkspace))]
    public class MeasureManagementViewModel : DocumentBase, IWorkspace
    {
        private IApprovalRepository m_Repository;
        private IPlantManagementViewModelFactory m_PlantManagementViewModelFactory;
        private IInspectionViewModelFactory m_InspectionViewModelFactory;
        private BindableCollection<InspectionViewModel> m_InspectionViewModels = new BindableCollection<InspectionViewModel>();
        private Collection<AuxillaryConditionViewModel> m_AuxillaryConditionViewModels = new Collection<AuxillaryConditionViewModel>();
        private BindableCollection<ConditionInspectionViewModel> m_ConditionInspectionViewModels = new BindableCollection<ConditionInspectionViewModel>();
        private ConditionInspectionViewModel m_SelectedConditionInspectionViewModel;
        private InspectionViewModel m_SelectedInspectionViewModel;
        private MeasureViewModel m_SelectedMeasureViewModel;
        private IScreen m_EditItem;
        private string m_SearchText;
        private bool m_FlyoutActivated;

            
        [ImportingConstructor]
        public MeasureManagementViewModel([Import] IApprovalRepository repository, [Import] IPlantManagementViewModelFactory plantManagementViewModelFactory, [Import] IInspectionViewModelFactory inspectionViewModelFactory)
        {
            m_Repository = repository;
            m_Repository.ContextChanged += (s, e) => Application.Current.Dispatcher.Invoke(Reload);
            m_PlantManagementViewModelFactory = plantManagementViewModelFactory;
            m_InspectionViewModelFactory = inspectionViewModelFactory;
            LoadData();

            FlyoutActivated = true;
        }

        public int Index
        {
            get { return 4; }
        }

        public bool IsEnabled
        {
            get { return true; }
        }

        public string Title
        {
            get { return TranslationProvider.Translate("Inspections"); }
        }

        public string SearchText
        {
            get { return m_SearchText; }
            set
            {
                if (m_SearchText == value)
                    return;

                m_SearchText = value;
                NotifyOfPropertyChange(() => FilterInspectionViewModels);
            }
        }

        public bool FlyoutActivated
        {
            get { return m_FlyoutActivated; }
            set
            {
                if (m_FlyoutActivated == value)
                {
                    return;
                }
                m_FlyoutActivated = value;
                NotifyOfPropertyChange(() => FlyoutActivated);
            }
        }

        public IEnumerable<InspectionViewModel> FilterInspectionViewModels
        {
            get { return FilterInspectionViewModelsBySearchText(); }
        }

        private IEnumerable<InspectionViewModel> FilterInspectionViewModelsBySearchText()
        {
            if (string.IsNullOrEmpty(m_SearchText))
                return m_InspectionViewModels;

            var searchText = m_SearchText.ToLower();

            var searchResult =
                m_InspectionViewModels.Where(
                    ivm => ((ivm.RelatedSeriesName != null && ivm.RelatedSeriesName.ToLower().Contains(searchText))));

            return searchResult;
        }

        public bool CanAddMeasure
        {
            get { return m_SelectedConditionInspectionViewModel != null; }
        }

        public IEnumerable<ConditionInspectionViewModel> ConditionInspectionViewModels
        {
            get { return FilterConditionInspectionViewModelsByInspection(); }
        }

        public IEnumerable<MeasureViewModel> FilteredMeasures
        {
            get { return FilterMeasureViewModels(); }
        }

        public ConditionInspectionViewModel SelectedConditionInspectionViewModel
        {
            get { return m_SelectedConditionInspectionViewModel; }
            set
            {
                m_SelectedConditionInspectionViewModel = value;
                NotifyOfPropertyChange(() => FilteredMeasures);
                NotifyOfPropertyChange(() => CanAddMeasure);
            }
        }

        public InspectionViewModel SelectedInspectionViewModel
        {
            get { return m_SelectedInspectionViewModel; }
            set
            {
                m_SelectedInspectionViewModel = value;
                NotifyOfPropertyChange(() => ConditionInspectionViewModels);
            }
        }

        public MeasureViewModel SelectedMeasureViewModel
        {
            get { return m_SelectedMeasureViewModel; }
            set
            {
                m_SelectedMeasureViewModel = value;
                NotifyOfPropertyChange(() => SelectedMeasureViewModel);
            }
        }

        private IEnumerable<ConditionInspectionViewModel> FilterConditionInspectionViewModelsByInspection()
        {
            if (m_SelectedInspectionViewModel == null)
                return null;

            var filteredConditionInspections = m_ConditionInspectionViewModels.Where(civm => civm.ModelInspection.Inspection == m_SelectedInspectionViewModel.Inspection);
            return filteredConditionInspections;
        }

        private IEnumerable<MeasureViewModel> FilterMeasureViewModels()
        {
            if (m_SelectedConditionInspectionViewModel == null)
                return null;

            var filteredMeasures =
                m_SelectedConditionInspectionViewModel.Measures.Select(
                    m => m_PlantManagementViewModelFactory.CreateMeasureViewModel(m));

            return filteredMeasures;
        }

        private void Reload()
        {
            //m_InspectionViewModels = null;
            //m_ConditionInspectionViewModels = null;
            //m_AuxillaryConditionViewModels = null;

            LoadData();
        }

        private void LoadData()
        {
            LoadInspectionViewModels();
            LoadAuxillaryConditionViewModels();
            LoadConditionInspectionViewModels();
        }

        private void LoadInspectionViewModels()
        {
            if (m_Repository.Inspections != null)
            {
                foreach (var inspection in m_Repository.Inspections.OfType<Approval_Inspection>().Where(i => i.Progress == 2))
                {
                    
                    m_InspectionViewModels.Add(m_InspectionViewModelFactory.CreateInspectionViewModel(inspection));
                }
            }
        }

        private void LoadAuxillaryConditionViewModels()
        {
            if (m_Repository.AuxillaryConditions != null)
            {
                foreach (var auxillaryCondition in m_Repository.AuxillaryConditions)
                {
                    m_AuxillaryConditionViewModels.Add(m_PlantManagementViewModelFactory.CreateAuxillaryConditionViewModel(auxillaryCondition));
                }
            }
        }

        private void LoadConditionInspectionViewModels()
        {
            if (m_Repository.ConditionInspections != null)
            {
                var conditionInspections = new ObservableCollection<ConditionInspection>();

                foreach (var conditionInspection in from conditionInspection in m_Repository.ConditionInspections from mInspectionViewModel in m_InspectionViewModels.Where(mInspectionViewModel => conditionInspection.InspectionId == mInspectionViewModel.Inspection.Id) select conditionInspection)
                {
                    conditionInspections.Add(conditionInspection);
                }


                foreach (var conditionInspection in conditionInspections)
                {
                    var inspection = m_InspectionViewModels.Single(ivm => ivm.Inspection.Id == conditionInspection.InspectionId);
                    var auxillaryCondition = m_AuxillaryConditionViewModels.Single(acvm => acvm.Model.Id == conditionInspection.AuxillaryConditionId);

                    m_ConditionInspectionViewModels.Add(m_PlantManagementViewModelFactory.CreateConditionInspectionViewModel(conditionInspection, inspection, auxillaryCondition));
                }
            }
        }

        private void OpenEditor(object dataContext)
        {
            m_EditItem = (IScreen)dataContext;
            Dialogs.ShowDialog(m_EditItem);
        }

        public void OpenMeasureAddDialog()
        {
            OpenEditor(m_PlantManagementViewModelFactory.CreateMeasureAddViewModel());
        }

        public void OpenMeasureEditDialog(object dataContext, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                if(dataContext is MeasureViewModel)
                    OpenEditor(m_PlantManagementViewModelFactory.CreateMeasureEditViewModel(((MeasureViewModel)dataContext).Model, RemoveMeasure));
            }
        }

        public void OpenMeasureEditDialog(object dataContext)
        {
            if (dataContext is MeasureViewModel)
            {
                m_SelectedMeasureViewModel = (MeasureViewModel) dataContext;
                OpenEditor(m_PlantManagementViewModelFactory.CreateMeasureEditViewModel(((MeasureViewModel)dataContext).Model, RemoveMeasure));
            }
        }

        public void AddMeasure(object dataContext)
        {
            var mavm = (MeasureAddViewModel)dataContext;
            m_SelectedConditionInspectionViewModel.Measures.Add(mavm.Model);
            m_Repository.Measures.Add(mavm.Model);
            m_Repository.Save();

            NotifyOfPropertyChange(() => FilteredMeasures);

            Cancel();
        }

        public void AcceptMeasure()
        {
            m_Repository.Save();
            NotifyOfPropertyChange(() => FilteredMeasures);
            Cancel();
        }

        public void RemoveMeasure()
        {
            var measureViewModel = m_SelectedMeasureViewModel;

            m_SelectedConditionInspectionViewModel.Measures.Remove(measureViewModel.Model);
            m_Repository.Measures.Remove(measureViewModel.Model);
            Cancel();
            NotifyOfPropertyChange(() => FilteredMeasures);
            m_Repository.Save();
        }

        public void Cancel()
        {
            m_EditItem.TryClose();
        }

        
    }
}
