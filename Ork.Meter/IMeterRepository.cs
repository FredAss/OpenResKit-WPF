﻿#region License

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
using Ork.Meter.DomainModelService;

namespace Ork.Meter
{
  public interface IMeterRepository
  {
    DataServiceCollection<DomainModelService.Meter> Meter { get; }
    IEnumerable<EntityDescriptor> Entities { get; }
    DataServiceCollection<ScheduledTask> MeterReadings { get; }
    bool HasConnection { get; }
    IEnumerable<LinkDescriptor> Links { get; }
    DataServiceCollection<DomainModelService.Map> Maps { get; }
    DataServiceCollection<ResponsibleSubject> ResponsibleSubjects { get; }
    DataServiceCollection<Series> Series { get; }
    event EventHandler ContextChanged;
    event EventHandler SaveCompleted;
    void DeleteObject(object objectToDelete);
    void Save();
  }
}