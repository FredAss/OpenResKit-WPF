using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Ork.Building.PDFs.Models;
using Ork.Building.ViewModels;
using Ork.Framework;
using Pechkin;
using Pechkin.Synchronized;
using RazorEngine;

namespace Ork.Building.Factories
{
    [Export(typeof (IPdfFactory))]
    internal class PdfFactory : IPdfFactory
    {
        public void CreateRoomList(IEnumerable<RoomViewModel> rooms)
        {
            var rlvm = new RoomListViewModel
            {
                Rooms = rooms
            };

            var html = CreateRoomListHtml(rlvm);

            ObjectConfig oc = new ObjectConfig();
            oc.SetPrintBackground(true);
            oc.SetAllowLocalContent(true);
            oc.SetScreenMediaType(true);
            oc.SetLoadImages(true);
            oc.Footer.SetFontSize(8);
            oc.Footer.SetRightText("Erstellt: " + DateTime.Now);

            var pdfBuf = new SynchronizedPechkin(new GlobalConfig()).Convert(oc, html);

            var _SD = new SaveFileDialog
            {
                Filter = "PDF File (*.pdf)|*.pdf",
                FileName = TranslationProvider.Translate("RoomList"),
                Title = TranslationProvider.Translate("SaveAs")
            };

            if (_SD.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllBytes(_SD.FileName, pdfBuf);
                    Process.Start(_SD.FileName);
                }
                catch (IOException e)
                {
                    MessageBox.Show(TranslationProvider.Translate("CantWriteFile"));
                }
            }

        }

        private string CreateRoomListHtml(RoomListViewModel rlvm)
        {
            var text =
                File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"PDFs/Views/RoomList.cshtml"));
            var renderedText = Razor.Parse(text, rlvm);

            var html = new StringBuilder();
            html.Append("<html><head><style type=\"text/css\">");
            html.Append(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"PDFs/Styles/RoomList.css")));
            html.Append("</style></head><body>");
            html.Append(renderedText);
            html.Append("</body></html>");

            return html.ToString();
        }

    }
}