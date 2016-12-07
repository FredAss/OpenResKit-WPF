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
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Caliburn.Micro;
using Ork.Framework;
using Ork.Measure.DomainModelService;
using Ork.Measure.Factories;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Ork.Measure.ViewModels
{
  [Export(typeof (IWorkspace))]
  public class MeasureManagementViewModel : DocumentBase, IWorkspace
  {
    private readonly BindableCollection<CatalogViewModel> m_Catalogs = new BindableCollection<CatalogViewModel>();
    private readonly IMeasureViewModelFactory m_MeasureViewModelFactory;
    private readonly BindableCollection<MeasureViewModel> m_Measures = new BindableCollection<MeasureViewModel>();
    private readonly IMeasureRepository m_Repository;
    private bool m_DgvVisible;
    private bool m_DgvVisibleCat;
    private IScreen m_EditItem;
    private bool m_FlyoutActivated;
    private bool m_IsEnabled;
    private PlotModel m_Plot;
    private bool m_PlotIsVisible;
    private string m_SearchText;
    private string m_SearchTextMeasures;
    private CatalogViewModel m_SelectedCatalog;

    [ImportingConstructor]
    public MeasureManagementViewModel([Import] IMeasureRepository contextRepository, [Import] IMeasureViewModelFactory measureViewModelFactory)
    {
      m_Repository = contextRepository;
      m_MeasureViewModelFactory = measureViewModelFactory;
      m_Repository.ContextChanged += (s, e) => Application.Current.Dispatcher.Invoke(Reload);

      Reload();

      FlyoutActivated = true;
    }

    public IEnumerable<MeasureViewModel> Measures
    {
      get
      {
        IEnumerable<MeasureViewModel> measure;
        if (SelectedCatalog != null)
        {
          measure = m_Measures.Where(mvm => SelectedCatalog.Measures.Contains(mvm.Model))
                              .ToArray();
        }
        else
        {
          measure = m_Measures;
        }

        VisiblePlot = !measure.Any();
        return SearchInMeasureList(measure);
      }
    }

    public IEnumerable<CatalogViewModel> Catalogs
    {
      get { return FilteredCatalogs; }
    }

    public IEnumerable<CatalogViewModel> FilteredCatalogs
    {
      get
      {
        var filteredCatalogs = SearchInCatalogList()
          .ToArray();
        return filteredCatalogs;
      }
    }

    public string SearchText
    {
      get { return m_SearchText; }
      set
      {
        m_SearchText = value;
        NotifyOfPropertyChange(() => Catalogs);
      }
    }

    public string SearchTextMeasures
    {
      get { return m_SearchTextMeasures; }
      set
      {
        m_SearchTextMeasures = value;
        NotifyOfPropertyChange(() => Measures);
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

    public CatalogViewModel SelectedCatalog
    {
      get { return m_SelectedCatalog; }
      set
      {
        if (value == null)
        {
          VisibleCat = true;
          VisibleNormal = false;
        }
        else
        {
          VisibleCat = false;
          VisibleNormal = true;
        }

        m_SelectedCatalog = value;
        NotifyOfPropertyChange(() => SelectedCatalog);
        NotifyOfPropertyChange(() => CanAdd);
        NotifyOfPropertyChange(() => Measures);
        NotifyOfPropertyChange(() => PlotModel);
      }
    }

    public string AllMeasures
    {
      get
      {
        var measures = m_Repository.Catalogs.SelectMany(cat => cat.Measures)
                                   .ToArray();
        if (!measures.Any())
        {
          return TranslationProvider.Translate("NoneAvailable");
        }
        var dateList = measures.Select(measure => measure.DueDate)
                               .OrderBy(m => m)
                               .ToArray();
        return dateList.First()
                       .ToShortDateString() + " - " + dateList.Last()
                                                              .ToShortDateString();
      }
    }

    public bool VisibleNormal
    {
      get { return m_DgvVisible; }
      set
      {
        m_DgvVisible = value;
        NotifyOfPropertyChange(() => VisibleNormal);
      }
    }

    public bool VisibleCat
    {
      get { return m_DgvVisibleCat; }
      set
      {
        m_DgvVisibleCat = value;
        NotifyOfPropertyChange(() => VisibleCat);
      }
    }

    public bool VisiblePlot
    {
      get { return m_PlotIsVisible; }
      set
      {
        m_PlotIsVisible = value;
        NotifyOfPropertyChange(() => VisiblePlot);
      }
    }

    public bool CanAdd
    {
      get
      {
        if (SelectedCatalog == null)
        {
          return false;
        }
        return true;
      }
    }

    public MeasureViewModel SelectedMeasure { get; set; }

    public PlotModel PlotModel
    {
      get
      {
        //todo: optimize initialization
        InitializePlot();

        var measures = Measures.ToArray();
        if (measures.Any())
        {
          GenerateGraphData(measures);
        }
        m_Plot.InvalidatePlot(true);
        return m_Plot;
      }
    }

    public int Index
    {
      get { return 300; }
    }

    public bool IsEnabled
    {
      get { return m_IsEnabled; }
      private set
      {
        m_IsEnabled = value;
        NotifyOfPropertyChange(() => IsEnabled);
      }
    }

    public string Title
    {
      get { return TranslationProvider.Translate("TitleMeasureManagementViewModel"); }
    }

    private IEnumerable<MeasureViewModel> SearchInMeasureList(IEnumerable<MeasureViewModel> measureList)
    {
      var searchText = m_SearchTextMeasures.ToLower();
      return measureList.Where(mvm => mvm.ResponsibleSubjectName.ToLower()
                                         .Contains(searchText));
    }

    private void AlterCatalogCollection(object sender, NotifyCollectionChangedEventArgs e)
    {
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
          foreach (var newItem in e.NewItems.OfType<Catalog>())
          {
            CreateCatalogViewModel(newItem);
          }
          break;
        case NotifyCollectionChangedAction.Remove:
          foreach (var catalogViewModel in e.OldItems.OfType<Catalog>()
                                            .Select(oldItem => m_Catalogs.Single(r => r.Model == oldItem)))
          {
            m_Catalogs.Remove(catalogViewModel);
            foreach (var mvm in m_Measures.Where(mvm => catalogViewModel.Measures.Contains(mvm.Model))
                                          .ToArray())
            {
              m_Measures.Remove(mvm);
            }
          }
          break;
      }
      NotifyOfPropertyChange(() => Catalogs);
    }

    public void AddCatalog(object dataContext)
    {
      var catalogAddViewModel = ((CatalogAddViewModel) dataContext);
      m_Repository.Catalogs.Add(catalogAddViewModel.Model);
      Save();
      SelectedCatalog = m_Catalogs.Last();
    }

    public void RemoveCatalog()
    {
      var catalogViewModel = SelectedCatalog;
      m_Repository.Catalogs.Remove(catalogViewModel.Model);
      Save();

      SelectedCatalog = null;
      NotifyOfPropertyChange(() => AllMeasures);
    }

    public void RemoveMeasure()
    {
      var measureViewModel = SelectedMeasure;
      if (SelectedCatalog != null)
      {
        SelectedCatalog.Measures.Remove(measureViewModel.Model);
      }
      else
      {
        measureViewModel.Catalog.Measures.Remove(measureViewModel.Model);
      }
      m_Measures.Remove(measureViewModel);

      Save();

      NotifyOfPropertyChange(() => Measures);
      NotifyOfPropertyChange(() => AllMeasures);
      NotifyOfPropertyChange(() => PlotModel);
    }

    public void AddMeasure(object dataContext)
    {
      var measureAddViewModel = ((MeasureAddViewModel) dataContext);
      SelectedCatalog.Measures.Add(measureAddViewModel.Model);
      CreateMeasureViewModel(measureAddViewModel.Model, SelectedCatalog);
      Save();

      CreateRelatedElement(dataContext);

      NotifyOfPropertyChange(() => AllMeasures);
      NotifyOfPropertyChange(() => Measures);
      NotifyOfPropertyChange(() => PlotModel);
    }

    private void Save()
    {
      CloseEditor();
      m_Repository.Save();
    }

    public void Accept(object dataContext)
    {
      Save();
      if (dataContext is MeasureAddViewModel)
      {
        CreateRelatedElement(dataContext);
      }
      NotifyOfPropertyChange(() => Measures);
      NotifyOfPropertyChange(() => Catalogs);
      NotifyOfPropertyChange(() => SelectedCatalog);
      NotifyOfPropertyChange(() => AllMeasures);
      NotifyOfPropertyChange(() => PlotModel);
    }

    private static void CreateRelatedElement(object dataContext)
    {
      var measureViewModel = ((MeasureAddViewModel) dataContext);
      var relatedElement = measureViewModel.RelatedElementProviders.SingleOrDefault(r => r.Elements.Any(e => e.IsSelected));
      if (relatedElement != null)
      {
        var element = relatedElement.Elements.Single(r => r.IsSelected)
                                    .Model;
        relatedElement.Provider.CreateRelatedElement(measureViewModel.Model.Id, element);
      }
      foreach (var vm in measureViewModel.RelatedElementProviders.Where(r => r.Elements.All(e => !e.IsSelected)))
      {
        vm.Provider.DeleteRelatedElement(measureViewModel.Model.Id);
      }
    }

    public IEnumerable<CatalogViewModel> SearchInCatalogList()
    {
      if (string.IsNullOrEmpty(SearchText))
      {
        return m_Catalogs;
      }
      var searchText = SearchText.ToLower();
      return m_Catalogs.Where(c => (((c.Name != null) && (c.Name.ToLower()
                                                           .Contains(searchText)) ||
                                     c.Measures.Any(m => (((Employee) m.ResponsibleSubject).LastName + " " + ((Employee) m.ResponsibleSubject).FirstName).ToLower()
                                                                                                                                                         .Contains(searchText)))));
    }

    private void OpenEditor(object dataContext)
    {
      m_EditItem = (IScreen) dataContext;
      Dialogs.ShowDialog(m_EditItem);
    }

    public void OpenMeasureAddDialog()
    {
      if (!VisibleCat)
      {
        OpenEditor(m_MeasureViewModelFactory.CreateAddViewModel());
      }
    }

    public void OpenCatalogAddDialog()
    {
      OpenEditor(m_MeasureViewModelFactory.CreateCatalogAddViewModel());
    }

    private void OpenCatalogEditDialog(object dataContext)
    {
      SelectedCatalog = (CatalogViewModel) dataContext;
      OpenEditor(m_MeasureViewModelFactory.CreateCatalogEditViewModel((CatalogViewModel) dataContext, RemoveCatalog));
    }

    private void OpenMeasureEditDialog(MeasureViewModel measureViewModel)
    {
      if (measureViewModel != null)
      {
        OpenEditor(m_MeasureViewModelFactory.CreateEditViewModel(measureViewModel.Model, RemoveMeasure));
      }
    }

    public void OpenEditor(object dataContext, MouseButtonEventArgs e)
    {
      if (e.ClickCount < 2)
      {
        return;
      }
      if (dataContext is CatalogViewModel)
      {
        OpenCatalogEditDialog(dataContext);
      }
      else if (dataContext is MeasureManagementViewModel)
      {
        OpenMeasureEditDialog(SelectedMeasure);
      }
    }

    public void Cancel()
    {
      CloseEditor();
    }

    private void CloseEditor()
    {
      m_EditItem.TryClose();
      m_EditItem = null;
    }

    private void Reload()
    {
      IsEnabled = m_Repository.HasConnection;
      if (IsEnabled)
      {
        LoadData();
      }
      NotifyOfPropertyChange(() => AllMeasures);
      ShowAllMeasures();
    }

    public void ShowAllMeasures()
    {
      SelectedCatalog = null;
    }

    private void LoadData()
    {
      m_Measures.Clear();
      m_Catalogs.Clear();
      LoadCatalogs();
    }

    private void LoadCatalogs()
    {
      m_Repository.Catalogs.CollectionChanged += AlterCatalogCollection;
      foreach (var catalog in m_Repository.Catalogs)
      {
        CreateCatalogViewModel(catalog);
      }
      NotifyOfPropertyChange(() => Catalogs);
    }

    private void CreateCatalogViewModel(Catalog catalog)
    {
      var cvm = m_MeasureViewModelFactory.CreateFromExisting(catalog);
      foreach (var measure in catalog.Measures)
      {
        CreateMeasureViewModel(measure, cvm);
      }
      m_Catalogs.Add(cvm);
    }

    private void CreateMeasureViewModel(DomainModelService.Measure measure, CatalogViewModel cvm)
    {
      var mvm = m_MeasureViewModelFactory.CreateFromExisting(measure, cvm);
      m_Measures.Add(mvm);
    }

    private void InitializePlot()
    {
      m_Plot = new PlotModel
               {
                 LegendTitle = TranslationProvider.Translate(Assembly.GetExecutingAssembly(), "TitleMeasureManagementViewModel"),
               };

      m_Plot.Axes.Clear();
      m_Plot.Series.Clear();
      var catalogName = SelectedCatalog == null
        ? TranslationProvider.Translate("AllMeasures")
        : SelectedCatalog.Name;

      var textForegroundColor = (Color) Application.Current.Resources["TextForegroundColor"];
      var lightControlColor = (Color) Application.Current.Resources["LightControlColor"];

      m_Plot.Title = catalogName;
      m_Plot.LegendOrientation = LegendOrientation.Horizontal;
      m_Plot.LegendPlacement = LegendPlacement.Outside;
      m_Plot.TextColor = OxyColor.Parse(textForegroundColor.ToString());
      m_Plot.PlotAreaBorderColor = OxyColor.Parse(textForegroundColor.ToString());
      m_Plot.PlotAreaBorderThickness = new OxyThickness(1);


      var monthArray = new string[6];
      var k = 0;
      for (var i = -5; i < 1; i++)
      {
        monthArray[k++] = DateTime.Now.AddMonths(i)
                                  .ToString("MMM");
      }


      var categoryAxis = new CategoryAxis(TranslationProvider.Translate("Month"), monthArray.ToArray())
                         {
                           TicklineColor = OxyColor.Parse(textForegroundColor.ToString()),
                           IsZoomEnabled = false,
                           IsPanEnabled = false
                         };

      m_Plot.Axes.Add(categoryAxis);

      var valueAxis = new LinearAxis(AxisPosition.Left, 0)
                      {
                        ShowMinorTicks = true,
                        MinorGridlineStyle = LineStyle.Dot,
                        MajorGridlineStyle = LineStyle.Dot,
                        MajorGridlineColor = OxyColor.Parse(lightControlColor.ToString()),
                        MinorGridlineColor = OxyColor.Parse(lightControlColor.ToString()),
                        Title = TranslationProvider.Translate("Count"),
                        TicklineColor = OxyColor.Parse(textForegroundColor.ToString()),
                        IsZoomEnabled = false,
                        IsPanEnabled = false
                      };

      m_Plot.Axes.Add(valueAxis);
    }

    private void GenerateGraphData(IEnumerable<MeasureViewModel> measures)
    {
      var columnDelayed = new ColumnSeries
                          {
                            StrokeThickness = 0,
                            Title = TranslationProvider.Translate("DelayedCompleted"),
                            FillColor = OxyColors.IndianRed,
                            IsStacked = true,
                            StrokeColor = OxyColors.Red
                          };

      var columnCompleted = new ColumnSeries
                            {
                              StrokeThickness = 0,
                              Title = TranslationProvider.Translate("Completed"),
                              FillColor = OxyColors.GreenYellow,
                              IsStacked = true,
                              StrokeColor = OxyColors.GreenYellow
                            };

      var columnCompletedPrior = new ColumnSeries
                                 {
                                   StrokeThickness = 0,
                                   Title = TranslationProvider.Translate("CompletedForFuture"),
                                   FillColor = OxyColors.Green,
                                   IsStacked = true,
                                   StrokeColor = OxyColors.Green
                                 };

      var columnPlanned = new ColumnSeries
                          {
                            StrokeThickness = 0,
                            Title = TranslationProvider.Translate("PlannedThisMonth"),
                            FillColor = OxyColors.Yellow,
                            IsStacked = false,
                            StrokeColor = OxyColors.Yellow,
                          };


      var counter = 0;
      var total = 0;

      var lastDayofMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1)
                                                                                     .Subtract(new TimeSpan(0, 0, 0, 1));

      var sortedList = measures.Where(m => (m.DueDate <= (lastDayofMonth)) && (m.DueDate >= (lastDayofMonth.Subtract(new TimeSpan(200, 0, 0, 0)))))
                               .OrderBy(m => m.DueDate);

      var monthArray = new int[6];
      var k = 0;

      for (var i = -5; i < 1; i++)
      {
        monthArray[k++] = DateTime.Now.AddMonths(i)
                                  .Month;
      }


      foreach (var month in monthArray)
      {
        var completed = measures.Count(m => m.Status == 2 && m.DueDate.Month == m.EntryDate.GetValueOrDefault()
                                                                                 .Month && m.EntryDate <= m.DueDate && m.EntryDate.GetValueOrDefault()
                                                                                                                        .Month == month);
        var delayed = measures.Count(m => m.Status == 2 && m.EntryDate > m.DueDate && m.EntryDate.GetValueOrDefault()
                                                                                       .Month == month);
        var completedPrior = measures.Count(m => m.Status == 2 && m.DueDate.Month > m.EntryDate.GetValueOrDefault()
                                                                                     .Month && m.EntryDate <= m.DueDate && m.EntryDate.GetValueOrDefault()
                                                                                                                            .Month == month);

        var planned = measures.Count(m => m.DueDate.Month == month);


        columnCompleted.Items.Add(new ColumnItem(completed, counter));
        columnDelayed.Items.Add(new ColumnItem(delayed, counter));

        columnPlanned.Items.Add(new ColumnItem(planned, counter));
        columnCompletedPrior.Items.Add(new ColumnItem(completedPrior, counter));

        total = total + completed + completedPrior + planned + delayed;

        counter++;
      }

      if (total == 0)
      {
        VisiblePlot = true;
      }


      m_Plot.Series.Add(columnCompleted);
      m_Plot.Series.Add(columnCompletedPrior);
      m_Plot.Series.Add(columnDelayed);
      m_Plot.Series.Add(columnPlanned);
    }
  }
}