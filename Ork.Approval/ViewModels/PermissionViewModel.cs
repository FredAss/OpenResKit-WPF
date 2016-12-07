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
using System.Linq;
using Caliburn.Micro;
using Ork.Approval.DomainModelService;
using Ork.Framework;

namespace Ork.Approval.ViewModels
{
    public class PermissionViewModel : Screen
    {
        private Permission m_Model;

        public PermissionViewModel(Permission model)
        {
            m_Model = model;
        }

        public DataServiceCollection<Plant> Plants
        {
            get { return m_Model.Plants; }
        }

        public Permission Model
        {
            get { return m_Model; }
        }

        public string Name
        {
            get { return m_Model.Name; }
        }

        public int KindOfPermission
        {
            get { return m_Model.PermissionKind; }
        }

        public bool InEffect
        {
            get { return m_Model.InEffect; }
        }

        public string FileNumber
        {
            get { return m_Model.FileNumber; }
        }

        public DateTime DateOfApplication
        {
            get { return m_Model.DateOfApplication; }
        }

        public DateTime StartOfPermission
        {
            get { return m_Model.StartOfPermission; }
        }

        public DateTime EndOfPermission
        {
            get { return m_Model.EndOfPermission; }
        }

        public string PeriodOfPermission
        {
            get { return StartOfPermission.ToShortDateString() + " - " + EndOfPermission.ToShortDateString(); }
        }

        public DataServiceCollection<AuxillaryCondition> AuxillaryConditions
        {
            get { return m_Model.AuxillaryConditions; }
        }

        public string CountOfAuxillaryConditions
        {
            get
            {
                if (AuxillaryConditions.Any())
                {
                    var count = AuxillaryConditions.Count;
                    var countActive = AuxillaryConditions.Count(ac => ac.InEffect);
                    return String.Format(TranslationProvider.Translate("ConditionInfoWithCount"), count, countActive);
                }
                else
                {
                    return TranslationProvider.Translate("ConditionInfoWithoutCount");
                }
            }
        }

        public DataServiceCollection<Document> AttachedDocuments
        {
            get { return m_Model.AttachedDocuments; }
        }

        public string CountOfAttachedDocuments
        {
            get
            {
                if (AttachedDocuments.Any())
                {
                    var count = AttachedDocuments.Count;
                    return String.Format(TranslationProvider.Translate("DocumentInfoWithCount"), count);
                }
                else
                {
                    return TranslationProvider.Translate("DocumentInfoWithoutCount");
                }
            }
        }
    }
}
