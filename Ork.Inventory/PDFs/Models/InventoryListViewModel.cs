using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ork.Inventory.ViewModels;
using Ork.Framework;

namespace Ork.Inventory.PDFs.Models
{
    public class InventoryListViewModel
    {
        public string Base64Logo;
        public string PdfTitle = TranslationProvider.Translate("InventoryList");
        public string TitleName = TranslationProvider.Translate("Name");
        public string TitleDescription = TranslationProvider.Translate("Description");
        public string TitleInventoryId = TranslationProvider.Translate("InventoryId");
        public string TitleRoom = TranslationProvider.Translate("Room");
        public string TitleInventoryType = TranslationProvider.Translate("Type");

        public InventoryListViewModel()
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
        public IEnumerable<InventoryViewModel> Inventories { get; set; }
    }
}
