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

using System.Windows;
using System.Windows.Controls;

namespace Ork.Map
{
  /// <summary>
  ///   Interaction logic for ZoomSlider.xaml
  /// </summary>
  public partial class ZoomSlider : UserControl
  {
    public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register("MinValue", typeof (int), typeof (ZoomSlider), new FrameworkPropertyMetadata(10));


    public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register("MaxValue", typeof (int), typeof (ZoomSlider), new FrameworkPropertyMetadata(500));


    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof (int), typeof (ZoomSlider), new FrameworkPropertyMetadata(100));

    public ZoomSlider()
    {
      InitializeComponent();
    }

    public int MaxValue
    {
      get { return (int) GetValue(MaxValueProperty); }
      set { SetValue(MaxValueProperty, value); }
    }

    public int MinValue
    {
      get { return (int) GetValue(MinValueProperty); }
      set { SetValue(MinValueProperty, value); }
    }

    public int Value
    {
      get { return (int) GetValue(ValueProperty); }
      set { SetValue(ValueProperty, value); }
    }

    public event ZoomSliderEventHandler CenteredClicked;
    public event ZoomSliderEventHandler ValueChanged;
    public event ZoomSliderEventHandler ZoomChanged;


    protected void OnCenteredClicked()
    {
      var zoomSliderEventArgs = new ZoomSliderEventArgs(MaxValue, MinValue, Value);

      if (CenteredClicked != null)
      {
        CenteredClicked(this, zoomSliderEventArgs);
      }
    }


    /// <summary>
    ///   Event handler when slider value changes.
    /// </summary>
    /// <param name="newValue"></param>
    protected void OnValueChanged(int newValue)
    {
      var zoomSliderEventArgs = new ZoomSliderEventArgs(MaxValue, MinValue, Value, newValue);

      if (ValueChanged != null)
      {
        ValueChanged(this, zoomSliderEventArgs);
      }
    }


    /// <summary>
    ///   Event handler when user zooms with context menu.
    /// </summary>
    /// <param name="newValue"></param>
    protected void OnZoomChanged(int newValue)
    {
      var zoomSliderEventArgs = new ZoomSliderEventArgs(MaxValue, MinValue, Value, newValue);

      if (ZoomChanged != null)
      {
        ZoomChanged(this, zoomSliderEventArgs);
      }
    }

    private void ButtonFitToScreenClick(object sender, RoutedEventArgs e)
    {
      OnCenteredClicked();
    }


    private void SliderZoomLevelValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      // TODO: implement snapping mechanism (e.g. snap when in the near of 100% +/- 5%)
      if (m_SliderZoomLevel != null)
      {
        if (e.NewValue >= 97d &&
            e.NewValue <= 103d)
        {
          m_SliderZoomLevel.IsSnapToTickEnabled = true;
        }
        else
        {
          m_SliderZoomLevel.IsSnapToTickEnabled = false;
        }
      }

      OnValueChanged((int) e.NewValue);
    }
  }
}