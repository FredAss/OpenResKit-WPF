using System.Collections.Generic;
using Caliburn.Micro;

namespace Ork.Danger.ViewModels
{
    internal class AssessmentAddViewModel : Screen
    {
        private readonly string[,] m_Matrix = new string[6, 6];

        private readonly string[] m_MatrixValues =
        {
            "3", "2", "1", "1", "1", "3", "2", "1", "1", "1", "3", "2", "2", "1", "1", "3", "2", "2", "2", "1", "3", "3", "3", "2", "2"
        };

        private int m_DangerDimItem = 5;
        private int m_PossibilityItem = 5;
        private string m_RiskGroupResult = "";
        private string color = "";
        private string riskgroup = "";

        public List<string> FillPossibilityList
        {
            get
            {
                var possibilityList = new List<string>
                                      {
                                          "A - häufig",
                                          "B - gelegentlich",
                                          "C - selten",
                                          "D - unwahrscheinlich",
                                          "E - praktisch unmöglich",
                                          ""
                                      };
                return possibilityList;
            }
        }

        public int SelectedPossibility
        {
            set
            {
                m_PossibilityItem = value;
                NotifyOfPropertyChange(() => RiskGroupResult);
                NotifyOfPropertyChange(() => RiskColor);
            }
        }

        public List<string> FillDimensionList
        {
            get
            {
                var dimensionList = new List<string>
                                    {
                                        "V - ohne Arbeitsunfall",
                                        "IV - mit Arbeitsunfall",
                                        "III - leichter bleibender Gesundheitsschaden",
                                        "II - schwerer bleibender Gesundheitsschaden",
                                        "I - Tod",
                                        ""
                                    };
                return dimensionList;
            }
        }

        public int SelectedDimension
        {
            set
            {
                m_DangerDimItem = value;
                NotifyOfPropertyChange(() => RiskGroupResult);
                NotifyOfPropertyChange(() => RiskColor);
            }
        }

        public string RiskGroupResult
        {
            get
            {
                CreateRiskGroupMatrix();
                m_RiskGroupResult = m_Matrix[m_PossibilityItem, m_DangerDimItem];
                return m_RiskGroupResult;
            }
        }

        public string RiskColor
        {
            get
            {
                if (m_RiskGroupResult == "1")
                {
                    color = "Red";
                }
                if (m_RiskGroupResult == "2")
                {
                    color = "Yellow";
                }
                if (m_RiskGroupResult == "3")
                {
                    color = "LawnGreen";
                }
                if (m_RiskGroupResult != "1" &&
                    m_RiskGroupResult != "2" &&
                    m_RiskGroupResult != "3")
                {
                    color = "Transparent";
                }
                return color;
            }
        }

        public bool CancelIsEnabled
        {
            get { return true; }
        }

        public bool AcceptIsEnabled
        {
            get { return true; }
        }

        public bool RemoveIsEnabled
        {
            get { return false; }
        }

        public bool CopyIsEnabled
        {
            get { return false; }
        }

        public bool ExportIsEnabled
        {
            get { return false; }
        }

        public string CreateRiskGroupMatrix()
        {
            for (var i = 0; i < m_MatrixValues.Length; i++)
            {
                // Fill row
                for (var j = 0; j < 5; j++)
                {
                    // Fill column of row
                    for (var k = 0; k < 5; k++)
                    {
                        m_Matrix[j, k] = m_MatrixValues[i];
                        i++;
                    }
                }
            }

            m_RiskGroupResult = "";
            return m_RiskGroupResult;
        }
    }
}