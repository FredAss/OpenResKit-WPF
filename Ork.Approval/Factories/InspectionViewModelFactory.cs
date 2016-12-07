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

namespace Ork.Approval.Factories
{
    [Export(typeof(IInspectionViewModelFactory))]
    public class InspectionViewModelFactory : IInspectionViewModelFactory
    {
        private IPlantManagementViewModelFactory m_PlantManagementViewModelFactory;
        private IReadingPlanningViewModelFactory m_ReadingPlanningViewModelFactory;
        private IApprovalRepository m_Repository;

        [ImportingConstructor]
        public InspectionViewModelFactory(IPlantManagementViewModelFactory plantViewModelFactory, IReadingPlanningViewModelFactory readingPlanningViewModelFactory, IApprovalRepository repository)
        {
            m_PlantManagementViewModelFactory = plantViewModelFactory;
            m_ReadingPlanningViewModelFactory = readingPlanningViewModelFactory;
            m_Repository = repository;
        }

        public InspectionViewModel CreateInspectionViewModel(Approval_Inspection model)
        {
            return new InspectionViewModel(model, m_ReadingPlanningViewModelFactory.CreateSeriesViewModel(model.RelatedSeries), SearchForPlantsFromInspection(model));
        }

        private IEnumerable<Plant> SearchForPlantsFromInspection(Approval_Inspection inspection)
        {
            var auxillaryConditionsOfInspection =
                inspection.ConditionInspections.SelectMany(
                    ci => m_Repository.AuxillaryConditions.Where(ac => ac.Id == ci.AuxillaryConditionId && ac.InEffect  ));

            var permissionsOfAuxillaryConditions = new Collection<Permission>();

            foreach (var auxillaryCondition in auxillaryConditionsOfInspection)
            {
                foreach (var permission in m_Repository.Permissions)
                {
                    foreach (var condition in permission.AuxillaryConditions)
                    {
                        if (auxillaryCondition == condition)
                        {
                            permissionsOfAuxillaryConditions.Add(permission);
                        }
                    }
                }
            }

            IEnumerable<Plant> plantConnectedToPermissions = permissionsOfAuxillaryConditions.SelectMany(p => p.Plants.Select(p2 => p2)).Distinct();

            return plantConnectedToPermissions;
        }
    }
}
