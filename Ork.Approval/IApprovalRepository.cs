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
using System.Collections.Generic;
using System.Data.Services.Client;
using Ork.Approval.DomainModelService;

namespace Ork.Approval
{
    public interface IApprovalRepository
    {
        DataServiceCollection<Plant> Plants { get; }
        DataServiceCollection<Permission> Permissions { get; set; }
        DataServiceCollection<ScheduledTask> Inspections { get; set; }
        DataServiceCollection<AuxillaryCondition> AuxillaryConditions { get; set; } 
        DataServiceCollection<ResponsibleSubject> ResponsibleSubjects { get; set; } 
        DataServiceCollection<ConditionInspection> ConditionInspections { get; set; }
        DataServiceCollection<Approval_Measure> Measures { get; set; }
        DomainModelContext Context { get; }
        IEnumerable<LinkDescriptor> Links { get; }
        IEnumerable<EntityDescriptor> Entities { get; }
        bool HasConnection { get; }
        void DeleteObject(object objectToDelete);
        void Save();
        event EventHandler ContextChanged;
        event EventHandler SaveCompleted;
    }
}
