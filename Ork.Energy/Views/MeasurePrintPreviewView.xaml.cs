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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ork.Energy.Views
{
  /// <summary>
  ///   Interaktionslogik für MeasurePrintPreviewViewModel.xaml
  /// </summary>
  public partial class MeasurePrintPreviewView : UserControl
  {
    public MeasurePrintPreviewView()
    {
      InitializeComponent();
    }

    private void Print_Command(object sender, RoutedEventArgs e)
    {
      var printDialog = new PrintDialog();


      FullControl.ScrollToTop();
      PrintButton.Visibility = Visibility.Hidden;
      BackButton.Visibility = Visibility.Hidden;
      if (printDialog.ShowDialog() == true)
      {
        var capabilities = printDialog.PrintQueue.GetPrintCapabilities(printDialog.PrintTicket);


        var scale = Math.Min(capabilities.PageImageableArea.ExtentWidth / ActualWidth, capabilities.PageImageableArea.ExtentHeight / ActualHeight);


        LayoutTransform = new ScaleTransform(scale, scale);


        var sz = new Size(capabilities.PageImageableArea.ExtentWidth, capabilities.PageImageableArea.ExtentHeight);


        Measure(sz);
        Arrange(new Rect(new Point(capabilities.PageImageableArea.OriginWidth, capabilities.PageImageableArea.OriginHeight), sz));

        printDialog.PrintVisual(this, "Aktionsplan");
      }
      PrintButton.Visibility = Visibility.Visible;
      BackButton.Visibility = Visibility.Visible;
    }
  }
}