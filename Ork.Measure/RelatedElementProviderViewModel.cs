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
using Caliburn.Micro;
using Ork.Measure.ViewModels;

namespace Ork.Measure
{
  public class RelatedElementProviderViewModel : PropertyChangedBase
  {
    private bool m_IsExpanded;

    public RelatedElementProviderViewModel(string name, IEnumerable<RelatedElementViewModel> elements, IRelatedElementProvider provider)
    {
      Provider = provider;
      Name = name;
      Elements = elements;
    }

    public string Name { get; private set; }

    public bool IsExpanded
    {
      get { return m_IsExpanded; }
      set
      {
        m_IsExpanded = value;
        NotifyOfPropertyChange(() => IsExpanded);
      }
    }

    public IEnumerable<RelatedElementViewModel> Elements { get; private set; }

    public IRelatedElementProvider Provider { get; private set; }
  }
}