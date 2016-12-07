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
using System.IO;
using Ork.Waste.DomainModelService;
using Ork.Waste.ViewModels;

namespace Ork.Waste.Factories
{
  [Export(typeof (IMapViewModelFactory))]
  public class MapViewModelFactory : IMapViewModelFactory
  {
    public MapAddViewModel CreateAddViewModel(string fileName)
    {
      Stream imageStream = File.OpenRead(fileName);

      byte[] byteArray;
      using (var br = new BinaryReader(imageStream))
      {
        byteArray = br.ReadBytes((int) imageStream.Length);
      }

      return new MapAddViewModel(new DomainModelService.Map
                                 {
                                   MapSource = new MapSource
                                               {
                                                 BinarySource = byteArray
                                               },
                                   Name = "Karte"
                                 });
    }

    public MapEditViewModel CreateEditViewModel(DomainModelService.Map model, Action removeMapAction)
    {
      return new MapEditViewModel(model, removeMapAction);
    }

    public MapViewModel CreateFromExisting(DomainModelService.Map model)
    {
      return new MapViewModel(model);
    }
  }
}