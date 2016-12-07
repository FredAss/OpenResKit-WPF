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
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media;
using Ork.Framework;
using Ork.Invest.Utilities;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Ork.Invest.ViewModels
{
  [Export(typeof (InvestmentEvaluationContentBaseViewModel))]
  public class CapitalValueViewModel : InvestmentEvaluationContentBaseViewModel
  {
    private double m_PlotAnnualCapitalZoom = 0.25;
    private PlotModel m_PlotModelAnnualCapitalValueChange;
    private PlotModel m_PlotModelCapitalValue;
    private FinancialCalculation m_SelectedFinancialCalculation = new FinancialCalculation();


    public CapitalValueViewModel()
    {
      Index = 3;
    }

    public PlotModel PlotModelCapitalValue
    {
      get
      {
        InitializeCapitalValuePlot();

        AddSeriesForCapitalValue();

        m_PlotModelCapitalValue.InvalidatePlot(true);
        return m_PlotModelCapitalValue;
      }
    }


    public PlotModel PlotModelAnnualCapitalValueChange
    {
      get
      {
        InitializeAnnualCapitalValueChangelValuePlot();

        AddSeriesForAnnualCapitalValueChange();
        m_PlotModelAnnualCapitalValueChange.InvalidatePlot(true);

        return m_PlotModelAnnualCapitalValueChange;
      }
    }

    public string PlotAnnualCapitalZoomString
    {
      get { return (Math.Round(PlotAnnualCapitalZoom, 2) * 100) + " %"; }
    }

    public string Title
    {
      get { return TranslationProvider.Translate("CapitalValue"); }
    }

    public double PlotAnnualCapitalZoom
    {
      get { return m_PlotAnnualCapitalZoom; }
      set
      {
        m_PlotAnnualCapitalZoom = value;

        m_PlotModelAnnualCapitalValueChange.DefaultXAxis.Maximum = m_SelectedFinancialCalculation.KapitalwertArrayInvestition[500, 0] * (1 + PlotAnnualCapitalZoom);
        m_PlotModelAnnualCapitalValueChange.DefaultXAxis.Minimum = m_SelectedFinancialCalculation.KapitalwertArrayInvestition[500, 0] * (1 - PlotAnnualCapitalZoom);

        m_PlotModelCapitalValue.DefaultXAxis.Maximum = m_SelectedFinancialCalculation.KapitalwertArrayKosten[500, 0] * (1 + PlotAnnualCapitalZoom);
        m_PlotModelCapitalValue.DefaultXAxis.Minimum = m_SelectedFinancialCalculation.KapitalwertArrayKosten[500, 0] * (1 - PlotAnnualCapitalZoom);

        m_PlotModelAnnualCapitalValueChange.InvalidatePlot(true);
        m_PlotModelCapitalValue.InvalidatePlot(true);

        NotifyOfPropertyChange(() => PlotAnnualCapitalZoom);
        NotifyOfPropertyChange(() => PlotAnnualCapitalZoomString);
      }
    }

    public LinearAxis CreateValueAxis()
    {
      var valueAxis = new LinearAxis(AxisPosition.Left, 0)
                      {
                        MajorGridlineStyle = LineStyle.Dot,
                        MajorGridlineColor = OxyColors.DarkGray,
                        MinorGridlineColor = OxyColors.DarkGray,
                        TickStyle = TickStyle.Outside,
                        TicklineColor = OxyColors.White,
                        IsZoomEnabled = false,
                        IsPanEnabled = false
                      };
      return valueAxis;
    }

    private void AddSeriesForCapitalValue()
    {
      if (m_SelectedFinancialCalculation.KapitalwertArrayKosten.Length == 0)
      {
        return;
      }

      var columnActual = new LineSeries
                         {
                           StrokeThickness = 1,
                         };
      for (var index0 = 0; index0 < m_SelectedFinancialCalculation.KapitalwertArrayKosten.GetLength(0); index0++)
      {
        var x = m_SelectedFinancialCalculation.KapitalwertArrayKosten[index0, 0];
        var y = m_SelectedFinancialCalculation.KapitalwertArrayKosten[index0, 1];
        columnActual.Points.Add(new DataPoint(x, y));
      }
      m_PlotModelCapitalValue.Axes.Add(CreateValueAxis());
      m_PlotModelCapitalValue.Series.Add(columnActual);
    }

    private void AddSeriesForAnnualCapitalValueChange()
    {
      var columnActual = new LineSeries
                         {
                           StrokeThickness = 1,
                         };

      for (var index0 = 0; index0 < m_SelectedFinancialCalculation.KapitalwertArrayInvestition.GetLength(0); index0++)
      {
        var x = m_SelectedFinancialCalculation.KapitalwertArrayInvestition[index0, 0];
        var y = m_SelectedFinancialCalculation.KapitalwertArrayInvestition[index0, 1];
        columnActual.Points.Add(new DataPoint(x, y));
      }
      m_PlotModelAnnualCapitalValueChange.Axes.Add(CreateValueAxis());
      m_PlotModelAnnualCapitalValueChange.Series.Add(columnActual);
    }

    private void InitializeCapitalValuePlot()
    {
      m_PlotModelCapitalValue = SetupPlotModel(TranslationProvider.Translate("TitlePlotModelCapitalValue"));
    }

    private void InitializeAnnualCapitalValueChangelValuePlot()
    {
      m_PlotModelAnnualCapitalValueChange = SetupPlotModel(TranslationProvider.Translate("TitlePlotModelAnnualCapitalValueChange"));
    }

    private PlotModel SetupPlotModel(string title)
    {
      var plotModel = new PlotModel();
      var textForegroundColor = (Color) Application.Current.Resources["TextForegroundColor"];

      plotModel.Title = title;
      plotModel.TextColor = OxyColor.Parse(textForegroundColor.ToString());
      plotModel.PlotAreaBorderColor = OxyColor.Parse(textForegroundColor.ToString());
      plotModel.PlotAreaBorderThickness = new OxyThickness(1);

      return plotModel;
    }

    protected override void OnFinancialCalculationChanged(FinancialCalculation value)
    {
      m_SelectedFinancialCalculation = value;
      NotifyOfPropertyChange(() => PlotModelAnnualCapitalValueChange);
      NotifyOfPropertyChange(() => PlotModelCapitalValue);
    }
  }
}