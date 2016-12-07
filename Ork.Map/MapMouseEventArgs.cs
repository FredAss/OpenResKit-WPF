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
using System.Collections.Generic;
using System.Windows;
using Ork.Map.ViewModels;

namespace Ork.Map
{
  public delegate void MapMouseEventHandler(object sender, MapMouseEventArgs e);


  public class MapMouseEventArgs : EventArgs
  {
    private readonly Point m_Coordinates;
    private readonly IEnumerable<MapItemViewModel> m_SelectedMapItems;


    public MapMouseEventArgs(Point coordinates, IEnumerable<MapItemViewModel> selectedMapItems)
    {
      m_Coordinates = coordinates;
      m_SelectedMapItems = selectedMapItems;
    }


    public Point Coordinates
    {
      get { return m_Coordinates; }
    }


    public IEnumerable<MapItemViewModel> SelectedMapItems
    {
      get { return m_SelectedMapItems; }
    }
  }
}