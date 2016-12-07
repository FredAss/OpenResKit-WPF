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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Ork.Map.ViewModels;

namespace Ork.Map.Controls
{
  /// <summary>
  ///   Interaction logic for MapPositionEditor.xaml
  /// </summary>
  public partial class MapPositionEditor : UserControl
  {
    //private const double MiniMapHeight = 150d;
    //private const double MiniMapWidth = 250d;

    public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register("SelectedItems", typeof (IEnumerable<object>), typeof (MapPositionEditor),
      new FrameworkPropertyMetadata
      {
        PropertyChangedCallback = OnSelectedItemsChanged
      });


    public static readonly DependencyProperty MapSourceProperty = DependencyProperty.Register("MapSource", typeof (ImageSource), typeof (MapPositionEditor), new FrameworkPropertyMetadata
                                                                                                                                                             {
                                                                                                                                                               PropertyChangedCallback =
                                                                                                                                                                 OnMapSourceChanged
                                                                                                                                                             });

    public static readonly DependencyProperty CenterOnMapChangeProperty = DependencyProperty.Register("CenterOnMapChange", typeof (bool), typeof (MapPositionEditor),
      new FrameworkPropertyMetadata(true));


    public static readonly DependencyProperty MapBackgroundProperty = DependencyProperty.Register("MapBackground", typeof (Brush), typeof (MapPositionEditor),
      new FrameworkPropertyMetadata(new SolidColorBrush(Colors.LightGray)));


    public static readonly DependencyProperty MiniMapVisibilityProperty = DependencyProperty.Register("MiniMapVisibility", typeof (Visibility), typeof (MapPositionEditor),
      new FrameworkPropertyMetadata(Visibility.Visible));


    public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof (ContentControl), typeof (MapPositionEditor), new FrameworkPropertyMetadata());


    public static readonly DependencyProperty MapSizeProperty = DependencyProperty.Register("MapSize", typeof (double), typeof (MapPositionEditor), new FrameworkPropertyMetadata());


    public static readonly DependencyProperty MapHeightProperty = DependencyProperty.Register("MapHeight", typeof (double), typeof (MapPositionEditor), new FrameworkPropertyMetadata());


    public static readonly DependencyProperty MapWidthProperty = DependencyProperty.Register("MapWidth", typeof (double), typeof (MapPositionEditor), new FrameworkPropertyMetadata());


    public static readonly DependencyProperty MinSizeProperty = DependencyProperty.Register("MinSize", typeof (double), typeof (MapPositionEditor), new FrameworkPropertyMetadata());


    public static readonly DependencyProperty MaxSizeProperty = DependencyProperty.Register("MaxSize", typeof (double), typeof (MapPositionEditor), new FrameworkPropertyMetadata());


    public static readonly DependencyProperty MinZoomLevelProperty = DependencyProperty.Register("MinZoomLevel", typeof (double), typeof (MapPositionEditor), new FrameworkPropertyMetadata(10d));


    public static readonly DependencyProperty MaxZoomLevelProperty = DependencyProperty.Register("MaxZoomLevel", typeof (double), typeof (MapPositionEditor), new FrameworkPropertyMetadata(500d));


    public static readonly DependencyProperty ZoomIntervalProperty = DependencyProperty.Register("ZoomInterval", typeof (double), typeof (MapPositionEditor), new FrameworkPropertyMetadata(10d));


    public static readonly DependencyProperty ZoomValueProperty = DependencyProperty.Register("ZoomValue", typeof (double), typeof (MapPositionEditor), new FrameworkPropertyMetadata(10d));


    public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register("Zoom", typeof (double), typeof (MapPositionEditor), new FrameworkPropertyMetadata(1d));

    //private bool m_IsCollapsedMiniMap = true;
    private bool m_IsCtrlKeyLocked;

    public MapPositionEditor()
    {
      InitializeComponent();

      DataContextChanged += MapPositionEditor_DataContextChanged;

      SetMiniMapBorderRange();
      //CenterIfNecessary();
      CenterMap();
    }

    public bool CenterOnMapChange
    {
      get { return (bool) GetValue(CenterOnMapChangeProperty); }
      set { SetValue(CenterOnMapChangeProperty, value); }
    }

    public ContentControl ItemTemplate
    {
      get { return (ContentControl) GetValue(ItemTemplateProperty); }
      set { SetValue(ItemTemplateProperty, value); }
    }

