using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Ork.Building.ViewModels;
using Ork.Framework;

namespace Ork.Building.PDFs.Models
{
    public class RoomListViewModel
    {
        public string Base64Logo;
        public string PdfTitle = TranslationProvider.Translate("RoomList");
        public string TitleAreaInM2 = TranslationProvider.Translate("AreaInM2");
        public string TitleBuilding = TranslationProvider.Translate("Building");
        public string TitleDescription = TranslationProvider.Translate("Description");
        public string TitleHeightInM = TranslationProvider.Translate("HeightInM");
        public string TitleName = TranslationProvider.Translate("Name");
        public string TitleVolume = TranslationProvider.Translate("Volume");

        public RoomListViewModel()
        {
            using (var image = Image.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources/Images/logo.jpg")))
            {
                using (var m = new MemoryStream())
                {
                    image.Save(m, image.RawFormat);
                    var imageBytes = m.ToArray();

                    // Convert byte[] to Base64 String
                    Base64Logo = "data:image/jpg;base64," + Convert.ToBase64String(imageBytes);
                }
            }
        }


        public IEnumerable<RoomViewModel> Rooms { get; set; }
    }
}