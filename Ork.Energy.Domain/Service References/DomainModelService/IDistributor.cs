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

namespace Ork.Energy.Domain.DomainModelService
{
  public interface IDistributor
  {
    /// <summary>
    ///   There are no comments for Property Id in the schema.
    /// </summary>
    int Id { get; set; }

    /// <summary>
    ///   There are no comments for Property Name in the schema.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    ///   There are no comments for Property Comment in the schema.
    /// </summary>
    string Comment { get; set; }

    /// <summary>
    ///   There are no comments for Property IsMainDistributor in the schema.
    /// </summary>
    bool IsMainDistributor { get; set; }

    /// <summary>
    ///   There are no comments for Readings in the schema.
    /// </summary>
    DataServiceCollection<Reading> Readings { get; set; }

    /// <summary>
    ///   There are no comments for Room in the schema.
    /// </summary>
    Room Room { get; set; }
  }
}