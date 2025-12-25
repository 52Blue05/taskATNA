using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;

namespace Sale_Saas.Application.Utilities
{
    public class ExcelExportHelper
    {
        public static string ExcelContentType
        {
            get
            { return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"; }
        }

        public static ExcelWorksheet GetStyle(ExcelWorksheet workSheet,int column)
        {
            try
            {
				workSheet.TabColor = System.Drawing.Color.Black;
				//border
				workSheet.Cells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
				workSheet.Cells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
				workSheet.Cells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
				workSheet.Cells.Style.Border.Right.Style = ExcelBorderStyle.Thin;
				//center vertical
				workSheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
				//freeze top row
				workSheet.View.FreezePanes(2, 1);

				//color columns
				for (int i = 1; i <= column; i++)
				{
					workSheet.Column(i).Style.Fill.PatternType = ExcelFillStyle.Solid;
					workSheet.Column(i).Style.Fill.BackgroundColor.SetColor(Color.FromArgb(237, 237, 237));
					workSheet.Column(i).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
				}

				// Setting the properties 
				// of the first row 
				workSheet.Row(1).Height = 30;
				workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
				workSheet.Row(1).Style.Font.Bold = true;
				workSheet.Row(1).Style.Fill.PatternType = ExcelFillStyle.Solid;
				workSheet.Row(1).Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#305496"));
				workSheet.Row(1).Style.Font.Color.SetColor(ColorTranslator.FromHtml("#FFFFFF"));

                return workSheet;
			}
            catch(Exception ex)
            {
                return workSheet;
            }
        }

        public static DataTable ListToDataTable<T>(List<T> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable dataTable = new DataTable();

            for (int i = 0; i < properties.Count; i++)
            {
                PropertyDescriptor property = properties[i];
                dataTable.Columns.Add(property.Name, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
            }

            object[] values = new object[properties.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = properties[i].GetValue(item) ?? "";
                }

                dataTable.Rows.Add(values);
            }
            return dataTable;
        }

        public static byte[] ExportExcel(DataTable dataTable, string heading = "", bool showSrNo = false, bool convertToNumber = false, params string[] columnsToTake)
        {

            byte[] result = null;
            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet workSheet = package.Workbook.Worksheets.Add(String.Format("{0} Data", heading));
                int startRowFrom = String.IsNullOrEmpty(heading) ? 1 : 3;

                if (showSrNo)
                {
                    DataColumn dataColumn = dataTable.Columns.Add("#", typeof(int));
                    dataColumn.SetOrdinal(0);
                    int index = 1;
                    foreach (DataRow item in dataTable.Rows)
                    {
                        item[0] = index;
                        index++;
                    }
                }


                // add the content into the Excel file  
                workSheet.Cells["A" + startRowFrom].LoadFromDataTable(dataTable, true);

                if (convertToNumber)
                {
                    foreach (var cell in workSheet.Cells[workSheet.Dimension.Start.Row, 1, workSheet.Dimension.End.Row, dataTable.Columns.Count])
                    {
                        if (cell.Value != null)
                        {
                            string cellValue = cell.Value.ToString();
                            decimal decimalValue;
                            cell.Value = decimal.TryParse(cellValue, out decimalValue) ? decimalValue : cell.Value;
                        }
                    }
                }

                // autofit width of cells with small content  
                int columnIndex = 1;
                foreach (DataColumn column in dataTable.Columns)
                {
                    ExcelRange columnCells = workSheet.Cells[workSheet.Dimension.Start.Row, columnIndex, workSheet.Dimension.End.Row, columnIndex];
                    int maxLength = 0;
                    var findCells = columnCells.Where(m => m.Value != null);
                    if (findCells.FirstOrDefault() != null)
                    {
                        maxLength = findCells.Max(cell => cell.Value.ToString().Count());
                    }
                    if (maxLength < 150)
                    {
                        workSheet.Column(columnIndex).AutoFit();
                    }


                    columnIndex++;
                }

                // format header - bold, yellow on black  
                using (ExcelRange r = workSheet.Cells[startRowFrom, 1, startRowFrom, dataTable.Columns.Count])
                {
                    r.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    r.Style.Font.Bold = true;
                    r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    r.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#1fb5ad"));
                }

                // format cells - add borders  
                using (ExcelRange r = workSheet.Cells[startRowFrom + 1, 1, startRowFrom + dataTable.Rows.Count, dataTable.Columns.Count])
                {
                    r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Numberformat.Format = "#,##0";
                }

                // removed ignored columns  
                for (int i = dataTable.Columns.Count - 1; i >= 0; i--)
                {
                    if (i == 0 && showSrNo)
                    {
                        continue;
                    }
                    if (!columnsToTake.Contains(dataTable.Columns[i].ColumnName))
                    {
                        workSheet.DeleteColumn(i + 1);
                    }
                }

                if (!String.IsNullOrEmpty(heading))
                {
                    workSheet.Cells["A1"].Value = heading;
                    workSheet.Cells["A1"].Style.Font.Size = 20;
                    workSheet.Cells["A1"].Style.Font.Name = "Arial";
                    workSheet.InsertColumn(1, 1);
                    workSheet.InsertRow(1, 1);
                    workSheet.Column(1).Width = 5;
                }

                result = package.GetAsByteArray();
            }

            return result;
        }
        public static byte[] ExportExcelWithDataSet(DataSet dataSet, bool showSrNo = false, params string[] columnsToTake)
        {

            byte[] result = null;
            using (ExcelPackage package = new ExcelPackage())
            {
                var indexColumnsToTake = 0;
                foreach (DataTable dataTable in dataSet.Tables)
                {
                    ExcelWorksheet workSheet = package.Workbook.Worksheets.Add(String.Format("{0} Data", dataTable.TableName));
                    int startRowFrom = String.IsNullOrEmpty(dataTable.TableName) ? 1 : 3;

                    if (showSrNo)
                    {
                        DataColumn dataColumn = dataTable.Columns.Add("#", typeof(int));
                        dataColumn.SetOrdinal(0);
                        int index = 1;
                        foreach (DataRow item in dataTable.Rows)
                        {
                            item[0] = index;
                            index++;
                        }
                    }


                    // add the content into the Excel file  
                    workSheet.Cells["A" + startRowFrom].LoadFromDataTable(dataTable, true);

                    // autofit width of cells with small content  
                    int columnIndex = 1;
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        ExcelRange columnCells = workSheet.Cells[workSheet.Dimension.Start.Row, columnIndex, workSheet.Dimension.End.Row, columnIndex];
                        int maxLength = 0;
                        var findCells = columnCells.Where(m => m.Value != null);
                        if (findCells.FirstOrDefault() != null)
                        {
                            maxLength = findCells.Max(cell => cell.Value.ToString().Count());
                        }
                        if (maxLength < 150)
                        {
                            workSheet.Column(columnIndex).AutoFit();
                        }


                        columnIndex++;
                    }

                    // format header - bold, yellow on black  
                    using (ExcelRange r = workSheet.Cells[startRowFrom, 1, startRowFrom, dataTable.Columns.Count])
                    {
                        r.Style.Font.Color.SetColor(System.Drawing.Color.White);
                        r.Style.Font.Bold = true;
                        r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        r.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#1fb5ad"));
                    }

                    // format cells - add borders  
                    using (ExcelRange r = workSheet.Cells[startRowFrom + 1, 1, startRowFrom + dataTable.Rows.Count, dataTable.Columns.Count])
                    {
                        r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        r.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                        r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                        r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                        r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                        r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                    }

                    // removed ignored columns  
                    for (int i = dataTable.Columns.Count - 1; i >= 0; i--)
                    {
                        if (i == 0 && showSrNo)
                        {
                            continue;
                        }
                        if (!columnsToTake[indexColumnsToTake].Contains(dataTable.Columns[i].ColumnName))
                        {
                            workSheet.DeleteColumn(i + 1);
                        }
                    }

                    if (!String.IsNullOrEmpty(dataTable.TableName))
                    {
                        workSheet.Cells["A1"].Value = dataTable.TableName;
                        workSheet.Cells["A1"].Style.Font.Size = 20;
                        workSheet.Cells["A1"].Style.Font.Name = "Arial";

                        workSheet.InsertColumn(1, 1);
                        workSheet.InsertRow(1, 1);
                        workSheet.Column(1).Width = 5;
                    }

                    indexColumnsToTake++;
                }

                result = package.GetAsByteArray();
            }

