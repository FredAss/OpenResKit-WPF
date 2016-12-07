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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    public class PlantManagementViewModel: DocumentBase, IWorkspace
    {
        private IApprovalRepository m_Repository;
        private IScreen m_EditItem;
        private IPlantManagementViewModelFactory m_PlantManagementViewModelFactory;

        private BindableCollection<PlantViewModel> m_PlantViewModels = new BindableCollection<PlantViewModel>();
        private BindableCollection<PermissionViewModel> m_PermissionViewModels = new BindableCollection<PermissionViewModel>();
        private PlantViewModel m_SelectedPlantViewModel;
        private PermissionViewModel m_SelectedPermissionViewModel;
        private int m_SelectedInEffectStatus;
        private readonly IEnumerable m_InEffectStatus;
        private string m_SearchText;
        private bool m_FlyoutActivated;


        [ImportingConstructor]
        public PlantManagementViewModel([Import] IApprovalRepository repository, [Import] IPlantManagementViewModelFactory plantManagementViewModelFactory)
        {
            m_Repository = repository;
            m_PlantManagementViewModelFactory = plantManagementViewModelFactory;
            m_Repository.ContextChanged += (s, e) => Application.Current.Dispatcher.Invoke(LoadData);

            m_InEffectStatus = Enum.GetValues(typeof (InEffect));

            LoadData();

            SelectedPlantViewModel = null;
            FlyoutActivated = true;
        }

        public int Index
        {
            get { return 2; }
        }

        public bool IsEnabled
        {
            get { return true; }
        }

        public string Title
        {
            get { return TranslationProvider.Translate("Plant"); }
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

        public IEnumerable InEffectStatus
        {
            get { return m_InEffectStatus; }
        }

        public int SelectedInEffectStatus
        {
            get { return m_SelectedInEffectStatus; }
            set
            {
                m_SelectedInEffectStatus = value;
                NotifyOfPropertyChange(() => PermissionViewModelsOfSelectedPlant);
            }
        }

        public IEnumerable<PlantViewModel> FilteredPlantViewModels
        {
            get { return SearchInPlantViewModels(); }
        }

        public PlantViewModel SelectedPlantViewModel
        {
            get { return m_SelectedPlantViewModel; }
            set
            {
                if(m_SelectedPlantViewModel == value)
                    return;

                m_SelectedPlantViewModel = value;

                NotifyOfPropertyChange(() => SelectedPlantViewModel);
                NotifyOfPropertyChange(() => PermissionViewModelsOfSelectedPlant);
            }
        }

        public PermissionViewModel SelectedPermissionViewModel
        {
            get { return m_SelectedPermissionViewModel; }
            set
            {
                if(m_SelectedPermissionViewModel == value)
                    return;
                m_SelectedPermissionViewModel = value;

                NotifyOfPropertyChange(() => SelectedPermissionViewModel);
            }
        }

        public ObservableCollection<PermissionViewModel> PermissionViewModelsOfSelectedPlant
        {
            get
            {
                if (m_SelectedPlantViewModel == null && m_SelectedInEffectStatus == 0)
                    return m_PermissionViewModels;
                if (m_SelectedPlantViewModel == null && m_SelectedInEffectStatus == 1)
                    return new ObservableCollection<PermissionViewModel>(m_PermissionViewModels.Where(pvm => pvm.InEffect));
               
                if (m_SelectedPlantViewModel == null && m_SelectedInEffectStatus == 2)
                    return new ObservableCollection<PermissionViewModel>(m_PermissionViewModels.Where(pvm => pvm.InEffect == false));

                var permissionViewModelsOfPlant = GetPermissionsOfSelectedPlantViewModel();

                if (m_SelectedInEffectStatus == 1)
                {
                    return new ObservableCollection<PermissionViewModel>(permissionViewModelsOfPlant.Where(pvm => pvm.InEffect));
                }
                if (m_SelectedInEffectStatus == 2)
                {
                    return new ObservableCollection<PermissionViewModel>(permissionViewModelsOfPlant.Where(pvm => pvm.InEffect == false));
                }

                return permissionViewModelsOfPlant;
            }
        }

        private ObservableCollection<PermissionViewModel> GetPermissionsOfSelectedPlantViewModel()
        {
            var pvmList = new ObservableCollection<PermissionViewModel>();

            foreach (var mPermissionViewModel in m_SelectedPlantViewModel.Permissions.SelectMany(p => m_PermissionViewModels.Where(mPermissionViewModel => mPermissionViewModel.Model == p)))
            {
                pvmList.Add(mPermissionViewModel);
            }

            return pvmList;
        }

        public string SearchText
        {
            get { return m_SearchText; }
            set
            {
                if(m_SearchText == value)
                    return;

                m_SearchText = value;
                NotifyOfPropertyChange(() => FilteredPlantViewModels);
            }
        }

        public bool CanCreateNewPermission
        {
            get { return m_PlantViewModels.Any(); }
        }

        private void AlterPlantCollection(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var newItem in e.NewItems.OfType<Plant>())
                {
                    m_PlantManagementViewModelFactory.CreatePlantViewModel(newItem);
                }
                NotifyOfPropertyChange(() => FilteredPlantViewModels);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var oldItem in e.OldItems.OfType<Plant>())
                {
                    var plantViewModel = m_PlantViewModels.Single(r => r.Model == oldItem);

                    m_PlantViewModels.Remove(plantViewModel);
                }
                NotifyOfPropertyChange(() => FilteredPlantViewModels);
            }
        }

        private void AlterPermissionCollection(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var newItem in e.NewItems.OfType<Permission>())
                {
                    m_PlantManagementViewModelFactory.CreatePermissionViewModel(newItem);
                }
                NotifyOfPropertyChange(() => PermissionViewModelsOfSelectedPlant);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var oldItem in e.OldItems.OfType<Permission>())
                {
                    var permissionViewModel = m_PermissionViewModels.Single(r => r.Model == oldItem);

                    m_PermissionViewModels.Remove(permissionViewModel);
                }
                NotifyOfPropertyChange(() => PermissionViewModelsOfSelectedPlant);
            }
        }

        private IEnumerable<PlantViewModel> SearchInPlantViewModels()
        {
            var plantViewModelsResult = FilterPlantViewModelsBySearchText();

            if(plantViewModelsResult.Any())
                SelectedPlantViewModel = plantViewModelsResult.First();

            return plantViewModelsResult;
        }

        private IEnumerable<PlantViewModel> FilterPlantViewModelsBySearchText()
        {
            var searchText = SearchText.ToLower();
            var plantViewModelsResult = m_PlantViewModels.Where(pvm =>
                        ((pvm.Name != null) && (pvm.Name.ToLower().Contains(searchText))) ||
                        ((pvm.Number != null) && (pvm.Number.ToLower().Contains(searchText))));

            return plantViewModelsResult;
        }

        private void LoadData()
        {
            LoadPlantViewModels();
            LoadPermissionViewModels();
            NotifyOfPropertyChange(() => CanCreateNewPermission);
        }

        private void LoadPlantViewModels()
        {
            m_PlantViewModels.CollectionChanged += AlterPlantCollection;

            if (m_Repository.Plants != null)
            {
                foreach (var plant in m_Repository.Plants)
                {
                    m_PlantViewModels.Add(m_PlantManagementViewModelFactory.CreatePlantViewModel(plant));
                }
                NotifyOfPropertyChange(() => FilteredPlantViewModels);
            }        
        }

        private void LoadPermissionViewModels()
        {
            m_PermissionViewModels.CollectionChanged += AlterPermissionCollection;

            if (m_Repository.Permissions != null)
            {
                foreach (var permission in m_Repository.Permissions)
                {
                    m_PermissionViewModels.Add(new PermissionViewModel(permission));
                }
            }
        }

        private void OpenEditor(object dataContext)
        {
            m_EditItem = (IScreen)dataContext;
            Dialogs.ShowDialog(m_EditItem);
        }

        public void OpenPlantAddDialog()
        {
            OpenEditor(m_PlantManagementViewModelFactory.CreatePlantAddViewModel());
        }

        public void OpenPlantEditDialog(object dataContext, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                if(dataContext is PlantViewModel)
                    OpenEditor(m_PlantManagementViewModelFactory.CreatePlantEditViewModel(((PlantViewModel)dataContext).Model));
            }
        }

        public void OpenPlantEditDialog(object dataContext)
        {
            if (dataContext is PlantViewModel)
                OpenEditor(m_PlantManagementViewModelFactory.CreatePlantEditViewModel(((PlantViewModel)dataContext).Model));
        }

        public void OpenPermissionAddDialog()
        {
            OpenEditor(m_PlantManagementViewModelFactory.CreateApprovalAddViewModel());
        }

        public void OpenPermissionEditDialog(object dataContext, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                if (dataContext is PermissionViewModel)
                    OpenEditor(m_PlantManagementViewModelFactory.CreateApprovalEditViewModel(m_SelectedPermissionViewModel.Model));
            }
        }

        public void OpenPermissionEditDialog(object dataContext)
        {
            if (dataContext is PermissionViewModel)
            {
                m_SelectedPermissionViewModel = (PermissionViewModel) dataContext;
                OpenEditor(m_PlantManagementViewModelFactory.CreateApprovalEditViewModel(m_SelectedPermissionViewModel.Model));
            }
                
        }

        private void CloseEditor()
        {
            m_EditItem.TryClose();
        }

        public void AddPlant(object dataContext)
        {
            var plantAddViewModel = (PlantAddViewModel) dataContext;
            m_PlantViewModels.Add(m_PlantManagementViewModelFactory.CreatePlantViewModel(plantAddViewModel.Plant));
            m_Repository.Plants.Add(plantAddViewModel.Plant);
            m_Repository.Save();

            NotifyOfPropertyChange(() => CanCreateNewPermission);

            CloseEditor();

            SelectedPlantViewModel = m_PlantViewModels.Last();
        }

        public void AcceptPlant()
        {
            m_Repository.Save();
            NotifyOfPropertyChange(() => FilteredPlantViewModels);
            CloseEditor();
        }

        public void AddPermission(object dataContext)
        {
            var permissionAddViewModel = (PermissionAddViewModel) dataContext;
            var selectedPlants = permissionAddViewModel.PlantViewModels;
            m_PermissionViewModels.Add(m_PlantManagementViewModelFactory.CreatePermissionViewModel(permissionAddViewModel.Model));
            m_Repository.Permissions.Add(permissionAddViewModel.Model);

            foreach (var selectedPlantViewModel in selectedPlants)
            {
                if (selectedPlantViewModel.IsSelected)
                {
                    var pvm = m_PlantViewModels.Single(p => p.Model == selectedPlantViewModel.Model);
                    pvm.Permissions.Add(permissionAddViewModel.Model);
                }
            }

            SaveNewAuxillaryConditions(permissionAddViewModel.Model);

            NotifyOfPropertyChange(() => PermissionViewModelsOfSelectedPlant);

            m_Repository.Save();

            //LoadPlantsAndPermissions();

            CloseEditor();
        }

        public void AcceptPermission(Object dataContext)
        {
            var pevm = (PermissionEditViewModel) dataContext;            
            SetPermissionsToSelectedPlants(pevm.Model, pevm.PlantViewModels);
            SaveNewAuxillaryConditions(pevm.Model);
            m_Repository.Save();

            LoadPlantsAndPermissions();

            NotifyOfPropertyChange(() => FilteredPlantViewModels);
            NotifyOfPropertyChange(() => PermissionViewModelsOfSelectedPlant);
            CloseEditor();
        }

        private void SaveNewAuxillaryConditions(Permission permission)
        {
            foreach (var ac in permission.AuxillaryConditions)
            {
                if (m_Repository.AuxillaryConditions.Any(acr => acr == ac) == false)
                    m_Repository.AuxillaryConditions.Add(ac);
            }
        }

       private void SetPermissionsToSelectedPlants(Permission permission, IEnumerable<SelectedPlantViewModel> plantViewModels)
        {
            foreach (var plantViewModel in plantViewModels)
            {
                var plant = m_PlantViewModels.Single(p => p.Model == plantViewModel.Model);
                var plantHasSelectedPermission = plant.Permissions.Any(pe => pe == m_SelectedPermissionViewModel.Model);

                if (plantViewModel.IsSelected)
                {
                    if (!plantHasSelectedPermission)
                    {
                        plantViewModel.Permissions.Add(permission);
                        //m_Repository.Context.AddLink(plant.Model, "Permissions", permission);
                    }
                }
                else
                {
                    if (plantHasSelectedPermission)
                    {
                        var link = m_Repository.Context.Links.SingleOrDefault(l => l.Source == plant.Model && l.Target == m_SelectedPermissionViewModel.Model);
                        var b = m_Repository.Links.Count();
                        if (link != null)
                        {
                            m_Repository.Context.DeleteLink(plantViewModel.Model, "Permissions", m_SelectedPermissionViewModel.Model);
                            
                            //m_Repository.Context.DeleteLink(m_SelectedPermissionViewModel.Model, "Plants", plant.Model);
                            //m_SelectedPlantViewModel.Permissions.Remove(m_SelectedPermissionViewModel.Model);
                        }

                        var a = m_Repository.Links.Count();
                    }
                }
            }
        }

        private void LoadPlantsAndPermissions()
        {
            m_PlantViewModels.Clear();
            m_PermissionViewModels.Clear();
            
            LoadData();
        }

        public void ShowAllPermissions()
        {
            SelectedPlantViewModel = null;
        }

        public void Cancel()
        {
            m_EditItem.TryClose();
        }
    }
}
