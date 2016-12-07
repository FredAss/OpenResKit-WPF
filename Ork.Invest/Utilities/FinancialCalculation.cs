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
using Microsoft.VisualBasic;
using Ork.Invest.DomainModelService;
using Ork.Invest.ViewModels;

namespace Ork.Invest.Utilities
{
  public class FinancialCalculation
  {
    private readonly InvestmentPlan m_InvestmentPlan;
    private double m_EnergieKostenAltNeu = 0;
    private double m_EnergieKostenVergleich = 0;
    private double m_InvestmentAltNeu = 0;
    private double m_InvestmentVergleich = 0;
    private double m_KalkulatorischerZinssatz = 0;
    private int m_NutzungsdauerAltNeu = 0;
    private int m_NutzungsdauerVergleich = 0;
    private double m_RestWertNachEndeNutzungsdauerAltNeu = 0;
    private double m_RestWertNachEndeNutzungsdauerVergleich = 0;
    private double m_RestwertInvestitionHeuteAltNeu = 0;
    private double m_RestwertInvestitionHeuteVergleich = 0;
    private double m_SonstigeErträgeProJahrAltNeu = 0;
    private double m_SonstigeErträgeProJahrVergleich = 0;
    private double m_SonstigeKostenProJahrAltNeu = 0;
    private double m_SonstigeKostenProJahrVergleich = 0;
    private int m_Startjahr = 0;
    private double m_ÄnderungEnergiekostenProJahrAltNeu = 0;
    private double m_ÄnderungEnergiekostenProJahrVergleich = 0;
    private double m_ÄnderungSonstigeErträgeProJahr = 0;
    private double m_ÄnderungSonstigeKostenProJahr = 0;

    public FinancialCalculation()
    {
      KapitalwertArrayInvestition = new double[0, 0];
      KapitalwertArrayKosten = new double[0, 0];      
    }

    public FinancialCalculation(InvestmentPlan investmentPlan, Comparison comparison)
    {
      m_InvestmentPlan = investmentPlan;
      m_InvestmentPlan.PropertyChanged += (s, e) =>
                                          {
                                            Initialize();
                                            Calculate();
                                          };
      ComparisonModel = comparison;
      ComparisonModel.PropertyChanged += (s, e) =>
                                         {
                                           Initialize();
                                           Calculate();
                                         };
      Initialize();
      Calculate();
    }

    public InvestmentPlan InvestmentPlanModel
    {
      get { return m_InvestmentPlan; }
    }

    public Comparison ComparisonModel { get; private set; }

    public string ComparisonName
    {
      get { return ComparisonModel.ComparisonName; }
    }

    public double DynamicAmortization { get; private set; }
    public double DynamicAmortizationPercentage { get; private set; }
    public double InterneVerzinsung { get; private set; }
    public double JahreskostenAltNeu { get; private set; }
    public double JahreskostenVergleich { get; private set; }
    public double Kapitalwert { get; private set; }
    public double Kostenersparnis { get; private set; }
    public double KreditFürInvestition { get; private set; }
    public double StaticAmortization { get; private set; }
    public double StaticAmortizationPercentage { get; private set; }
    public IEnumerable<Payment> Payments { get; private set; }

    public double[,] KapitalwertArrayInvestition { get; private set; }
    public double[,] KapitalwertArrayKosten { get; private set; }