            return result;
        }

        public static byte[] ExportExcel<T>(List<T> data, string Heading = "", bool showSlno = false, params string[] ColumnsToTake)
        {
            return ExportExcel(ListToDataTable<T>(data), Heading, showSlno, false, ColumnsToTake);
        }

        public static byte[] ExportExcelSaleThuCoc(DataTable dataTable, string heading = "", bool showSrNo = false, bool convertToNumber = false, params string[] columnsToTake)
        {

            byte[] result = null;
            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet workSheet = package.Workbook.Worksheets.Add(String.Format("{0} Data", heading));
                int startRowFrom = String.IsNullOrEmpty(heading) ? 1 : 3;

                if (showSrNo)
                {
                    DataColumn dataColumn = dataTable.Columns.Add("#", typeof(int));
                    dataColumn.SetOrdinal(0);
                    int index = 1;
                    foreach (DataRow item in dataTable.Rows)
                    {
                        item[0] = index;
                        index++;
                    }
                }


                // add the content into the Excel file  
                workSheet.Cells["A" + startRowFrom].LoadFromDataTable(dataTable, true);

                if (convertToNumber)
                {
                    foreach (var cell in workSheet.Cells[workSheet.Dimension.Start.Row, 4, workSheet.Dimension.End.Row, dataTable.Columns.Count])
                    {
                        if (cell.Value != null)
                        {
                            string cellValue = cell.Value.ToString();
                            decimal decimalValue;
                            cell.Value = decimal.TryParse(cellValue, out decimalValue) ? decimalValue : cell.Value;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        }
                    }
                }

                // autofit width of cells with small content  
                int columnIndex = 1;
                foreach (DataColumn column in dataTable.Columns)
                {
                    ExcelRange columnCells = workSheet.Cells[workSheet.Dimension.Start.Row, columnIndex, workSheet.Dimension.End.Row, columnIndex];
                    int maxLength = 0;
                    var findCells = columnCells.Where(m => m.Value != null);
                    if (findCells.FirstOrDefault() != null)
                    {
                        maxLength = findCells.Max(cell => cell.Value.ToString().Count());
                    }
                    if (maxLength < 150)
                    {
                        workSheet.Column(columnIndex).AutoFit();
                    }


                    columnIndex++;
                }

                // format header - bold, yellow on black  
                using (ExcelRange r = workSheet.Cells[startRowFrom, 1, startRowFrom, dataTable.Columns.Count])
                {
                    r.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    r.Style.Font.Bold = true;
                    r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    r.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#1fb5ad"));
                }

                // format cells - add borders  
                using (ExcelRange r = workSheet.Cells[startRowFrom + 1, 1, startRowFrom + dataTable.Rows.Count, dataTable.Columns.Count])
                {
                    r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Numberformat.Format = "#,##0";
                }

                // removed ignored columns  
                for (int i = dataTable.Columns.Count - 1; i >= 0; i--)
                {
                    if (i == 0 && showSrNo)
                    {
                        continue;
                    }
                    if (!columnsToTake.Contains(dataTable.Columns[i].ColumnName))
                    {
                        workSheet.DeleteColumn(i + 1);
                    }
                }

                if (!String.IsNullOrEmpty(heading))
                {
                    workSheet.Cells["A1"].Value = heading;
                    workSheet.Cells["A1"].Style.Font.Size = 20;
                    workSheet.Cells["A1"].Style.Font.Name = "Arial";
                    workSheet.InsertColumn(1, 1);
                    workSheet.InsertRow(1, 1);
                    workSheet.Column(1).Width = 5;
                }

                result = package.GetAsByteArray();
            }

