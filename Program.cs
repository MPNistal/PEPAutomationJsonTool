using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Nodes;
using System.Transactions;

namespace PEPAutomationJsonTool
{
    public class Program
    {
        private static string _filePath;
        private static bool _isFilePathValid = false;
        private static bool _isJsonFileValid = false;
        private static bool _areThereDuplicateKeys = false;
        private static bool _areThereDuplicateLocatorNames = false;
        private static JObject? _jsonObjects = null;

        public static void Main(string[] args)
        {
            Console.WriteLine("System Team MNL UIMap Json Helper, for info reachout to Mario.Nistal");

            while (true)
            {
                Console.WriteLine("\nPlease choose an option:");
                Console.WriteLine("1. Alphabetized Elements");
                Console.WriteLine("2. Check for duplicate keys");
                Console.WriteLine("3. Check for duplicate locator names");
                Console.WriteLine("Q. Exit\n");

                string? input = Console.ReadLine().ToLower();

                switch (input)
                {
                    case "1":
                        AlphabetizeElementsOption();
                        break;
                    case "2":
                        PrintDuplicateKeyOption();
                        break;
                    case "3":
                        PrintDuplicateLocatorNamesOption();
                        break;
                    case "q":
                        Console.WriteLine("Exiting...");
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }

        #region ------------------ Main Methods ---------------------

        private static void AlphabetizeElementsOption()
        {
            PromtFilePath();
            _isFilePathValid = IsFilePathValid(_filePath);
            _isJsonFileValid = IsValidJsonFile(_filePath);
            Console.WriteLine($"\nFilepath valid?: {_isFilePathValid}");
            Console.WriteLine($"Is file a valid UI mapping json file?: {_isJsonFileValid}\n");
            if (_isJsonFileValid)
            {
                AlphabetizeJson(_jsonObjects);
            }
        }

        private static void PrintDuplicateKeyOption()
        {
            PromtFilePath();
            _isFilePathValid = IsFilePathValid(_filePath);
            _isJsonFileValid = IsValidJsonFile(_filePath);
            Console.WriteLine($"\nFilepath valid?: {_isFilePathValid}");
            Console.WriteLine($"Is file a valid UI mapping json file?: {_isJsonFileValid}\n");
            if (_isJsonFileValid)
            {
                PrintDuplicateKeys(_jsonObjects);
            }
        }

        private static void PrintDuplicateLocatorNamesOption()
        {
            PromtFilePath();
            _isFilePathValid = IsFilePathValid(_filePath);
            _isJsonFileValid = IsValidJsonFile(_filePath);
            Console.WriteLine($"\nFilepath valid?: {_isFilePathValid}");
            Console.WriteLine($"Is file a valid UI mapping json file?: {_isJsonFileValid}\n");
            if (_isJsonFileValid)
            {
                PrintDuplicateLocatorNames(_jsonObjects);
            }
        }

        #endregion

        #region ------------------- Internal Helpers -----------------
       
        private static void PromtFilePath()
        {
            if (!_isJsonFileValid)
            {
                Console.WriteLine("\nPlease enter a valid UI mapping json filepath: ");
                _filePath = Console.ReadLine();
            }
            else
            {
                Console.WriteLine($"\nCurrent file selected is: {_filePath}");
                string? response;
                do
                {
                    Console.WriteLine("Do you want to use the same file? (y/n)\n");
                    response = Console.ReadLine().ToLower();
                } while (response != "y" && response != "n");

                if (response == "n")
                {
                    Console.WriteLine("\nPlease enter the new filepath:");
                    _filePath = Console.ReadLine();
                }
            }
        }

        // Check if _filepath is null

        private static bool IsFilePathValid(string filePath)
        {
            try
            {
                // Check if the path is valid
                if (string.IsNullOrWhiteSpace(filePath) || !Path.IsPathRooted(filePath))
                {
                    return false;
                }

                // Check if the file exists
                if (File.Exists(filePath))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                Console.WriteLine($"Filepath invalid! Please enter a valid file path.");
                return false;
            }
        }

        private static bool IsValidJsonFile(string filePath)
        {
            if (_isFilePathValid)
            {
                try
                {
                    string content = File.ReadAllText(filePath);
                    _jsonObjects = JObject.Parse(content);
                    var elementObjects = _jsonObjects["Elements"].ToObject<List<JObject>>();
                    return true;
                }
                catch (Exception)
                {
                    Console.WriteLine("Please enter a valid UI mapping json file.\n");
                    return false;
                }
            }
            else
            {
                return false;
            }

        }

        private static string GetAlphabetizedFilePath(string originalFilePath)
        {
            string directory = Path.GetDirectoryName(originalFilePath);
            string filename = Path.GetFileNameWithoutExtension(originalFilePath);
            string extension = Path.GetExtension(originalFilePath);

            return Path.Combine(directory, $"{filename}_alphabetized{extension}");
        }

        private static void AlphabetizeJson(JObject jsonObjects)
        {
            if (_isJsonFileValid)
            {
                try
                {
                    List<JObject>? elements = jsonObjects["Elements"].ToObject<List<JObject>>();

                    var sortedElements = elements.OrderBy(e => e["Key"].ToString()).ToList();

                    // Replace the original elements with the sorted ones
                    jsonObjects["Elements"] = JArray.FromObject(sortedElements);
                    string sortedJson = JsonConvert.SerializeObject(jsonObjects, Formatting.Indented);

                    Console.WriteLine("Alphabetization successful!\n");
                    string? response;
                    do
                    {
                        Console.WriteLine("Do you want to: 1 - Print the results to screen, 2 - Write results to file? \n");
                        response = Console.ReadLine().ToLower();
                    } while (response != "1" && response != "2");

                    if(response == "1")
                    {
                        Console.WriteLine(sortedJson);
                    }
                    else
                    {
                        string alphabetizedFilePath = GetAlphabetizedFilePath(_filePath);
                        File.WriteAllText(alphabetizedFilePath, sortedJson);
                        Console.WriteLine($"File created at {alphabetizedFilePath}\n");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Please enter a valid UI Mapping .json File.\n");
                }
            }
        }

        public static void PrintDuplicateKeys(JObject jsonObjects)
        {
            if (_isJsonFileValid)
            {
                _areThereDuplicateKeys = false;
                // Get the elements array
                var elements = jsonObjects["Elements"].ToObject<List<JObject>>();

                // Dictionary to store the count of each key
                var keyCount = new Dictionary<string, List<string>>();

                // Iterate through the elements to count the keys
                foreach (var element in elements)
                {
                    string key = element["Key"].ToString();
                    string locatorName = element["LocatorName"].ToString();

                    if (keyCount.ContainsKey(key))
                    {
                        keyCount[key].Add(locatorName);
                    }
                    else
                    {
                        keyCount[key] = new List<string> { locatorName };
                    }
                }

                // Output duplicate keys and their locator names
                foreach (var kvp in keyCount)
                {
                    if (kvp.Value.Count > 1)
                    {
                        Console.WriteLine($"Duplicate Key: {kvp.Key}");
                        foreach (var locatorName in kvp.Value)
                        {
                            Console.WriteLine($"  LocatorName: {locatorName}");
                        }
                        if (!_areThereDuplicateKeys)
                        {
                            _areThereDuplicateKeys = true;
                        }
                    }
                }

                if (!_areThereDuplicateKeys)
                {
                    Console.WriteLine("No duplicate keys found.");
                }
            }
        }

        public static void PrintDuplicateLocatorNames(JObject jsonObjects)
        {
            if (_isJsonFileValid)
            {
                _areThereDuplicateLocatorNames = false;
                // Get the elements array
                var elements = jsonObjects["Elements"].ToObject<List<JObject>>();

                // Dictionary to store the count of each key
                var locatorNameCount = new Dictionary<string, List<string>>();

                // Iterate through the elements to count the keys
                foreach (var element in elements)
                {
                    string locatorName = element["LocatorName"].ToString();
                    string key = element["Key"].ToString();

                    if (locatorNameCount.ContainsKey(locatorName))
                    {
                        locatorNameCount[locatorName].Add(key);
                    }
                    else
                    {
                        locatorNameCount[locatorName] = new List<string> { key };
                    }
                }

                // Output duplicate keys and their locator names
                foreach (var kvp in locatorNameCount)
                {
                    if (kvp.Value.Count > 1)
                    {
                        Console.WriteLine($"Duplicate LocatorName: {kvp.Key}");
                        foreach (var key in kvp.Value)
                        {
                            Console.WriteLine($"  Key: {key}");
                        }
                        if (!_areThereDuplicateLocatorNames)
                        {
                            _areThereDuplicateLocatorNames = true;
                        }
                    }
                }

                if (!_areThereDuplicateKeys)
                {
                    Console.WriteLine("No duplicate keys found.");
                }
            }
        }
        #endregion
    }
}
