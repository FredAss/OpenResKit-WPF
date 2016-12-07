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
using System.Linq;
using Moq;
using NUnit.Framework;
using Ork.Waste.DomainModelService;
using DayOfWeek = System.DayOfWeek;

namespace Ork.Waste.Tests
{
  [TestFixture]
  public class TaskGeneratorFixture
  {
    private TaskGeneratorConfig Setup()
    {
      var repository = new MockRepository(MockBehavior.Strict);
      var config = new TaskGeneratorConfig();

      config.Begin = new DateTime(2000, 1, 1, 0, 0, 0);
      config.End = new DateTime(2000, 1, 1, 1, 0, 0);

      config.Repeat = true;
      config.RecurrenceInterval = 2;
      config.EndsWithDate = false;
      config.RepeatUntilDate = DateTime.MinValue;
      config.NumberOfRecurrences = 3;
      config.Container = repository.Create<WasteContainer>()
                                   .Object;
      config.ResponsibleSubject = repository.Create<ResponsibleSubject>()
                                            .Object;
      config.Series = repository.Create<Series>()
                                .Object;

      config.WeekDays = new[]
                        {
                          DayOfWeek.Monday, DayOfWeek.Wednesday
                        };

      return config;
    }

    private TaskGeneratorConfig AdvancedSetup()
    {
      var repository = new MockRepository(MockBehavior.Strict);
      var config = new TaskGeneratorConfig();

      config.Begin = new DateTime(2013, 5, 21, 10, 0, 0);
      config.End = new DateTime(2013, 5, 21, 11, 0, 0);

      config.Repeat = true;
      config.EndsWithDate = false;
      config.RecurrenceInterval = 2;
      config.RepeatUntilDate = DateTime.MinValue;
      config.NumberOfRecurrences = 5;
      config.Container = repository.Create<WasteContainer>()
                                   .Object;
      config.ResponsibleSubject = repository.Create<ResponsibleSubject>()
                                            .Object;
      config.Series = repository.Create<Series>()
                                .Object;
      config.IsWeekdayRecurrence = false;
      config.Series = repository.Create<Series>()
                                .Object;

      config.WeekDays = new[]
                        {
                          DayOfWeek.Monday, DayOfWeek.Wednesday
                        };

      return config;
    }

    [Test]
    public void Generate_Daily_Task_End_Date()
    {
      var cut = new TaskGenerator();
      var config = AdvancedSetup();

      config.RepeatUntilDate = new DateTime(2013, 5, 30, 0, 0, 0);
      config.EndsWithDate = true;

      var beginFirst = new DateTime(2013, 5, 21, 10, 0, 0);
      var endFirst = new DateTime(2013, 5, 21, 11, 0, 0);
      var beginMiddle = new DateTime(2013, 5, 25, 10, 0, 0);
      var endMiddle = new DateTime(2013, 5, 25, 11, 0, 0);
      var beginEnd = new DateTime(2013, 5, 29, 10, 0, 0);
      var endEnd = new DateTime(2013, 5, 29, 11, 0, 0);
      var result = cut.GenerateFillLevelReadings(config);
      Assert.AreEqual(5, result.Count());
      Assert.AreEqual(beginFirst, result.First()
                                        .DueDate.Begin);
      Assert.AreEqual(endFirst, result.First()
                                      .DueDate.End);
      Assert.AreEqual(beginMiddle, result.Take(3)
                                         .Last()
                                         .DueDate.Begin);
      Assert.AreEqual(endMiddle, result.Take(3)
                                       .Last()
                                       .DueDate.End);
      Assert.AreEqual(beginEnd, result.Last()
                                      .DueDate.Begin);
      Assert.AreEqual(endEnd, result.Last()
                                    .DueDate.End);
    }