    private void Initialize()
    {
      m_Startjahr = m_InvestmentPlan.StartYear.Year;
      m_NutzungsdauerAltNeu = m_InvestmentPlan.Lifetime;
      m_KalkulatorischerZinssatz = m_InvestmentPlan.ImputedInterestRate;
      m_InvestmentAltNeu = m_InvestmentPlan.InvestmentSum;
      m_RestwertInvestitionHeuteAltNeu = m_InvestmentPlan.RecoveryValueToday;
      m_RestWertNachEndeNutzungsdauerAltNeu = m_InvestmentPlan.RecoveryValueAfterLifetime;
      m_EnergieKostenAltNeu = m_InvestmentPlan.EnergyCostsAnnual;
      m_ÄnderungEnergiekostenProJahrAltNeu = m_InvestmentPlan.EnergyCostsChangePA;
      m_SonstigeKostenProJahrAltNeu = m_InvestmentPlan.OtherCostsPA;
      m_ÄnderungSonstigeKostenProJahr = m_InvestmentPlan.OtherCostsChangePA;
      m_SonstigeErträgeProJahrAltNeu = m_InvestmentPlan.OtherRevenuePA;
      m_ÄnderungSonstigeErträgeProJahr = m_InvestmentPlan.OtherRevenueChangePA;

      m_NutzungsdauerVergleich = ComparisonModel.Lifetime;
      m_InvestmentVergleich = ComparisonModel.InvestmentSum;
      m_RestwertInvestitionHeuteVergleich = ComparisonModel.RecoveryValueToday;
      m_RestWertNachEndeNutzungsdauerVergleich = ComparisonModel.RecoveryValueAfterLifetime;
      m_EnergieKostenVergleich = ComparisonModel.EnergyCostsAnnual;
      m_ÄnderungEnergiekostenProJahrVergleich = ComparisonModel.EnergyCostsChangePA;
      m_SonstigeKostenProJahrVergleich = ComparisonModel.OtherCostsPA;
      m_SonstigeErträgeProJahrVergleich = ComparisonModel.OtherRevenuePA;
    }

    private void Calculate()
    {
      try
      {
        StaticAmortization = CalculateStaticAmortization();
        StaticAmortizationPercentage = CalculateStaticAmortizationPercentage();
        KreditFürInvestition = CalculateKreditFürInvestition();
        Payments = CalculatePayments();
        Kapitalwert = CalculateKapitalwert();
        DynamicAmortization = CalculateDynamicAmortization();
        DynamicAmortizationPercentage = CalculateDynamicAmortizationPercentage();
        InterneVerzinsung = CalculateInterneVerzinsung();
        JahreskostenAltNeu = CalculateJahreskostenAltNeu();
        JahreskostenVergleich = CalculateJahreskostenVergleich();
        Kostenersparnis = CalculateKostenersparnis();
        KapitalwertArrayInvestition = CalculateKapitalwertInvestitionArray();
        KapitalwertArrayKosten = CalculateKapitalwertKostenArray();
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
      }
    }

    private double CalculateKostenersparnis()
    {
      return JahreskostenAltNeu - JahreskostenVergleich;
    }

    private double CalculateJahreskosten(int nd, double invest, double restwert, double energieK, double ändEP, double sonstigeK, double sonstigeEr)
    {
      double erg = 0;
      if (m_KalkulatorischerZinssatz != 0)
      {
        for (var jahr = 1; jahr <= nd; jahr++)
        {
          erg = erg +
                (energieK * Math.Pow(1 + ändEP, jahr - 1) + sonstigeK * Math.Pow(1 + m_ÄnderungSonstigeKostenProJahr, jahr - 1) - sonstigeEr * Math.Pow(1 + m_ÄnderungSonstigeErträgeProJahr, jahr - 1)) /
                Math.Pow(1 + m_KalkulatorischerZinssatz, jahr);
        }
        erg = erg * (m_KalkulatorischerZinssatz * Math.Pow(1 + m_KalkulatorischerZinssatz, nd)) / (Math.Pow(1 + m_KalkulatorischerZinssatz, nd) - 1);
        erg = erg + invest * (m_KalkulatorischerZinssatz * Math.Pow(1 + m_KalkulatorischerZinssatz, nd)) / (Math.Pow(1 + m_KalkulatorischerZinssatz, nd) - 1);
        erg = erg -
              restwert / Math.Pow(1 + m_KalkulatorischerZinssatz, nd) * (m_KalkulatorischerZinssatz * Math.Pow(1 + m_KalkulatorischerZinssatz, nd)) / (Math.Pow(1 + m_KalkulatorischerZinssatz, nd) - 1);
      }
      else
      {
        for (var jahr = 1; jahr <= nd - 1; jahr++)
        {
          erg = erg +
                (energieK * Math.Pow(1 + ändEP, jahr) + sonstigeK * Math.Pow(1 + m_ÄnderungSonstigeKostenProJahr, jahr) - sonstigeEr * Math.Pow(1 + m_ÄnderungSonstigeKostenProJahr, jahr)) /
                Math.Pow(1 + m_KalkulatorischerZinssatz, jahr);
        }
        erg = erg / nd + invest / nd - restwert / nd;
      }
      return erg;
    }