    public Brush MapBackground
    {
      get { return (Brush) GetValue(MapBackgroundProperty); }
      set { SetValue(MapBackgroundProperty, value); }
    }

    public double MapHeight
    {
      get { return (double) GetValue(MapHeightProperty); }
      set { SetValue(MapHeightProperty, value); }
    }

    public double MapSize
    {
      get { return (double) GetValue(MapSizeProperty); }
      set { SetValue(MapSizeProperty, value); }
    }

    public ImageSource MapSource
    {
      get { return (ImageSource) GetValue(MapSourceProperty); }
      set
      {
        SetValue(MapSourceProperty, value);
        CenterIfNecessary();
        //RaisePropertyChanged("MapSource");
      }
    }

    public double MapWidth
    {
      get { return (double) GetValue(MapWidthProperty); }
      set { SetValue(MapWidthProperty, value); }
    }

    public double MaxSize
    {
      get { return (double) GetValue(MaxSizeProperty); }
      set { SetValue(MaxSizeProperty, value); }
    }

    public double MaxZoomLevel
    {
      get { return (double) GetValue(MaxZoomLevelProperty); }
      set { SetValue(MaxZoomLevelProperty, value); }
    }

    public double MinSize
    {
      get { return (double) GetValue(MinSizeProperty); }
      set { SetValue(MinSizeProperty, value); }
    }

    public double MinZoomLevel
    {
      get { return (double) GetValue(MinZoomLevelProperty); }
      set { SetValue(MinZoomLevelProperty, value); }
    }

    //public Visibility MiniMapVisibility { get { return (Visibility) GetValue(MiniMapVisibilityProperty); } set { SetValue(MiniMapVisibilityProperty, value); } }
    public IEnumerable<object> SelectedItems
    {
      get { return (IEnumerable<object>) GetValue(SelectedItemsProperty); }
      set { SetValue(SelectedItemsProperty, value); }
    }

    public double Zoom
    {
      get { return (double) GetValue(ZoomProperty); }
      set { SetValue(ZoomProperty, value); }
    }

    public double ZoomInterval
    {
      get { return (double) GetValue(ZoomIntervalProperty); }
      set { SetValue(ZoomIntervalProperty, value); }
    }

    public double ZoomValue
    {
      get { return (double) GetValue(ZoomValueProperty); }
      set
      {
        if (value < MinZoomLevel ||
            value > MaxZoomLevel)
        {
          throw new InvalidOperationException("Value out of range");
        }
        SetValue(ZoomValueProperty, value);
      }
    }

    public event MapMouseEventHandler MapDoubleClicked;
    //public event PropertyChangedEventHandler PropertyChanged;


    public void CenterMap()
    {
      // 100% map size (original image size)
      var mapActualWidth = Map.ActualWidth;
      var mapActualHeight = Map.ActualHeight;

      var factorWidth = mapActualWidth / ActualWidth;
      var factorHeight = mapActualHeight / ActualHeight;

      if (mapActualHeight == 0.0 &&
          mapActualWidth == 0.0)
      {
        MapHeight = ActualHeight;
        MapWidth = ActualWidth;
      }
      else
      {
        var height = ActualHeight * 100 / mapActualHeight;
        var width = ActualWidth * 100 / mapActualWidth;

        if (factorHeight > factorWidth)
        {
          SetZoom((int) height);
        }
        else
        {
          SetZoom((int) width);
        }
      }

      SetMiniMapBorderRange();
    }

    public void SetSelectedItemsSource(IEnumerable<object> selectedItemsSource)
    {
      SelectedItems = selectedItemsSource;
    }


    public void SetZoom(int percentage)
    {
      if (percentage < MinZoomLevel)
      {
        percentage = (int) MinZoomLevel;
      }

      if (percentage > MaxZoomLevel)
      {
        percentage = (int) MaxZoomLevel;
      }

      ZoomMapTo(percentage);
    }

    protected void OnMapDoubleClicked(Point coordinates, IEnumerable<MapItemViewModel> selectedMapItems)
    {
      if (MapDoubleClicked != null)
      {
        MapDoubleClicked(this, new MapMouseEventArgs(coordinates, selectedMapItems));
      }
    }

    //private void ButtonMiniMapSizeClick(object sender, RoutedEventArgs e)
    //{
    //  if (m_IsCollapsedMiniMap)
    //  {
    //    GridMiniMap.Height = MiniMapHeight;
    //    GridMiniMap.Width = MiniMapWidth;

