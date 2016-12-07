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
using System.IO;
using System.Windows.Media.Imaging;
using Caliburn.Micro;

namespace Ork.Waste.ViewModels
{
  public class MapViewModel : PropertyChangedBase
  {
    private readonly BitmapImage m_Map;
    private readonly DomainModelService.Map m_Model;

    public MapViewModel(DomainModelService.Map model)
    {
      m_Model = model;
      m_Model.PropertyChanged += ModelPropertyChanged;
      m_Map = GetBitmapImageFromByteArray(model.MapSource.BinarySource);
    }

    public BitmapImage Map
    {
      get { return m_Map; }
    }

    public DomainModelService.Map Model
    {
      get { return m_Model; }
    }

    public string Name
    {
      get { return m_Model.Name; }
    }

    private BitmapImage GetBitmapImageFromByteArray(byte[] bitmapImage)
    {
      var image = new BitmapImage();
      if (bitmapImage.Length == 0)
      {
        return image;
      }
      image.BeginInit();
      image.StreamSource = new MemoryStream(bitmapImage);
      image.EndInit();
      return image;
    }

    private void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      NotifyOfPropertyChange(e.PropertyName);
    }
  }
}