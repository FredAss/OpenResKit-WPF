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

using Caliburn.Micro;
using Ork.Approval.DomainModelService;

namespace Ork.Approval.ViewModels
{
    public class AuxillaryConditionViewModel : Screen
    {
        private AuxillaryCondition m_Model;
        private bool m_IsDeletable;

        public AuxillaryConditionViewModel(AuxillaryCondition model)
        {
            m_Model = model;
        }

        public AuxillaryCondition Model
        {
            get { return m_Model; }
        }

        public string Condition
        {
            get { return m_Model.Condition; }
        }

        public bool InEffect
        {
            get { return m_Model.InEffect; }
            set { m_Model.InEffect = value; }
        }

        public bool IsDeletable
        {
            get { return m_IsDeletable; }
            set { m_IsDeletable = value; }
        }
    }
}
