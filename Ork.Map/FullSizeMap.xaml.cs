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

using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ork.Map
{
  /// <summary>
  ///   Interaction logic for FullSizeMap.xaml
  /// </summary>
  public partial class FullSizeMap : UserControl
  {
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof (IEnumerable), typeof (FullSizeMap), new FrameworkPropertyMetadata());


    public static readonly DependencyProperty MapSourceProperty = DependencyProperty.Register("MapSource", typeof (ImageSource), typeof (FullSizeMap), new FrameworkPropertyMetadata());


    public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof (DataTemplate), typeof (FullSizeMap));

    public static readonly DependencyProperty ItemTemplateSelectorProperty = DependencyProperty.Register("ItemTemplateSelector", typeof (DataTemplateSelector), typeof (FullSizeMap));

    public FullSizeMap()
    {
      InitializeComponent();

      DataContextChanged += FullSizeMapDataContextChanged;
    }

    public DataTemplate ItemTemplate
    {
      get { return (DataTemplate) GetValue(ItemTemplateProperty); }
      set { SetValue(ItemTemplateProperty, value); }
    }

    public DataTemplateSelector ItemTemplateSelector
    {
      get { return (DataTemplateSelector) GetValue(ItemTemplateSelectorProperty); }
      set { SetValue(ItemTemplateSelectorProperty, value); }
    }

    public IEnumerable ItemsSource
    {
      get { return (IEnumerable) GetValue(ItemsSourceProperty); }
      set { SetValue(ItemsSourceProperty, value); }
    }

    public ImageSource MapSource
    {
      get { return (ImageSource) GetValue(MapSourceProperty); }
      set { SetValue(MapSourceProperty, value); }
    }

    private void FullSizeMapDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      // TODO: this is a temp solution because binding (ItemsSource <--> ItemsSource) does not work.
      if (DataContext is IEnumerable)
      {
        ItemsSource = (IEnumerable) DataContext;
      }
    }
  }
}