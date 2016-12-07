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

using System.Data.Services.Client;
using Caliburn.Micro;
using Ork.Approval.DomainModelService;

namespace Ork.Approval.ViewModels
{
    public class PlantViewModel : Screen
    {
        private Plant m_Model;

        public PlantViewModel(Plant model)
        {
            m_Model = model;
        }

        public Plant Model
        {
            get { return m_Model; }
        }

        public string Name
        {
            get { return m_Model.Name; }
        }

        public string Number
        {
            get { return m_Model.Number; }
        }

        public string Position
        {
            get { return m_Model.Position; }
        }

        public DataServiceCollection<Document> AttachedDocuments
        {
            get { return m_Model.AttachedDocuments; }
        }

        public string Description
        {
            get { return m_Model.Description; }
        }

        public DataServiceCollection<Permission> Permissions
        {
            get { return m_Model.Permissions; }
        }
    }
}
