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
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Ork.Discard.DomainModelService;
using Ork.Framework;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Ork.Discard.ViewModels
{
  [Export(typeof (IWorkspace))]
  public class EvaluationManagementViewModel : DocumentBase, IWorkspace
  {
    private readonly BindableCollection<InspectionViewModel> m_InspectionViewModels = new BindableCollection<InspectionViewModel>();
    private readonly BindableCollection<ProductionItemViewModel> m_ProductionItemViewModels = new BindableCollection<ProductionItemViewModel>();
    private readonly IDiscardRepository m_Repository;
    private PlotModel m_BarModel;
    private bool m_FlyoutActivated = true;
    private bool m_IsEnabled;
    private PlotModel m_PieModel;
    private String m_SearchText;
    private InspectionViewModel m_SelectedInspectionViewModel;
    private ProductionItemViewModel m_SelectedProductionItemViewModel;
    private PlotModel m_SingleBarModel;
    private PlotModel m_SinglePieModel;


    [ImportingConstructor]
    public EvaluationManagementViewModel([Import] IDiscardRepository contextRepository)
    {
      m_Repository = contextRepository;
      m_Repository.ContextChanged += (s, e) => Application.Current.Dispatcher.Invoke(Reload);
      Reload();
      if (m_ProductionItemViewModels.Any())
      {
        m_SelectedProductionItemViewModel = m_ProductionItemViewModels.First();
      }
      if (m_SelectedInspectionViewModel != null)
      {
        m_SelectedInspectionViewModel = InspectionsFromSelectedProductionItems.First();
        NotifyOfPropertyChange(() => InspectionName);
      }
    }


    public IEnumerable<ProductionItemViewModel> FilteredProductionItems
    {
      get
      {
        return SearchInProductionItemList()
          .ToArray();
      }
    }

    public string SearchText
    {
      get { return m_SearchText; }
      set
      {
        m_SearchText = value;
        NotifyOfPropertyChange(() => FilteredProductionItems);
      }
    }

    public bool FlyoutActivated
    {
      get { return m_FlyoutActivated; }
      set
      {
        if (m_FlyoutActivated == value)
        {
          return;
        }
        m_FlyoutActivated = value;
        NotifyOfPropertyChange(() => FlyoutActivated);
      }
    }

    public ProductionItemViewModel SelectedProductionItem
    {
      get { return m_SelectedProductionItemViewModel; }
      set
      {
        m_SelectedProductionItemViewModel = value;

        NotifyOfPropertyChange(() => InspectionsFromSelectedProductionItems);
        m_SelectedInspectionViewModel = InspectionsFromSelectedProductionItems.FirstOrDefault();
        NotifyOfPropertyChange(() => InspectionName);
        NotifyOfPropertyChange(() => PieModel);
        NotifyOfPropertyChange(() => BarModel);
        NotifyOfPropertyChange(() => SinglePieModel);
        NotifyOfPropertyChange(() => SingleBarModel);
      }
    }

    public InspectionViewModel SelectedInspection
    {
      get { return m_SelectedInspectionViewModel; }
      set
      {
        m_SelectedInspectionViewModel = value;
        NotifyOfPropertyChange(() => InspectionName);
        NotifyOfPropertyChange(() => PieModel);
        NotifyOfPropertyChange(() => BarModel);
        NotifyOfPropertyChange(() => SinglePieModel);
        NotifyOfPropertyChange(() => SingleBarModel);
      }
    }

    public string InspectionName
    {
      get
      {
        if (m_SelectedInspectionViewModel == null)
        {
          return "";
        }
        return m_SelectedInspectionViewModel.Name;
      }
    }


    public IEnumerable<InspectionViewModel> InspectionsFromSelectedProductionItems
    {
      get { return m_InspectionViewModels.Where(i => i.ProductionItem == m_SelectedProductionItemViewModel.Model); }
    }

    public PlotModel PieModel
    {
      get
      {
        InitializePlot();
        GenerateAllData();
        return m_PieModel;
      }
    }

    public PlotModel BarModel
    {
      get
      {
        InitializePlot();
        GenerateAllData();
        return m_BarModel;
      }
    }

    public PlotModel SinglePieModel
    {
      get
      {
        InitializePlot();
        GenerateSingleData();
        return m_SinglePieModel;
      }
    }

    public PlotModel SingleBarModel
    {
      get
      {
        InitializePlot();
        GenerateSingleData();
        return m_SingleBarModel;
      }
    }


    public int Index
    {
      get { return 21; }
    }

    public bool IsEnabled
    {
      get { return m_IsEnabled; }
      private set
      {
        if (value.Equals(m_IsEnabled))
        {
          return;
        }
        m_IsEnabled = value;
        NotifyOfPropertyChange(() => IsEnabled);
      }
    }

    public string Title
    {
      get { return TranslationProvider.Translate("Balance"); }
    }

    private void Reload()
    {
      IsEnabled = m_Repository.HasConnection;
      if (IsEnabled)
      {
        LoadData();
      }
    }

    private void LoadData()
    {
      if (m_Repository.ProductionItems == null ||
          m_Repository.Inspections == null)
      {
        return;
      }
      m_Repository.ProductionItems.CollectionChanged += AlterProductionItemCollection;
      m_Repository.Inspections.CollectionChanged += AlterInspectionCollection;
      LoadProductionItems();
      LoadInspections();
    }

    private void LoadProductionItems()
    {
      m_ProductionItemViewModels.Clear();

      foreach (var productionItem in m_Repository.ProductionItems)
      {
        m_ProductionItemViewModels.Add(new ProductionItemViewModel(productionItem));
      }
      NotifyOfPropertyChange(() => FilteredProductionItems);
    }

    private void LoadInspections()
    {
      m_InspectionViewModels.Clear();

      foreach (var inspection in m_Repository.Inspections)
      {
        m_InspectionViewModels.Add(new InspectionViewModel(inspection));
      }
      NotifyOfPropertyChange(() => InspectionsFromSelectedProductionItems);
    }


    private void AlterInspectionCollection(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.Action == NotifyCollectionChangedAction.Add)
      {
        foreach (var newItem in e.NewItems.OfType<Inspection>())
        {
          CreateInspectionViewModel(newItem);
        }
        NotifyOfPropertyChange(() => InspectionsFromSelectedProductionItems);
      }
      else if (e.Action == NotifyCollectionChangedAction.Remove)
      {
        foreach (var oldItem in e.OldItems.OfType<Inspection>())
        {
          var inspectionViewModel = m_InspectionViewModels.Single(r => r.Model == oldItem);

          m_InspectionViewModels.Remove(inspectionViewModel);
        }
        NotifyOfPropertyChange(() => InspectionsFromSelectedProductionItems);
      }
    }


    private void AlterProductionItemCollection(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.Action == NotifyCollectionChangedAction.Add)
      {
        foreach (var newItem in e.NewItems.OfType<ProductionItem>())
        {
          CreateProductionItemViewModel(newItem);
        }

        NotifyOfPropertyChange(() => FilteredProductionItems);
      }
      else if (e.Action == NotifyCollectionChangedAction.Remove)
      {
        foreach (var oldItem in e.OldItems.OfType<ProductionItem>())
        {
          var productionItemViewModel = m_ProductionItemViewModels.Single(r => r.Model == oldItem);

          m_ProductionItemViewModels.Remove(productionItemViewModel);
        }

        NotifyOfPropertyChange(() => FilteredProductionItems);
      }
    }

    private void CreateProductionItemViewModel(ProductionItem newItem)
    {
      var pivm = new ProductionItemViewModel(newItem);
      pivm.PropertyChanged += ProductionItemPropertyChanged;
      m_ProductionItemViewModels.Add(pivm);
    }


    private void ProductionItemPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "Model")
      {
        NotifyOfPropertyChange(() => FilteredProductionItems);
      }
    }

    private void CreateInspectionViewModel(Inspection newItem)
    {
      var ivm = new InspectionViewModel(newItem);
      ivm.PropertyChanged += InspectionPropertyChanged;
      m_InspectionViewModels.Add(ivm);
    }


    private void InspectionPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "Model")
      {
        NotifyOfPropertyChange(() => InspectionsFromSelectedProductionItems);
      }
    }

    private IEnumerable<ProductionItemViewModel> SearchInProductionItemList()
    {
      if (string.IsNullOrEmpty(SearchText))
      {
        return m_ProductionItemViewModels;
      }

      var searchText = SearchText.ToLower();
      var searchResult = m_ProductionItemViewModels.Where(c => ((c.ItemName != null) && (c.ItemName.ToLower()
                                                                                          .Contains(searchText)) || (c.Customer.Name != null) && (c.Customer.Name.ToLower()
                                                                                                                                                   .Contains(searchText))));

      return searchResult;
    }


    private void GenerateSingleData()
    {
      if (SelectedInspection == null)
      {
        return;
      }

      var lightControlColor = (Color) Application.Current.Resources["HighlightColor"];
      var mediumControlColor = (Color) Application.Current.Resources["HighlightMediumColor"];
      m_SinglePieModel.Series.Clear();
      var pieSeries = new PieSeries
                      {
                        StrokeThickness = 1,
                        InsideLabelFormat = string.Empty,
                        OutsideLabelFormat = "{1}: {0}"
                      };


      foreach (var di in SelectedInspection.DiscardItems)
      {
        pieSeries.Slices.Add(new PieSlice(di.InspectionAttribute.Description, (double) di.Quantity));
      }


      m_SinglePieModel.Series.Add(pieSeries);

      m_SingleBarModel.Series.Clear();
      var barValueAxis = CreateBarValueAxis();
      m_SingleBarModel.Axes.Add(barValueAxis);

      var barSample = CreateBarSeriesWithTitle("Sample");

      var barDiscard = CreateBarSeriesWithTitle("TotalDiscardItems");

      var totalDisacard = SelectedInspection.DiscardItems.Sum(di => di.Quantity);

      barSample.Items.Add(new BarItem(SelectedInspection.SampleSize));
      barDiscard.Items.Add(new BarItem(totalDisacard));
      barSample.FillColor = OxyColor.Parse(mediumControlColor.ToString());
      barDiscard.FillColor = OxyColor.Parse(lightControlColor.ToString());

      m_SingleBarModel.Series.Add(barSample);
      m_SingleBarModel.Series.Add(barDiscard);

      var categoryAxis = CreateCategoryAxisWithoutLabels();
      m_SingleBarModel.Axes.Add(categoryAxis);
    }

    private LinearAxis CreateBarValueAxis()
    {
      var textForegroundColor = (Color) Application.Current.Resources["TextForegroundColor"];

      var barValueAxis = new LinearAxis(AxisPosition.Bottom, 0)
                         {
                           MajorGridlineColor = OxyColor.Parse(textForegroundColor.ToString()),
                           TicklineColor = OxyColor.Parse(textForegroundColor.ToString()),
                           TextColor = OxyColor.Parse(textForegroundColor.ToString()),
                           MajorGridlineStyle = LineStyle.Dot,
                           IsZoomEnabled = false,
                           IsPanEnabled = false
                         };
      return barValueAxis;
    }

    private void GenerateAllData()
    {
      var lightControlColor = (Color) Application.Current.Resources["HighlightColor"];
      var mediumControlColor = (Color) Application.Current.Resources["HighlightMediumColor"];

      m_PieModel.Series.Clear();

      var summary = from discardItem in InspectionsFromSelectedProductionItems.SelectMany(inspection => inspection.DiscardItems)
                    let inspectionAttribute = new
                                              {
                                                InspectionAttribute = discardItem.InspectionAttribute
                                              }
                    group discardItem by inspectionAttribute
                    into grouped
                    select new
                           {
                             inspectionAttributeDescription = grouped.Key.InspectionAttribute.Description,
                             quantity = grouped.Sum(dis => dis.Quantity)
                           };

      var pieSeries = new PieSeries
                      {
                        StrokeThickness = 1,
                        ItemsSource = summary.ToArray(),
                        ValueField = "quantity",
                        LabelField = "inspectionAttributeDescription",
                        OutsideLabelFormat = "{1}: {0}",
                        InsideLabelFormat = string.Empty
                      };

      m_PieModel.Series.Add(pieSeries);


      m_BarModel.Series.Clear();
      var barValueAxis = CreateBarValueAxis();
      m_BarModel.Axes.Add(barValueAxis);
      var barSample = CreateBarSeriesWithTitle("Sample");

      var barDiscard = CreateBarSeriesWithTitle("TotalDiscardItems");

      var totalSampleSize = 0;
      var totalQuantity = 0;
      foreach (var inspection in InspectionsFromSelectedProductionItems)
      {
        totalSampleSize += inspection.SampleSize;
        totalQuantity += inspection.DiscardItems.Sum(diss => diss.Quantity);
      }

      barSample.Items.Add(new BarItem(totalSampleSize, 0));
      barDiscard.Items.Add(new BarItem(totalQuantity, 0));
      barSample.FillColor = OxyColor.Parse(mediumControlColor.ToString());
      barDiscard.FillColor = OxyColor.Parse(lightControlColor.ToString());
      m_BarModel.Series.Add(barSample);
      m_BarModel.Series.Add(barDiscard);

      var categoryAxis = CreateCategoryAxisWithoutLabels();
      m_BarModel.Axes.Add(categoryAxis);
    }

    private CategoryAxis CreateCategoryAxisWithoutLabels()
    {
      var categoryAxis = new CategoryAxis
                         {
                           Position = AxisPosition.Left
                         };
      categoryAxis.Labels.Add("");
      return categoryAxis;
    }

    private BarSeries CreateBarSeriesWithTitle(string title)
    {
      var barSeries = new BarSeries
                      {
                        StrokeThickness = 0,
                        IsStacked = false,
                        LabelFormatString = string.Format(TranslationProvider.Translate(title)),
                        LabelPlacement = LabelPlacement.Base,
                      };
      return barSeries;
    }

    private PlotModel CreateBarPlotModel()
    {
      var textForegroundColor = (Color) Application.Current.Resources["TextForegroundColor"];
      var workspaceBackgroundColor = (Color) Application.Current.Resources["WorkspaceBackgroundColor"];

      var barPlotModel = new PlotModel
                         {
                           PlotAreaBorderColor = OxyColor.Parse(textForegroundColor.ToString()),
                           TextColor = OxyColor.Parse(textForegroundColor.ToString()),
                           LegendOrientation = LegendOrientation.Horizontal,
                           LegendPlacement = LegendPlacement.Outside,
                           LegendPosition = LegendPosition.TopRight,
                           LegendBackground = OxyColor.Parse(workspaceBackgroundColor.ToString()),
                           LegendBorder = OxyColor.Parse(textForegroundColor.ToString())
                         };
      return barPlotModel;
    }

    private PlotModel CreatePiePlotModel()
    {
      var textForegroundColor = (Color) Application.Current.Resources["TextForegroundColor"];

      var piePlotModel = new PlotModel
                         {
                           PlotAreaBorderColor = OxyColor.Parse(textForegroundColor.ToString()),
                           TextColor = OxyColor.Parse(textForegroundColor.ToString())
                         };
      return piePlotModel;
    }

    private void InitializePlot()
    {
      m_BarModel = CreateBarPlotModel();
      m_PieModel = CreatePiePlotModel();

      m_SingleBarModel = CreateBarPlotModel();
      m_SinglePieModel = CreatePiePlotModel();
    }
  }
}