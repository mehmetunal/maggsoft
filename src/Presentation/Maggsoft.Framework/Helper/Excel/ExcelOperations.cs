using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using FastMember;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace Maggsoft.Framework.Helper.Excel
{
    public static class ExcelOperations
    {
        public static byte[] ExportToExcel<T>(IEnumerable<T> data)
        {
            return ExportToExcel(data, "Sheet1");
        }

        public static byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName)
        {
            using (DataTable table = new())
            {
                using (var reader = ObjectReader.Create(data))
                {
                    table.Load(reader);
                }

                MemoryStream memoryXl = new();
                using (SpreadsheetDocument document = SpreadsheetDocument.Create(memoryXl, SpreadsheetDocumentType.Workbook))
                {
                    WorkbookPart workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    WorkbookStylesPart workbookStyles = workbookPart.AddNewPart<WorkbookStylesPart>();
                    workbookStyles.Stylesheet = CreateStylesheet();

                    int styleIndex = workbookStyles.Stylesheet.CellFormats.ChildElements.Count - 1;

                    WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();

                    var sheetData = new SheetData();
                    {
                        worksheetPart.Worksheet = new(sheetData);

                        Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
                        Sheet sheet = new()
                        {
                            Id = workbookPart.GetIdOfPart(worksheetPart),
                            SheetId = 1,
                            Name = sheetName
                        };

                        sheets.Append(sheet);
                        Row headerRow = new();
                        List<string> columns = new();
                        foreach (DataColumn column in table.Columns)
                        {
                            columns.Add(column.ColumnName);

                            Cell cell = new();
                            cell.DataType = CellValues.String;
                            cell.CellValue = new CellValue(column.ColumnName);
                            cell.StyleIndex = Convert.ToUInt32(styleIndex);
                            _ = headerRow.AppendChild(cell);
                        }
                        _ = sheetData.AppendChild(headerRow);

                        foreach (DataRow dsrow in table.Rows)
                        {
                            Row newRow = new();
                            foreach (string col in columns)
                            {
                                Cell cell = new();
                                cell.DataType = CellValues.String;
                                cell.CellValue = new CellValue(dsrow[col].ToString());
                                _ = newRow.AppendChild(cell);
                            }

                            _ = sheetData.AppendChild(newRow);
                        }
                        workbookPart.Workbook.Save();
                    }
                }
                _ = memoryXl.Seek(0, SeekOrigin.Begin);
                return memoryXl.ToArray();
            }
        }

        private static Stylesheet CreateStylesheet()
        {
            Stylesheet ss = new Stylesheet();

            Font font0 = new Font();         // Default font

            Font font1 = new Font();         // Bold font
            Bold bold = new Bold();
            font1.Append(bold);

            Fonts fonts = new Fonts();      // <APENDING Fonts>
            fonts.Append(font0);
            fonts.Append(font1);

            // <Fills>
            Fill fill0 = new Fill();        // Default fill

            Fills fills = new Fills();      // <APENDING Fills>
            fills.Append(fill0);

            // <Borders>
            Border border0 = new Border();     // Defualt border

            Borders borders = new Borders();    // <APENDING Borders>
            borders.Append(border0);

            CellFormat cellformat0 = new CellFormat { FontId = 0, FillId = 0, BorderId = 0 }; // Default style : Mandatory | Style ID =0

            CellFormat cellformat1 = new CellFormat { FontId = 1 };
            CellFormats cellformats = new CellFormats();
            cellformats.Append(cellformat0);
            cellformats.Append(cellformat1);


            ss.Append(fonts);
            ss.Append(fills);
            ss.Append(borders);
            ss.Append(cellformats);


            return ss;
        }
    }
}
