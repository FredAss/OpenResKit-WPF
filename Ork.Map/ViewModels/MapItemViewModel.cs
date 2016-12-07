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

using System.ComponentModel;
using System.Windows;

namespace Ork.Map.ViewModels
{
  public class MapItemViewModel : INotifyPropertyChanged
  {
    public const string IsSelectedPropertyName = "IsSelected"; // NON-NLS-1
    public const string ItemPropertyName = "Item"; // NON-NLS-1
    public const string LocationPropertyName = "Location"; // NON-NLS-1
    private bool m_IsSelected;
    private object m_Item;
    private Point m_Location;


    public MapItemViewModel(object item, Point location, bool isSelected)
    {
      m_Item = item;
      m_Location = location;
      m_IsSelected = isSelected;
    }


    public bool IsSelected
    {
      get { return m_IsSelected; }
      set
      {
        if (m_IsSelected == value)
        {
          return;
        }
        m_IsSelected = value;
        RaisePropertyChanged(IsSelectedPropertyName);
      }
    }

    public object Item
    {
      get { return m_Item; }
      set
      {
        if (value.Equals(m_Item))
        {
          return;
        }
        m_Item = value;
        RaisePropertyChanged(ItemPropertyName);
      }
    }

    public Point Location
    {
      get { return m_Location; }
      set
      {
        if (m_Location == value)
        {
          return;
        }
        m_Location = value;
        RaisePropertyChanged(LocationPropertyName);
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;


    private void RaisePropertyChanged(string propertyName)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
      }
    }
  }
}