    private double CalculateJahreskostenVergleich()
    {
      return CalculateJahreskosten(m_NutzungsdauerVergleich, m_InvestmentVergleich - m_RestwertInvestitionHeuteAltNeu, m_RestWertNachEndeNutzungsdauerVergleich, m_EnergieKostenVergleich,
        m_ÄnderungEnergiekostenProJahrVergleich, m_SonstigeKostenProJahrVergleich, m_SonstigeErträgeProJahrVergleich);
    }

    private double CalculateJahreskostenAltNeu()
    {
      return CalculateJahreskosten(m_NutzungsdauerAltNeu, m_InvestmentAltNeu, m_RestWertNachEndeNutzungsdauerAltNeu, m_EnergieKostenAltNeu, m_ÄnderungEnergiekostenProJahrAltNeu,
        m_SonstigeKostenProJahrAltNeu, m_SonstigeErträgeProJahrAltNeu);
    }

    private double CalculateInterneVerzinsung()
    {
      var betrag = KreditFürInvestition > 0
        ? -KreditFürInvestition
        : 0;
      var values = new[]
                   {
                     betrag
                   }.Concat(Payments.Select(p => p.JährlicherRückfluss))
                    .ToArray();
      return Financial.IRR(ref values);
    }

    private double CalculateDynamicAmortization()
    {
      double dynintv = Payments.Count(p => p.Abzahlungsbetrag != 0);
      var ersterOhneAbzahlungsbetrag = new Payment();
      if (!Payments.Any(p => p.Abzahlungsbetrag == 0))
      {
        var nullPayment = new Payment();
        nullPayment.Abzahlungsbetrag = 0;
        nullPayment.JährlicherRückfluss = 0;
        nullPayment.Tilgung = 0;
        nullPayment.Zins = 0;
        nullPayment.Überschüsse = 0;
        nullPayment.Kapitalwert = 0;
        ersterOhneAbzahlungsbetrag = nullPayment;
      }
      else
      {
        ersterOhneAbzahlungsbetrag = Payments.First(p => p.Abzahlungsbetrag == 0);
      }
      
      
      var berechnung = (-ersterOhneAbzahlungsbetrag.Zins - ersterOhneAbzahlungsbetrag.Tilgung) / ersterOhneAbzahlungsbetrag.JährlicherRückfluss;
      if (dynintv > 0 &&
          (ersterOhneAbzahlungsbetrag.Year - m_Startjahr < m_NutzungsdauerAltNeu || (ersterOhneAbzahlungsbetrag.Year - m_Startjahr == m_NutzungsdauerAltNeu && berechnung <= 1)))
      {
        return dynintv + berechnung;
      }
      if (dynintv == 0 &&
          ersterOhneAbzahlungsbetrag.Tilgung < 0)
      {
        return berechnung;
      }
      return double.NaN;
    }

    private double CalculateKapitalwert()
    {
      var betrag = KreditFürInvestition < 0
        ? -KreditFürInvestition
        : 0;
      return Payments.Sum(p => p.Kapitalwert) + betrag;
    }

    private double CalculateStaticAmortization()
    {
      return (m_InvestmentVergleich * m_NutzungsdauerAltNeu / m_NutzungsdauerVergleich - m_InvestmentAltNeu - m_RestwertInvestitionHeuteAltNeu + m_RestWertNachEndeNutzungsdauerAltNeu -
              m_RestWertNachEndeNutzungsdauerVergleich * m_NutzungsdauerAltNeu / m_NutzungsdauerVergleich) /
             (m_EnergieKostenAltNeu - m_EnergieKostenVergleich + m_SonstigeKostenProJahrAltNeu - m_SonstigeKostenProJahrVergleich - m_SonstigeErträgeProJahrAltNeu + m_SonstigeErträgeProJahrVergleich);
    }

