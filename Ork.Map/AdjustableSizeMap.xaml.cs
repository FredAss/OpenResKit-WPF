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
using System.Windows.Input;
using System.Windows.Media;

namespace Ork.Map
{
  /// <summary>
  ///   Interaction logic for AdjustableSizeMap.xaml
  /// </summary>
  public partial class AdjustableSizeMap : UserControl
  {
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof (IEnumerable), typeof (AdjustableSizeMap), new FrameworkPropertyMetadata());


    public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof (ControlTemplate), typeof (AdjustableSizeMap), new FrameworkPropertyMetadata());


    public static readonly DependencyProperty MapSourceProperty = DependencyProperty.Register("MapSource", typeof (ImageSource), typeof (AdjustableSizeMap), new FrameworkPropertyMetadata());

    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof (object), typeof (AdjustableSizeMap), new FrameworkPropertyMetadata());

    public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register("Zoom", typeof (double), typeof (AdjustableSizeMap), new FrameworkPropertyMetadata(1d));


    public static readonly DependencyProperty MapHeightProperty = DependencyProperty.Register("MapHeight", typeof (double), typeof (AdjustableSizeMap), new FrameworkPropertyMetadata());


    public static readonly DependencyProperty MapWidthProperty = DependencyProperty.Register("MapWidth", typeof (double), typeof (AdjustableSizeMap), new FrameworkPropertyMetadata());


    public static readonly DependencyProperty MaxZoomLevelProperty = DependencyProperty.Register("MaxZoomLevel", typeof (double), typeof (AdjustableSizeMap), new FrameworkPropertyMetadata(100d));


    private ContentControl m_CurrentContentControl;
    private bool m_IsCtrlLocked;


    private Point? m_LastDragPoint;
    private Alignment m_MarkerAlignment = Alignment.Center;

    public AdjustableSizeMap()
    {
      InitializeComponent();

      DataContextChanged += AdjustableSizeMapDataContextChanged;
    }

    public ControlTemplate ItemTemplate
    {
      get { return (ControlTemplate) GetValue(ItemTemplateProperty); }
      set { SetValue(ItemTemplateProperty, value); }
    }

    public IEnumerable ItemsSource
    {
      get { return (IEnumerable) GetValue(ItemsSourceProperty); }
      set { SetValue(ItemsSourceProperty, value); }
    }

    public double MapHeight
    {
      get { return (double) GetValue(MapHeightProperty); }
      set { SetValue(MapHeightProperty, value); }
    }

    public ImageSource MapSource
    {
      get { return (ImageSource) GetValue(MapSourceProperty); }
      set { SetValue(MapSourceProperty, value); }
    }

    public double MapWidth
    {
      get { return (double) GetValue(MapWidthProperty); }
      set { SetValue(MapWidthProperty, value); }
    }

    public Alignment MarkerAlignment
    {
      get { return m_MarkerAlignment; }
      set { m_MarkerAlignment = value; }
    }

    public double MaxZoomLevel
    {
      get { return (double) GetValue(MaxZoomLevelProperty); }
      set { SetValue(MaxZoomLevelProperty, value); }
    }

    public object SelectedItem
    {
      get { return GetValue(SelectedItemProperty); }
      set { SetValue(SelectedItemProperty, value); }
    }

    public double Zoom
    {
      get { return (double) GetValue(ZoomProperty); }
      set
      {
        SetValue(ZoomProperty, value);
        //Map.Width = MapSource.Width * Zoom;
        //Map.Height = MapSource.Height * Zoom;
        //CanvasAdjustableSizeMap.Width = MapSource.Width * Zoom;
        //CanvasAdjustableSizeMap.Height = MapSource.Height * Zoom;
      }
    }

    private void AdjustableSizeMapDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      // TODO: this is a temp solution because binding (ItemsSource <--> ItemsSource) does not work.
      if (DataContext is IEnumerable)
      {
        ItemsSource = (IEnumerable) DataContext;
      }
    }

    private void AdjustableSizeMapMouseWheel(object sender, MouseWheelEventArgs e)
    {
      ZoomInOut(e.Delta);
    }

    private void AdjustableSizeMapPreviewKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.LeftCtrl | e.Key == Key.RightCtrl)
      {
        m_IsCtrlLocked = true;
      }
    }

    private void AdjustableSizeMapPreviewKeyUp(object sender, KeyEventArgs e)
    {
      m_IsCtrlLocked = false;
    }

    private void AdjustableSizeMapPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
      ZoomInOut(e.Delta);
    }

    private void CanvasAdjustableSizeMapPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      var mouseCoords = e.GetPosition(CanvasAdjustableSizeMap);
      var selectedMarker = CanvasAdjustableSizeMap.SelectedItem;
      var selectedListBoxItem = CanvasAdjustableSizeMap.ItemContainerGenerator.ContainerFromItem(selectedMarker) as ListBoxItem;

      if (selectedListBoxItem == null)
      {
        return;
      }

      m_CurrentContentControl = FindVisualChildByName<ContentControl>(selectedListBoxItem, "Marker");

      var topSize = (mouseCoords.Y / Zoom);
      var leftSize = (mouseCoords.X / Zoom);

      Orientation.SetScaledPosition(ref topSize, ref leftSize, m_CurrentContentControl.ActualHeight, m_CurrentContentControl.ActualWidth, Zoom, Alignment.Center);

      Canvas.SetTop(m_CurrentContentControl, topSize);
      Canvas.SetLeft(m_CurrentContentControl, leftSize);

      m_CurrentContentControl.CaptureMouse();
    }

    private static TChild FindVisualChild<TChild>(DependencyObject obj) where TChild : DependencyObject
    {
      for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
      {
        var child = VisualTreeHelper.GetChild(obj, i);
        if (child != null &&
            child is TChild)
        {
          return (TChild) child;
        }

        var childOfChild = FindVisualChild<TChild>(child);
        if (childOfChild != null)
        {
          return childOfChild;
        }
      }
      return null;
    }

    private static TChild FindVisualChildByName<TChild>(DependencyObject obj, string name) where TChild : FrameworkElement
    {
      for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
      {
        var child = VisualTreeHelper.GetChild(obj, i);
        if (child != null &&
            child is TChild &&
            ((TChild) child).Name == name)
        {
          return (TChild) child;
        }

        var childOfChild = FindVisualChild<TChild>(child);
        if (childOfChild != null)
        {
          return childOfChild;
        }
      }
      return null;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      MapHeight = ActualHeight;
      MapWidth = ActualWidth;
    }

    private void ScrollViewerAdjustableSizeMapMouseMove(object sender, MouseEventArgs e)
    {
      if (m_LastDragPoint.HasValue)
      {
        var posNow = e.GetPosition(ScrollViewerAdjustableSizeMap);

        var dX = posNow.X - m_LastDragPoint.Value.X;
        var dY = posNow.Y - m_LastDragPoint.Value.Y;

        m_LastDragPoint = posNow;

        ScrollViewerAdjustableSizeMap.ScrollToHorizontalOffset(ScrollViewerAdjustableSizeMap.HorizontalOffset - dX);
        ScrollViewerAdjustableSizeMap.ScrollToVerticalOffset(ScrollViewerAdjustableSizeMap.VerticalOffset - dY);
      }
    }

    private void ScrollViewerAdjustableSizeMapPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      var mousePos = e.GetPosition(ScrollViewerAdjustableSizeMap);
      if (mousePos.X <= ScrollViewerAdjustableSizeMap.ViewportWidth &&
          mousePos.Y < ScrollViewerAdjustableSizeMap.ViewportHeight)
      {
        ScrollViewerAdjustableSizeMap.Cursor = Cursors.SizeAll;
        m_LastDragPoint = mousePos;
        Mouse.Capture(ScrollViewerAdjustableSizeMap);
      }
    }

    private void ScrollViewerAdjustableSizeMapPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      ScrollViewerAdjustableSizeMap.Cursor = Cursors.Arrow;
      ScrollViewerAdjustableSizeMap.ReleaseMouseCapture();
      m_LastDragPoint = null;
    }


    private void ScrollViewerAdjustableSizeMapPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
      e.Handled = true;
    }

    private void ZoomInOut(int mouseDelta)
    {
      if (m_IsCtrlLocked)
      {
        if (mouseDelta == 120)
        {
          if (MapHeight < ActualHeight * MaxZoomLevel)
          {
            MapHeight += 20;
            MapWidth += 20;
          }
        }
        else
        {
          if (MapHeight > ActualHeight + 20)
          {
            MapHeight -= 20;
            MapWidth -= 20;
          }
          else
          {
            MapHeight = ActualHeight - 16;
            MapWidth = ActualWidth - 16;
          }
        }
      }
    }
  }
}