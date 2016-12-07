using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ork.Framework;
using Caliburn.Micro;

namespace Ork.Danger.ViewModels
{
    [Export(typeof(IWorkspace))]
    public class OverViewsManagementViewModel: DocumentBase, IWorkspace
    {
        public int Index
        {
            get { return 118; }
        }

        public bool IsEnabled
        {
            get { return true; }
        }

        public string Title
        {
            get { return "Übersichten"; }
        }
    }
}
