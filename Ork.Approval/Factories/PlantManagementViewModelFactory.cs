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
using Ork.Approval.DomainModelService;
using Ork.Approval.ViewModels;
using Action = System.Action;

namespace Ork.Approval.Factories
{
    [Export(typeof(IPlantManagementViewModelFactory))]
    public class PlantManagementViewModelFactory : IPlantManagementViewModelFactory
    {
        private IApprovalRepository m_Repository;
        private IResponsibleSubjectViewModelFactory m_ResponsibleSubjectViewModelFactory;

        [ImportingConstructor]
        public PlantManagementViewModelFactory(IApprovalRepository repository, IResponsibleSubjectViewModelFactory responsibleSubjectViewModelFactory)
        {
            m_Repository = repository;
            m_ResponsibleSubjectViewModelFactory = responsibleSubjectViewModelFactory;
        }

        public PlantViewModel CreatePlantViewModel(Plant plant)
        {
            return new PlantViewModel(plant);
        }

        public PlantAddViewModel CreatePlantAddViewModel()
        {
            return new PlantAddViewModel(new Plant());
        }

        public PlantEditViewModel CreatePlantEditViewModel(Plant plant)
        {
            return new PlantEditViewModel(plant);
        }

        public PermissionViewModel CreatePermissionViewModel(Permission permission)
        {
            return new PermissionViewModel(permission);
        }

        public PermissionAddViewModel CreateApprovalAddViewModel()
        {
            return new PermissionAddViewModel(new Permission(), GetSelectablePlantViewModels(), m_Repository);
        }

        public PermissionEditViewModel CreateApprovalEditViewModel(Permission permission)
        {
            return new PermissionEditViewModel(permission, GetSelectablePlantViewModels2(permission), m_Repository);
        }

        public SelectedPlantViewModel CreateSelectablePlantViewModel(Plant plant)
        {
            return new SelectedPlantViewModel(plant);
        }

        private ObservableCollection<SelectedPlantViewModel> GetSelectablePlantViewModels()
        {
            return new ObservableCollection<SelectedPlantViewModel>(m_Repository.Plants.Select(CreateSelectablePlantViewModel));
        }

        private ObservableCollection<SelectedPlantViewModel> GetSelectablePlantViewModels2(Permission permission)
        {
            var a = GetSelectablePlantViewModels();

            foreach (var selectedPlantViewModel in a)
            {
                var b = selectedPlantViewModel.Permissions.Any(p => permission == p);
                if (b)
                {
                    selectedPlantViewModel.IsSelected = true;
                }
            }

            return a;
        }

        private ObservableCollection<AuxillaryCondition> GetAuxillaryConditions(Permission permission)
        {
            var a =
                permission.AuxillaryConditions.SelectMany(
                    ac => m_Repository.AuxillaryConditions.Where(aci => aci.Id == ac.Id));

            return new ObservableCollection<AuxillaryCondition>(a);
        }

        public ConditionInspectionViewModel CreateConditionInspectionViewModel(ConditionInspection conditionInspection)
        {
            return new ConditionInspectionViewModel(conditionInspection);
        }

        public ConditionInspectionViewModel CreateConditionInspectionViewModel(ConditionInspection conditionInspection, InspectionViewModel inspection, AuxillaryConditionViewModel auxillaryCondition)
        {
            return new ConditionInspectionViewModel(conditionInspection, inspection, auxillaryCondition);
        }

        public AuxillaryConditionViewModel CreateAuxillaryConditionViewModel(AuxillaryCondition auxillaryCondition)
        {
            return new AuxillaryConditionViewModel(auxillaryCondition);
        }

        public MeasureViewModel CreateMeasureViewModel(Approval_Measure measure)
        {
            return new MeasureViewModel(measure);
        }

        public MeasureAddViewModel CreateMeasureAddViewModel()
        {
            return new MeasureAddViewModel(new Approval_Measure(), CreateResponsibleSubjectViewModels());
        }

        private IEnumerable<ResponsibleSubjectViewModel> CreateResponsibleSubjectViewModels()
        {
            var a = new Collection<ResponsibleSubjectViewModel>();

            foreach (var responsibleSubject in m_Repository.ResponsibleSubjects)
            {
                if (responsibleSubject.IsOfType<Employee>())
                    a.Add(m_ResponsibleSubjectViewModelFactory.CreateEmployeeViewModel((Employee)responsibleSubject));
                else
                    a.Add(m_ResponsibleSubjectViewModelFactory.CreateGroupViewModel((EmployeeGroup)responsibleSubject));
            }

            return a;
        }

        public MeasureEditViewModel CreateMeasureEditViewModel(Approval_Measure measure, Action removeMeasureAction)
        {
            return new MeasureEditViewModel(measure, CreateResponsibleSubjectViewModels(), removeMeasureAction);
        }
    }
}
