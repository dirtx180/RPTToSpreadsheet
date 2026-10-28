using Microsoft.Extensions.Configuration;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using table.lib;

namespace RPTToSpreadsheet
{
    public class SpreadSheet
    {
        private readonly IConfiguration _config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        public async Task<DirectoryInfo> CreateFiles(List<Control> controls, string fileName)
        {
            string? saveDirectory = _config["Config:OutputPath"];
            if (string.IsNullOrWhiteSpace(saveDirectory))
            {
                throw new InvalidOperationException("Config:OutputPath is missing from appsettings.json.");
            }
            var resultDirectory = Directory.CreateDirectory(Path.Combine(saveDirectory, fileName.Replace(".rpt", "")));
            var htmlFile = await CreateHTMLFile(controls, fileName, resultDirectory.FullName);
            var psvFile = await CreatePSVFile(controls, fileName, resultDirectory.FullName);

            return resultDirectory;

        }
        public async Task<FileInfo> CreateHTMLFile(List<Control> controls, string fileName, string saveDirectory)
        {
            

            fileName = fileName.Replace(".rpt", ".html");
            
            var file = new FileInfo(Path.Combine(saveDirectory, fileName));


            if (file.Exists)
            {
                file.Delete();
            }
            var table = Table<Control>.Add(controls);
            
            await using (var stream = File.CreateText(file.FullName))
            {
                await stream.WriteLineAsync("<style>td { text-align: left !important; }</style>");
                table.ColumnContentTextJustification(new Dictionary<string, TextJustification>{
                {"ControlName", TextJustification.Left },
                {"Name", TextJustification.Left },
                {"Prompt", TextJustification.Left },
                {"SQLPath", TextJustification.Left },
                {"Script", TextJustification.Left }
            }).WriteHtml(stream);
            }

            return file;
        }
        public async Task<FileInfo> CreatePSVFile(List<Control> controls, string fileName, string saveDirectory)
        {

            fileName = fileName.Replace(".rpt", ".csv");
            Directory.CreateDirectory(saveDirectory);
            var file = new FileInfo(Path.Combine(saveDirectory, fileName));

            if (file.Exists)
            {
                file.Delete();
            }

            foreach (var control in controls)
            {
                if (control.Script != null)
                {
                    control.Script = control.Script.Replace("\r\n", @"~");
                    control.Script = "\"" + control.Script + "\"";
                }
            }


            var table = Table<Control>.Add(controls);

            await using (var stream = File.CreateText(file.FullName))
            {
                table.WriteCsv(stream);
            }

            return file;
        }
    }
}
