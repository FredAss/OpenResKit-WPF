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

using System.Collections.Generic;
using Ork.Energy.Domain.DomainModelService;

namespace Ork.Energy.ViewModels
{
  public interface IDistributorModifyViewModel
  {
    Distributor Model { get; }
    ReadingAddViewModel ReadingAddVm { get; set; }
    string Name { get; set; }
    string Comment { get; set; }
    bool IsMainDistributor { get; set; }
    IList<ReadingViewModel> Readings { get; }
    Room Room { get; set; }
    IEnumerable<Room> Rooms { get; }
    void AddNewReading(object dataContext);
    void DeleteReading(object dataContext);
  }
}