    [Test]
    public void Generate_Daily_Task_Recurrence()
    {
      var cut = new TaskGenerator();
      var config = AdvancedSetup();

      config.EndsWithDate = false;

      var beginFirst = new DateTime(2013, 5, 21, 10, 0, 0);
      var endFirst = new DateTime(2013, 5, 21, 11, 0, 0);
      var beginMiddle = new DateTime(2013, 5, 25, 10, 0, 0);
      var endMiddle = new DateTime(2013, 5, 25, 11, 0, 0);
      var beginEnd = new DateTime(2013, 5, 29, 10, 0, 0);
      var endEnd = new DateTime(2013, 5, 29, 11, 0, 0);
      var result = cut.GenerateFillLevelReadings(config);
      Assert.AreEqual(5, result.Count());
      Assert.AreEqual(beginFirst, result.First()
                                        .DueDate.Begin);
      Assert.AreEqual(endFirst, result.First()
                                      .DueDate.End);
      Assert.AreEqual(beginMiddle, result.Take(3)
                                         .Last()
                                         .DueDate.Begin);
      Assert.AreEqual(endMiddle, result.Take(3)
                                       .Last()
                                       .DueDate.End);
      Assert.AreEqual(beginEnd, result.Last()
                                      .DueDate.Begin);
      Assert.AreEqual(endEnd, result.Last()
                                    .DueDate.End);
    }

    [Test]
    public void Generate_Daily_Tasks_Every_Other_Day()
    {
      var cut = new TaskGenerator();
      var config = Setup();

      var beginMiddle = new DateTime(2000, 1, 3, 0, 0, 0);
      var endMiddle = new DateTime(2000, 1, 3, 1, 0, 0);
      var beginEnd = new DateTime(2000, 1, 5, 0, 0, 0);
      var endEnd = new DateTime(2000, 1, 5, 1, 0, 0);
      var result = cut.GenerateFillLevelReadings(config);

      Assert.AreEqual(3, result.Count());
      Assert.AreEqual(config.Begin, result.First()
                                          .DueDate.Begin);
      Assert.AreEqual(config.End, result.First()
                                        .DueDate.End);
      Assert.AreEqual(beginMiddle, result.Take(2)
                                         .Last()
                                         .DueDate.Begin);
      Assert.AreEqual(endMiddle, result.Take(2)
                                       .Last()
                                       .DueDate.End);
      Assert.AreEqual(beginEnd, result.Last()
                                      .DueDate.Begin);
      Assert.AreEqual(endEnd, result.Last()
                                    .DueDate.End);
    }

    [Test]
    public void Generate_Daily_Tasks_Recurrence_FullDay()
    {
      var cut = new TaskGenerator();
      var config = AdvancedSetup();

      config.EndsWithDate = false;
      config.IsAllDay = true;

      var lastDay = new DateTime(2013, 5, 29);


      var result = cut.GenerateFillLevelReadings(config);
      Assert.AreEqual(5, result.Count());
      Assert.AreEqual(true, result.First()
                                  .DueDate.IsAllDay);
      Assert.AreEqual(lastDay, result.Last()
                                     .DueDate.Begin.Date);
    }

    [Test]
    public void Generate_Monthly_Tasks_End_Date_DayOfMonthRecurrence()
    {
      var cut = new TaskGenerator();
      var config = AdvancedSetup();

      var RepeatUntilDate = new DateTime(2014, 1, 23, 0, 0, 0);
      config.RepeatUntilDate = RepeatUntilDate;
      config.EndsWithDate = true;
      config.NumberOfRecurrences = 10;
      config.Cycle = 2;
      var beginFirst = new DateTime(2013, 5, 21, 10, 0, 0);
      var endFirst = new DateTime(2013, 5, 21, 11, 0, 0);
      var beginMiddle = new DateTime(2013, 9, 21, 10, 0, 0);
      var endMiddle = new DateTime(2013, 9, 21, 11, 0, 0);
      var beginEnd = new DateTime(2014, 1, 21, 10, 0, 0);
      var endEnd = new DateTime(2014, 1, 21, 11, 0, 0);
      var result = cut.GenerateFillLevelReadings(config);
      Assert.AreEqual(5, result.Count());
      Assert.AreEqual(beginFirst, result.First()
                                        .DueDate.Begin);
      Assert.AreEqual(endFirst, result.First()
                                      .DueDate.End);
      Assert.AreEqual(beginMiddle, result.Take(3)
                                         .Last()
                                         .DueDate.Begin);
      Assert.AreEqual(endMiddle, result.Take(3)
                                       .Last()
                                       .DueDate.End);
      Assert.AreEqual(beginEnd, result.Last()
                                      .DueDate.Begin);
      Assert.AreEqual(endEnd, result.Last()
                                    .DueDate.End);
    }

