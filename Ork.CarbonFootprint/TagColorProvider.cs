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
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;

namespace Ork.CarbonFootprint
{
  [Export]
  public class TagColorProvider
  {
    private readonly string m_FileName = "TagColors.xml";
    private readonly string m_FolderName = "Ork" + Path.DirectorySeparatorChar + "CarbonFootprint";
    private readonly Random m_Random = new Random();
    private readonly ContextRepository m_Repository;
    private readonly ObservableCollection<TagColor> m_TagColors = new ObservableCollection<TagColor>();

    public EventHandler ColorsUpdated;

    [ImportingConstructor]
    public TagColorProvider([Import] ContextRepository repository)
    {
      m_Repository = repository;
      m_Repository.ContextChanged += (s, e) => Application.Current.Dispatcher.Invoke(Initialize);
      m_FolderName = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + Path.DirectorySeparatorChar + m_FolderName;
      if (!Directory.Exists(m_FolderName))
      {
        Directory.CreateDirectory(m_FolderName);
      }
      m_FileName = m_FolderName + Path.DirectorySeparatorChar + m_FileName;
      Initialize();
    }

    public IEnumerable<TagColor> TagColors
    {
      get { return m_TagColors; }
    }

    [Export]
    public TagColor GetColorForTag(string tag)
    {
      var tagColor = TagColors.SingleOrDefault(tc => tc.Tag == tag);
      if (tagColor != null)
      {
        return tagColor;
      }
      var color = AddNewColorForTag(tag);
      return color;
    }


    public void SaveColorsToXml()
    {
      var xmlWriterSettings = new XmlWriterSettings
                              {
                                Indent = true
                              };

      using (var stream = File.Open(m_FileName, FileMode.Create))
      {
        var serializer = new XmlSerializer(typeof (List<TagColor>));
        using (var xmlWriter = XmlWriter.Create(stream, xmlWriterSettings))
        {
          serializer.Serialize(xmlWriter, TagColors.ToList());
        }
      }
      if (ColorsUpdated != null)
      {
        ColorsUpdated(this, new EventArgs());
      }
    }

    public void Initialize()
    {
      if (!m_Repository.HasConnection)
      {
        return;
      }

      m_TagColors.Clear();
      var tags = m_Repository.CarbonFootprints.SelectMany(cf => cf.Positions)
                             .Select(cfp => cfp.Tag)
                             .Distinct();

      var tagColorsFromXml = ReadColorsFromXml()
        .Where(tc => tags.Contains(tc.Tag))
        .ToArray();

      foreach (var tag in tags)
      {
        var savedTagColor = tagColorsFromXml.SingleOrDefault(c => c.Tag == tag);
        if (savedTagColor != null)
        {
          m_TagColors.Add(savedTagColor);
        }
        else
        {
          AddNewColorForTag(tag);
        }
      }
      SaveColorsToXml();
    }

    private TagColor AddNewColorForTag(string tag)
    {
      var colorBytes = new byte[3];
      m_Random.NextBytes(colorBytes);
      var tagColor = new TagColor(tag, Color.FromArgb(255, colorBytes[0], colorBytes[1], colorBytes[2]));
      m_TagColors.Add(tagColor);
      return tagColor;
    }

    private IEnumerable<TagColor> ReadColorsFromXml()
    {
      if (!File.Exists(m_FileName))
      {
        return new TagColor[0];
      }
      using (var stream = File.Open(m_FileName, FileMode.Open))
      {
        var serializer = new XmlSerializer(typeof (List<TagColor>));
        var data = (List<TagColor>) serializer.Deserialize(stream);
        return data;
      }
    }
  }
}