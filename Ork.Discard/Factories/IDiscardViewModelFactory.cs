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
using Ork.Discard.DomainModelService;
using Ork.Discard.ViewModels;

namespace Ork.Discard.Factories
{
  public interface IDiscardViewModelFactory
  {
    ProductionItemAddViewModel CreateAddViewModel(ProductionItem productionItem, Customer customer);
    ProductionItemEditViewModel CreateEditViewModel(ProductionItem productionItem, Action removeProductionItemAction);
    CustomerViewModel CreateFromExisting(Customer customer);
    CustomerAddViewModel CreateCustomerAddViewModel();
    CustomerEditViewModel CreateCustomerEditViewModel(CustomerViewModel customerViewModel, Action removeCustomerAction);
    InspectionAddViewModel CreateInspectionAddViewModel(CustomerViewModel customer);
    InspectionEditViewModel CreateInspectionEditViewModel(Inspection inspection, CustomerViewModel customer, Action removeInspectionAction);
  }
}