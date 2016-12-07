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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ork.Approval.DomainModelService;
using Ork.Framework;
using Ork.Framework.Utilities;
using ZXing;
using ZXing.QrCode;
using File = System.IO.File;
using Image = System.Drawing.Image;

namespace Ork.Approval.ViewModels
{
    public class PlantAddViewModel : DocumentBase
    {
        private Plant m_Plant;
        
        
        public PlantAddViewModel(Plant plant)
        {
            m_Plant = plant;
            DisplayName = TranslationProvider.Translate("PlantAdd");
            
        }

        public Plant Plant
        {
            get { return m_Plant; }
        }

        public string Name
        {
            get { return m_Plant.Name; }
            set { m_Plant.Name = value; }
        }

        public string Number
        {
            get { return m_Plant.Number; }
            set
            {
                m_Plant.Number = value;
                NotifyOfPropertyChange(() => QRImage);
            }
        }

        public string Position
        {
            get { return m_Plant.Position; }
            set { m_Plant.Position = value; }
        }

        public string Description
        {
            get { return m_Plant.Description; }
            set { m_Plant.Description = value; }
        }

        public byte[] QRImage
        {
            get
            {
                if (m_Plant.Number == null)
                    return null;

                var a = CreateQRCodeFromNumber();
                

                return CreateQRCodeFromNumber();
            }
        }

        public PlantImageSource PlantImageSource
        {
            get { return m_Plant.PlantImageSource; }
            set
            {
                m_Plant.PlantImageSource = value;
                NotifyOfPropertyChange(() => PlantImageSource);
            }
        }

        public byte[] PlantImage
        {
            get
            {
                if (m_Plant.PlantImageSource != null && m_Plant.PlantImageSource.BinarySource != null)
                    return m_Plant.PlantImageSource.BinarySource;

                Stream imageStream = File.OpenRead(@".\Resources\Images\Camera.png");

                byte[] byteArray;
                using (var br = new BinaryReader(imageStream))
                {
                    byteArray = br.ReadBytes((int)imageStream.Length);
                    return byteArray;
                }
            }
        }

        private byte[] CreateQRCodeFromNumber()
        {
            var numberForQRCode = m_Plant.Number;
            var qrWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions()
                {
                    Width = 512,
                    Height = 512
                }
            };

            var result = qrWriter.Write(numberForQRCode);
            var qrImage = new Bitmap(result);
            return ImageToByteArray(qrImage);
        }

        private byte[] ImageToByteArray(Image image)
        {
            MemoryStream ms = new MemoryStream();
            image.Save(ms, ImageFormat.Png);
            return ms.ToArray();
        }

        public void SetImage()
        {
            var buffer = FileHelpers.GetByeArrayFromUserSelectedFile("Image Files |*.jpeg;*.png;*.jpg;*.gif");

            if (buffer == null)
            {
                return;
            }

            PlantImageSource = new PlantImageSource()
            {
                BinarySource = ImageHelpers.ResizeImage(buffer, 1134, 756, ImageFormat.Jpeg)
            };
            NotifyOfPropertyChange(() => PlantImage);
        }

        public IEnumerable<Document> AttachedDocuments
        {
            get
            {
                if (m_Plant.AttachedDocuments != null)
                {
                    return m_Plant.AttachedDocuments;
                }
                return null;
            }
        }

        public void Test()
        {
            var filePath = FileHelpers.GetPathFromSelectedFile(string.Empty);

            if (filePath != null)
            {
                var reader = new StreamReader(File.OpenRead(filePath));

                string line = string.Empty;
                string[] row = new string[7];

                while ((line = reader.ReadLine()) != null)
                {
                    row = line.Split(";");

                    var substance = new Substance
                    {
                        Name = row[0],
                        Description = row[1],
                        Category = int.Parse(row[2]),
                        Type = int.Parse(row[3]),
                        DangerTypes = row[4],
                        Action = row[5],
                        RiskPotential = int.Parse(row[6])
                    };

                    m_Plant.Substances.Add(substance);
                }
            }
        }

        public void AddDocument()
        {
            var filename = string.Empty;
            var binarySourceDocument = FileHelpers.GetByeArrayFromUserSelectedFile(string.Empty, out filename);

            if (binarySourceDocument != null)
            {
                m_Plant.AttachedDocuments.Add(new Document()
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
            m_Plant.AttachedDocuments.Remove(context);
            NotifyOfPropertyChange(() => AttachedDocuments);
        }

        public void RemoveImage()
        {
            PlantImageSource = null;
            NotifyOfPropertyChange(() => PlantImage);
        }

        public void PrintQRCode()
        {
            if (m_Plant.Number != null && !m_Plant.Number.Equals(""))
            {
                var qrImage = CreateBitmapImage();
                var vis = CreateDrawingVisual(qrImage);

                var printDialog = new PrintDialog();
                
                if(printDialog.ShowDialog() == true)

                printDialog.PrintVisual(vis, "MyImage");
            }
        }


        private BitmapImage CreateBitmapImage()
        {
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = new MemoryStream(QRImage);
            bi.EndInit();
            return bi;
        }

        private DrawingVisual CreateDrawingVisual(BitmapImage bi)
        {
            var vis = new DrawingVisual();
            var dc = vis.RenderOpen();
            dc.DrawImage(bi, new Rect { Width = 512, Height = 512 });
            dc.Close();
            return vis;
        }
    }
}