    [Test]
    public void Generate_Monthly_Tasks_End_Date_WeekdayRecurrence()
    {
      var cut = new TaskGenerator();
      var config = AdvancedSetup();

      var RepeatUntilDate = new DateTime(2014, 1, 23, 0, 0, 0);
      config.RepeatUntilDate = RepeatUntilDate;
      config.EndsWithDate = true;
      config.NumberOfRecurrences = 10;
      config.IsWeekdayRecurrence = true;
      config.Cycle = 2;
      var beginFirst = new DateTime(2013, 5, 21, 10, 0, 0);
      var endFirst = new DateTime(2013, 5, 21, 11, 0, 0);
      var beginMiddle = new DateTime(2013, 9, 17, 10, 0, 0);
      var endMiddle = new DateTime(2013, 9, 17, 11, 0, 0);
      var beginEnd = new DateTime(2014, 1, 21, 10, 0, 0);
      var endEnd = new DateTime(2014, 1, 21, 11, 0, 0);
      var result = cut.GenerateFillLevelReadings(config);
      Assert.AreEqual(5, result.Count());
      Assert.AreEqual(beginFirst, result.First()
                                        .DueDate.Begin);
      Assert.AreEqual(endFirst, result.First()
                                      .DueDate.End);
      Assert.AreEqual(beginMiddle, result.Take(3)
                                         .Last()
                                         .DueDate.Begin);
      Assert.AreEqual(endMiddle, result.Take(3)
                                       .Last()
                                       .DueDate.End);
      Assert.AreEqual(beginEnd, result.Last()
                                      .DueDate.Begin);
      Assert.AreEqual(endEnd, result.Last()
                                    .DueDate.End);
    }

    [Test]
    public void Generate_Monthly_Tasks_Leapyear()
    {
      var cut = new TaskGenerator();
      var config = AdvancedSetup();

      var beginFirst = new DateTime(2011, 12, 31, 23, 0, 0);
      var endFirst = new DateTime(2012, 1, 1, 1, 0, 0);
      var beginMiddle = new DateTime(2012, 2, 29, 23, 0, 0);
      var endMiddle = new DateTime(2012, 3, 1, 1, 0, 0);
      var beginEnd = new DateTime(2013, 12, 31, 23, 0, 0);
      var endEnd = new DateTime(2014, 1, 1, 1, 0, 0);

      config.Begin = new DateTime(2011, 12, 31, 23, 0, 0);
      config.End = new DateTime(2012, 1, 1, 1, 0, 0);
      config.IsWeekdayRecurrence = false;
      config.RecurrenceInterval = 1;
      config.NumberOfRecurrences = 25;
      config.Cycle = 2;
      var result = cut.GenerateFillLevelReadings(config);
      Assert.AreEqual(25, result.Count());
      Assert.AreEqual(beginFirst, result.First()
                                        .DueDate.Begin);
      Assert.AreEqual(endFirst, result.First()
                                      .DueDate.End);
      Assert.AreEqual(beginMiddle, result.Take(3)
                                         .Last()
                                         .DueDate.Begin);
      Assert.AreEqual(endMiddle, result.Take(3)
                                       .Last()
                                       .DueDate.End);
      Assert.AreEqual(beginEnd, result.Last()
                                      .DueDate.Begin);
      Assert.AreEqual(endEnd, result.Last()
                                    .DueDate.End);
    }

