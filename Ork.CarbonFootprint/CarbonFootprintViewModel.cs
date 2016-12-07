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
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Caliburn.Micro;
using Ork.CarbonFootprint.ViewModels;
using Ork.Framework;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Ork.CarbonFootprint
{
  public class CarbonFootprintViewModel : DocumentBase
  {
    private readonly ContextRepository m_ContextRepository;
    private readonly BindableCollection<PositionViewModel> m_Positions = new BindableCollection<PositionViewModel>();
    private readonly IEnumerable<ResponsibleSubjectViewModel> m_ResponsibleSubjects;
    private readonly TagColorProvider m_TagColorProvider;
    private Visibility m_EditAreaVisibility = Visibility.Collapsed;
    private PositionAddViewModel m_EditPosition;
    private CategoryAxis m_EmployeeCategoryAxis;
    private PlotModel m_EmployeeChartModel;
    private bool m_FlyoutActivated = true;
    private bool m_IsCfpChoiceVisible;
    private bool m_IsSelected;
    private CategoryAxis m_TypeCategoryAxis;
    private PlotModel m_TypeChartModel;
    private IEnumerable<Lazy<IPositionViewModelFactory, IPositionMetadata>> m_PositionFactories;
    private String m_SearchText;
    private PositionViewModel m_SelectedPosition;
    private CategoryAxis m_TagCategoryAxis;
    private PlotModel m_TagChartModel;

    public CarbonFootprintViewModel(DomainModelService.CarbonFootprint cf, IEnumerable<Lazy<IPositionViewModelFactory, IPositionMetadata>> factories, ContextRepository contextRepository,
      TagColorProvider tagColorProvider, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects, IDialogManager dialogs)
    {
      Dialogs = dialogs;
      m_ResponsibleSubjects = responsibleSubjects;
      m_ContextRepository = contextRepository;
      m_TagColorProvider = tagColorProvider;
      m_IsSelected = true;
      PositionFactories = factories;
      Model = cf;
      InitializePositions();
      FlyoutActivated = true;
    }

    public string SearchText
    {
      get { return m_SearchText; }
      set
      {
        m_SearchText = value;
        NotifyOfPropertyChange(() => FilteredPositions);
      }
    }

    public IEnumerable<PositionViewModel> FilteredPositions
    {
      get
      {
        return SearchInPositionList()
          .ToArray();
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

    public string Calculation
    {
      get
      {
        var kgvalue = String.Format("{0:0.##}", Model.Calculation / 1000000);
        return kgvalue;
      }
    }

    public string Description
    {
      get { return Model.Description; }
      set
      {
        Model.Description = value;
        NotifyOfPropertyChange(() => Description);
      }
    }

    public bool IsCfpChoiceVisible
    {
      get { return m_IsCfpChoiceVisible; }
      set
      {
        if (value.Equals(m_IsCfpChoiceVisible))
        {
          return;
        }
        m_IsCfpChoiceVisible = value;
        NotifyOfPropertyChange(() => IsCfpChoiceVisible);
      }
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
        NotifyOfPropertyChange(() => IsSelected);
        if (IsSelectedChanged != null)
        {
          IsSelectedChanged(this, new EventArgs());
        }
      }
    }

    public DomainModelService.CarbonFootprint Model { get; private set; }

    public string Name
    {
      get { return Model.Name; }
      set
      {
        Model.Name = value;
        NotifyOfPropertyChange(() => Name);
      }
    }

    public IEnumerable<Lazy<IPositionViewModelFactory, IPositionMetadata>> PositionFactories
    {
      get { return m_PositionFactories.Where(pf => pf.Value.CanCreate); }
      private set { m_PositionFactories = value; }
    }

    public PositionViewModel SelectedPosition
    {
      get { return m_SelectedPosition; }
      set
      {
        if (m_SelectedPosition == value)
        {
          return;
        }
        m_SelectedPosition = value;
        NotifyOfPropertyChange(() => SelectedPosition);
      }
    }

    public Visibility EditAreaVisibility
    {
      get { return m_EditAreaVisibility; }
      set
      {
        m_EditAreaVisibility = value;
        NotifyOfPropertyChange(() => EditAreaVisibility);
      }
    }

    public PlotModel TagChartModel
    {
      get
      {
        if (m_TagChartModel == null)
        {
          InitializeTagChart();
        }
        GenerateTagData();
        return m_TagChartModel;
      }
    }

    public PlotModel EmployeeChartModel
    {
      get
      {
        if (m_EmployeeChartModel == null)
        {
          InitializeEmployeeChart();
        }
        GenerateEmployeeData();
        return m_EmployeeChartModel;
      }
    }

    public PlotModel TypeChartModel
    {
      get
      {
        if (m_TypeChartModel == null)
        {
          InitializeTypeChart();
        }
        GenerateTypeData();
        return m_TypeChartModel;
      }
    }

    public void OpenPositionEditDialog(object dataContext)
    {
      OpenEditor(new PositionEditViewModel((PositionViewModel) dataContext, Delete));
    }

    public void OpenEditor(object dataContext, MouseButtonEventArgs e)
    {
      if (e.ClickCount >= 2)
      {
        if (dataContext is PositionViewModel)
        {
          OpenPositionEditDialog(dataContext);
        }
      }
    }

    private void OpenEditor(PositionAddViewModel dataContext)
    {
      m_EditPosition = dataContext;
      Dialogs.ShowDialog(m_EditPosition);
    }

    private IEnumerable<PositionViewModel> SearchInPositionList()
    {
      if (string.IsNullOrEmpty(SearchText))
      {
        return m_Positions;
      }

      var searchText = SearchText.ToLower();
      var searchResult = m_Positions.Where(c => ((!string.IsNullOrEmpty(c.Name)) && (c.Name.ToLower()
                                                                                      .Contains(searchText))) || ((!string.IsNullOrEmpty(c.Tag)) && (c.Tag.ToLower()
                                                                                                                                                      .Contains(searchText))));

      return searchResult;
    }

    public event EventHandler IsSelectedChanged;

    public void OpenPositionAddDialog(Lazy<IPositionViewModelFactory, IPositionMetadata> factory)
    {
      var position = factory.Value.CreateNew(m_ResponsibleSubjects);
      position.Start = DateTime.Today;
      position.Finish = DateTime.Today.AddYears(1);
      IsCfpChoiceVisible = !IsCfpChoiceVisible;
      OpenEditor(new PositionAddViewModel(position));
    }

    private void GenerateEmployeeData()
    {
      m_EmployeeCategoryAxis.Labels.Clear();
      m_EmployeeChartModel.Series.Clear();

      var employees = FilteredPositions.Where(p => p.ResponsibleSubject != null)
                                       .Select(p => p.ResponsibleSubject.Name)
                                       .Distinct()
                                       .ToList();

      var allTags = FilteredPositions.Select(cfp => cfp.Tag)
                                     .Distinct();
      foreach (var tag in allTags)
      {
        var columnSeries = new ColumnSeries
                           {
                             StrokeThickness = 0,
                             Title = tag,
                             IsStacked = true,
                             FillColor = m_TagColorProvider.GetColorForTag(tag)
                                                           .OxyColor
                           };
        m_EmployeeChartModel.Series.Add(columnSeries);
      }

      foreach (var employee in employees)
      {
        m_EmployeeCategoryAxis.Labels.Add(employee);

        var positionsPerEmployee = m_Positions.Where(cfp => cfp.ResponsibleSubject != null && cfp.ResponsibleSubject.Name == employee);

        foreach (var positionPerEmployee in positionsPerEmployee)
        {
          var columnSeries = (ColumnSeries) m_EmployeeChartModel.Series.Single(s => s.Title == positionPerEmployee.Tag);
          columnSeries.Items.Add(new ColumnItem(positionPerEmployee.CalculatedValue / 1000, employees.IndexOf(employee)));
        }
      }

      m_EmployeeChartModel.ResetAllAxes();
    }

    private void GenerateTagData()
    {
      m_TagCategoryAxis.Labels.Clear();
      m_TagChartModel.Series.Clear();

      var tags = m_Positions.Select(p => p.Tag)
                            .Distinct()
                            .ToList();
      foreach (var tag in tags)
      {
        m_TagCategoryAxis.Labels.Add(tag);

        var positionsPerCategory = m_Positions.Where(cfp => cfp.Tag == tag);
        foreach (var positionPerCategory in positionsPerCategory)
        {
          var columnSeries = new ColumnSeries
                             {
                               StrokeThickness = 0,
                               Title = positionPerCategory.Name,
                               IsStacked = false,
                               FillColor = m_TagColorProvider.GetColorForTag(positionPerCategory.Tag)
                                                             .OxyColor
                             };
          m_TagChartModel.Series.Add(columnSeries);

          columnSeries.Items.Add(new ColumnItem(positionPerCategory.CalculatedValue / 1000, tags.IndexOf(positionPerCategory.Tag)));
        }
      }

      m_TagChartModel.InvalidatePlot(true);
    }

    private void GenerateTypeData()
    {
      m_TypeCategoryAxis.Labels.Clear();
      m_TypeChartModel.Series.Clear();

      foreach (var factory in m_PositionFactories)
      {
        m_TypeCategoryAxis.Labels.Add(TranslationProvider.Translate(Assembly.GetExecutingAssembly(), factory.Metadata.Name) + "\n");

        var positionsPerType = m_Positions.Where(cfp => factory.Value.CanDecorate(cfp.Model));

        foreach (var positionPerType in positionsPerType)
        {
          var columnSeries = new ColumnSeries
                             {
                               StrokeThickness = 0,
                               Title = TranslationProvider.Translate(Assembly.GetExecutingAssembly(), factory.Metadata.Name),
                               IsStacked = false,
                               FillColor = m_TagColorProvider.GetColorForTag(positionPerType.Tag)
                                                             .OxyColor
                             };

          m_TypeChartModel.Series.Add(columnSeries);

          //columnSeries.FillColor = m_TagColorProvider.GetColorForTag(positionPerType.Tag)
          //                                           .OxyColor;

          columnSeries.Items.Add(new ColumnItem(positionPerType.CalculatedValue / 1000, m_PositionFactories.ToList()
                                                                                                           .IndexOf(factory)));
        }
      }

      m_TypeChartModel.InvalidatePlot(true);
    }

    private void InitializeEmployeeChart()
    {
      var textForegroundColor = OxyColor.Parse(((Color) Application.Current.Resources["TextForegroundColor"]).ToString());
      var lightControlColor = OxyColor.Parse(((Color) Application.Current.Resources["LightControlColor"]).ToString());

      m_EmployeeChartModel = new PlotModel
                             {
                               Title = TranslationProvider.Translate(Assembly.GetExecutingAssembly(), "employeeAnalysis"),
                               PlotAreaBorderColor = textForegroundColor,
                               TextColor = textForegroundColor,
                               IsLegendVisible = true,
                               LegendOrientation = LegendOrientation.Horizontal,
                               LegendPlacement = LegendPlacement.Outside,
                               LegendPosition = LegendPosition.BottomLeft,
                               PlotAreaBorderThickness = new OxyThickness(1)
                             };

      var valueAxis = new LinearAxis(AxisPosition.Left, 0)
                      {
                        ShowMinorTicks = true,
                        MinorGridlineStyle = LineStyle.Dot,
                        MajorGridlineStyle = LineStyle.Dot,
                        MajorGridlineColor = lightControlColor,
                        MinorGridlineColor = lightControlColor,
                        Title = "kg CO₂",
                        TicklineColor = textForegroundColor,
                        IsZoomEnabled = false,
                        IsPanEnabled = false
                      };

      m_EmployeeChartModel.Axes.Add(valueAxis);

      m_EmployeeCategoryAxis = new CategoryAxis
                               {
                                 TicklineColor = textForegroundColor,
                                 IsZoomEnabled = false,
                                 IsPanEnabled = false,
                                 MinorStep = 1
                               };
      m_EmployeeChartModel.Axes.Add(m_EmployeeCategoryAxis);
    }

    private void InitializeTagChart()
    {
      var textForegroundColor = OxyColor.Parse(((Color) Application.Current.Resources["TextForegroundColor"]).ToString());
      var lightControlColor = OxyColor.Parse(((Color) Application.Current.Resources["LightControlColor"]).ToString());

      m_TagChartModel = new PlotModel
                        {
                          PlotAreaBorderColor = textForegroundColor,
                          TextColor = textForegroundColor,
                          Title = TranslationProvider.Translate(Assembly.GetExecutingAssembly(), "categoryAnalysis"),
                          IsLegendVisible = false,
                          PlotAreaBorderThickness = new OxyThickness(1)
                        };

      var valueAxis = new LinearAxis(AxisPosition.Left, 0)
                      {
                        ShowMinorTicks = true,
                        MinorGridlineStyle = LineStyle.Dot,
                        MajorGridlineStyle = LineStyle.Dot,
                        MajorGridlineColor = lightControlColor,
                        MinorGridlineColor = lightControlColor,
                        Title = "kg CO₂",
                        TicklineColor = textForegroundColor,
                        IsZoomEnabled = false,
                        IsPanEnabled = false
                      };
      m_TagChartModel.Axes.Add(valueAxis);

      m_TagCategoryAxis = new CategoryAxis
                          {
                            TicklineColor = textForegroundColor,
                            IsZoomEnabled = false,
                            IsPanEnabled = false,
                            MinorStep = 1
                          };

      m_TagChartModel.Axes.Add(m_TagCategoryAxis);
    }

    private void InitializeTypeChart()
    {
      var textForegroundColor = OxyColor.Parse(((Color) Application.Current.Resources["TextForegroundColor"]).ToString());
      var lightControlColor = OxyColor.Parse(((Color) Application.Current.Resources["LightControlColor"]).ToString());

      m_TypeChartModel = new PlotModel
                           {
                             Title = TranslationProvider.Translate(Assembly.GetExecutingAssembly(), "positionAnalysis"),
                             PlotAreaBorderColor = textForegroundColor,
                             TextColor = textForegroundColor,
                             IsLegendVisible = false,
                             PlotAreaBorderThickness = new OxyThickness(1)
                           };

      var valueAxis = new LinearAxis(AxisPosition.Left, 0)
                      {
                        ShowMinorTicks = true,
                        MinorGridlineStyle = LineStyle.Dot,
                        MajorGridlineStyle = LineStyle.Dot,
                        MajorGridlineColor = lightControlColor,
                        MinorGridlineColor = lightControlColor,
                        Title = "kg CO₂",
                        TicklineColor = textForegroundColor,
                        IsZoomEnabled = false,
                        IsPanEnabled = false
                      };
      m_TypeChartModel.Axes.Add(valueAxis);

      m_TypeCategoryAxis = new CategoryAxis
                             {
                               TicklineColor = textForegroundColor,
                               IsZoomEnabled = false,
                               IsPanEnabled = false,
                               MinorStep = 1,
                               Angle = -10
                             };

      m_TypeChartModel.Axes.Add(m_TypeCategoryAxis);
    }

    public void Delete(PositionViewModel positionViewModel)
    {
      Model.Positions.Remove(positionViewModel.Model);
      m_Positions.Remove(positionViewModel);
      CloseEditor();
      NotifyOfPropertyChange(() => FilteredPositions);
      Save();
    }

    public void Save()
    {
      m_ContextRepository.Save();
    }

    public void Back()
    {
      Save();
      m_ContextRepository.Calculate(Model.Id);
    }

    public void Add(object dataContext)
    {
      var position = ((PositionAddViewModel) dataContext).Model;
      Model.Positions.Add(position.Model);
      m_Positions.Add(position);
      CloseEditor();
      NotifyOfPropertyChange(() => FilteredPositions);
      Save();
    }

    public void Accept()
    {
      Save();
      CloseEditor();
    }

    public void Cancel()
    {
      CloseEditor();
    }

    private void CloseEditor()
    {
      m_EditPosition.TryClose();
    }

    private void InitializePositions()
    {
      var factories = m_PositionFactories.Select(pf => pf.Value)
                                         .ToArray();
      foreach (var carbonFootprintPosition in Model.Positions)
      {
        foreach (var positionFactory in
          factories.Where(positionFactory => positionFactory.CanDecorate(carbonFootprintPosition)))
        {
          m_Positions.Add(positionFactory.CreateFromExisting(carbonFootprintPosition, m_ResponsibleSubjects));
        }
      }
    }
  }
}