            return result;
        }


        public static byte[] ExportExcelBaoCaoDoanhSoKinhDoanhKhachHang(DataTable dataTable, string heading = "", bool showSrNo = false, bool convertToNumber = false, params string[] columnsToTake)
        {

            byte[] result = null;
            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet workSheet = package.Workbook.Worksheets.Add(String.Format("{0} Data", heading));
                int startRowFrom = String.IsNullOrEmpty(heading) ? 1 : 3;

                if (showSrNo)
                {
                    DataColumn dataColumn = dataTable.Columns.Add("#", typeof(int));
                    dataColumn.SetOrdinal(0);
                    int index = 1;
                    foreach (DataRow item in dataTable.Rows)
                    {
                        item[0] = index;
                        index++;
                    }
                }


                // add the content into the Excel file  
                workSheet.Cells["A" + startRowFrom].LoadFromDataTable(dataTable, true);

                if (convertToNumber)
                {
                    foreach (var cell in workSheet.Cells[workSheet.Dimension.Start.Row, 5, workSheet.Dimension.End.Row, 35])
                    {
                        if (cell.Value != null)
                        {
                            string cellValue = cell.Value.ToString();
                            decimal decimalValue;
                            cell.Value = decimal.TryParse(cellValue, out decimalValue) ? decimalValue : cell.Value;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        }
                    }

                    foreach (var cell in workSheet.Cells[workSheet.Dimension.Start.Row, 40, workSheet.Dimension.End.Row, dataTable.Columns.Count])
                    {
                        if (cell.Value != null)
                        {
                            string cellValue = cell.Value.ToString();
                            decimal decimalValue;
                            cell.Value = decimal.TryParse(cellValue, out decimalValue) ? decimalValue : cell.Value;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        }
                    }
                }

                // autofit width of cells with small content  
                int columnIndex = 1;
                foreach (DataColumn column in dataTable.Columns)
                {
                    ExcelRange columnCells = workSheet.Cells[workSheet.Dimension.Start.Row, columnIndex, workSheet.Dimension.End.Row, columnIndex];
                    int maxLength = 0;
                    var findCells = columnCells.Where(m => m.Value != null);
                    if (findCells.FirstOrDefault() != null)
                    {
                        maxLength = findCells.Max(cell => cell.Value.ToString().Count());
                    }
                    if (maxLength < 150)
                    {
                        workSheet.Column(columnIndex).AutoFit();
                    }


                    columnIndex++;
                }

                // format header - bold, yellow on black  
                using (ExcelRange r = workSheet.Cells[startRowFrom, 1, startRowFrom, dataTable.Columns.Count])
                {
                    r.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    r.Style.Font.Bold = true;
                    r.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    r.Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#1fb5ad"));
                }

                // format cells - add borders  
                using (ExcelRange r = workSheet.Cells[startRowFrom + 1, 1, startRowFrom + dataTable.Rows.Count, dataTable.Columns.Count])
                {
                    r.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    r.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    r.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                    r.Style.Numberformat.Format = "#,##0";
                }

                // removed ignored columns  
                for (int i = dataTable.Columns.Count - 1; i >= 0; i--)
                {
                    if (i == 0 && showSrNo)
                    {
                        continue;
                    }
                    if (!columnsToTake.Contains(dataTable.Columns[i].ColumnName))
                    {
                        workSheet.DeleteColumn(i + 1);
                    }
                }

                if (!String.IsNullOrEmpty(heading))
                {
                    workSheet.Cells["A1"].Value = heading;
                    workSheet.Cells["A1"].Style.Font.Size = 20;
                    workSheet.Cells["A1"].Style.Font.Name = "Arial";
                    workSheet.InsertColumn(1, 1);
                    workSheet.InsertRow(1, 1);
                    workSheet.Column(1).Width = 5;
                }

                result = package.GetAsByteArray();
            }

            return result;
        }
    }
}
