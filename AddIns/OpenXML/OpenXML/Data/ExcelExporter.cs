﻿#region Copyright

/////////////////////////////////////////////////////////////////////////////
//    Altaxo:  a data processing and data plotting program
//    Copyright (C) 2002-2020 Dr. Dirk Lellinger
//
//    This program is free software; you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation; either version 2 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program; if not, write to the Free Software
//    Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
//
/////////////////////////////////////////////////////////////////////////////

#endregion Copyright

using System;
using System.Collections.Generic;
using System.Text;
using Altaxo.Gui;
using Altaxo.Gui.Common.MultiRename;
using Altaxo.Main;
using Altaxo.Main.Commands;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Altaxo.Data
{
  /// <summary>
  /// Contains static methods for exporting <see cref="Altaxo.Data.DataTable"/>s to Microsoft Excel files.
  /// </summary>
  public class ExcelExporter
  {
    /// <summary>
    /// Exports the data table given in the argument into an Excel file. In order to ask for the file name, a file save dialog is shown to the user.
    /// </summary>
    /// <param name="dataTable">The data table.</param>
    public static void DoExportTableShowDialog(DataTable dataTable)
    {
      if (dataTable is null)
        throw new ArgumentNullException(nameof(dataTable));

      var dlg = new SaveFileOptions();
      dlg.AddFilter("*.xlsx", "Microsoft Excel files (*.xlsx)");
      dlg.AddExtension = true;
      dlg.Title = "Enter Microsoft Excel file name:";
      if (true == Current.Gui.ShowSaveFileDialog(dlg))
      {
        Altaxo.Data.ExcelExporter.ExportToExcel(dataTable, dlg.FileName);
      }
    }

    /// <summary>
    /// Shows the multi file export dialog and exports the tables given in the argument
    /// </summary>
    /// <param name="documents">List with <see cref="DataTable"/>s to export.</param>
    public static void ShowExportMultipleExcelFilesDialog(IEnumerable<DataTable> documents)
    {
      if (documents is null)
        throw new ArgumentNullException(nameof(documents));

      var mrData = new MultiRenameData() { IsRenameOperationFileSystemBased = true };
      MultiRenameDocuments.RegisterCommonDocumentShortcutsForFileOperations(mrData);
      mrData.RegisterStringShortcut("E", (o, i) => ".xlsx", "File extension (*.xslx)");

      mrData.RegisterRenameActionHandler(DoExportExcelFiles);

      mrData.AddObjectsToRename(documents);

      mrData.RegisterListColumn("FullName", MultiRenameDocuments.GetFullNameWithAugmentingProjectFolderItems);
      mrData.RegisterListColumn("File name", (obj, newName) => newName);
      mrData.RegisterListColumn("Creation date", MultiRenameDocuments.GetCreationDateString);

      mrData.DefaultPatternString = "[PA]\\[SN][E]";
      mrData.IsRenameOperationFileSystemBased = true;

      var mrController = new MultiRenameController();
      mrController.InitializeDocument(mrData);
      Current.Gui.ShowDialog(mrController, "Export multiple tables");
    }

    private static List<object> DoExportExcelFiles(MultiRenameData mrData)
    {
      List<object> failedItems = null!;
      StringBuilder errors = null!;
      Current.Gui.ExecuteAsUserCancellable(1000, (reporter) => (failedItems, errors) = DoExportExcelFiles(mrData, reporter));

      if (errors.Length != 0)
        Current.Gui.ErrorMessageBox(errors.ToString(), "Export failed for some items");
      else
        Current.Gui.InfoMessageBox($"{mrData.ObjectsToRenameCount - failedItems.Count} tables successfully exported.");

      return failedItems;
    }

    private static (List<object> FailedItems, StringBuilder Errors) DoExportExcelFiles(MultiRenameData mrData, IProgressReporter reporter)
    {
      var failedItems = new List<object>();
      var errors = new StringBuilder();

      string? firstNotRootedFilePath = null;
      object? firstNotRootedObject = null;
      for (int i = 0; i < mrData.ObjectsToRenameCount; ++i)
      {
        var fileName = mrData.GetNewNameForObject(i);
        if (!System.IO.Path.IsPathRooted(fileName))
        {
          firstNotRootedFilePath = fileName;
          firstNotRootedObject = mrData.GetObjectToRename(i);
          break;
        }
      }

      if (firstNotRootedFilePath is not null)
      {
        errors.AppendLine($"Some of the items will be exported to a non-rooted file path.");
        errors.AppendLine($"The first such item is {(firstNotRootedObject is INamedObject no ? no.Name : firstNotRootedObject)} resulting in file {firstNotRootedFilePath}.");
        for (int i = 0; i < mrData.ObjectsToRenameCount; ++i)
        {
          failedItems.Add(mrData.GetObjectToRename(i)); // add remaining items to failed list
        }
        return (failedItems, errors);
      }

      for (int i = 0; i < mrData.ObjectsToRenameCount; ++i)
      {
        var table = (DataTable)mrData.GetObjectToRename(i);
        var fileName = mrData.GetNewNameForObject(i);

        try
        {
          // Create the directory, if not already present
          var dir = System.IO.Path.GetDirectoryName(fileName) ?? throw new InvalidOperationException($"Can not get directory name from file name {fileName}");
          if (!System.IO.Directory.Exists(dir))
            System.IO.Directory.CreateDirectory(dir);

          // now export the DataTable
          ExportToExcel(table, fileName);
        }
        catch (Exception ex)
        {
          failedItems.Add(table);
          errors.AppendFormat("Graph {0} -> file name {1}: export failed, {2}\n", table.Name, fileName, ex.Message);
        }

        if (reporter.CancellationPending)
        {
          for (i = i + 1; i < mrData.ObjectsToRenameCount; ++i)
          {
            failedItems.Add(mrData.GetObjectToRename(i)); // add remaining items to failed list
          }
          break;
        }
        if (reporter.ShouldReportNow)
        {
          reporter.ReportProgress($"Processed table: {table.Name}", (i + 1d) / mrData.ObjectsToRenameCount);
        }
      }

      return (failedItems, errors);
    }

    private static string DoubleToExcelString(double x)
    {
      if (double.IsNaN(x))
        return string.Empty;

      if (double.IsInfinity(x))
        x = double.IsPositiveInfinity(x) ? 1.7976931348623E+308 : -1.7976931348623E+308;

      return x.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Exports a <see cref="DataTable"/> to an Excel file.
    /// </summary>
    /// <param name="dataTable">The data table to export.</param>
    /// <param name="fileName">Full name of the Excel file that is the destination of the export.</param>
    public static void ExportToExcel(DataTable dataTable, string fileName)
    {
      if (dataTable is null)
        throw new ArgumentNullException(nameof(dataTable));
      if (string.IsNullOrEmpty(fileName))
        throw new ArgumentNullException(nameof(fileName));

      var spreadsheetDocument = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook);

      // Add a WorkbookPart to the document.
      WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
      workbookpart.Workbook = new Workbook();

      // Add a WorksheetPart to the WorkbookPart.
      WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
      worksheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(new SheetData());

      // Add Sheets to the Workbook.
      Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

      Sheet sheet = new Sheet()
      {
        Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
        SheetId = 1,
        Name = "Table 1",
      };
      sheets.Append(sheet);

      // now fill the table
      var worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet();
      SheetData sheetData = new SheetData();

      var numberOfRows = dataTable.DataRowCount;
      var numberOfColumns = dataTable.DataColumnCount;

      int rowOffset = 0;
      int columnOffset = 1;

      string spanFrom1ToNumberOfColumns = "1:" + (numberOfColumns + columnOffset).ToString(System.Globalization.CultureInfo.InvariantCulture);

      // First, export the column names
      Row headerRow = new Row() { RowIndex = (uint)(1), Spans = new ListValue<StringValue>() { InnerText = spanFrom1ToNumberOfColumns } };
      for (int nCol = 0; nCol < numberOfColumns; ++nCol)
      {
        var dataColName = dataTable.DataColumns.GetColumnName(nCol);
        Cell cell = new Cell()
        {
          CellReference = GetCellReferenceName(0 + rowOffset, nCol + columnOffset),
          DataType = CellValues.String,
          CellValue = new CellValue(dataColName),
        };

        headerRow.Append(cell);
      }
      sheetData.Append(headerRow);

      rowOffset += 1;

      // export the property columns

      for (int nPCol = 0; nPCol < dataTable.PropertyColumnCount; ++nPCol, ++rowOffset)
      {
        Row row = new Row() { RowIndex = (uint)(1 + rowOffset), Spans = new ListValue<StringValue>() { InnerText = spanFrom1ToNumberOfColumns } };

        {
          // First, add the name of the property column
          var dataColName = dataTable.PropertyColumns.GetColumnName(nPCol);
          Cell cell = new Cell()
          {
            CellReference = GetCellReferenceName(rowOffset, 0),
            DataType = CellValues.String,
            CellValue = new CellValue(dataColName),
          };
          row.Append(cell);
        }

        // now add the values
        var propertyColumn = dataTable.PropertyColumns[nPCol];
        for (int nCol = 0; nCol < propertyColumn.Count; ++nCol)
        {
          if (!propertyColumn.IsElementEmpty(nCol))
          {
            CellValue cellValue;
            if (propertyColumn is DoubleColumn)
              cellValue = new CellValue(DoubleToExcelString((double)propertyColumn[nCol]));
            else if (propertyColumn is DateTimeColumn)
              cellValue = new CellValue((DateTime)propertyColumn[nCol]);
            else
              cellValue = new CellValue(propertyColumn[nCol].ToString());

            Cell cell = new Cell()
            {

              CellReference = GetCellReferenceName(rowOffset, nCol + columnOffset),
              DataType = GetCellDataType(propertyColumn),
              CellValue = cellValue,
            };

            row.Append(cell);
          }
        }
        sheetData.Append(row);
      }



      // now export the data
      for (int nRow = 0; nRow < numberOfRows; ++nRow)
      {
        Row row = new Row() { RowIndex = (uint)(nRow + 1 + rowOffset), Spans = new ListValue<StringValue>() { InnerText = spanFrom1ToNumberOfColumns } };

        for (int nCol = 0; nCol < numberOfColumns; ++nCol)
        {
          var dataCol = dataTable[nCol];

          if (!dataCol.IsElementEmpty(nRow))
          {
            CellValue cellValue;
            if (dataCol is DoubleColumn)
              cellValue = new CellValue(DoubleToExcelString((double)dataCol[nRow]));
            else if (dataCol is DateTimeColumn)
              cellValue = new CellValue((DateTime)dataCol[nRow]);
            else
              cellValue = new CellValue(dataCol[nRow].ToString());


            Cell cell = new Cell()
            {

              CellReference = GetCellReferenceName(nRow + rowOffset, nCol + columnOffset),
              DataType = GetCellDataType(dataTable[nCol]),
              CellValue = cellValue,
            };

            row.Append(cell);
          }
        }
        sheetData.Append(row);
      }
      worksheet.Append(sheetData);
      worksheetPart.Worksheet = worksheet;



      workbookpart.Workbook.Save();

      // Close the document.
      spreadsheetDocument.Dispose();
    }

    /// <summary>
    /// Gets the type of the cell data in dependence of the type of <see cref="DataColumn"/>.
    /// </summary>
    /// <param name="dataColumn">The data column.</param>
    /// <returns>Type of cell data.</returns>
    /// <exception cref="NotImplementedException">This type of column {dataColumn?.GetType()} is not implemented here.</exception>
    public static CellValues GetCellDataType(DataColumn dataColumn)
    {

      if (dataColumn is DoubleColumn)
        return CellValues.Number;
      else if (dataColumn is TextColumn)
        return CellValues.String;
      else if (dataColumn is DateTimeColumn)
        return CellValues.Date;
      else
        throw new NotImplementedException($"This type of column {dataColumn?.GetType()} is not implemented here.");
    }


    /// <summary>
    /// Gets the name of the Excel cell with given row and column number. Both row number and column number are zero based, thus (0,0) means the cell in the first row and first column.
    /// </summary>
    /// <param name="nRow">The row number (zero based).</param>
    /// <param name="nCol">The column number (zero based).</param>
    /// <returns>The name of the cell with given row and column number.</returns>
    public static string GetCellReferenceName(int nRow, int nCol)
    {
      return GetExcelColumnName(nCol) + (nRow + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Gets the name of the Excel column. Note that the argument here is zero-based, thus zero means the first column.
    /// </summary>
    /// <param name="nCol">The number of the Excel column (zero based).</param>
    /// <returns>The name of the Excel column with the given number.</returns>
    /// <exception cref="ArgumentOutOfRangeException">nCol - Must be &gt;= 0</exception>
    public static string GetExcelColumnName(int nCol)
    {
      if (nCol < 0)
        throw new ArgumentOutOfRangeException(nameof(nCol), "Must be >= 0");
      string result = string.Empty;
      int rem = nCol;
      int quo, rest;
      do
      {
        quo = rem / 26;
        rest = rem % 26;

        result = ((char)('A' + rest)) + result;
        rem = quo - 1;
      } while (rem >= 0);
      return result;
    }
  }
}
