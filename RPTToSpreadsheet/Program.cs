
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;


namespace RPTToSpreadsheet
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("");
            Console.WriteLine("Enter the path to the .rpt file you want to convert:");
            string? filePath = Console.ReadLine();
            //for testing purposes only
            //filePath = "C:\\Users\\conor\\OneDrive\\Documents\\GitHub\\RPTToSpreadsheet\\RPTToSpreadsheet\\Test Resources\\Input\\MATOPT (1)(1).rpt";
            if (string.IsNullOrWhiteSpace(filePath) ||
                !string.Equals(Path.GetExtension(filePath), ".rpt", StringComparison.OrdinalIgnoreCase) ||
                !File.Exists(filePath))
            {
                Console.WriteLine("Error: Invalid file path. Enter the path to an existing .rpt file.");
                await Main(Array.Empty<string>());
            }

            string fileName = Path.GetFileName(filePath);

            try
            {
                string rawText = File.ReadAllText(filePath);
                List<Control> controls = RptParser.ParseControls(rawText);
                foreach (var control in controls)
                {
                    Console.WriteLine(new string('-', 40));
                    Console.WriteLine("Control: " + control.ControlName);
                    Console.WriteLine("Name: " + control.Name);
                    Console.WriteLine("Prompt: " + control.Prompt);
                    Console.WriteLine("SQLPath: " + control.SQLPath);
                    Console.WriteLine("Script: \n" + control.Script);
                }

                Console.WriteLine($"Parsed {controls.Count} controls:");
                Console.WriteLine("Press enter to continue");
                Console.ReadKey();
                SpreadSheet spreadSheet = new SpreadSheet();
                var saveDirectory = await spreadSheet.CreateFiles(controls, fileName);

                Console.WriteLine("CSV and HTML files created in your output folder");
                Console.WriteLine("Would you like to open the output folder? Press enter for yes, space for no");

                if(Console.ReadKey().Key == ConsoleKey.Enter)
                {
                    Console.WriteLine("Opening Folder");
                    Process.Start(new ProcessStartInfo
                {
                    FileName = saveDirectory.FullName,
                    UseShellExecute = true
                });
                }
                
            }
            catch (IOException e)
            {
                Console.WriteLine($"Error: Could not read the file: {e.Message}");
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine($"Error: Could not access the file: {e.Message}");
            }
            catch (FormatException e)
            {
                Console.WriteLine($"Error: Could not parse the RPT file: {e.Message}");
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Error: Could not decode an RPT field: {e.Message}");
            }
            await Main(Array.Empty<string>());
        }
    }
}
