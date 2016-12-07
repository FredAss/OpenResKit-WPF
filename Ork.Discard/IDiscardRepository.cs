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
using Ork.Discard.DomainModelService;

namespace Ork.Discard
{
  public interface IDiscardRepository
  {
    DataServiceCollection<ResponsibleSubject> ResponsibleSubjects { get; }
    DataServiceCollection<Inspection> Inspections { get; }
    DataServiceCollection<InspectionAttribute> InspectionAttributes { get; }
    DataServiceCollection<ProductionItem> ProductionItems { get; }
    DataServiceCollection<Customer> Customers { get; }
    bool HasConnection { get; }
    void Save();
    event EventHandler ContextChanged;
    event EventHandler SaveCompleted;
  }
}