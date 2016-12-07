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
using System.Linq;
using Moq;
using NUnit.Framework;
using Ork.Energy.Domain.DomainModelService;
using Ork.Energy.Factories;
using Ork.Energy.ViewModels;
using Ork.Framework;

namespace Ork.Energy.Tests
{
  [TestFixture]
  internal class SearchFilterTest
  {
    [SetUp]
    public void SetUp()
    {
      m_DialogManagerMock = new Mock<IDialogManager>();
      m_EnergyViewModelFactoryMock = new Mock<IEnergyViewModelFactory>();
      m_Repository = new Mock<IEnergyRepository>();


      var items = new Consumer[]
                  {
                    new Consumer(), new Consumer(),
                  }.AsQueryable();

      var sdf = new DataServiceCollection<Consumer>();
      sdf.Load(items);

      m_Repository.SetupGet(r => r.Consumers)
                  .Returns(sdf);

      m_EnergyManagementViewModel = new EnergyManagementViewModel(m_Repository.Object, m_EnergyViewModelFactoryMock.Object, m_DialogManagerMock.Object);

      m_EnergyManagementViewModel.SelectedConsumerGroup = new ConsumerGroupViewModel(new ConsumerGroup()
                                                                                     {
                                                                                       GroupName = "Consumer"
                                                                                     }, m_Repository.Object);
      m_EnergyManagementViewModel.SelectedDistributor = new DistributorViewModel(new Distributor()
                                                                                 {
                                                                                   Name = "Verteiler"
                                                                                 }, m_Repository.Object);
      m_EnergyManagementViewModel.NewConsumerName = "NeuerVerbraucher";

      m_EnergyManagementViewModel.AddNewConsumer();
    }

    private EnergyManagementViewModel m_EnergyManagementViewModel;
    private Mock<IDialogManager> m_DialogManagerMock;
    private Mock<IEnergyViewModelFactory> m_EnergyViewModelFactoryMock;
    private Mock<IEnergyRepository> m_Repository;
    private Mock<DomainModelContext> m_Context;

    [Test]
    public void NoRestrictions()
    {
      Assert.IsTrue(m_EnergyManagementViewModel.Consumers.Count() == 1);
    }
  }
}