    //    BorderMiniMapRange.Visibility = Visibility.Visible;
    //    BorderMiniMap.Visibility = Visibility.Visible;
    //    MiniMap.Visibility = Visibility.Visible;

    //    m_IsCollapsedMiniMap = false;

    //    SetMiniMapBorderRange();
    //  }
    //  else
    //  {
    //    var buttonHeight = ButtonToggleMiniMapSize.ActualHeight;
    //    var buttonWidth = ButtonToggleMiniMapSize.ActualWidth;

    //    GridMiniMap.Height = buttonHeight;
    //    GridMiniMap.Width = buttonWidth;

    //    BorderMiniMapRange.Visibility = Visibility.Collapsed;
    //    BorderMiniMap.Visibility = Visibility.Collapsed;
    //    MiniMap.Visibility = Visibility.Collapsed;

    //    m_IsCollapsedMiniMap = true;
    //  }
    //}


    //private ContentControl m_CurrentContentControl;

    private void CanvasMapPositionEditorPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      var mouseCoords = e.GetPosition(CanvasMapPositionEditor);
      var selectedItems = ((IEnumerable<MapItemViewModel>) CanvasMapPositionEditor.ItemsSource).Where(item => item.IsSelected)
                                                                                               .ToList();

      OnMapDoubleClicked(new Point(mouseCoords.X, mouseCoords.Y), selectedItems);

      //var mouseCoords = e.GetPosition(CanvasMapPositionEditor);
      //var selectedMarker = CanvasMapPositionEditor.SelectedItem;


      //var selectedListBoxItem =
      //  CanvasMapPositionEditor.ItemContainerGenerator.ContainerFromItem(selectedMarker) as ListBoxItem;

      //if(selectedListBoxItem == null)
      //{
      //  return;
      //}

      //m_CurrentContentControl = FindVisualChildByName<ContentControl>(selectedListBoxItem, "Marker");

      //var topSize = (mouseCoords.Y / Zoom);
      //var leftSize = (mouseCoords.X / Zoom);

      //Orientation.SetScaledPosition(ref topSize,
      //                              ref leftSize,
      //                              m_CurrentContentControl.ActualHeight,
      //                              m_CurrentContentControl.ActualWidth,
      //                              Zoom,
      //                              Alignment.Center);

