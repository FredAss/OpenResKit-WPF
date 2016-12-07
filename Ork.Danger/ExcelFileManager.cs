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
using System.Data;
using System.Windows;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Ork.Danger
{
  public class ExcelFileManager
  {
    public static bool CreateExcelDocument<T>(List<T> list, string xlsxFilePath)
    {
      var ds = new DataSet();
      var targetFilename = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + xlsxFilePath + ".xlsx";
      ds.Tables.Add(ListToDataTable(list));

      return CreateExcelDocument(ds, targetFilename);
    }

    public static void CreateExcelDocument()
    {
      var ds = CreateSampleData();

      var targetFilename = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Gefährdungsbeurteilung.xlsx";
      try
      {
        CreateExcelDocument(ds, targetFilename);
      }
      catch (Exception ex)
      {
        MessageBox.Show("Couldn't create Excel file.\r\nException: " + ex.Message);
        return;
      }
    }

    public static DataTable ListToDataTable<T>(List<T> list)
    {
      var dt = new DataTable("Arbeitsplatz");
      return FillDataTableWithValues(DefineColumnsOfDataTable(dt, list), list);
    }

    private static Type GetListType<T>(IList<T> Test)
    {
      return typeof (T);
    }

    private static DataTable DefineColumnsOfDataTable<T>(DataTable dt, List<T> list)
    {
      foreach (var info in typeof (T).GetProperties())
      {
        if (info.PropertyType.IsPrimitive ||
            info.PropertyType == typeof (string) ||
            info.PropertyType == typeof (decimal))
        {
          dt.Columns.Add(new DataColumn(info.Name, GetNullableType(info.PropertyType)));
        }
        else
        {
          dt.Columns.Add(new DataColumn(info.Name, GetNullableType(typeof (string))));
        }
      }

      return dt;
    }

    private static DataTable FillDataTableWithValues<T>(DataTable dt, List<T> list)
    {
      foreach (var t in list)
      {
        var row = dt.NewRow();
        foreach (var info in typeof (T).GetProperties())
        {
          if (info.PropertyType.IsPrimitive ||
              info.PropertyType == typeof (string) ||
              info.PropertyType == typeof (decimal))
          {
            //if (!IsNullableType(info.PropertyType))
            //    row[info.Name] = info.GetValue(t, null);
            //else
            row[info.Name] = (info.GetValue(t, null) ?? DBNull.Value);
          }
          else
          {
            if (info.GetValue(t, null) is IList)
            {
              var dynamicObjectsInList = (IList) info.GetValue(t, null);
              foreach (var activitiy in dynamicObjectsInList)
              {
                var valuesOfObject = string.Empty;

                foreach (var propertyInfo in activitiy.GetType()
                                                      .GetProperties())
                {
                  valuesOfObject += propertyInfo.GetValue(activitiy, null);
                }

                row[info.Name] += valuesOfObject + "; ";
              }
            }
            else
            {
              //string valuesOfObject = string.Empty;
              //var d = info.GetValue(t, null);

              //foreach (PropertyInfo propertyInfo in d.GetType().GetProperties())
              //{
              //    valuesOfObject += propertyInfo.GetValue(propertyInfo, null);
              //}

              //row[info.Name] = valuesOfObject;
              //if (!IsNullableType(info.PropertyType))
              //    row[info.Name] = info.GetValue(t, null);
              //else
              //row[info.Name] = (info.GetValue(t, null) ?? DBNull.Value);
            }
          }
        }
        dt.Rows.Add(row);
      }
      return dt;
    }

    private static Type GetNullableType(Type t)
    {
      var returnType = t;
      if (t.IsGenericType &&
          t.GetGenericTypeDefinition()
           .Equals(typeof (Nullable<>)))
      {
        returnType = Nullable.GetUnderlyingType(t);
      }
      return returnType;
    }

    private static bool IsNullableType(Type type)
    {
      return (type == typeof (string) || type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition()
                                                                                    .Equals(typeof (Nullable<>))));
    }

    public static bool CreateExcelDocument(DataTable dt, string xlsxFilePath)
    {
      var ds = new DataSet();
      ds.Tables.Add(dt);
      var result = CreateExcelDocument(ds, xlsxFilePath);
      ds.Tables.Remove(dt);
      return result;
    }

    public static bool CreateExcelDocument(DataSet ds, string excelFilename)
    {
      try
      {
        using (var document = SpreadsheetDocument.Create(excelFilename, SpreadsheetDocumentType.Workbook))
        {
          WriteExcelFile(ds, document);
        }
        MessageBox.Show(excelFilename + " wurde erstellt");

        return true;
      }
      catch (Exception ex)
      {
        MessageBox.Show(excelFilename + " wurde nicht erstellt");
        return false;
      }
    }

    private static void WriteExcelFile(DataSet ds, SpreadsheetDocument spreadsheet)
    {
      //  Create the Excel file contents.  This function is used when creating an Excel file either writing 
      //  to a file, or writing to a MemoryStream.
      spreadsheet.AddWorkbookPart();
      spreadsheet.WorkbookPart.Workbook = new Workbook();

      //  My thanks to James Miera for the following line of code (which prevents crashes in Excel 2010)
      spreadsheet.WorkbookPart.Workbook.Append(new BookViews(new WorkbookView()));

      //  If we don't add a "WorkbookStylesPart", OLEDB will refuse to connect to this .xlsx file !
      var workbookStylesPart = spreadsheet.WorkbookPart.AddNewPart<WorkbookStylesPart>("rIdStyles");
      var stylesheet = new Stylesheet();
      workbookStylesPart.Stylesheet = stylesheet;

      //  Loop through each of the DataTables in our DataSet, and create a new Excel Worksheet for each.
      uint worksheetNumber = 1;
      foreach (DataTable dt in ds.Tables)
      {
        //  For each worksheet you want to create
        var workSheetID = "rId" + worksheetNumber.ToString();
        var worksheetName = dt.TableName;

        var newWorksheetPart = spreadsheet.WorkbookPart.AddNewPart<WorksheetPart>();
        newWorksheetPart.Worksheet = new Worksheet();

        // create sheet data
        newWorksheetPart.Worksheet.AppendChild(new SheetData());

        // save worksheet
        WriteDataTableToExcelWorksheet(dt, newWorksheetPart);
        newWorksheetPart.Worksheet.Save();

        // create the worksheet to workbook relation
        if (worksheetNumber == 1)
        {
          spreadsheet.WorkbookPart.Workbook.AppendChild(new Sheets());
        }

        spreadsheet.WorkbookPart.Workbook.GetFirstChild<Sheets>()
                   .AppendChild(new Sheet()
                                {
                                  Id = spreadsheet.WorkbookPart.GetIdOfPart(newWorksheetPart),
                                  SheetId = (uint) worksheetNumber,
                                  Name = dt.TableName
                                });

        worksheetNumber++;
      }

      spreadsheet.WorkbookPart.Workbook.Save();
    }


    private static void WriteDataTableToExcelWorksheet(DataTable dt, WorksheetPart worksheetPart)
    {
      var worksheet = worksheetPart.Worksheet;
      var sheetData = worksheet.GetFirstChild<SheetData>();

      var cellValue = "";

      //  Create a Header Row in our Excel file, containing one header for each Column of data in our DataTable.
      //
      //  We'll also create an array, showing which type each column of data is (Text or Numeric), so when we come to write the actual
      //  cells of data, we'll know if to write Text values or Numeric cell values.
      var numberOfColumns = dt.Columns.Count;
      var IsNumericColumn = new bool[numberOfColumns];

      var excelColumnNames = new string[numberOfColumns];
      for (var n = 0; n < numberOfColumns; n++)
      {
        excelColumnNames[n] = GetExcelColumnName(n);
      }

      //
      //  Create the Header row in our Excel Worksheet
      //
      uint rowIndex = 1;

      var headerRow = new Row
                      {
                        RowIndex = rowIndex
                      }; // add a row at the top of spreadsheet
      sheetData.Append(headerRow);

      for (var colInx = 0; colInx < numberOfColumns; colInx++)
      {
        var col = dt.Columns[colInx];
        AppendTextCell(excelColumnNames[colInx] + "1", col.ColumnName, headerRow);
        IsNumericColumn[colInx] = (col.DataType.FullName == "System.Decimal") || (col.DataType.FullName == "System.Int32");
      }

      //
      //  Now, step through each row of data in our DataTable...
      //
      double cellNumericValue = 0;
      foreach (DataRow dr in dt.Rows)
      {
        // ...create a new row, and append a set of this row's data to it.
        ++rowIndex;
        var newExcelRow = new Row
                          {
                            RowIndex = rowIndex
                          }; // add a row at the top of spreadsheet
        sheetData.Append(newExcelRow);

        for (var colInx = 0; colInx < numberOfColumns; colInx++)
        {
          cellValue = dr.ItemArray[colInx].ToString();

          // Create cell with data
          if (IsNumericColumn[colInx])
          {
            //  For numeric cells, make sure our input data IS a number, then write it out to the Excel file.
            //  If this numeric value is NULL, then don't write anything to the Excel file.
            cellNumericValue = 0;
            if (double.TryParse(cellValue, out cellNumericValue))
            {
              cellValue = cellNumericValue.ToString();
              AppendNumericCell(excelColumnNames[colInx] + rowIndex.ToString(), cellValue, newExcelRow);
            }
          }
          else
          {
            //  For text cells, just write the input data straight out to the Excel file.
            AppendTextCell(excelColumnNames[colInx] + rowIndex.ToString(), cellValue, newExcelRow);
          }
        }
      }
    }

    private static void AppendTextCell(string cellReference, string cellStringValue, Row excelRow)
    {
      //  Add a new Excel Cell to our Row 
      var cell = new Cell()
                 {
                   CellReference = cellReference,
                   DataType = CellValues.String
                 };
      var cellValue = new CellValue();
      cellValue.Text = cellStringValue;
      cell.Append(cellValue);
      excelRow.Append(cell);
    }

    private static void AppendNumericCell(string cellReference, string cellStringValue, Row excelRow)
    {
      //  Add a new Excel Cell to our Row 
      var cell = new Cell()
                 {
                   CellReference = cellReference
                 };

      var cellValue = new CellValue();
      cellValue.Text = cellStringValue;
      cell.Append(cellValue);
      excelRow.Append(cell);
    }

    private static string GetExcelColumnName(int columnIndex)
    {
      //  Convert a zero-based column index into an Excel column reference  (A, B, C.. Y, Y, AA, AB, AC... AY, AZ, B1, B2..)
      //
      //  eg  GetExcelColumnName(0) should return "A"
      //      GetExcelColumnName(1) should return "B"
      //      GetExcelColumnName(25) should return "Z"
      //      GetExcelColumnName(26) should return "AA"
      //      GetExcelColumnName(27) should return "AB"
      //      ..etc..
      //
      if (columnIndex < 26)
      {
        return ((char) ('A' + columnIndex)).ToString();
      }

      var firstChar = (char) ('A' + (columnIndex / 26) - 1);
      var secondChar = (char) ('A' + (columnIndex % 26));

      return string.Format("{0}{1}", firstChar, secondChar);
    }

    private static DataSet CreateSampleData()
    {
      //  Create a sample DataSet, containing three DataTables.
      //  (Later, this will save to Excel as three Excel worksheets.)
      //
      var ds = new DataSet();

      //  Create the first table of sample data
      var dt1 = new DataTable("Drivers");
      dt1.Columns.Add("UserID", Type.GetType("System.Decimal"));
      dt1.Columns.Add("Surname", Type.GetType("System.String"));
      dt1.Columns.Add("Forename", Type.GetType("System.String"));
      dt1.Columns.Add("Sex", Type.GetType("System.String"));
      dt1.Columns.Add("Date of Birth", Type.GetType("System.DateTime"));

      dt1.Rows.Add(new object[]
                   {
                     1, "James", "Brown", "M", new DateTime(1962, 3, 19)
                   });
      dt1.Rows.Add(new object[]
                   {
                     2, "Edward", "Jones", "M", new DateTime(1939, 7, 12)
                   });
      dt1.Rows.Add(new object[]
                   {
                     3, "Janet", "Spender", "F", new DateTime(1996, 1, 7)
                   });
      dt1.Rows.Add(new object[]
                   {
                     4, "Maria", "Percy", "F", new DateTime(1991, 10, 24)
                   });
      dt1.Rows.Add(new object[]
                   {
                     5, "Malcolm", "Marvelous", "M", new DateTime(1973, 5, 7)
                   });
      ds.Tables.Add(dt1);


      //  Create the second table of sample data
      var dt2 = new DataTable("Vehicles");
      dt2.Columns.Add("Vehicle ID", Type.GetType("System.Decimal"));
      dt2.Columns.Add("Make", Type.GetType("System.String"));
      dt2.Columns.Add("Model", Type.GetType("System.String"));

      dt2.Rows.Add(new object[]
                   {
                     1001, "Ford", "Banana"
                   });
      dt2.Rows.Add(new object[]
                   {
                     1002, "GM", "Thunderbird"
                   });
      dt2.Rows.Add(new object[]
                   {
                     1003, "Porsche", "Rocket"
                   });
      dt2.Rows.Add(new object[]
                   {
                     1004, "Toyota", "Gas guzzler"
                   });
      dt2.Rows.Add(new object[]
                   {
                     1005, "Fiat", "Spangly"
                   });
      dt2.Rows.Add(new object[]
                   {
                     1006, "Peugeot", "Lawnmower"
                   });
      dt2.Rows.Add(new object[]
                   {
                     1007, "Jaguar", "Freeloader"
                   });
      dt2.Rows.Add(new object[]
                   {
                     1008, "Aston Martin", "Caravanette"
                   });
      dt2.Rows.Add(new object[]
                   {
                     1009, "Mercedes-Benz", "Hitchhiker"
                   });
      dt2.Rows.Add(new object[]
                   {
                     1010, "Renault", "Sausage"
                   });
      dt2.Rows.Add(new object[]
                   {
                     1011, "Saab", "Chickennuggetmobile"
                   });
      ds.Tables.Add(dt2);


      //  Create the third table of sample data
      var dt3 = new DataTable("Vehicle owners");
      dt3.Columns.Add("User ID", Type.GetType("System.Decimal"));
      dt3.Columns.Add("Vehicle_ID", Type.GetType("System.Decimal"));

      dt3.Rows.Add(new object[]
                   {
                     1, 1002
                   });
      dt3.Rows.Add(new object[]
                   {
                     2, 1000
                   });
      dt3.Rows.Add(new object[]
                   {
                     3, 1010
                   });
      dt3.Rows.Add(new object[]
                   {
                     5, 1006
                   });
      dt3.Rows.Add(new object[]
                   {
                     6, 1007
                   });
      ds.Tables.Add(dt3);

      return ds;
    }
  }


  public class CreateExcelFile
  {
    #region HELPER_FUNCTIONS

    //  This function is adapated from: http://www.codeguru.com/forum/showthread.php?t=450171
    //  My thanks to Carl Quirion, for making it "nullable-friendly".

    #endregion

#if INCLUDE_WEB_FUNCTIONS
  /// <summary>
  /// Create an Excel file, and write it out to a MemoryStream (rather than directly to a file)
  /// </summary>
  /// <param name="dt">DataTable containing the data to be written to the Excel.</param>
  /// <param name="filename">The filename (without a path) to call the new Excel file.</param>
  /// <param name="Response">HttpResponse of the current page.</param>
  /// <returns>True if it was created succesfully, otherwise false.</returns>
        public static bool CreateExcelDocument(DataTable dt, string filename, System.Web.HttpResponse Response)
        {
            try
            {
                DataSet ds = new DataSet();
                ds.Tables.Add(dt);
                CreateExcelDocumentAsStream(ds, filename, Response);
                ds.Tables.Remove(dt);
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Failed, exception thrown: " + ex.Message);
                return false;
            }
        }

        public static bool CreateExcelDocument<T>(List<T> list, string filename, System.Web.HttpResponse Response)
        {
            try
            {
                DataSet ds = new DataSet();
                ds.Tables.Add(ListToDataTable(list));
                CreateExcelDocumentAsStream(ds, filename, Response);
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Failed, exception thrown: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Create an Excel file, and write it out to a MemoryStream (rather than directly to a file)
        /// </summary>
        /// <param name="ds">DataSet containing the data to be written to the Excel.</param>
        /// <param name="filename">The filename (without a path) to call the new Excel file.</param>
        /// <param name="Response">HttpResponse of the current page.</param>
        /// <returns>Either a MemoryStream, or NULL if something goes wrong.</returns>
        public static bool CreateExcelDocumentAsStream(DataSet ds, string filename, System.Web.HttpResponse Response)
        {
            try
            {
                System.IO.MemoryStream stream = new System.IO.MemoryStream();
                using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, true))
                {
                    WriteExcelFile(ds, document);
                }
                stream.Flush();
                stream.Position = 0;

                Response.ClearContent();
                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";

                //  NOTE: If you get an "HttpCacheability does not exist" error on the following line, make sure you have
                //  manually added System.Web to this project's References.

                Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
                Response.AddHeader("content-disposition", "attachment; filename=" + filename);
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                byte[] data1 = new byte[stream.Length];
                stream.Read(data1, 0, data1.Length);
                stream.Close();
                Response.BinaryWrite(data1);
                Response.Flush();
                Response.End();

                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Failed, exception thrown: " + ex.Message);
                return false;
            }
        }
#endif //  End of "INCLUDE_WEB_FUNCTIONS" section

    /// <summary>
    /// Create an Excel file, and write it to a file.
    /// </summary>
    /// <param name="ds">DataSet containing the data to be written to the Excel.</param>
    /// <param name="excelFilename">Name of file to be written.</param>
    /// <returns>True if successful, false if something went wrong.</returns>
  }
}