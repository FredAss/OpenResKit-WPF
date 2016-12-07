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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Ork.Framework;
using Ork.Framework.Utilities;
using Ork.Measure.DomainModelService;

namespace Ork.Measure.ViewModels
{
  public class MeasureAddViewModel : DocumentBase
  {
    private readonly DomainModelService.Measure m_Model;
    private readonly IEnumerable m_Priorities;
    protected readonly IRelatedElementProvider[] m_RelatedElementProviders;
    private readonly IEnumerable<ResponsibleSubjectViewModel> m_ResponsibleSubjects;

    protected IEnumerable<RelatedElementProviderViewModel> m_RelatedElementProviderViewModels;
    private string m_ResponsibleSubjectSearchText = string.Empty;
    private ResponsibleSubjectViewModel m_SelectedResponsibleSubject;

    public MeasureAddViewModel(DomainModelService.Measure model, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjectViewModels, IRelatedElementProvider[] relatedElementProvider)
    {
      m_Model = model;
      if (m_Model.CreationDate == new DateTime())
      {
        m_Model.CreationDate = DateTime.Now;
        m_Model.DueDate = DateTime.Now;
      }
      m_Priorities = Enum.GetValues(typeof (Priority));
      m_ResponsibleSubjects = responsibleSubjectViewModels;
      DisplayName = TranslationProvider.Translate("TitleMeasureAddViewModel");
      m_RelatedElementProviders = relatedElementProvider;
    }

    public virtual IEnumerable<RelatedElementProviderViewModel> RelatedElementProviders
    {
      get
      {
        if (m_RelatedElementProviderViewModels == null)
        {
          m_RelatedElementProviderViewModels = m_RelatedElementProviders.Select(r => r.CreateViewModel())
                                                                        .ToArray();
          foreach (var relatedElementProviderViewModel in m_RelatedElementProviderViewModels)
          {
            relatedElementProviderViewModel.PropertyChanged += RelatedElementViewModelIsSelected;
          }
        }
        return m_RelatedElementProviderViewModels;
      }
    }

    public bool CanMeasureAdd
    {
      get { return !(m_Model.Name == "" | m_Model.Description == "" | m_Model.ResponsibleSubject == null); }
    }

    public string Name
    {
      get { return m_Model.Name; }
      set
      {
        m_Model.Name = value;
        NotifyOfPropertyChange(() => CanMeasureAdd);
      }
    }

    public MeasureImageSource MeasureImageSource
    {
      get { return m_Model.MeasureImageSource; }
      set
      {
        m_Model.MeasureImageSource = value;
        NotifyOfPropertyChange(() => MeasureImageSource);
      }
    }

    public byte[] Image
    {
      get
      {
        if (m_Model.MeasureImageSource != null &&
            m_Model.MeasureImageSource.BinarySource != null)
        {
          return m_Model.MeasureImageSource.BinarySource;
        }

        Stream imageStream = File.OpenRead(@".\Resources\Images\Camera.png");

        byte[] byteArray;
        using (var br = new BinaryReader(imageStream))
        {
          byteArray = br.ReadBytes((int) imageStream.Length);
          return byteArray;
        }
      }
    }

    public Document Document
    {
      get
      {
        if (m_Model.AttachedDocuments != null &&
            m_Model.AttachedDocuments.All(ad => ad.DocumentSource != null))
        {
          return m_Model.AttachedDocuments.First();
        }
        return null;
      }
    }

    public IEnumerable<Document> AttachedDocuments
    {
      get
      {
        if (m_Model.AttachedDocuments != null)
        {
          return m_Model.AttachedDocuments;
        }
        return null;
      }
    }

    public string Description
    {
      get { return m_Model.Description; }
      set
      {
        m_Model.Description = value;
        NotifyOfPropertyChange(() => CanMeasureAdd);
      }
    }

    public DateTime DueDate
    {
      get { return m_Model.DueDate; }
      set { m_Model.DueDate = value; }
    }

    public int Priority
    {
      get { return m_Model.Priority; }
      set { m_Model.Priority = value; }
    }

    public IEnumerable Priorities
    {
      get { return m_Priorities; }
    }

    public DomainModelService.Measure Model
    {
      get { return m_Model; }
    }

    public string ResponsibleSubjectSearchText
    {
      get { return m_ResponsibleSubjectSearchText; }
      set
      {
        m_ResponsibleSubjectSearchText = value;
        NotifyOfPropertyChange(() => FilteredResponsibleSubjects);
      }
    }

    public IEnumerable<ResponsibleSubjectViewModel> FilteredResponsibleSubjects
    {
      get { return SearchInResponsibleObjectList(); }
    }

    public ResponsibleSubjectViewModel SelectedResponsibleSubject
    {
      get { return m_SelectedResponsibleSubject; }
      set
      {
        m_SelectedResponsibleSubject = value;
        m_Model.ResponsibleSubject = value.Model;
        NotifyOfPropertyChange(() => m_Model.ResponsibleSubject);
        NotifyOfPropertyChange(() => CanMeasureAdd);
      }
    }

    public void SetImage()
    {
      var buffer = FileHelpers.GetByeArrayFromUserSelectedFile("Image Files |*.jpeg;*.png;*.jpg;*.gif");

      if (buffer == null)
      {
        return;
      }

      MeasureImageSource = new MeasureImageSource()
                           {
                             BinarySource = ImageHelpers.ResizeImage(buffer, 1134, 756, ImageFormat.Jpeg)
                           };
      NotifyOfPropertyChange(() => Image);
    }

    public void DeleteImage()
    {
      if (m_Model.MeasureImageSource == null ||
          m_Model.MeasureImageSource.BinarySource == null)
      {
        return;
      }

      m_Model.MeasureImageSource = null;
      NotifyOfPropertyChange(() => Image);
    }

    public void AddDocument()
    {
      var filename = string.Empty;
      var binarySourceDocument = FileHelpers.GetByeArrayFromUserSelectedFile(string.Empty, out filename);

      if (binarySourceDocument != null)
      {
        m_Model.AttachedDocuments.Add(new Document()
                                      {
                                        DocumentSource = new DocumentSource()
                                                         {
                                                           BinarySource = binarySourceDocument
                                                         },
                                        Name = filename
                                      });

        NotifyOfPropertyChange(() => AttachedDocuments);
      }
    }

    public void OpenDocument(Document context)

    {
      File.WriteAllBytes(Path.GetTempPath() + context.Name, context.DocumentSource.BinarySource);

      Process.Start(Path.GetTempPath() + context.Name);
    }


    public void DeleteDocument(Document context)
    {
      m_Model.AttachedDocuments.Remove(context);
      NotifyOfPropertyChange(() => AttachedDocuments);
    }

    protected void RelatedElementViewModelIsSelected(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "IsSelected")
      {
        NotifyOfPropertyChange(() => CanMeasureAdd);
      }
    }

    private IEnumerable<ResponsibleSubjectViewModel> SearchInResponsibleObjectList()
    {
      if (string.IsNullOrEmpty(ResponsibleSubjectSearchText))
      {
        return m_ResponsibleSubjects;
      }
      var searchText = ResponsibleSubjectSearchText.ToLower();

      var searchResultResponsibleSubjects = m_ResponsibleSubjects.Where(e => (e.Infotext != null) && (e.Infotext.ToLower()
                                                                                                       .Contains(searchText.ToLower())));

      return searchResultResponsibleSubjects;
    }
  }

  //public class DocumentViewModel
  //{

  //  private string m_Name ;
  //  private byte[] m_Content;

  //  public DocumentViewModel(Document)
  //  {

  //  }
  //}
}