    [Test]
    public void Generate_Monthly_Tasks_Recurrence_DayOfMonthRecurrence()
    {
      var cut = new TaskGenerator();
      var config = AdvancedSetup();
      config.Cycle = 2;
      var beginFirst = new DateTime(2013, 5, 21, 10, 0, 0);
      var endFirst = new DateTime(2013, 5, 21, 11, 0, 0);
      var beginMiddle = new DateTime(2013, 9, 21, 10, 0, 0);
      var endMiddle = new DateTime(2013, 9, 21, 11, 0, 0);
      var beginEnd = new DateTime(2014, 1, 21, 10, 0, 0);
      var endEnd = new DateTime(2014, 1, 21, 11, 0, 0);


      var result = cut.GenerateFillLevelReadings(config);
      Assert.AreEqual(5, result.Count());
      Assert.AreEqual(beginFirst, result.First()
                                        .DueDate.Begin);
      Assert.AreEqual(endFirst, result.First()
                                      .DueDate.End);
      Assert.AreEqual(beginMiddle, result.Take(3)
                                         .Last()
                                         .DueDate.Begin);
      Assert.AreEqual(endMiddle, result.Take(3)
                                       .Last()
                                       .DueDate.End);
      Assert.AreEqual(beginEnd, result.Last()
                                      .DueDate.Begin);
      Assert.AreEqual(endEnd, result.Last()
                                    .DueDate.End);
    }

    [Test]
    public void Generate_Monthly_Tasks_Recurrence_WeekdayRecurrence()
    {
      var cut = new TaskGenerator();
      var config = AdvancedSetup();

      var beginFirst = new DateTime(2013, 5, 21, 10, 0, 0);
      var endFirst = new DateTime(2013, 5, 21, 11, 0, 0);
      var beginMiddle = new DateTime(2013, 9, 17, 10, 0, 0);
      var endMiddle = new DateTime(2013, 9, 17, 11, 0, 0);
      var beginEnd = new DateTime(2014, 1, 21, 10, 0, 0);
      var endEnd = new DateTime(2014, 1, 21, 11, 0, 0);

      config.IsWeekdayRecurrence = true;
      config.Cycle = 2;
      var result = cut.GenerateFillLevelReadings(config);
      Assert.AreEqual(5, result.Count());
      Assert.AreEqual(beginFirst, result.First()
                                        .DueDate.Begin);
      Assert.AreEqual(endFirst, result.First()
                                      .DueDate.End);
      Assert.AreEqual(beginMiddle, result.Take(3)
                                         .Last()
                                         .DueDate.Begin);
      Assert.AreEqual(endMiddle, result.Take(3)
                                       .Last()
                                       .DueDate.End);
      Assert.AreEqual(beginEnd, result.Last()
                                      .DueDate.Begin);
      Assert.AreEqual(endEnd, result.Last()
                                    .DueDate.End);
    }

    [Test]
    public void Generate_Monthly_Tasks_WeekdayRecurrence_Special31()
    {
      var cut = new TaskGenerator();
      var config = AdvancedSetup();

      var beginFirst = new DateTime(2013, 5, 31, 10, 0, 0);
      var endFirst = new DateTime(2013, 5, 31, 11, 0, 0);
      var beginMiddle = new DateTime(2013, 7, 31, 10, 0, 0);
      var endMiddle = new DateTime(2013, 7, 31, 11, 0, 0);
      var beginEnd = new DateTime(2013, 9, 30, 10, 0, 0);
      var endEnd = new DateTime(2013, 9, 30, 11, 0, 0);

      config.Begin = new DateTime(2013, 5, 31, 10, 0, 0);
      config.End = new DateTime(2013, 5, 31, 11, 0, 0);
      config.IsWeekdayRecurrence = false;
      config.RecurrenceInterval = 1;
      config.Cycle = 2;
      var result = cut.GenerateFillLevelReadings(config);
      Assert.AreEqual(5, result.Count());
      Assert.AreEqual(beginFirst, result.First()
                                        .DueDate.Begin);
      Assert.AreEqual(endFirst, result.First()
                                      .DueDate.End);
      Assert.AreEqual(beginMiddle, result.Take(3)
                                         .Last()
                                         .DueDate.Begin);
      Assert.AreEqual(endMiddle, result.Take(3)
                                       .Last()
                                       .DueDate.End);
      Assert.AreEqual(beginEnd, result.Last()
                                      .DueDate.Begin);
      Assert.AreEqual(endEnd, result.Last()
                                    .DueDate.End);
    }

