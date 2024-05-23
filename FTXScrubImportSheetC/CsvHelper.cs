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

            await LogHelper.UpdateStatusAndLog(_viewModel,
                "Expand Export ONly Option Initiated",
                "Expand Only Option Initiated.");

            string timestamp = DateTime.Now.ToString("MMddyy_hhmmss");
            //expanded function: 

            foreach (var item in _viewModel.ImportNativeData.ToList())
            {
                if (string.IsNullOrEmpty(item.upc))
                {
                    //Skip Processing
                    continue;
                }

                item.upc = ExpandUPC(item.upc);

                _viewModel.ImportExpandedUPCData.Add(item);
            }

            string filePath = Path.Combine(InstallDirectory, "ImportExpandedOnly_" + timestamp);
            WriteToCSV(_viewModel.ImportExpandedUPCData, "ImportExpandedOnly_" + timestamp);

            string message = $@"File Saved Successfully: {filePath}";
            MessageBox.Show(message, "Expand Export Only Option", MessageBoxButton.OK, MessageBoxImage.Information);

            await LogHelper.UpdateStatusAndLog(_viewModel,
                "Expand Export Only Option Complete",
                "Expand Export Only Option Complete");

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

            await LogHelper.UpdateStatusAndLog(_viewModel,
                "Scrub Cleanup: Wiping Internals",
                "Scrub Cleanup: Wiping Internals");
        }

        public void WriteToCSV(List<clsProductData> data, string fileNamePrefix)
        {
            try
            {
                // Create a file name using the specified prefix and the current date and time
                string fileName = $"{fileNamePrefix}.csv";

                // Combine the directory path and file name to get the full file path
                string filePath = Path.Combine(InstallDirectory, fileName);


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
                Console.WriteLine("Error writing to CSV file: " + ex.Message + "\nInner Exception: " +
                                  ex.InnerException?.Message);
                MessageBox.Show("Erorr Writing to CSV File: " + ex.Message + ex.StackTrace + "\nInner Exception: " +
                                ex.InnerException?.Message);
            }
        }

        /// <summary>
        /// ExpandUPC Function takes UPC and expands it with options below
        /// </summary>
        /// <param name="UPC"></param>
        /// <returns></returns>
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

        public string ReverseExpandUPC(string expandedUPC)
        {
            try
            {
                if (expandedUPC.Length < 8 || expandedUPC.Length > 12 || !long.TryParse(expandedUPC, out _))
                {
                    return expandedUPC;
                }

                // Remove the check digit
                expandedUPC = expandedUPC.Substring(0, expandedUPC.Length - 1);

                // Determine the original length and reverse transformations
                if (expandedUPC.Length == 11)
                {
                    if (expandedUPC.StartsWith("0"))
                    {
                        expandedUPC = expandedUPC.Substring(1);
                    }

                    return expandedUPC;
                }

                if (expandedUPC.Length == 10)
                {
                    if (expandedUPC.StartsWith("00"))
                    {
                        expandedUPC = expandedUPC.Substring(2);
                    }

                    return expandedUPC;
                }

                if (expandedUPC.Length == 9)
                {
                    if (expandedUPC.StartsWith("000"))
                    {
                        expandedUPC = expandedUPC.Substring(3);
                    }

                    return expandedUPC;
                }

                if (expandedUPC.Length == 12)
                {
                    if (expandedUPC.StartsWith("0"))
                    {
                        string original = expandedUPC.Substring(1);
                        if (original[2] == '0' && original[3] == '0' && original[4] == '0' && original[5] == '0' &&
                            original[6] == '0')
                        {
                            return "0" + original.Substring(0, 2) + original.Substring(7, 5);
                        }
                        else if (original[2] == '1' && original[3] == '0' && original[4] == '0' && original[5] == '0' &&
                                 original[6] == '0')
                        {
                            return "0" + original.Substring(0, 2) + original.Substring(7, 5);
                        }
                        else if (original[2] == '2' && original[3] == '0' && original[4] == '0' && original[5] == '0' &&
                                 original[6] == '0')
                        {
                            return "0" + original.Substring(0, 2) + original.Substring(7, 5);
                        }
                        else if (original[3] == '0' && original[4] == '0' && original[5] == '0' && original[6] == '0')
                        {
                            return "0" + original.Substring(0, 3) + original.Substring(7, 4);
                        }
                        else if (original[4] == '0' && original[5] == '0' && original[6] == '0')
                        {
                            return "0" + original.Substring(0, 4) + original.Substring(7, 3);
                        }
                        else if (original[6] == '0')
                        {
                            return "0" + original.Substring(0, 5) + original[11];
                        }
                        else
                        {
                            return original;
                        }
                    }
                }

                return expandedUPC;
            }
            catch
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

                await LogHelper.UpdateStatusAndLog(_viewModel,
                    "Master Products Import Complete...",
                    "Master Products Import Complete...");
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
                await LogHelper.UpdateStatusAndLog(_viewModel,
                    "Importing Alias List...",
                    "Importing Alias List...");


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

                await LogHelper.UpdateStatusAndLog(_viewModel,
                    "Master Alias Import Complete.",
                    "Master Alias Import Complete.");

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
                await LogHelper.UpdateStatusAndLog(_viewModel,
                    "Importing Client Import Products...",
                    "Importing Client Import Products...");

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

                await LogHelper.UpdateStatusAndLog(_viewModel,
                    "Client Import Products Import Complete...",
                    "Client Import Products Import Complete...");

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

        private async Task<bool> AliasScrub(MainWindowViewModel _viewModel, clsProductData importData)
        {
            if (string.IsNullOrEmpty(importData.upc))
            {
                //Skip Processing
                return false;
            }

            var upc = importData.upc.Trim().Replace("*", ""); // Remove leading and trailing '*' characters

            string tmpAlias = null; //holder for the alias that is found

            var matchingAliasUPCs = _viewModel.CurrentMasterAliases
                .Where(p => p.alias.Contains(upc))
                .Select(p => new { Alias = p.alias, UPC = p.upc })
                .ToList();
            
            var upcList = matchingAliasUPCs.Select(p => p.Alias).ToList();

            var tmpAliasList = matchingAliasUPCs.Select(p => p.UPC).ToList();

            string tmpAliasFound = tmpAliasList.FirstOrDefault();
            
            string tmpUpcFound = upcList.FirstOrDefault();

            //Scrub Results
            if (matchingAliasUPCs.Any())
            {
                var recordsToUpdate = _viewModel.ImportNativeData
                    .Where(p => upcList.Contains(p.upc))
                    .ToList();

                //todo we're good to this point
                try
                {
                    foreach (var recordToUpdate in recordsToUpdate)
                    {
                        tmpAlias = _viewModel.CurrentMasterAliases
                            .FirstOrDefault(p => p.upc == recordToUpdate.upc)?.upc;
                       
                        // Search CurrentMasterProducts for mmatching Alias Match. 
                        // var matchingMasterUPC =
                        //     _viewModel.CurrentMasterProducts.FirstOrDefault(p => p.upc.Contains(matchingAliasUPCs.UPC));

                        var matchingMasterUPC = matchingAliasUPCs
                            .Where(a => a.UPC == recordToUpdate.upc)
                            .Select(a => _viewModel.CurrentMasterProducts.FirstOrDefault(p => p.upc.Contains(a.Alias)))
                            .FirstOrDefault();
                        
                        if (!string.IsNullOrEmpty(tmpAlias) && matchingMasterUPC != null)
                        {
                            recordToUpdate.upc = tmpAlias;
                            if (_viewModel.CKUpdateCategories)
                            {
                                recordToUpdate.category = matchingMasterUPC.category;
                            }

                            if (_viewModel.CKUpdateDescriptions)
                            {
                                recordToUpdate.description = matchingMasterUPC.description;
                            }

                            if (_viewModel.CKUpdateDescriptions)
                            {
                                recordToUpdate.department = matchingMasterUPC.department;
                            }

                            if (_viewModel.CKUpdateManufBrand)
                            {
                                if (matchingMasterUPC.brand != null)
                                {
                                    recordToUpdate.brand = matchingMasterUPC.brand;
                                }

                                if (matchingMasterUPC.manufacturer != null)
                                {
                                    recordToUpdate.manufacturer = matchingMasterUPC.manufacturer;
                                }
                            }

                            _viewModel.ImportAliasFoundData.Add(recordToUpdate);
                        }

                        //     //todo cleanup here
                        _viewModel.ImportNativeData.RemoveAll(p => upcList.Contains(p.upc));

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

                
                Console.WriteLine($"Updated UPCs to Aliases and Moved them over to ImportModifiedData");
                
                return true;
            }

            return false;
        }

        private async Task<bool> UpcExpandedMasterProductCompare(MainWindowViewModel _viewModel, clsProductData item)
        {
            // in this function we're expanding the UPC, then comparing that over to the CurrentMasterSheet 
            //If it's found then it's moved over to the FullFound
            //if Not, then it can be moved over to the Needs Attention Variable
            //We'll handle moving to Full Found here
            //we'll set the varaible boolean to remove from the native data 
            //and either populate the other data or not at all. 


            if (string.IsNullOrEmpty(item.upc))
            {
                //Skip Processing
                return false;
            }

            var originUpc = item.upc; //record originalUPC of product

            item.upc = ExpandUPC(item.upc); //updates data with expanded UPC

            var matchingMasterUpcs = _viewModel.CurrentMasterProducts
                .Where(p => p.upc.Contains(item.upc))
                // .Select(p => p.upc)
                .ToList();

            Console.WriteLine("Master Product Search Pattern: " + item.upc);

            if (matchingMasterUpcs.Any())
            {
                //make changes to scrub
                var matchingProduct = matchingMasterUpcs.FirstOrDefault();

                if (_viewModel.CKUpdateCategories)
                {
                    item.category = matchingProduct.category;
                }

                if (_viewModel.CKUpdateDescriptions)
                {
                    item.description = matchingProduct.category;
                }

                if (_viewModel.IsUpdateDepartmentsEnabled)
                {
                    item.department = matchingProduct.department;
                }

                if (_viewModel.CKUpdateManufBrand)
                {
                    if (matchingProduct.brand != null)
                    {
                        item.brand = matchingProduct.brand;
                    }

                    if (matchingProduct.manufacturer != null)
                    {
                        item.manufacturer = matchingProduct.manufacturer;
                    }
                }

                _viewModel.ImportFullData.Add(item);
                return true;
            }

            return false;
        }

        public async Task ScrubImport(MainWindowViewModel _viewModel) //Modified
        {
            //Step1 : Search Via Alias

            await LogHelper.UpdateStatusAndLog(_viewModel,
                "Scrub Step 1: Alias List Search",
                "Scrub Step 1: Alias List Search...");

            var itemsToMove = new List<clsProductData>();

            foreach (var item in _viewModel.ImportNativeData.ToList())
            {
                if (string.IsNullOrEmpty(item.upc))
                {
                    //Skip Processing
                    continue;
                }

                //TODO location to activate step 1: Alias Scrubber
                bool shouldRemove = await AliasScrub(_viewModel, item);
                if (shouldRemove)
                {
                    itemsToMove.Add(item);
                }
            }

            //remove Items from NativeData Function
            foreach (var item in itemsToMove)
            {
                _viewModel.ImportAliasFoundData.Add(item);
                _viewModel.ImportNativeData.Remove(item);
            }

            itemsToMove.Clear(); //clear variable to ready for next set

            await LogHelper.UpdateStatusAndLog(_viewModel,
                "Scrub Step 1 Complete: Alias List Search",
                "Scrub Step 1 Complete: Alias List Search...");

            //Step2: 
            await LogHelper.UpdateStatusAndLog(_viewModel,
                "Scrub Step 2 : Expanded Native Data Compare Against Master Products",
                "Scrub Step 2 : Expanded Native Data Compare against Master Products...");

            foreach (var item in _viewModel.ImportNativeData.ToList())
            {
                var originUpc = item.upc;

                bool successExpandedMasterProductCompareJob = await UpcExpandedMasterProductCompare(_viewModel, item);

                if (successExpandedMasterProductCompareJob)
                {
                    item.upc = originUpc;

                    _viewModel.ImportNativeData.Remove(item); //Updates Native Data
                }
                else
                {
                    item.upc = originUpc;
                    _viewModel.ImportNAData.Add(item);
                    _viewModel.ImportNativeData.Remove(item);
                }
            }

            await LogHelper.UpdateStatusAndLog(_viewModel,
                "Scrub Step 2 Completed : Expanded Native Data Compare Against Master Products",
                "Scrub Step 2 Completed : Expanded Native Data Compare against Master Products...");

            //Step 3 : Collate and Cleanup
            await LogHelper.UpdateStatusAndLog(_viewModel,
                "Scrub Step 4: Collate and Write to CSV",
                "Scrub Step 4: Collate and Write to CSV");

            string timestamp = DateTime.Now.ToString("MMddyy_hhmmss");
            string tmpPrintFilePath = MainWindowViewModel.InstallDirectory;

            WriteToCSV(_viewModel.ImportNAData, $"ImportNeedsAttentionFound_{timestamp}");
            //WriteToCSV(_viewModel.ImportNotFoundData, $"ImportNotFound_{timestamp}");
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

            await LogHelper.UpdateStatusAndLog(_viewModel,
                "Scrub Step 5: Clear Temp Variable Data",
                "Scrub Step 5: Clear Temp Variable Data");

            //Clear Data: 
            DataHelper.ClearProductDataList(_viewModel.CurrentMasterProducts);
            DataHelper.ClearProductDataList(_viewModel.ImportFullData);
            DataHelper.ClearProductDataList(_viewModel.ImportNativeData);
            DataHelper.ClearProductDataList(_viewModel.ImportAliasFoundData);
            DataHelper.ClearProductDataList(_viewModel.ImportNAData);
            //DataHelper.ClearProductDataList(_viewModel.ImportNotFoundData);
            DataHelper.ClearProductDataList(_viewModel.ImportExpandedUPCData);
            DataHelper.ClearProductAliasList(_viewModel.ImportMasterProducts);
            DataHelper.ClearProductAliasList(_viewModel.CurrentMasterAliases);


            _viewModel.UpdateStatusTxt = $"Idle...";
            await Task.Delay(TimeSpan.FromMilliseconds(0.5));
            //
            // _viewModel.AddLogMessage(tmpUpdateTxt = "Scrubbing Complete");
            // await Task.Delay(TimeSpan.FromMilliseconds(0.5));
            //             
        }
    }

    //todo second step work to be done
}

#endregion

#endregion

#region Trash Area

//TODO to remove after implemented

//Former alias search function

// var upc = importData.upc.Trim().Replace("*", ""); // Remove leading and trailing '*' characters
//
// string tmpAlias = null; //holder for the alias that is found
//
// //matches UPC in with those in CurrentMasterAliases
// var matchingAliasUPCs = _viewModel.CurrentMasterAliases
//     .Where(p => p.upc.Contains(upc))
//     .Select(p => p.upc)
//     .ToList();
//
// if (matchingAliasUPCs.Any())
// {
//     var recordsToUpdate = _viewModel.ImportNativeData
//         .Where(p => matchingAliasUPCs.Contains(p.upc))
//         .ToList();
//
//     foreach (var recordToUpdate in recordsToUpdate)
//     {
//         tmpAlias = _viewModel.CurrentMasterAliases
//             .FirstOrDefault(p => p.upc == recordToUpdate.upc)?.alias;
//         
//         if (!string.IsNullOrEmpty(tmpAlias))
//         {
//             recordToUpdate.upc = tmpAlias;
//
//             _viewModel.ImportAliasFoundData.Add(recordToUpdate);
//             
//             //add here to update scrub data
//             
//         }
//
//         _viewModel.ImportNativeData.RemoveAll(p => matchingAliasUPCs.Contains(p.upc));
//         
//         Console.WriteLine($"Updated UPCs to Aliases and Moved them over to ImportModifiedData");
//         return;
//     }

//Step2 : Compare Expanded UPC to Master Products 


// public async Task ScrubImport(MainWindowViewModel _viewModel) //Original
// {
//     string tmpUpdateTxt;
//
//     // Step 1: Wildcard search of ImportNativeData UPC field against CurrentMasterProducts
//
//     await LogHelper.UpdateStatusAndLog(_viewModel,
//         "Scrub Step 1: Wildcard Search of Import Data Against Master Products",
//         "Scrub Step 1: Wildcard Search of Import Data...");
//
//     foreach (var importData in _viewModel.ImportNativeData)
//     {
//         if (string.IsNullOrEmpty(importData.upc))
//         {
//             //Skip Processing
//             continue;
//         }
//
//         var upc = importData.upc.Trim().Replace("*", ""); // Remove leading and trailing '*' characters
//
//         var matchingUPCs = _viewModel.CurrentMasterProducts
//             .Where(p => p.upc.Contains(upc))
//             .Select(p => p.upc)
//             .ToList();
//
//         Console.WriteLine("Pattern: " + upc);
//
//         if (matchingUPCs.Any())
//         {
//             // Add all variables of matching UPCs to ImportNAData
//
//             _viewModel.ImportNAData.Add(importData);  //Import to ImportNAData first without expansion
//             importData.upc = ExpandUPC(matchingUPCs.First()); //ExpandUPC
//
//             _viewModel.ImportExpandedUPCData.Add(importData); //add results after expansion to importexpanding upcdata
//         }
//         else
//         {
//             // Add all variables of non-matching UPCs to ImportNotFoundData
//             _viewModel.ImportNotFoundData.Add(importData);
//         }
//     }
//
//     var countImportNAData = _viewModel.ImportNAData.Count;
//     var countExpanded = _viewModel.ImportExpandedUPCData.Count;
//     var countImportNotFound = _viewModel.ImportNAData.Count;
//     var countImportNativeData = _viewModel.ImportNativeData.Count;
//     var countNotFound = _viewModel.ImportNotFoundData.Count;
//     
//     Console.WriteLine($"ImportNAData Count: {countImportNAData}");
//     Console.WriteLine($"ImportExpandedUPCData Count: {countExpanded}");
//     Console.WriteLine($"ImportNotFoundData Count: {countImportNotFound}");
//     Console.WriteLine($"ImportNativeData Count: {countImportNativeData}");
//     Console.WriteLine($"NotFoundData Count: {countNotFound}");
//
//     Console.WriteLine("Testing here");
//
//     // Step 2: Search CurrentMasterProducts against ImportExpandedUPCData
//
//     await LogHelper.UpdateStatusAndLog(
//         _viewModel,
//         "Scrub Step 2: Compare Master Products against Import Expanded UPC",
//         "Scrub Step 2: Compare Master Products against Expanded UPC..."
//     );
//
//     // Expand UPCs
//     var expandedUPCs = _viewModel.ImportExpandedUPCData.Select(p => p.upc).ToList();
//
//     // Find matching UPCs
//     var matchedUPCs = _viewModel.CurrentMasterProducts
//         .Where(p => expandedUPCs.Contains(p.upc))
//         .Select(p => p.upc)
//         .ToList();
//
//     foreach (var matchUPC in matchedUPCs)
//     {
//         var matchedProducts = _viewModel.CurrentMasterProducts
//             .Where(p => p.upc == matchUPC)
//             .ToList();
//
//         foreach (var product in matchedProducts)
//         {
//             var importExpandedProduct = _viewModel.ImportExpandedUPCData
//                 .FirstOrDefault(d => d.upc == product.upc);
//
//             var updatedProduct = new clsProductData
//             {
//                 upc = product.upc,
//                 category = _viewModel.CKUpdateCategories ? product.category : importExpandedProduct?.category,
//                 description = _viewModel.CKUpdateDescriptions
//                     ? product.description
//                     : importExpandedProduct?.description,
//                 department = _viewModel.CKUpdateDept ? product.department : importExpandedProduct?.department,
//                 manufacturer = _viewModel.CKUpdateManufBrand
//                     ? product.manufacturer
//                     : importExpandedProduct?.manufacturer,
//                 brand = _viewModel.CKUpdateManufBrand ? product.brand : importExpandedProduct?.brand,
//                 name = product.name,
//                 department_number = product.department_number,
//                 is_active = product.is_active,
//                 cost = product.cost,
//                 price = product.price,
//                 vendor = product.vendor,
//                 part_num = product.part_num,
//                 part_num_units = product.part_num_units,
//                 part_cost = product.part_cost,
//                 child_upc = product.child_upc,
//                 num_units = product.num_units
//             };
//
//             _viewModel.ImportFullData.Add(updatedProduct);
//         }
//
//         // Remove matches from the ImportNAData
//         _viewModel.ImportNAData.RemoveAll(p => p.upc == matchUPC);
//     }
//
//
//     // await LogHelper.UpdateStatusAndLog(_viewModel, 
//     //     "Scrub Step 2: Compare Master Products against Import Expanded UPC", 
//     //     "Scrub Step 2: Compare Master Products against Expanded UPC...");
//     //
//     // //Expand Upcs
//     // var expandedUPCs = _viewModel.ImportExpandedUPCData.Select(p => p.upc).ToList();
//     // var matchedUPCs = _viewModel.CurrentMasterProducts.Where(p => expandedUPCs.Contains(p.upc))
//     //     .Select(p => p.upc)
//     //     .ToList();
//     //
//     // foreach (var matchUPC in matchedUPCs)
//     // {
//     //     var matchedProducts = _viewModel.CurrentMasterProducts.Where(p => p.upc == matchUPC).ToList();
//     //
//     //     foreach (var product in matchedProducts)
//     //     {
//     //         var updatedProduct = new clsProductData
//     //         {
//     //             upc = product.upc,
//     //             category = _viewModel.CKUpdateCategories
//     //                 ? product.category
//     //                 : _viewModel.ImportFullData.FirstOrDefault(d => d.upc == product.upc)?.category,
//     //             description = _viewModel.CKUpdateDescriptions
//     //                 ? product.description
//     //                 : _viewModel.ImportFullData.FirstOrDefault(d => d.upc == product.upc)?.description,
//     //             department = _viewModel.CKUpdateDept
//     //                 ? product.department
//     //                 : _viewModel.ImportFullData.FirstOrDefault(d => d.upc == product.upc)?.department,
//     //             manufacturer = _viewModel.CKUpdateManufBrand
//     //                 ? product.manufacturer
//     //                 : _viewModel.ImportFullData.FirstOrDefault(d => d.upc == product.upc)?.manufacturer,
//     //             brand = _viewModel.CKUpdateManufBrand
//     //                 ? product.brand
//     //                 : _viewModel.ImportFullData.FirstOrDefault(d => d.upc == product.upc)?.brand,
//     //             name = product.name,
//     //             department_number = product.department_number,
//     //             is_active = product.is_active,
//     //             cost = product.cost,
//     //             price = product.price,
//     //             vendor = product.vendor,
//     //             part_num = product.part_num,
//     //             part_num_units = product.part_num_units,
//     //             part_cost = product.part_cost,
//     //             child_upc = product.child_upc,
//     //             num_units = product.num_units
//     //         };
//     //
//     //         _viewModel.ImportFullData.Add(updatedProduct);
//     //     }
//     //
//     //     _viewModel.ImportNAData.RemoveAll(p => p.upc == matchUPC);  //This removes matches from the ImportNAData
//     // }
//
//     
//     
//     // Step 3: Search CurrentMasterAlias for aliases in ImportNotFoundData
//
//     await LogHelper.UpdateStatusAndLog(_viewModel,
//         "Scrub Step 3: ImportNotFound UPC search in Current Master Alias",
//         "Scrub Step : ImportNotFound UPC search...");
//
//     foreach (var importData in _viewModel.ImportNotFoundData)
//     {
//         var originUpc = importData.upc;
//         if (string.IsNullOrEmpty(originUpc))
//         {
//             continue; // Skip if UPC is empty
//         }
//
//         var matchingAlias = _viewModel.CurrentMasterAliases.FirstOrDefault(p => p.upc == originUpc);
//         if (matchingAlias != null)
//         {
//             // Update UPC to Alias
//             importData.upc = matchingAlias.alias;
//
//             var matchingProduct = _viewModel.CurrentMasterProducts.FirstOrDefault(p => p.upc == importData.upc);
//
//             if (matchingProduct != null)
//             {
//                 // Update other data based on flags
//                 if (_viewModel.CKUpdateCategories)
//                 {
//                     importData.category = matchingProduct.category;
//                 }
//
//                 if (_viewModel.CKUpdateDescriptions)
//                 {
//                     importData.description = matchingProduct.description;
//                 }
//
//                 if (_viewModel.CKUpdateDept)
//                 {
//                     importData.department = matchingProduct.department;
//                 }
//
//                 if (_viewModel.CKUpdateManufBrand)
//                 {
//                     importData.manufacturer = matchingProduct.manufacturer;
//                     importData.brand = matchingProduct.brand;
//                 }
//
//                 _viewModel.ImportFullData.Add(importData);
//
//                 _viewModel.ImportAliasFoundData.Add(importData);
//             }
//         }
//     }
//
//     string timestamp = DateTime.Now.ToString("MMddyy_hhmmss");
//     string tmpPrintFilePath = MainWindowViewModel.InstallDirectory;
//
//     await LogHelper.UpdateStatusAndLog(_viewModel,
//         "Scrub Step 4: Collate and Write to CSV",
//         "Scrub Step 4: Collate and Write to CSV");
//
//
//     //Print Results
//
//     WriteToCSV(_viewModel.ImportNAData, $"ImportNeedsAttentionFound_{timestamp}");
//     WriteToCSV(_viewModel.ImportNotFoundData, $"ImportNotFound_{timestamp}");
//     WriteToCSV(_viewModel.ImportFullData, $"ImportFullFound_{timestamp}");
//     WriteToCSV(_viewModel.ImportAliasFoundData, $"ImportAliasFoundData_{timestamp}");
//
//     String message = @"Scrubbing Completed." + Environment.NewLine +
//                      "Files Can Be found here:" + Environment.NewLine + Environment.NewLine +
//                      $"{tmpPrintFilePath}_ImportNeedsAttentionFound_{timestamp}" + Environment.NewLine +
//                      $"{tmpPrintFilePath}_ImportNotFound_{timestamp}" + Environment.NewLine +
//                      $"{tmpPrintFilePath}_ImportFullFound_{timestamp}" + Environment.NewLine +
//                      $"{tmpPrintFilePath}_ImportAliasFoundData_{timestamp}";
//
//     MessageBox.Show(message);
//
//     _viewModel.AddLogMessage(message);
//     await Task.Delay(TimeSpan.FromMilliseconds(0.5));
//     await LogHelper.UpdateStatusAndLog(_viewModel,
//         "Scrub Step 5: Clear Temp Variable D   `4ata",
//         "Scrub Step 5: Clear Temp Variable Data");
//
//     //Clear Data: 
//     DataHelper.ClearProductDataList(_viewModel.CurrentMasterProducts);
//     DataHelper.ClearProductDataList(_viewModel.ImportFullData);
//     DataHelper.ClearProductDataList(_viewModel.ImportNativeData);
//     DataHelper.ClearProductDataList(_viewModel.ImportAliasFoundData);
//     DataHelper.ClearProductDataList(_viewModel.ImportNAData);
//     DataHelper.ClearProductDataList(_viewModel.ImportNotFoundData);
//     DataHelper.ClearProductDataList(_viewModel.ImportExpandedUPCData);
//     DataHelper.ClearProductAliasList(_viewModel.ImportMasterProducts);
//     DataHelper.ClearProductAliasList(_viewModel.CurrentMasterAliases);
//
//
//     _viewModel.UpdateStatusTxt = $"Idle...";
//     await Task.Delay(TimeSpan.FromMilliseconds(0.5));
//
//     _viewModel.AddLogMessage(tmpUpdateTxt = "Scrubbing Complete");
//     await Task.Delay(TimeSpan.FromMilliseconds(0.5));
// }

// public async Task Pruner(MainWindowViewModel _viewModel)
// {
// }
//
// public async Task DuplicateHunter(MainWindowViewModel _viewmodel)
// {
// }

#endregion