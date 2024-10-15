using OfficeOpenXml;
using OfficeOpenXml.Table;
using System.Data;

namespace DBChatPro.Services
{
    public class ExcelService
    {
        public byte[] ExportToExcel(DataTable dataTable, string sheetName = "Sheet1")
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add(sheetName);

                // Cargar los datos en la hoja de cálculo
                worksheet.Cells["A1"].LoadFromDataTable(dataTable, true, TableStyles.Light1);

                // Ajustar el ancho de las columnas
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                return package.GetAsByteArray();
            }
        }
    }


}