    private double CalculateStaticAmortizationPercentage()
    {
      return StaticAmortization / m_NutzungsdauerAltNeu * 100;
    }

    private double CalculateDynamicAmortizationPercentage()
    {
      return DynamicAmortization / m_NutzungsdauerAltNeu * 100;
    }

    private double CalculateKreditFürInvestition()
    {
      return -Financial.PV(m_KalkulatorischerZinssatz, m_NutzungsdauerAltNeu, -Financial.Pmt(m_KalkulatorischerZinssatz, m_NutzungsdauerVergleich, m_InvestmentVergleich)) - m_InvestmentAltNeu -
             Financial.PV(m_KalkulatorischerZinssatz, m_NutzungsdauerAltNeu,
               Financial.Pmt(m_KalkulatorischerZinssatz, m_NutzungsdauerVergleich, m_RestWertNachEndeNutzungsdauerVergleich / Math.Pow((1 + m_KalkulatorischerZinssatz), m_NutzungsdauerVergleich)) -
               m_RestwertInvestitionHeuteAltNeu + m_RestWertNachEndeNutzungsdauerAltNeu / Math.Pow((1 + m_KalkulatorischerZinssatz), m_NutzungsdauerAltNeu));
    }


    private IEnumerable<Payment> CalculatePayments()
    {
      var payments = new List<Payment>();
      var betrag = KreditFürInvestition > 0
        ? -KreditFürInvestition
        : 0;
      for (var year = 1; year <= m_NutzungsdauerAltNeu; year++)
      {
        var payment = new Payment
                      {
                        Year = m_Startjahr + year
                      };
        payment.JährlicherRückfluss = CalculateJährlicherRückfluss(year);

        if (betrag < 0 &&
            payment.JährlicherRückfluss > 0)
        {
          payment.Zins = betrag * m_KalkulatorischerZinssatz;
          payment.Tilgung = (betrag < (-payment.JährlicherRückfluss - payment.Zins)
            ? -payment.JährlicherRückfluss - payment.Zins
            : betrag);
        }
        else
        {
          payment.Zins = 0;
          payment.Tilgung = 0;
        }

        betrag = betrag - payment.Tilgung < 0 && payment.JährlicherRückfluss > 0
          ? betrag - payment.Tilgung
          : 0;

        payment.Abzahlungsbetrag = betrag;

        payment.Überschüsse = payment.Zins + payment.Tilgung + payment.JährlicherRückfluss;

        var test = CalculateJährlicherRückfluss(year + 1);


        if (payment.Überschüsse > 0)
        {
          payment.Kapitalwert = payment.Überschüsse / Math.Pow((1 + m_KalkulatorischerZinssatz), year);
        }
        else if (payment.Abzahlungsbetrag < 0 &&
                test == 0)
        {
          payment.Kapitalwert = payment.Abzahlungsbetrag / Math.Pow((1 + m_KalkulatorischerZinssatz), year);
        }
        else
        {
          payment.Kapitalwert = 0;
        }

        payments.Add(payment);
      }
      return payments;
    }

    private double CalculateJährlicherRückfluss(int year)
    {
      if (year <= m_NutzungsdauerAltNeu)
      {
        var abstand = year - 1;
        return m_EnergieKostenAltNeu * Math.Pow((1 + m_ÄnderungEnergiekostenProJahrAltNeu), abstand) - m_EnergieKostenVergleich * Math.Pow((1 + m_ÄnderungEnergiekostenProJahrVergleich), abstand) +
               (m_SonstigeKostenProJahrAltNeu - m_SonstigeKostenProJahrVergleich) * Math.Pow((1 + m_ÄnderungSonstigeKostenProJahr), abstand) -
               (m_SonstigeErträgeProJahrAltNeu - m_SonstigeErträgeProJahrVergleich) * Math.Pow((1 + m_ÄnderungSonstigeErträgeProJahr), abstand);
      }
      return 0;
    }

