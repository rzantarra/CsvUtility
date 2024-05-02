using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace FTXScrubImportSheetC
{
    public class CsvHelper
    {
        #region Constructor

        public CsvHelper(string installDirectory, string csvColumnHeaders)
        {
            InstallDirectory = installDirectory;
            CSVColumnHeaders = csvColumnHeaders;
        }

        #endregion

        #region Variables

        public string InstallDirectory { get; private set; }
        public string CSVColumnHeaders { get; private set; }

        #endregion

        #region Functions

        public async Task ExpandUpcOnly(MainWindowViewModel _viewModel)
        {
            string tmpUpdateTxt;
            //string PrintFilePath = MainWindowViewModel.InstallDirectory;

            _viewModel.UpdateStatusTxt = $"Expand Export Only Option Initiated";
            await Task.Delay(TimeSpan.FromMilliseconds(0.5));
            _viewModel.AddLogMessage(tmpUpdateTxt = "Expand Export Only Option Initiated.");
            await Task.Delay(TimeSpan.FromMilliseconds(0.5));

            string filePath = Path.Combine(InstallDirectory, "ImportExpandedOnly_");
            WriteToCSV(_viewModel.ImportExpandedUPCData, "ImportExpandedOnly_");
            string message = $@"File Saved Successfully: {filePath}";
            //MessageBox.Show(message, "Expand Export Only Option", MessageBoxButton.OK, MessageBoxImage.Information);

            _viewModel.UpdateStatusTxt = $"Expand Export Only Option Complete";
            await Task.Delay(TimeSpan.FromMilliseconds(0.5));
            _viewModel.AddLogMessage(tmpUpdateTxt = "Expand Export Only Option Complete");
            await Task.Delay(TimeSpan.FromMilliseconds(0.5));

            //Clear each of the used Lists
            DataHelper.ClearProductDataList(_viewModel.CurrentMasterProducts);
            DataHelper.ClearProductDataList(_viewModel.ImportFullData);
            DataHelper.ClearProductDataList(_viewModel.ImportNativeData);
            DataHelper.ClearProductDataList(_viewModel.ImportAliasFoundData);
            DataHelper.ClearProductDataList(_viewModel.ImportNAData);
            DataHelper.ClearProductDataList(_viewModel.ImportNotFoundData);
            DataHelper.ClearProductDataList(_viewModel.ImportExpandedUPCData);
            DataHelper.ClearProductAliasList(_viewModel.ImportMasterProducts);
            DataHelper.ClearProductAliasList(_viewModel.CurrentMasterAliases);

            _viewModel.UpdateStatusTxt = $"Scrub Cleanup: Wiping Internals";
            await Task.Delay(TimeSpan.FromMilliseconds(0.5));
            _viewModel.AddLogMessage(tmpUpdateTxt = "Scrub Cleanup: Wiping Internals.");
            await Task.Delay(TimeSpan.FromMilliseconds(0.5));

            return;
        }

        public void WriteToCSV(List<clsProductData> data, string fileNamePrefix)
        {
            try
            {
                // Create a file name using the specified prefix and the current date and time
                string fileName = $"{fileNamePrefix}.csv";

                // Combine the directory path and file name to get the full file path
                string filePath = Path.Combine(InstallDirectory, fileName);
                //PrintFilePath = filePath; //PrintFilePath is not defined here 

                // Open the CSV file for writing
                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    // Write the column headers as the initial top line
                    sw.WriteLine(CSVColumnHeaders);

                    // Write each string in the list to the CSV file
                    foreach (clsProductData productData in data)
                    {
                        // Format the product data as a CSV line
                        string line =
                            $"{EscapeCsvField(productData.upc)},{EscapeCsvField(productData.name)},{EscapeCsvField(productData.description)},{EscapeCsvField(productData.department)},{EscapeCsvField(productData.department_number)},{EscapeCsvField(productData.category)},{EscapeCsvField(productData.manufacturer)},{EscapeCsvField(productData.brand)},{EscapeCsvField(productData.is_active)},{EscapeCsvField(productData.cost)},{EscapeCsvField(productData.price)},{EscapeCsvField(productData.vendor)},{EscapeCsvField(productData.part_num)},{EscapeCsvField(productData.part_num_units)},{EscapeCsvField(productData.part_cost)},{EscapeCsvField(productData.child_upc)},{EscapeCsvField(productData.num_units)}";
                        sw.WriteLine(line);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during writing
                Console.WriteLine("Error writing to CSV file: " + ex.Message + "\nInner Exception: " +
                                  ex.InnerException?.Message);
                MessageBox.Show("Erorr Writing to CSV File: " + ex.Message + ex.StackTrace + "\nInner Exception: " +
                                ex.InnerException?.Message);
            }
        }

        public static string ExpandUPC(string UPC)
        {
            int OddSum, EvenSum, TotalSum, CheckDigit;
            string Char1, Char2, Char3, Char4, Char5, Char6;
            string ReturnUPC, tmpUPC;
            // int something = 5;
            UPC = UPC.Trim();
            try
            {
                if (Int64.TryParse(UPC, out _) == false)
                {
                    return UPC;
                }

                UPC = UPC.Trim();

                if (UPC.StartsWith("a"))
                {
                    UPC = UPC.Substring(1);
                }

                if (UPC.EndsWith("b"))
                {
                    UPC = UPC.Substring(0, UPC.Length - 1);
                }

                if (UPC.Length > 12)
                {
                    return UPC;
                }
                else if (UPC.Length == 12)
                {
                    return UPC;
                }
                else if (UPC.Length == 11)
                {
                    UPC = UPC.Trim();
                    OddSum = 0;
                    for (int i = 0; i < UPC.Length; i += 2)
                    {
                        OddSum += int.Parse(UPC[i].ToString());
                    }

                    OddSum *= 3;

                    EvenSum = 0;
                    for (int i = 1; i < UPC.Length; i += 2)
                    {
                        EvenSum += int.Parse(UPC[i].ToString());
                    }

                    TotalSum = EvenSum + OddSum;

                    CheckDigit = TotalSum % 10;
                    if (CheckDigit != 0)
                    {
                        CheckDigit = 10 - CheckDigit;
                    }

                    UPC += CheckDigit;
                    ReturnUPC = UPC;
                }
                else if (UPC.Length == 10)
                {
                    UPC = "0" + UPC;
                    UPC = UPC.Trim();
                    OddSum = 0;
                    for (int i = 0; i < UPC.Length; i += 2)
                    {
                        OddSum += int.Parse(UPC[i].ToString());
                    }

                    OddSum *= 3;

                    EvenSum = 0;
                    for (int i = 1; i < UPC.Length; i += 2)
                    {
                        EvenSum += int.Parse(UPC[i].ToString());
                    }

                    TotalSum = EvenSum + OddSum;

                    CheckDigit = TotalSum % 10;
                    if (CheckDigit != 0)
                    {
                        CheckDigit = 10 - CheckDigit;
                    }

                    UPC += CheckDigit;
                    ReturnUPC = UPC;
                }
                else if (UPC.Length == 9)
                {
                    UPC = "00" + UPC;
                    UPC = UPC.Trim();
                    OddSum = 0;
                    for (int i = 0; i < UPC.Length; i += 2)
                    {
                        OddSum += int.Parse(UPC[i].ToString());
                    }

                    OddSum *= 3;

                    EvenSum = 0;
                    for (int i = 1; i < UPC.Length; i += 2)
                    {
                        EvenSum += int.Parse(UPC[i].ToString());
                    }

                    TotalSum = EvenSum + OddSum;

                    CheckDigit = TotalSum % 10;
                    if (CheckDigit != 0)
                    {
                        CheckDigit = 10 - CheckDigit;
                    }

                    UPC += CheckDigit;
                    ReturnUPC = UPC;
                }
                else if (UPC.Length == 8)
                {
                    if (UPC.StartsWith("0"))
                    {
                        UPC = UPC.Substring(1, 6);

                        tmpUPC = UPC;
                        Char1 = UPC[0].ToString();
                        Char2 = UPC[1].ToString();
                        Char3 = UPC[2].ToString();
                        Char4 = UPC[3].ToString();
                        Char5 = UPC[4].ToString();
                        Char6 = UPC[5].ToString();

                        if (Char6 == "0")
                        {
                            UPC = "0" + Char1 + Char2 + "00000" + Char3 + Char4 + Char5;
                        }
                        else if (Char6 == "1")
                        {
                            UPC = "0" + Char1 + Char2 + "10000" + Char3 + Char4 + Char5;
                        }
                        else if (Char6 == "2")
                        {
                            UPC = "0" + Char1 + Char2 + "20000" + Char3 + Char4 + Char5;
                        }
                        else if (Char6 == "3")
                        {
                            UPC = "0" + Char1 + Char2 + Char3 + "00000" + Char4 + Char5;
                        }
                        else if (Char6 == "4")
                        {
                            UPC = "0" + Char1 + Char2 + Char3 + Char4 + "00000" + Char5;
                        }
                        else if (Char6 == "5")
                        {
                            UPC = "0" + Char1 + Char2 + Char3 + Char4 + Char5 + "00005";
                        }
                        else if (Char6 == "6")
                        {
                            UPC = "0" + Char1 + Char2 + Char3 + Char4 + Char5 + "00006";
                        }
                        else if (Char6 == "7")
                        {
                            UPC = "0" + Char1 + Char2 + Char3 + Char4 + Char5 + "00007";
                        }
                        else if (Char6 == "8")
                        {
                            UPC = "0" + Char1 + Char2 + Char3 + Char4 + Char5 + "00008";
                        }
                        else if (Char6 == "9")
                        {
                            UPC = "0" + Char1 + Char2 + Char3 + Char4 + Char5 + "00009";
                        }
                        else
                        {
                            UPC = "111111111111";
                        }

                        OddSum = 0;
                        for (int i = 0; i < UPC.Length; i += 2)
                        {
                            OddSum += int.Parse(UPC[i].ToString());
                        }

                        OddSum *= 3;

                        EvenSum = 0;
                        for (int i = 1; i < UPC.Length; i += 2)
                        {
                            EvenSum += int.Parse(UPC[i].ToString());
                        }

                        TotalSum = EvenSum + OddSum;

                        CheckDigit = TotalSum % 10;
                        if (CheckDigit != 0)
                        {
                            CheckDigit = 10 - CheckDigit;
                        }

                        UPC += CheckDigit;
                        ReturnUPC = UPC;
                    }
                    else
                    {
                        ReturnUPC = UPC;
                    }
                }
                else if (UPC.Length == 7)
                {
                    tmpUPC = UPC;
                    Char1 = UPC[0].ToString();
                    Char2 = UPC[1].ToString();
                    Char3 = UPC[2].ToString();
                    Char4 = UPC[3].ToString();
                    Char5 = UPC[4].ToString();
                    Char6 = UPC[5].ToString();

                    if (Char6 == "0")
                    {
                        UPC = "0" + Char1 + Char2 + "00000" + Char3 + Char4 + Char5;
                    }
                    else if (Char6 == "1")
                    {
                        UPC = "0" + Char1 + Char2 + "10000" + Char3 + Char4 + Char5;
                    }
                    else if (Char6 == "2")
                    {
                        UPC = "0" + Char1 + Char2 + "20000" + Char3 + Char4 + Char5;
                    }
                    else if (Char6 == "3")
                    {
                        UPC = "0" + Char1 + Char2 + Char3 + "00000" + Char4 + Char5;
                    }
                    else if (Char6 == "4")
                    {
                        UPC = "0" + Char1 + Char2 + Char3 + Char4 + "00000" + Char5;
                    }
                    else if (Char6 == "5")
                    {
                        UPC = "0" + Char1 + Char2 + Char3 + Char4 + Char5 + "00005";
                    }
                    else if (Char6 == "6")
                    {
                        UPC = "0" + Char1 + Char2 + Char3 + Char4 + Char5 + "00006";
                    }
                    else if (Char6 == "7")
                    {
                        UPC = "0" + Char1 + Char2 + Char3 + Char4 + Char5 + "00007";
                    }
                    else if (Char6 == "8")
                    {
                        UPC = "0" + Char1 + Char2 + Char3 + Char4 + Char5 + "00008";
                    }
                    else if (Char6 == "9")
                    {
                        UPC = "0" + Char1 + Char2 + Char3 + Char4 + Char5 + "00009";
                    }
                    else
                    {
                        UPC = "111111111111";
                    }

                    OddSum = 0;
                    for (int i = 0; i < UPC.Length; i += 2)
                    {
                        OddSum += int.Parse(UPC[i].ToString());
                    }

                    OddSum *= 3;

                    EvenSum = 0;
                    for (int i = 1; i < UPC.Length; i += 2)
                    {
                        EvenSum += int.Parse(UPC[i].ToString());
                    }

                    TotalSum = EvenSum + OddSum;

                    CheckDigit = TotalSum % 10;
                    if (CheckDigit != 0)
                    {
                        CheckDigit = 10 - CheckDigit;
                    }

                    UPC += CheckDigit;
                    ReturnUPC = UPC;
                }
                else
                {
                    ReturnUPC = UPC.PadLeft(12, '0');
                }

                return ReturnUPC;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        private string EscapeCsvField(string field)
        {
            if (field != null) // Null check
            {
                // If the field contains double quotes, escape them by doubling them
                if (field.Contains("\""))
                {
                    field = field.Replace("\"", "\"\"");
                }

                // If the field contains commas or double quotes, enclose it in double quotes
                if (field.Contains(",") || field.Contains("\""))
                {
                    field = $"\"{field}\"";
                }
            }

            return field;
        }


        public static async Task<bool> LoadMasterProducts(string FileToUse, MainWindowViewModel _viewModel)
        {
            try
            {
                using (var tmpFileReader = new StreamReader(FileToUse))
                {
                    // Skip the header row
                    tmpFileReader.ReadLine();
                    string tmpLine;
                    int i = 0;
                    while ((tmpLine = tmpFileReader.ReadLine()) != null)
                    {
                        i++;

                        string statusMessagetoDisplay = "Processing Master Products Row " + i.ToString();

                        Console.WriteLine(statusMessagetoDisplay);
                        if (tmpLine.Contains("|"))
                        {
                            var values = tmpLine.Split('|').Select(v => v.Replace("\"", "")).ToArray();
                            if (values.Length >= 17)
                            {
                                clsProductData tmpNewProduct = new clsProductData
                                {
                                    upc = values[0],
                                    name = values[1],
                                    description = values[2],
                                    department = values[3],
                                    department_number = values[4],
                                    category = values[5],
                                    manufacturer = values[6],
                                    brand = values[7],
                                    is_active = values[8],
                                    cost = values[9],
                                    price = values[10],
                                    vendor = values[11],
                                    part_num = values[12],
                                    part_num_units = values[13],
                                    part_cost = values[14],
                                    child_upc = values[15],
                                    num_units = values[16]
                                };
                                _viewModel.CurrentMasterProducts.Add(tmpNewProduct);
                            }
                        }
                    }
                }

                string tmpUpdateTxt;

                _viewModel.UpdateStatusTxt = "Master Products Import Complete...";
                await Task.Delay(TimeSpan.FromMilliseconds(0.5));
                _viewModel.AddLogMessage(tmpUpdateTxt = "Master Products Import Complete...");
                await Task.Delay(TimeSpan.FromMilliseconds(0.5));
                return true;
            }
            catch (Exception ex)
            {
                _viewModel.UpdateStatusTxt = "Idle";
                MessageBox.Show("Error Loading Master Products From File: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> LoadMasterAliases(string FileToUse, MainWindowViewModel _viewModel)
        {
            string tmpUpdateTxt;
            try
            {
                _viewModel.UpdateStatusTxt = "Importing Alias List...";
                // await Task.Delay(100);
                await Task.Delay(TimeSpan.FromMilliseconds(0.5));

                _viewModel.AddLogMessage(tmpUpdateTxt = "Importing Alias List...");
                await Task.Delay(TimeSpan.FromMilliseconds(0.5));


                string tmpUPC, tmpAlias;
                using (var tmpFileReader = new StreamReader(FileToUse))
                {
                    // Skip the header row
                    tmpFileReader.ReadLine();

                    string tmpLine;
                    int i = 0;
                    while ((tmpLine = tmpFileReader.ReadLine()) != null)
                    {
                        i++;
                        string statusMessagetoDisplay = "Processing Alias Row " + i.ToString();

                        Console.WriteLine(statusMessagetoDisplay);
                        ;
                        if (tmpLine.Contains("|"))
                        {
                            tmpUPC = tmpLine.Split('|')[0];
                            tmpUPC = tmpUPC.Replace("\"", "");
                            tmpUPC = tmpUPC.Replace(",", "");

                            tmpAlias = tmpLine.Split('|')[1];
                            tmpAlias = tmpAlias.Replace("\"", "");
                            tmpAlias = tmpAlias.Replace(",", "");

                            clsProductAlias tmpNewAlias = new clsProductAlias();
                            tmpNewAlias.alias = tmpAlias;
                            tmpNewAlias.upc = tmpUPC;
                            _viewModel.CurrentMasterAliases.Add(tmpNewAlias);
                        }
                    }
                }

                _viewModel.UpdateStatusTxt = "Master Alias Import Complete";
                _viewModel.AddLogMessage(tmpUpdateTxt = "Master Alias Import Complete...");
                await Task.Delay(TimeSpan.FromMilliseconds(0.5));
                return true;
            }
            catch (Exception ex)
            {
                _viewModel.UpdateStatusTxt = "Idle...";
                MessageBox.Show("Error Loading Master Aliases From File: " + ex.Message);
                return false;
            }
        }

        public static async Task<bool> LoadImportSheetProducts(string FileToUse, MainWindowViewModel _viewModel)
        {
            string tmpUpdateTxt;
            try
            {
                _viewModel.UpdateStatusTxt = "Importing Client Import Products...";
                await Task.Delay(1);
                _viewModel.AddLogMessage(tmpUpdateTxt = "Importing Client Import Prosducts...");
                await Task.Delay(TimeSpan.FromMilliseconds(0.5));

                using (var tmpFileReader = new System.IO.StreamReader(FileToUse))
                {
                    string tmpLine;
                    tmpLine = tmpFileReader.ReadLine();
                    int i = 0;

                    while (tmpLine != null)
                    {
                        i++;

                        if (i > 1 && tmpLine.Contains(","))
                        {
                            var values = tmpLine.Split(',').Select(v => v.Replace("\"", "")).ToArray();
                            if (values.Length >= 17)
                            {
                                clsProductData tmpNewProduct = new clsProductData
                                {
                                    upc = values[0],
                                    name = values[1],
                                    description = values[2],
                                    department = values[3],
                                    department_number = values[4],
                                    category = values[5],
                                    manufacturer = values[6],
                                    brand = values[7],
                                    is_active = values[8],
                                    cost = values[9],
                                    price = values[10],
                                    vendor = values[11],
                                    part_num = values[12],
                                    part_num_units = values[13],
                                    part_cost = values[14],
                                    child_upc = values[15],
                                    num_units = values[16]
                                };
                                _viewModel.ImportNativeData.Add(tmpNewProduct);
                            }
                        }

                        tmpLine = tmpFileReader.ReadLine();
                    }
                }

                _viewModel.UpdateStatusTxt = "Client Import Products Import Complete...";

                _viewModel.AddLogMessage(tmpUpdateTxt = "Client Import Products Import Complete...");
                await Task.Delay(TimeSpan.FromMilliseconds(0.5));
                return true;
            }
            catch (Exception ex)
            {
                _viewModel.UpdateStatusTxt = "Idle";
                MessageBox.Show("Error Loading Import Sheet Products From File: " + ex.Message + ex.StackTrace);
                return false;
            }
        }

        #region Tab Data Functions

        public async Task ScrubImport(MainWindowViewModel _viewModel)
        {
            string tmpUpdateTxt;

            // Step 1: Wildcard search of ImportNativeData UPC field against CurrentMasterProducts
            _viewModel.UpdateStatusTxt = $"Scrub Step 1: Wildcard Search of Import Data Against Master Products";
            await Task.Delay(TimeSpan.FromMilliseconds(0.5));
            _viewModel.AddLogMessage(tmpUpdateTxt = "Scrub Step 1: Wildcard Search of Import Data...");
            await Task.Delay(TimeSpan.FromMilliseconds(0.5));

            foreach (var importData in _viewModel.ImportNativeData)
            {
                if (string.IsNullOrEmpty(importData.upc))
                {
                    //Skip Processing
                    continue;
                }

                var upc = importData.upc.Trim().Replace("*", ""); // Remove leading and trailing '*' characters

                var pattern =
                    ".*" + Regex.Escape(upc) + ".*"; // Match UPC with wildcards at the beginning, end, or both


                Console.WriteLine("Pattern: " + pattern);
                var matchingUPCs = _viewModel.CurrentMasterProducts.Where(p => Regex.IsMatch(p.upc, pattern))
                    .Select(p => p.upc)
                    .ToList();

                if (matchingUPCs.Any())
                {
                    // Add all variables of matching UPCs to ImportNAData
                    _viewModel.ImportNAData.AddRange(
                        _viewModel.CurrentMasterProducts.Where(p => matchingUPCs.Contains(p.upc)));
                }
                else
                {
                    // Add all variables of non-matching UPCs to ImportNotFoundData
                    _viewModel.ImportNotFoundData.Add(importData);
                }
            }

            // Step 2: Search CurrentMasterProducts against ImportExpandedUPCData
            _viewModel.UpdateStatusTxt = $"Scrub Step 2: Compare Master Products against Import Expanded UPC";
            await Task.Delay(TimeSpan.FromMilliseconds(0.5));
            _viewModel.AddLogMessage(tmpUpdateTxt = "Scrub Step 2: Compare Mster Products against Expanded UPC...");
            await Task.Delay(TimeSpan.FromMilliseconds(0.5));

            var expandedUPCs = _viewModel.ImportExpandedUPCData.Select(p => p.upc).ToList();
            var matchedUPCs = _viewModel.CurrentMasterProducts.Where(p => expandedUPCs.Contains(p.upc))
                .Select(p => p.upc)
                .ToList();

            foreach (var matchUPC in matchedUPCs)
            {
                var matchedProducts = _viewModel.CurrentMasterProducts.Where(p => p.upc == matchUPC).ToList();

                foreach (var product in matchedProducts)
                {
                    var updatedProduct = new clsProductData
                    {
                        upc = product.upc,
                        category = _viewModel.CKUpdateCategories
                            ? product.category
                            : _viewModel.ImportFullData.FirstOrDefault(d => d.upc == product.upc)?.category,
                        description = _viewModel.CKUpdateDescriptions
                            ? product.description
                            : _viewModel.ImportFullData.FirstOrDefault(d => d.upc == product.upc)?.description,
                        department = _viewModel.CKUpdateDept
                            ? product.department
                            : _viewModel.ImportFullData.FirstOrDefault(d => d.upc == product.upc)?.department,
                        manufacturer = _viewModel.CKUpdateManufBrand
                            ? product.manufacturer
                            : _viewModel.ImportFullData.FirstOrDefault(d => d.upc == product.upc)?.manufacturer,
                        brand = _viewModel.CKUpdateManufBrand
                            ? product.brand
                            : _viewModel.ImportFullData.FirstOrDefault(d => d.upc == product.upc)?.brand,
                        name = product.name,
                        department_number = product.department_number,
                        is_active = product.is_active,
                        cost = product.cost,
                        price = product.price,
                        vendor = product.vendor,
                        part_num = product.part_num,
                        part_num_units = product.part_num_units,
                        part_cost = product.part_cost,
                        child_upc = product.child_upc,
                        num_units = product.num_units
                    };

                    _viewModel.ImportFullData.Add(updatedProduct);
                }

                _viewModel.ImportNAData.RemoveAll(p => p.upc == matchUPC);
            }

            // Step 3: Search CurrentMasterAlias for aliases in ImportNotFoundData
            _viewModel.UpdateStatusTxt = $"Scrub Step 3: ImportNotFound UPC search in Current Master Alias";
            await Task.Delay(TimeSpan.FromMilliseconds(0.5));
            _viewModel.AddLogMessage(tmpUpdateTxt = "Scrub Step : ImportNotFound UPC search...");
            await Task.Delay(TimeSpan.FromMilliseconds(0.5));

            foreach (var importData in _viewModel.ImportNotFoundData)
            {
                var originUpc = importData.upc;
                if (string.IsNullOrEmpty(originUpc))
                {
                    continue; // Skip if UPC is empty
                }

                var matchingAlias = _viewModel.CurrentMasterAliases.FirstOrDefault(p => p.upc == originUpc);
                if (matchingAlias != null)
                {
                    // Update UPC to Alias
                    importData.upc = matchingAlias.alias;

                    var matchingProduct = _viewModel.CurrentMasterProducts.FirstOrDefault(p => p.upc == importData.upc);

                    if (matchingProduct != null)
                    {
                        // Update other data based on flags
                        if (_viewModel.CKUpdateCategories)
                        {
                            importData.category = matchingProduct.category;
                        }

                        if (_viewModel.CKUpdateDescriptions)
                        {
                            importData.description = matchingProduct.description;
                        }

                        if (_viewModel.CKUpdateDept)
                        {
                            importData.department = matchingProduct.department;
                        }

                        if (_viewModel.CKUpdateManufBrand)
                        {
                            importData.manufacturer = matchingProduct.manufacturer;
                            importData.brand = matchingProduct.brand;
                        }

                        _viewModel.ImportFullData.Add(importData);
                        // Add to ImportAliasFoundData
                        _viewModel.ImportAliasFoundData.Add(importData);
                    }
                }
            }

            string timestamp = DateTime.Now.ToString("MMddyy_hhmmss");
            string tmpPrintFilePath = MainWindowViewModel.InstallDirectory;

            _viewModel.UpdateStatusTxt = $"Scrub Step 4: Collate and Write to CSV";
            await Task.Delay(TimeSpan.FromMilliseconds(0.5));
            _viewModel.AddLogMessage(tmpUpdateTxt = "Scrub Step 4: Collate and Write CSV...");
            await Task.Delay(TimeSpan.FromMilliseconds(0.5));

            //Print Results

            WriteToCSV(_viewModel.ImportNAData, $"ImportNeedsAttentionFound_{timestamp}");
            WriteToCSV(_viewModel.ImportNotFoundData, $"ImportNotFound_{timestamp}");
            WriteToCSV(_viewModel.ImportFullData, $"ImportFullFound_{timestamp}");
            WriteToCSV(_viewModel.ImportAliasFoundData, $"ImportAliasFoundData_{timestamp}");

            String message = @"Scrubbing Completed." + Environment.NewLine +
                             "Files Can Be found here:" + Environment.NewLine + Environment.NewLine +
                             $"{tmpPrintFilePath}_ImportNeedsAttentionFound_{timestamp}" + Environment.NewLine +
                             $"{tmpPrintFilePath}_ImportNotFound_{timestamp}" + Environment.NewLine +
                             $"{tmpPrintFilePath}_ImportFullFound_{timestamp}" + Environment.NewLine +
                             $"{tmpPrintFilePath}_ImportAliasFoundData_{timestamp}";

            MessageBox.Show(message);

            _viewModel.AddLogMessage(message);
            await Task.Delay(TimeSpan.FromMilliseconds(0.5));

            _viewModel.UpdateStatusTxt = $"Scrub Step 5: Clear Temp Variable Data";
            _viewModel.AddLogMessage(tmpUpdateTxt = "Scrub Step 5: Clear Temp Data...");
            await Task.Delay(TimeSpan.FromMilliseconds(0.5));

            //Clear Data: 
            DataHelper.ClearProductDataList(_viewModel.CurrentMasterProducts);
            DataHelper.ClearProductDataList(_viewModel.ImportFullData);
            DataHelper.ClearProductDataList(_viewModel.ImportNativeData);
            DataHelper.ClearProductDataList(_viewModel.ImportAliasFoundData);
            DataHelper.ClearProductDataList(_viewModel.ImportNAData);
            DataHelper.ClearProductDataList(_viewModel.ImportNotFoundData);
            DataHelper.ClearProductDataList(_viewModel.ImportExpandedUPCData);
            DataHelper.ClearProductAliasList(_viewModel.ImportMasterProducts);
            DataHelper.ClearProductAliasList(_viewModel.CurrentMasterAliases);

            Console.WriteLine("testingfor Clearing of Variables");

            _viewModel.UpdateStatusTxt = $"Idle...";
            await Task.Delay(TimeSpan.FromMilliseconds(0.5));

            _viewModel.AddLogMessage(tmpUpdateTxt = "Scrubbing Complete");
            await Task.Delay(TimeSpan.FromMilliseconds(0.5));
        }
        
        public async Task Pruner(MainWindowViewModel _viewModel)
        {}
        

        #endregion

        #endregion
    }
}