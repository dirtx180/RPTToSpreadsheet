using table.lib;

namespace RPTToSpreadsheet
{
    public class SpreadSheet
    {


        public async Task<DirectoryInfo> CreateFiles(List<Control> controls, FileInfo file)
        {


            var resultDirectory = Directory.CreateDirectory(Path.Combine(file.DirectoryName, file.Name.Replace(".rpt", "")));
            var htmlFile = await CreateHTMLFile(controls, file.Name, resultDirectory.FullName);
            var psvFile = await CreatePSVFile(controls, file.Name, resultDirectory.FullName);

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
