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
  public class SummaryViewModel : InvestmentEvaluationContentBaseViewModel
  {
    private PlotModel m_PlotModelCapital;
    private PlotModel m_PlotModelTime;
    private PlotModel m_PlotModelZinsen;
    private FinancialCalculation m_SelectedFinancialCalculation = new FinancialCalculation();

    public SummaryViewModel()
    {
      Index = 1;
    }

    public PlotModel PlotModelZinsen
    {
      get
      {
        InitializeZinsenPlot();
        m_PlotModelZinsen.InvalidatePlot(true);

        return m_PlotModelZinsen;
      }
    }

    public PlotModel PlotModelTime
    {
      get
      {
        InitializeZeitPlot();
        m_PlotModelTime.InvalidatePlot(true);

        return m_PlotModelTime;
      }
    }

    public PlotModel PlotModelCapital
    {
      get
      {
        InitializeCapitalPlot();
        m_PlotModelCapital.InvalidatePlot(true);

        return m_PlotModelCapital;
      }
    }


    public double StaticAmortisation
    {
      get { return Math.Round(m_SelectedFinancialCalculation.StaticAmortization, 2); }
    }

    public double StaticAmortisationPercentage
    {
      get { return Math.Round(m_SelectedFinancialCalculation.StaticAmortizationPercentage, 2); }
    }

    public double Amortisation10Percent
    {
      get { return Math.Round(m_SelectedFinancialCalculation.DynamicAmortization, 2); }
    }

    public double Amortisation10PercentPercentage
    {
      get { return Math.Round(m_SelectedFinancialCalculation.DynamicAmortizationPercentage, 2); }
    }

    public double CapitalValue10Percent
    {
      get { return Math.Round(m_SelectedFinancialCalculation.Kapitalwert, 2); }
    }

    public double CapitalValue10PercentPercentage
    {
      get { return Math.Round(m_SelectedFinancialCalculation.Kapitalwert, 2); }
    }

    public double InterneVerzinsung
    {
      get { return Math.Round(m_SelectedFinancialCalculation.InterneVerzinsung, 2); }
    }

    public double JahreskostenAltNeu
    {
      get { return Math.Round(m_SelectedFinancialCalculation.JahreskostenAltNeu, 2); }
    }

    public double JahreskostenVergleich
    {
      get { return Math.Round(m_SelectedFinancialCalculation.JahreskostenVergleich, 2); }
    }

    public double Kostenersparnis
    {
      get { return Math.Round(m_SelectedFinancialCalculation.Kostenersparnis, 2); }
    }

    public double InterestRate
    {
      get
      {
        if (m_SelectedFinancialCalculation.InvestmentPlanModel == null)
        {
          return 0;
        }

        return Math.Round(m_SelectedFinancialCalculation.InvestmentPlanModel.ImputedInterestRate * 100, 1);
      }
    }


    public string Title
    {
      get { return TranslationProvider.Translate("Summary"); }
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


    private void InitializeZinsenPlot()
    {
      m_PlotModelZinsen = SetupPlotModel(TranslationProvider.Translate("TitlePlotModelZinsen"));
      var columnSeries = new ColumnSeries
                         {
                           StrokeThickness = 0,
                           IsStacked = false,
                           LabelPlacement = LabelPlacement.Base,
                         };
      columnSeries.Items.Add(new ColumnItem(10, 0)); // Fester Zins für immer?
      columnSeries.Items.Add(new ColumnItem(InterneVerzinsung * 100, 1));
      m_PlotModelZinsen.Axes.Add(CreateValueAxis());
      m_PlotModelZinsen.Axes.Add(new CategoryAxis("", new string[]
                                                      {
                                                        "kalk.Zinssatz", "interne Verzinsung"
                                                      }));

      m_PlotModelZinsen.Series.Add(columnSeries);
    }

    private void InitializeCapitalPlot()
    {
      m_PlotModelCapital = SetupPlotModel(TranslationProvider.Translate("TitlePlotModelCapital"));
      var columnSeries = new ColumnSeries
                         {
                           StrokeThickness = 0,
                           IsStacked = false,
                           LabelPlacement = LabelPlacement.Base,
                         };

      columnSeries.Items.Add(new ColumnItem(m_SelectedFinancialCalculation.KreditFürInvestition, 0));
      columnSeries.Items.Add(new ColumnItem(m_SelectedFinancialCalculation.Kapitalwert, 1));

      m_PlotModelCapital.Axes.Add(new CategoryAxis("", new string[]
                                                       {
                                                         "Investition", "Kapitalwert"
                                                       }));
      m_PlotModelCapital.Axes.Add(CreateValueAxis());

      m_PlotModelCapital.Series.Add(columnSeries);
    }

    private void InitializeZeitPlot()
    {
      m_PlotModelTime = SetupPlotModel(TranslationProvider.Translate("TitlePlotModelTime"));

      if (m_SelectedFinancialCalculation.InvestmentPlanModel == null)
      {
        return;
      }

      var columnSeries = new ColumnSeries
                         {
                           StrokeThickness = 0,
                           IsStacked = false,
                           LabelPlacement = LabelPlacement.Inside,
                         };
      columnSeries.Items.Add(new ColumnItem(Amortisation10Percent, 0));
      columnSeries.Items.Add(new ColumnItem(m_SelectedFinancialCalculation.InvestmentPlanModel.Lifetime, 1));
      columnSeries.Items.Add(new ColumnItem(m_SelectedFinancialCalculation.ComparisonModel.Lifetime, 2));

      var categoryAxes = new CategoryAxis("", new string[]
                                              {
                                                "dyn. Amortisation", "ND Alt/neu", "ND"
                                              });

      m_PlotModelTime.Axes.Add(categoryAxes);
      m_PlotModelTime.Axes.Add(CreateValueAxis());

      m_PlotModelTime.Series.Add(columnSeries);
    }

    private void OnAxisChanged(object sender, AxisChangedEventArgs axisChangedEventArgs)
    {
      if (sender != null)
      {
      }
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
    }
  }
}