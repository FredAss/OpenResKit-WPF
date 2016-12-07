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

using Ork.Approval.DomainModelService;
using Ork.Approval.ViewModels;
using Action = System.Action;

namespace Ork.Approval.Factories
{
    public interface IPlantManagementViewModelFactory
    {
        PlantViewModel CreatePlantViewModel(Plant plant);
        PlantAddViewModel CreatePlantAddViewModel();
        PlantEditViewModel CreatePlantEditViewModel(Plant plant);
        PermissionViewModel CreatePermissionViewModel(Permission permission);
        PermissionAddViewModel CreateApprovalAddViewModel();
        PermissionEditViewModel CreateApprovalEditViewModel(Permission permission);
        ConditionInspectionViewModel CreateConditionInspectionViewModel(ConditionInspection conditionInspection, InspectionViewModel inspection, AuxillaryConditionViewModel auxillaryCondition);
        ConditionInspectionViewModel CreateConditionInspectionViewModel(ConditionInspection conditionInspection);
        AuxillaryConditionViewModel CreateAuxillaryConditionViewModel(AuxillaryCondition auxillaryCondition);
        MeasureViewModel CreateMeasureViewModel(Approval_Measure measure);
        MeasureAddViewModel CreateMeasureAddViewModel();
        MeasureEditViewModel CreateMeasureEditViewModel(Approval_Measure measure, Action removeMeasureAction);
    }
}
