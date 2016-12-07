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
using System.ComponentModel.Composition;
using System.Linq;
using Ork.Discard.DomainModelService;
using Ork.Discard.ViewModels;

namespace Ork.Discard.Factories
{
  [Export(typeof (IDiscardViewModelFactory))]
  internal class DiscardViewModelFactory : IDiscardViewModelFactory
  {
    private readonly IDiscardRepository m_Repository;
    private readonly IResponsibleSubjectViewModelFactory m_ResponsibleSubjectViewModelFactory;

    [ImportingConstructor]
    public DiscardViewModelFactory([Import] IDiscardRepository contextRepository, [Import] IResponsibleSubjectViewModelFactory responsibleSubjectViewModelFactory)
    {
      m_Repository = contextRepository;
      m_ResponsibleSubjectViewModelFactory = responsibleSubjectViewModelFactory;
    }


    public ProductionItemAddViewModel CreateAddViewModel(ProductionItem productionItem, Customer customer)
    {
      productionItem.Customer = customer;
      return new ProductionItemAddViewModel(productionItem);
    }

    public ProductionItemEditViewModel CreateEditViewModel(ProductionItem productionItem, Action removeProductionItemAction)
    {
      return new ProductionItemEditViewModel(productionItem, removeProductionItemAction);
    }

    public CustomerViewModel CreateFromExisting(Customer customer)
    {
      return new CustomerViewModel(customer);
    }

    public CustomerAddViewModel CreateCustomerAddViewModel()
    {
      return new CustomerAddViewModel(new Customer());
    }

    public CustomerEditViewModel CreateCustomerEditViewModel(CustomerViewModel customerViewModel, Action removeCustomerAction)
    {
      return new CustomerEditViewModel(customerViewModel, removeCustomerAction);
    }

    public InspectionAddViewModel CreateInspectionAddViewModel(CustomerViewModel customer)
    {
      return new InspectionAddViewModel(new Inspection(), customer, CreateResponsibleSubjects(), m_Repository);
    }

    public InspectionEditViewModel CreateInspectionEditViewModel(Inspection inspection, CustomerViewModel customer, Action removeInspectionAction)
    {
      return new InspectionEditViewModel(inspection, customer, CreateResponsibleSubjects(), m_Repository, removeInspectionAction);
    }

    private ResponsibleSubjectViewModel[] CreateResponsibleSubjects()
    {
      return m_Repository.ResponsibleSubjects.Select(m_ResponsibleSubjectViewModelFactory.CreateFromExisting)
                         .ToArray();
    }
  }
}