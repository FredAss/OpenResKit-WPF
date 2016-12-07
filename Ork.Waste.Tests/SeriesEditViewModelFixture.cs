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

using NUnit.Framework;

namespace Ork.Waste.Tests
{
  [TestFixture]
  public class SeriesEditViewModelFixture
  {
    [Test]
    public void Update_Following_Readings()
    {
      //var mockRepository = new MockRepository(MockBehavior.Loose);
      //var repositoryMock = mockRepository.Create<IWasteRepository>();
      //var series = mockRepository.Create<Series>();
      //var containerFactoryMock = mockRepository.Create<IContainerViewModelFactory>();
      //var responsibleSubjectFactoryMock = mockRepository.Create<IResponsibleSubjectViewModelFactory>();
      //var taskgeneratorMock = mockRepository.Create<ITaskGenerator>();
      //repositoryMock.SetupGet(r => r.Container)
      //              .Returns(new DataServiceCollection<WasteContainer>());
      //repositoryMock.SetupGet(r => r.ResponsibleSubjects)
      //              .Returns(new DataServiceCollection<ResponsibleSubject>());
      //var pastReadingMock = mockRepository.Create<FillLevelReading>();
      //var selectedReadingMock = mockRepository.Create<FillLevelReading>();
      //var futureReadingMock = mockRepository.Create<FillLevelReading>();

      //var pastAppointmentMock = mockRepository.Create<Appointment>();
      //var selectedAppointmentMock = mockRepository.Create<Appointment>();
      //var futureAppointmentMock = mockRepository.Create<Appointment>();

      //var todayBegin = DateTime.Now;
      //var todayEnd = todayBegin.AddHours(1);

      //selectedReadingMock.Object.DueDate = selectedAppointmentMock.Object;
      //selectedAppointmentMock.Object.Begin = todayBegin;
      //selectedAppointmentMock.Object.End = todayEnd;

      //futureReadingMock.Object.DueDate = futureAppointmentMock.Object;
      //futureAppointmentMock.Object.Begin = todayBegin.AddDays(1);
      //futureAppointmentMock.Object.End = todayEnd.AddDays(1);

      //pastReadingMock.Object.DueDate = pastAppointmentMock.Object;
      //pastAppointmentMock.Object.Begin = todayBegin.AddDays(-1);
      //pastAppointmentMock.Object.End = todayEnd.AddDays(-1);

      //IEnumerable<FillLevelReading> allReadingMocks = new[]
      //  {
      //    pastReadingMock.Object,
      //    selectedReadingMock.Object,
      //    futureReadingMock.Object
      //  };

      //var sut = new SeriesEditViewModel(series.Object,
      //                                  repositoryMock.Object,
      //                                  containerFactoryMock.Object,
      //                                  responsibleSubjectFactoryMock.Object,
      //                                  taskgeneratorMock.Object,
      //                                  selectedReadingMock.Object,
      //                                  allReadingMocks);

      //sut.UpdateFollowingReadings();

      //mockRepository.VerifyAll();
    }
  }
}