    [Test]
    public void Generate_Monthly_Tasks_WeekdayRecurrence_SpecialLastWeekdayinMonth()
    {
      var cut = new TaskGenerator();
      var config = AdvancedSetup();

      var beginFirst = new DateTime(2013, 5, 31, 10, 0, 0);
      var endFirst = new DateTime(2013, 5, 31, 11, 0, 0);
      var beginMiddle = new DateTime(2013, 7, 26, 10, 0, 0);
      var endMiddle = new DateTime(2013, 7, 26, 11, 0, 0);
      var beginEnd = new DateTime(2013, 9, 27, 10, 0, 0);
      var endEnd = new DateTime(2013, 9, 27, 11, 0, 0);

      config.Begin = new DateTime(2013, 5, 31, 10, 0, 0);
      config.End = new DateTime(2013, 5, 31, 11, 0, 0);
      config.IsWeekdayRecurrence = true;
      config.RecurrenceInterval = 1;
      config.Cycle = 2;
      var result = cut.GenerateFillLevelReadings(config);
      Assert.AreEqual(5, result.Count());
      Assert.AreEqual(beginFirst, result.First()
                                        .DueDate.Begin);
      Assert.AreEqual(endFirst, result.First()
                                      .DueDate.End);
      Assert.AreEqual(beginMiddle, result.Take(3)
                                         .Last()
                                         .DueDate.Begin);
      Assert.AreEqual(endMiddle, result.Take(3)
                                       .Last()
                                       .DueDate.End);
      Assert.AreEqual(beginEnd, result.Last()
                                      .DueDate.Begin);
      Assert.AreEqual(endEnd, result.Last()
                                    .DueDate.End);
    }

    [Test]
    public void Generate_Single_Task()
    {
      var cut = new TaskGenerator();

      var config = Setup();

      var begin = new DateTime(2000, 1, 1, 0, 0, 0);
      var end = new DateTime(2000, 1, 1, 1, 0, 0);
      config.Repeat = false;

      var result = cut.GenerateFillLevelReadings(config);

      Assert.AreEqual(1, result.Count());
      Assert.AreEqual(begin, result.First()
                                   .DueDate.Begin);
      Assert.AreEqual(end, result.First()
                                 .DueDate.End);
      Assert.AreEqual(config.ResponsibleSubject, result.First()
                                                       .AppointmentResponsibleSubject);
      Assert.AreEqual(config.Container, result.First()
                                              .ReadingContainer);
    }

    [Test]
    public void Generate_Weekly_Task_On_Muliple_Weekdays_End_Date()
    {
      var cut = new TaskGenerator();
      var config = AdvancedSetup();

      config.RepeatUntilDate = new DateTime(2013, 6, 20, 0, 0, 0);
      config.EndsWithDate = true;
      config.Cycle = 1;
      var beginFirst = new DateTime(2013, 5, 22, 10, 0, 0);
      var endFirst = new DateTime(2013, 5, 22, 11, 0, 0);
      var beginMiddle = new DateTime(2013, 6, 5, 10, 0, 0);
      var endMiddle = new DateTime(2013, 6, 5, 11, 0, 0);
      var beginEnd = new DateTime(2013, 6, 19, 10, 0, 0);
      var endEnd = new DateTime(2013, 6, 19, 11, 0, 0);
      var result = cut.GenerateFillLevelReadings(config);
      Assert.AreEqual(5, result.Count());
      Assert.AreEqual(beginFirst, result.First()
                                        .DueDate.Begin);
      Assert.AreEqual(endFirst, result.First()
                                      .DueDate.End);
      Assert.AreEqual(beginMiddle, result.Take(3)
                                         .Last()
                                         .DueDate.Begin);
      Assert.AreEqual(endMiddle, result.Take(3)
                                       .Last()
                                       .DueDate.End);
      Assert.AreEqual(beginEnd, result.Last()
                                      .DueDate.Begin);
      Assert.AreEqual(endEnd, result.Last()
                                    .DueDate.End);
    }

