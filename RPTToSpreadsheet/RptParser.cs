using System.Text.Json;
using System.Text.RegularExpressions;

namespace RPTToSpreadsheet
{
    public static class RptParser
    {
        //Regex to determine the start of a control block
        private static readonly Regex ControlHeader = new(
            "^Control=\"(?<type>[^\"]+)\"",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static List<Control> ParseControls(string rptText)
        {
            ArgumentNullException.ThrowIfNull(rptText);
            //Split the string into all of its lines
            string[] lines = rptText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var controls = new List<Control>();
            bool mainControlCaptured = false;
            //Loop through each line in the rpt text
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                if (!mainControlCaptured)
                {
                    int mainControlStartLine = lineIndex;

                    var mainControl = new Control
                    {
                        ControlName = "MAIN FORM CONTROL"
                    };
                    for (; lineIndex < lines.Length; lineIndex++)
                    {
                        string line = lines[lineIndex];
                        if (ControlHeader.IsMatch(line))
                        {
                            lineIndex--;
                            mainControlCaptured = true;
                            break;
                        }

                        int separatorIndex = line.IndexOf('=');
                        //If there is no =, skip to the next line
                        if (separatorIndex < 0)
                        {
                            continue;
                        }

                        //Get the Key and its value from the line using the index of the =
                        string key = line[..separatorIndex].Trim();
                        string rawValue = line[(separatorIndex + 1)..].Trim();

                        //Determine if the key is one of the values we want to take
                        switch (key)
                        {
                            case "Name":
                                mainControl.Name = ParseValue(rawValue);
                                break;
                            case "Prompt":
                                mainControl.Prompt = ParseValue(rawValue);
                                break;
                            case "SQLPath":
                                mainControl.SQLPath = ParseValue(rawValue);
                                break;
                            case "Script":
                                mainControl.Script = ParseValue(rawValue);
                                break;
                            default:
                                continue;
                        }
                    }
                    if (!mainControlCaptured)
                    {
                        throw new FormatException("Unable to capture the main control");
                    }

                    controls.Add(mainControl);
                }

                //Check to see if a line matches the regex for the Control block
                Match headerMatch = ControlHeader.Match(lines[lineIndex]);
                //If the line doesn't match, skip to the next iteration of the loop
                if (!headerMatch.Success)
                {
                    continue;
                }

                //Set the index for where this control block's information starts
                int controlStartLine = lineIndex + 1;
                //Create a new Control object with the ControlName attribute having the value found in the header
                var control = new Control
                {
                    ControlName = headerMatch.Groups["type"].Value
                };

                bool closed = false;

                for (lineIndex++; lineIndex < lines.Length; lineIndex++)
                {
                    string line = lines[lineIndex].Trim();
                    //Check to see if the Control block has finished
                    if (ControlHeader.IsMatch(line) || lineIndex == lines.Length - 1)
                    {
                        lineIndex--;
                        closed = true;
                        break;
                    }

                    //Find where in the line the = is so we know where the value begins
                    int separatorIndex = line.IndexOf('=');
                    //If there is no =, skip to the next line
                    if (separatorIndex < 0)
                    {
                        continue;
                    }

                    //Get the Key and its value from the line using the index of the =
                    string key = line[..separatorIndex].Trim();
                    string rawValue = line[(separatorIndex + 1)..].Trim();

                    //Determine if the key is one of the values we want to take
                    switch (key)
                    {
                        case "Name":
                            control.Name = ParseValue(rawValue);
                            break;
                        case "Prompt":
                            control.Prompt = ParseValue(rawValue);
                            break;
                        case "SQLPath":
                            control.SQLPath = ParseValue(rawValue);
                            break;
                        case "Script":
                            control.Script = ParseValue(rawValue);
                            break;
                        default:
                            continue;
                    }
                }
                //Will get here if the Control block is finished but never got closed with a }
                if (!closed)
                {
                    throw new FormatException($"Control block starting on line {controlStartLine} is not closed.");
                }

                controls.Add(control);
            }

            if (controls.Count == 0)
            {
                throw new FormatException("No Control blocks were found in the RPT content.");
            }

            return controls;
        }

        //This is responsible for making sure the values are actually formatted right, with the appropriate white spaces and such
        private static string ParseValue(string value)
        {
            if (value.Length < 2 || value[0] != '"' || value[^1] != '"')
            {
                return value;
            }
            //Deserialises the string so all the newline carriages and stuff are implemented into the string
            return JsonSerializer.Deserialize<string>(value)
                ?? throw new FormatException("An RPT field contains a null string value.");
        }
    }
}
