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
using System.Data.Services.Client;
using Caliburn.Micro;
using Ork.Approval.DomainModelService;

namespace Ork.Approval.ViewModels
{
    public class ConditionInspectionViewModel : Screen
    {
        private ConditionInspection m_Model;
        private AuxillaryConditionViewModel m_AuxillaryCondition;
        private InspectionViewModel m_Inspection;

        public ConditionInspectionViewModel(ConditionInspection model)
        {
            m_Model = model;
        }

        public ConditionInspectionViewModel(ConditionInspection model, InspectionViewModel inspection, AuxillaryConditionViewModel auxillaryCondition)
        {
            m_Model = model;
            m_Inspection = inspection;
            m_AuxillaryCondition = auxillaryCondition;
        }

        public ConditionInspection Model
        {
            get { return m_Model; }
        }

        public InspectionViewModel ModelInspection
        {
            get { return m_Inspection; }
        }

        public AuxillaryConditionViewModel ModelAuxillaryCondition
        {
            get { return m_AuxillaryCondition; }
        }

        public bool Status
        {
            get { return m_Model.Status; }
        }

        public string Description
        {
            get { return m_Model.Description; }
        }

        public string Condition
        {
            get { return m_AuxillaryCondition.Condition; }
        }

        public DataServiceCollection<Approval_Measure> Measures
        {
            get { return m_Model.Measures; }
        }

        public DateTime EntryDate
        {
            get { return m_Model.EntryDate; }
        }
    }
}