    [Test]
    public void Generate_Weekly_Task_On_Multiple_Weekdays_Recurrence()
    {
      var cut = new TaskGenerator();
      var config = AdvancedSetup();
      config.Cycle = 1;
      var beginFirst = new DateTime(2013, 5, 22, 10, 0, 0);
      var endFirst = new DateTime(2013, 5, 22, 11, 0, 0);
      var beginMiddle = new DateTime(2013, 6, 5, 10, 0, 0);
      var endMiddle = new DateTime(2013, 6, 5, 11, 0, 0);
      var beginEnd = new DateTime(2013, 6, 19, 10, 0, 0);
      var endEnd = new DateTime(2013, 6, 19, 11, 0, 0);
      var result = cut.GenerateFillLevelReadings(config);
      Assert.AreEqual(5, result.Count());
      Assert.AreEqual(beginFirst, result.First()
                                        .DueDate.Begin);
      Assert.AreEqual(endFirst, result.First()
                                      .DueDate.End);
      Assert.AreEqual(beginMiddle, result.Take(3)
                                         .Last()
                                         .DueDate.Begin);
      Assert.AreEqual(endMiddle, result.Take(3)
                                       .Last()
                                       .DueDate.End);
      Assert.AreEqual(beginEnd, result.Last()
                                      .DueDate.Begin);
      Assert.AreEqual(endEnd, result.Last()
                                    .DueDate.End);
    }

    [Test]
    public void Generate_Yearly_Tasks_End_Date()
    {
      var cut = new TaskGenerator();
      var config = AdvancedSetup();
      config.Cycle = 3;
      var RepeatUntilDate = new DateTime(2021, 5, 30, 0, 0, 0);
      config.RepeatUntilDate = RepeatUntilDate;
      config.EndsWithDate = true;

      var beginFirst = new DateTime(2013, 5, 21, 10, 0, 0);
      var endFirst = new DateTime(2013, 5, 21, 11, 0, 0);
      var beginMiddle = new DateTime(2017, 5, 21, 10, 0, 0);
      var endMiddle = new DateTime(2017, 5, 21, 11, 0, 0);
      var beginEnd = new DateTime(2021, 5, 21, 10, 0, 0);
      var endEnd = new DateTime(2021, 5, 21, 11, 0, 0);
      var result = cut.GenerateFillLevelReadings(config);
      Assert.AreEqual(5, result.Count());
      Assert.AreEqual(beginFirst, result.First()
                                        .DueDate.Begin);
      Assert.AreEqual(endFirst, result.First()
                                      .DueDate.End);
      Assert.AreEqual(beginMiddle, result.Take(3)
                                         .Last()
                                         .DueDate.Begin);
      Assert.AreEqual(endMiddle, result.Take(3)
                                       .Last()
                                       .DueDate.End);
      Assert.AreEqual(beginEnd, result.Last()
                                      .DueDate.Begin);
      Assert.AreEqual(endEnd, result.Last()
                                    .DueDate.End);
    }

    [Test]
    public void Generate_Yearly_Tasks_Recurrence()
    {
      var cut = new TaskGenerator();
      var config = AdvancedSetup();
      config.Cycle = 3;
      config.EndsWithDate = false;

      var beginFirst = new DateTime(2013, 5, 21, 10, 0, 0);
      var endFirst = new DateTime(2013, 5, 21, 11, 0, 0);
      var beginMiddle = new DateTime(2017, 5, 21, 10, 0, 0);
      var endMiddle = new DateTime(2017, 5, 21, 11, 0, 0);
      var beginEnd = new DateTime(2021, 5, 21, 10, 0, 0);
      var endEnd = new DateTime(2021, 5, 21, 11, 0, 0);
      var result = cut.GenerateFillLevelReadings(config);
      Assert.AreEqual(5, result.Count());
      Assert.AreEqual(beginFirst, result.First()
                                        .DueDate.Begin);
      Assert.AreEqual(endFirst, result.First()
                                      .DueDate.End);
      Assert.AreEqual(beginMiddle, result.Take(3)
                                         .Last()
                                         .DueDate.Begin);
      Assert.AreEqual(endMiddle, result.Take(3)
                                       .Last()
                                       .DueDate.End);
      Assert.AreEqual(beginEnd, result.Last()
                                      .DueDate.Begin);
      Assert.AreEqual(endEnd, result.Last()
                                    .DueDate.End);
    }
  }
}