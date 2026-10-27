using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace RPTToSpreadsheet
{
    public class SpreadSheet
    {
        private readonly IConfiguration _config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        public async Task CreateSpreadsheet(List<Control> controls, string fileName)
        {
            ExcelPackage.License.SetNonCommercialPersonal("Dirt");
            
            string saveDirectory = _config["Config:OutputPath"];

            

            fileName = fileName.Replace(".rpt", ".xlsx");
            var file = new FileInfo(saveDirectory + "\\" + fileName);

            await SaveExcelFile(controls, file);
        }

        private static async Task SaveExcelFile(List<Control> controls, FileInfo file)
        {
            if (file.Exists)
            {
                file.Delete();
            }
            string worksheetName = file.Name.Replace(".xlsx", "") + " Documentation";
            using (var package = new ExcelPackage(file))
            {
                var ws = package.Workbook.Worksheets.Add(worksheetName);

                var range = ws.Cells["A1"].LoadFromCollection(controls, true);
                
                //Styling
                range.Style.Font.Size = 16;
                range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Row(1).Style.Font.Size = 24;
                ws.Row(1).Style.Font.Bold = true;
                ws.Row(2).Style.Font.Bold = true;
                ws.Column(5).Style.WrapText = true;
                ws.Column(5).Width = 300;
                ws.Column(5).Style.Font.Name = "Consolas";
                ws.Column(5).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                ws.Row(1).Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Row(1).Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                ws.Row(2).Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Row(2).Style.Fill.BackgroundColor.SetColor(Color.PaleVioletRed);
                range.Style.Border.Top.Style = ExcelBorderStyle.Medium;
                range.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
                range.Style.Border.Left.Style = ExcelBorderStyle.Medium;
                range.Style.Border.Right.Style = ExcelBorderStyle.Medium;
                range.AutoFitColumns();
                


                await package.SaveAsync();
            }
        }  
    }
}