    private double[,] CalculateKapitalwertInvestitionArray()
    {
      var values = new double[1000, 2];


      var investitionAltNeu = m_InvestmentAltNeu;
      var investitionVergleich = m_InvestmentVergleich;
      var restwertHeuteAltNeu = m_RestwertInvestitionHeuteAltNeu;
      var restwertNutzungsdauerAltNeu = m_RestWertNachEndeNutzungsdauerAltNeu;
      var restwertNutzungsdauerVergleich = m_RestWertNachEndeNutzungsdauerVergleich;
  

      for (var i = 0; i < 1000; i++)
      {
        m_InvestmentAltNeu = investitionAltNeu * (0.5 + i * 0.001);
        m_InvestmentVergleich = investitionVergleich * (0.5 + i * 0.001);
        m_RestwertInvestitionHeuteAltNeu = restwertHeuteAltNeu * (0.5 + i * 0.001);
        m_RestWertNachEndeNutzungsdauerAltNeu = restwertNutzungsdauerAltNeu * (0.5 + i * 0.001);
        m_RestWertNachEndeNutzungsdauerVergleich = restwertNutzungsdauerVergleich * (0.5 + i * 0.001);
        KreditFürInvestition = CalculateKreditFürInvestition();
        Payments = CalculatePayments();

        values[i, 0] = CalculateKreditFürInvestition();

  
        values[i, 1] = CalculateKapitalwert();
      }

      m_InvestmentAltNeu = investitionAltNeu;
      m_InvestmentVergleich = investitionVergleich;
      m_RestwertInvestitionHeuteAltNeu = restwertHeuteAltNeu;
      m_RestWertNachEndeNutzungsdauerAltNeu = restwertNutzungsdauerAltNeu;
      m_RestWertNachEndeNutzungsdauerVergleich = restwertNutzungsdauerVergleich;
      KreditFürInvestition = CalculateKreditFürInvestition();
      Payments = CalculatePayments();
      return values;
    }

    private double[,] CalculateKapitalwertKostenArray()
    {
      var values = new double[1000, 2];

      var energieKostenAltNeu = m_EnergieKostenAltNeu;
      var energieKostenVergleich = m_EnergieKostenVergleich;
      var sonstigeKostenAltNeu = m_SonstigeKostenProJahrAltNeu;
      var sonstigeKostenVergleich = m_SonstigeKostenProJahrVergleich;
      var sonstigeErträgeAltNeu = m_SonstigeErträgeProJahrAltNeu;
      var sonstigeErträgeVergleich = m_SonstigeErträgeProJahrVergleich;
    

      for (var i = 0; i < 1000; i++)
      {
        m_EnergieKostenAltNeu = energieKostenAltNeu * (0.5 + i * 0.001);
        m_EnergieKostenVergleich = energieKostenVergleich * (0.5 + i * 0.001);
        m_SonstigeKostenProJahrAltNeu = sonstigeKostenAltNeu * (0.5 + i * 0.001);
        m_SonstigeKostenProJahrVergleich = sonstigeKostenVergleich * (0.5 + i * 0.001);
        m_SonstigeErträgeProJahrAltNeu = sonstigeErträgeAltNeu * (0.5 + i * 0.001);
        m_SonstigeErträgeProJahrVergleich = sonstigeErträgeVergleich * (0.5 + i * 0.001);
        KreditFürInvestition = CalculateKreditFürInvestition();
        Payments = CalculatePayments();

        values[i, 0] = CalculateJährlicherRückfluss(1);
        values[i, 1] = CalculateKapitalwert();
        
      }
      m_EnergieKostenAltNeu = energieKostenAltNeu;
      m_EnergieKostenVergleich = energieKostenVergleich;
      m_SonstigeKostenProJahrAltNeu = sonstigeKostenAltNeu;
      m_SonstigeKostenProJahrVergleich = sonstigeKostenVergleich;
      m_SonstigeErträgeProJahrAltNeu = sonstigeErträgeAltNeu;
      m_SonstigeErträgeProJahrVergleich = sonstigeErträgeVergleich;
      KreditFürInvestition = CalculateKreditFürInvestition();
      Payments = CalculatePayments();
      return values;
    }
  }
}