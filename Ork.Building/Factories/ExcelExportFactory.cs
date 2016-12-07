using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using OfficeOpenXml;
using Ork.Building.ViewModels;
using Ork.Framework;

namespace Ork.Building.Factories
{
    [Export(typeof (IExcelExportFactory))]
    internal class ExcelExportFactory : IExcelExportFactory
    {
        public void CreateRoomList(IEnumerable<RoomViewModel> rooms)
        {
            var _SD = new SaveFileDialog
            {
                Filter = "Excel File (*.xlsx)|*.xlsx",
                FileName = TranslationProvider.Translate("RoomList"),
                Title = TranslationProvider.Translate("SaveAs")
            };

            if (_SD.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    FileInfo newFile = new FileInfo(_SD.FileName);
                    if (newFile.Exists)
                    {
                        newFile.Delete();  // ensures we create a new workbook
                        newFile = new FileInfo(_SD.FileName);
                    }

                    var package = new ExcelPackage(newFile);
                    var worksheet = package.Workbook.Worksheets.Add("Rooms");

                    FillWorksheet(worksheet, rooms);
                    worksheet.Cells.AutoFitColumns(0);

                    package.Save();

                    Process.Start(_SD.FileName);
                }
                catch (IOException e)
                {
                    MessageBox.Show(TranslationProvider.Translate("CantWriteFile"));
                }
            }
        }

        private void CreateHeader(ExcelWorksheet worksheet)
        {
            worksheet.Cells["A1"].Value = TranslationProvider.Translate("Name");
            worksheet.Cells["B1"].Value = TranslationProvider.Translate("Description");
            worksheet.Cells["C1"].Value = TranslationProvider.Translate("AreaInM2");
            worksheet.Cells["D1"].Value = TranslationProvider.Translate("HeightInM");
            worksheet.Cells["E1"].Value = TranslationProvider.Translate("Volume");
            worksheet.Cells["F1"].Value = TranslationProvider.Translate("Building");
        }

        private void FillLine(ExcelWorksheet worksheet, RoomViewModel room, int line)
        {
            worksheet.Cells[line, 1].Value = room.Name;
            worksheet.Cells[line, 2].Value = room.Description;
            worksheet.Cells[line, 3].Value = room.Area;
            worksheet.Cells[line, 4].Value = room.Height;
            worksheet.Cells[line, 5].Value = room.Volume;
            worksheet.Cells[line, 6].Value = room.Building.Name;
        }

        private void FillWorksheet(ExcelWorksheet worksheet, IEnumerable<RoomViewModel> rooms)
        {
            var roomsArray = rooms.ToArray();
            CreateHeader(worksheet);

            for (var i = 0; i < roomsArray.Length; i++)
            {
                FillLine(worksheet, roomsArray[i], i + 2);
            }
        }
    }
}