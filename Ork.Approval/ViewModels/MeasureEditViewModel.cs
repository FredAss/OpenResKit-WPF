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

using System.Collections.Generic;
using System.Linq;
using Ork.Approval.DomainModelService;
using Ork.Framework;
using Action = System.Action;

namespace Ork.Approval.ViewModels
{
    public class MeasureEditViewModel : MeasureAddViewModel
    {
        private Action m_RemoveMeasure;

        public MeasureEditViewModel(Approval_Measure model, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjectViewModels, Action removeMeasureAction)
            : base(model, responsibleSubjectViewModels)
        {
            m_RemoveMeasure = removeMeasureAction;
            DisplayName = TranslationProvider.Translate("MeasureEdit");

            SelectedResponsibleSubjectViewModel =
                ResponsibleSubjectViewModels.Single(rs => rs.Model == model.ResponsibleSubject);
        }

        public void RemoveMeasure()
        {
            m_RemoveMeasure();
        }
    }
}