      //Canvas.SetTop(m_CurrentContentControl, topSize);
      //Canvas.SetLeft(m_CurrentContentControl, leftSize);
    }

    private void CenterIfNecessary()
    {
      if (CenterOnMapChange)
      {
        CenterMap();
      }
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

    private void MapPositionEditorMouseWheel(object sender, MouseWheelEventArgs e)
    {
      ZoomInOut(e.Delta);
    }

    private void MapPositionEditorPreviewKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.LeftCtrl | e.Key == Key.RightCtrl)
      {
        m_IsCtrlKeyLocked = true;
      }
    }


    private void MapPositionEditorPreviewKeyUp(object sender, KeyEventArgs e)
    {
      m_IsCtrlKeyLocked = false;
    }

    private void MapPositionEditorPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
      ZoomInOut(e.Delta);
    }

    private void MapPositionEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      SetMiniMapBorderRange();
      //CenterIfNecessary();
      CenterMap();
    }

    private static void OnSelectedItemsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      if (e.NewValue == e.OldValue)
      {
        return;
      }

      //var mapPositionEditor = (MapPositionEditor) dependencyObject;
      //mapPositionEditor.m_ViewModel.SelectedItems = (IEnumerable<object>) e.NewValue;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      SetMiniMapBorderRange();
    }

    //private void RaisePropertyChanged(string propertyName)
    //{
    //  if (PropertyChanged != null)
    //  {
    //    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    //  }
    //}

    private void ScrollViewerAdjustableSizeMapPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
      e.Handled = true;
    }

    private void SetMiniMapBorderRange()
    {
      //var scaledMiniMapHeight = BorderMiniMap.ActualHeight * ActualHeight / MapHeight;
      //var scaledMiniMapWidth = BorderMiniMap.ActualWidth * ActualWidth / MapWidth;

      //if (scaledMiniMapHeight > BorderMiniMap.ActualHeight)
      //{
      //  scaledMiniMapHeight = BorderMiniMap.ActualHeight;
      //}

      //if (scaledMiniMapWidth > BorderMiniMap.ActualWidth)
      //{
      //  scaledMiniMapWidth = BorderMiniMap.ActualWidth;
      //}

      ScrollViewerAdjustableSizeMap.UpdateLayout();

      //var verticalOffset = ScrollViewerAdjustableSizeMap.VerticalOffset * BorderMiniMap.ActualHeight / MapHeight;
      //var horizontalOffset = ScrollViewerAdjustableSizeMap.HorizontalOffset * BorderMiniMap.ActualWidth / MapWidth;

      //BorderMiniMapRange.Height = scaledMiniMapHeight;
      //BorderMiniMapRange.Width = scaledMiniMapWidth;

      //Canvas.SetTop(BorderMiniMapRange, verticalOffset);
      //Canvas.SetLeft(BorderMiniMapRange, horizontalOffset);
    }

    private void ZoomInOut(int mouseDelta)
    {
      if (m_IsCtrlKeyLocked)
      {
        if (mouseDelta == 120)
        {
          SetZoom((int) (ZoomValue + ZoomInterval));
        }
        else
        {
          SetZoom((int) (ZoomValue - ZoomInterval));
        }
      }

      SetMiniMapBorderRange();
    }

    private void ZoomMapTo(int percentage)
    {
      // 100% map size
      var mapActualWidth = Map.ActualWidth;
      var mapActualHeight = Map.ActualHeight;

      if (mapActualHeight == 0.0 &&
          mapActualWidth == 0.0)
      {
        MapHeight = ActualHeight;
        MapWidth = ActualWidth;

        var mapWidth = Map.Width;
        if (!double.IsNaN(mapWidth) &&
            mapWidth > 0.0)
        {
          ZoomValue = ActualWidth * 100 / mapWidth;
        }
      }
      else
      {
        var w = mapActualWidth * percentage / 100;
        MapWidth = w;

        var h = mapActualHeight * percentage / 100;
        MapHeight = h;

        ZoomValue = percentage;
        SetMiniMapBorderRange();
      }
    }

    #region DragStuff

    private Point? m_LastDragPoint;

    private void ScrollViewerAdjustableSizeMapMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      ScrollViewerAdjustableSizeMap.Cursor = Cursors.Arrow;
      ScrollViewerAdjustableSizeMap.ReleaseMouseCapture();
      m_LastDragPoint = null;
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

        if (e.LeftButton == MouseButtonState.Pressed)
        {
          SetMiniMapBorderRange();
        }
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

    #endregion

    #region MapItemsSource

    public static readonly DependencyProperty MapItemsSourceProperty = DependencyProperty.Register("MapItemsSource", typeof (IEnumerable<MapItemViewModel>), typeof (MapPositionEditor),
      new FrameworkPropertyMetadata
      {
        PropertyChangedCallback = OnMapItemsSourcePropertyChanged
      });

    public IEnumerable<MapItemViewModel> MapItemsSource
    {
      get { return (IEnumerable<MapItemViewModel>) GetValue(MapItemsSourceProperty); }
      set { SetValue(MapItemsSourceProperty, value); }
    }

    private static void OnMapItemsSourcePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      if (e.NewValue == e.OldValue)
      {
        return;
      }
    }

    private static void OnMapSourceChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      var map = e.NewValue;
    }

    #endregion

    #region GetCoordinates

    public static readonly DependencyProperty GetCoordinatesProperty = DependencyProperty.Register("GetCoordinates", typeof (Func<object, Point>), typeof (MapPositionEditor),
      new FrameworkPropertyMetadata
      {
        PropertyChangedCallback = OnGetCoordinatesChanged
      });

    public Func<object, Point> GetCoordinates
    {
      get { return (Func<object, Point>) GetValue(GetCoordinatesProperty); }
      set { SetValue(GetCoordinatesProperty, value); }
    }

    private static void OnGetCoordinatesChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      if (e.NewValue == e.OldValue)
      {
        return;
      }
    }

    #endregion

    #region ItemsSource

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof (IEnumerable<object>), typeof (MapPositionEditor), new FrameworkPropertyMetadata
                                                                                                                                                                         {
                                                                                                                                                                           PropertyChangedCallback =
                                                                                                                                                                             OnItemsSourceChanged
                                                                                                                                                                         });

    public IEnumerable<object> ItemsSource
    {
      get { return (IEnumerable<object>) GetValue(ItemsSourceProperty); }
      set { SetValue(ItemsSourceProperty, value); }
    }

    private static void OnItemsSourceChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      if (e.NewValue == e.OldValue)
      {
        return;
      }
    }

    #endregion
  }
}