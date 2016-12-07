using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ork.Danger.ViewModels;

namespace Ork.Danger
{
    public interface IAssessmentObject
    {
        AssessmentViewModel assessmentViewModel { get; set; }
        event EventHandler WorkplaceChanged;
        void Save();
    }
}
