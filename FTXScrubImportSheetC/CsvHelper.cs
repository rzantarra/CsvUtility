using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using FTXScrubImportSheetC;

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

        /// <summary>
        /// ExpandUPCOnly Function
        /// </summary>
        /// <param name="_viewModel"></param>
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
            DataHelper.ClearProductDataList(_viewModel.Import_NEEDS_REVIEW);
            DataHelper.ClearProductDataList(_viewModel.ImportExpandedUPCData);
            DataHelper.ClearProductAliasList(_viewModel.ImportMasterProducts);
            DataHelper.ClearProductAliasList(_viewModel.CurrentMasterAliases);

            await LogHelper.UpdateStatusAndLog(_viewModel,
                "Scrub Cleanup: Wiping Internals",
                "Scrub Cleanup: Wiping Internals");
        }

        /// <summary>
        /// WriteToCSV : Local Write to CSV
        /// </summary>
        /// <param name="data"></param>
        /// <param name="fileNamePrefix"></param>
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

        /// <summary>
        /// ReserseExpandUpc Function reverses the Expanded UPC
        /// </summary>
        /// <param name="expandedUPC"></param>
        /// <returns></returns>
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

        /// <summary>
        /// EscapeCSVField 
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
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

        /// <summary>
        /// LoadMasterPRoducts
        /// </summary>
        /// <param name="FileToUse"></param>
        /// <param name="_viewModel"></param>
        /// <returns></returns>
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

        /// <summary>
        /// LoadMasterAliases
        /// </summary>
        /// <param name="FileToUse"></param>
        /// <param name="_viewModel"></param>
        /// <returns></returns>
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

        /// <summary>
        /// LoadImportSheetsProducts
        /// </summary>
        /// <param name="FileToUse"></param>
        /// <param name="_viewModel"></param>
        /// <returns></returns>
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

        /// <summary>
        /// ScrubImport : Initial part of the Scrub Import Feature, decides on Alias Scrubber and Regular Scrubber
        /// </summary>
        /// <param name="_viewModel"></param>
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

                
                bool shouldRemove = await AliasScrub(_viewModel, item);
                if (shouldRemove)
                {
                    itemsToMove.Add(item);
                }
            }

            //remove Items from NativeData Function
            foreach (var item in itemsToMove)
            {
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
                "Scrub Step 3: Collate and Write to CSV",
                "Scrub Step 3: Collate and Write to CSV");

            string timestamp = DateTime.Now.ToString("MMddyy_hhmmss");
            string tmpPrintFilePath = MainWindowViewModel.InstallDirectory;

            WriteToCSV(_viewModel.ImportNAData, $"Import_Needs_Attention_{timestamp}");
            WriteToCSV(_viewModel.Import_NEEDS_REVIEW, $"Import_Needs_Review_Expanded_{timestamp}");
            WriteToCSV(_viewModel.ImportFullData, $"Import_Full_Found_{timestamp}");
            WriteToCSV(_viewModel.ImportAliasFoundData, $"Import_Alias_Found_{timestamp}");

            String message = @"Scrubbing Completed." + Environment.NewLine +
                             "Files Can Be found here:" + Environment.NewLine + Environment.NewLine +
                             $"{tmpPrintFilePath}_Import_Needs_Attention_{timestamp}" + Environment.NewLine +
                             $"{tmpPrintFilePath}_Import_NEEDS_REVIEW_{timestamp}" + Environment.NewLine +
                             $"{tmpPrintFilePath}_Import_Full_Found_{timestamp}" + Environment.NewLine +
                             $"{tmpPrintFilePath}_Import_Alias_Found_{timestamp}";

            MessageBox.Show(message);

            _viewModel.AddLogMessage(message);

            await Task.Delay(TimeSpan.FromMilliseconds(0.5));

            await LogHelper.UpdateStatusAndLog(_viewModel,
                "Scrub Step 4: Clear Temp Variable Data",
                "Scrub Step 4: Clear Temp Variable Data");

            //Clear Data: 
            DataHelper.ClearProductDataList(_viewModel.CurrentMasterProducts);
            DataHelper.ClearProductDataList(_viewModel.ImportFullData);
            DataHelper.ClearProductDataList(_viewModel.ImportNativeData);
            DataHelper.ClearProductDataList(_viewModel.ImportAliasFoundData);
            DataHelper.ClearProductDataList(_viewModel.ImportNAData);
            DataHelper.ClearProductDataList(_viewModel.Import_NEEDS_REVIEW);
            DataHelper.ClearProductDataList(_viewModel.ImportExpandedUPCData);
            DataHelper.ClearProductAliasList(_viewModel.ImportMasterProducts);
            DataHelper.ClearProductAliasList(_viewModel.CurrentMasterAliases);


            _viewModel.UpdateStatusTxt = $"Idle...";
            await Task.Delay(TimeSpan.FromMilliseconds(0.5));
        }

        /// <summary>
        /// Alias Scrub
        /// </summary>
        /// <param name="_viewModel"></param>
        /// <param name="importData"></param>
        /// <returns></returns>
        private async Task<bool> AliasScrub(MainWindowViewModel _viewModel, clsProductData importData)
        {
            if (string.IsNullOrEmpty(importData.upc))
            {
                // Skip processing if the UPC is null or empty
                return false;
            }

            var upc = importData.upc.Trim().Replace("*", ""); // Remove leading and trailing '*' characters

            // Get the matching alias UPCs based on the trimmed UPC
            var matchingAliasUPCs = _viewModel.CurrentMasterAliases
                .Where(p => p.alias.Contains(upc))
                .Select(p => new { Alias = p.alias, UPC = p.upc })
                .ToList();

            if (!matchingAliasUPCs.Any())
            {
                // No matching aliases found
                return false;
            }

            // Extract alias and UPC lists
            var aliasList = matchingAliasUPCs.Select(p => p.Alias).ToList();
            var upcList = matchingAliasUPCs.Select(p => p.UPC).ToList();

            // Find records in ImportNativeData that match any of the aliases
            var recordsToUpdate = _viewModel.ImportNativeData
                .Where(p => aliasList.Contains(p.upc))
                .ToList();

            try
            {
                var updatedRecords = new List<clsProductData>();

                foreach (var recordToUpdate in recordsToUpdate)
                {
                    // Get the alias for the current record
                    string tmpAlias = _viewModel.CurrentMasterAliases
                        .FirstOrDefault(p => p.upc == recordToUpdate.upc)?.alias;

                    // Find the matching master product using the alias
                    var matchingMasterUPC = matchingAliasUPCs
                        .Where(a => a.Alias == recordToUpdate.upc)
                        .Select(a => _viewModel.CurrentMasterProducts.FirstOrDefault(p => p.upc.Contains(a.UPC)))
                        .FirstOrDefault();

                    if (matchingMasterUPC != null)
                    {
                        // Update record fields based on conditions
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

                        updatedRecords.Add(recordToUpdate);
                        _viewModel.ImportAliasFoundData.Add(recordToUpdate);
                    }
                }

                // Remove updated records from ImportNativeData
                _viewModel.ImportNativeData.RemoveAll(p => aliasList.Contains(p.upc));

                Console.WriteLine($"Updated UPCs to Aliases and Moved them over to ImportAliasFoundData");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// UPCExpandedMasterProductCompare
        /// </summary>
        /// <param name="_viewModel"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private async Task<bool> UpcExpandedMasterProductCompare(MainWindowViewModel _viewModel, clsProductData item)
        {
            if (string.IsNullOrEmpty(item.upc))
            {
                //Skip Processing
                return false;
            }

            Console.WriteLine("Master Product Search Pattern: " + item.upc);

            var originUpc = item.upc; //record originalUPC of product

            List<clsProductData> expandedMatchingUpcs = new List<clsProductData>();

            if (item.upc.Length < 12)
            {
                item.upc = item.upc.PadLeft(12, '0');

                expandedMatchingUpcs = _viewModel.CurrentMasterProducts
                    .Where(p => p.upc.Contains(item.upc))
                    .ToList();
                foreach (var product in expandedMatchingUpcs)
                {
                    product.upc = item.upc;
                }

                //clear expand and return to original 
                item.upc = originUpc;
            }

            item.upc = ExpandUPC(item.upc); //updates data with expanded UPC

            var matchingMasterUpcs = _viewModel.CurrentMasterProducts
                .Where(p => p.upc.Contains(item.upc))
                // .Select(p => p.upc)
                .ToList();

            if (matchingMasterUpcs.Any() || expandedMatchingUpcs.Any())
            {
                var matchingProduct = expandedMatchingUpcs.FirstOrDefault() ?? matchingMasterUpcs.FirstOrDefault();

                if (_viewModel.CKUpdateCategories)
                {
                    item.category = matchingProduct.category;
                }

                if (_viewModel.CKUpdateDescriptions)
                {
                    item.description = matchingProduct.description;
                }

                if (_viewModel.CKUpdateDept)
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

                if (expandedMatchingUpcs.Any())
                {
                    _viewModel.Import_NEEDS_REVIEW.Add(item); // Add to ImportExpandedData if UPC was expanded
                }
                else
                {
                    _viewModel.ImportFullData.Add(item); // Add to ImportFullData if UPC was not expanded
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Pruner Function
        /// </summary>
        /// <param name="_viewModel"></param>
        public async Task Pruner(MainWindowViewModel _viewModel)
        {
            await LogHelper.UpdateStatusAndLog(_viewModel,
                "Pruner Initialized : Step 1",
                "Pruner Initialized :Step 1");

            List<clsProductData> prunedData = new List<clsProductData>();

            foreach (var item in _viewModel.ImportNativeData.ToList())
            {
                if (string.IsNullOrEmpty(item.upc))
                {
                    //Skip Processing
                    continue;
                }

                if (_viewModel.CKTruncateName)
                {
                    const int maxLength = 50;

                    if (item.name.Length > maxLength)
                    {
                        item.name = item.name.Substring(0, maxLength);
                    }
                }

                if (_viewModel.CKPruneDollar)
                {
                    if (!string.IsNullOrEmpty(item.cost) && item.cost.Contains("$"))
                    {
                        item.cost = item.cost.Replace("$", "");
                    }

                    if (!string.IsNullOrEmpty(item.price) && item.price.Contains("$"))
                    {
                        item.price  = item.price.Replace("$", "");
                    }

                    if (!string.IsNullOrEmpty(item.part_cost) && item.part_cost.Contains("$"))
                    {
                        item.part_cost = item.part_cost.Replace("$", "");
                    }
                }

                prunedData.Add(item);
            }

            await LogHelper.UpdateStatusAndLog(_viewModel,
                "Pruner Step 2 : Writing to CSV and Clearing Variables",
                "Pruner Step 2:  Writing to CSV and Clearing Variables");

            string timestamp = DateTime.Now.ToString("MMddyy_hhmmss");
            string tmpPrintFilePath = MainWindowViewModel.InstallDirectory;

            WriteToCSV(prunedData, $"Pruned_Import_Data_Sheet_{timestamp}");

            String message = @"Scrubbing Completed." + Environment.NewLine +
                             "Files Can Be found here:" + Environment.NewLine + Environment.NewLine +
                             $"{tmpPrintFilePath}Pruned_Import_Data_Sheet_{timestamp}";

            MessageBox.Show(message);

            _viewModel.AddLogMessage(message);

            await Task.Delay(TimeSpan.FromMilliseconds(0.5));

            //Clear Data: 
            DataHelper.ClearProductDataList(_viewModel.CurrentMasterProducts);
            DataHelper.ClearProductDataList(_viewModel.ImportFullData);
            DataHelper.ClearProductDataList(_viewModel.ImportNativeData);
            DataHelper.ClearProductDataList(_viewModel.ImportAliasFoundData);
            DataHelper.ClearProductDataList(_viewModel.ImportNAData);
            DataHelper.ClearProductDataList(_viewModel.Import_NEEDS_REVIEW);
            DataHelper.ClearProductDataList(_viewModel.ImportExpandedUPCData);
            DataHelper.ClearProductAliasList(_viewModel.ImportMasterProducts);
            DataHelper.ClearProductAliasList(_viewModel.CurrentMasterAliases);
            DataHelper.ClearProductDataList(prunedData);
            
            await LogHelper.UpdateStatusAndLog(_viewModel,
                "Pruner Completed ",
                "Pruner Completed ");
        }

        /// <summary>
        /// DuplicateHunter
        /// </summary>
        /// <param name="_viewModel"></param>
        public async Task DuplicateHunter(MainWindowViewModel _viewModel)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(0.5));

            await LogHelper.UpdateStatusAndLog(_viewModel,
                "Duplicate Hunter Initialized ",
                "Duplicate Hunter Initialized ");


            List<clsProductData> NonDuplicateData = new List<clsProductData>();
            List<clsProductData> DuplicateData = new List<clsProductData>();
            HashSet<string> uniqueUPCs = new HashSet<string>();

            foreach (var item in _viewModel.ImportNativeData.ToList())
            {
                if (string.IsNullOrEmpty(item.upc))
                {
                    //Skip Processing
                    continue;
                }

                if (uniqueUPCs.Contains(item.upc))
                {
                    DuplicateData.Add(item);
                }
                else
                {
                    NonDuplicateData.Add(item);
                    uniqueUPCs.Add(item.upc);
                }
            }

            //Print Results and update Log

            string timestamp = DateTime.Now.ToString("MMddyy_hhmmss");
            string tmpPrintFilePath = MainWindowViewModel.InstallDirectory;

            WriteToCSV(DuplicateData, $"Duplicate_Data_Found_{timestamp}");
            WriteToCSV(NonDuplicateData, $"NonDuplicate_Data_Found_{timestamp}");

            String message = @"Duplicate Hunter Complete." + Environment.NewLine +
                             "Files Can Be found here:" + Environment.NewLine + Environment.NewLine +
                             $"{tmpPrintFilePath}_Duplicate_Data_Found){timestamp}" + Environment.NewLine +
                             $"{tmpPrintFilePath}_NonDuplicate_Data_Found{timestamp}" + Environment.NewLine;

            MessageBox.Show(message);

            _viewModel.AddLogMessage(message);

            await Task.Delay(TimeSpan.FromMilliseconds(0.5));

            await LogHelper.UpdateStatusAndLog(_viewModel,
                "Duplicate Hunter Step 2, Clearing Tmp Variable ",
                "Duplicate Hunter Step 2, Clearing Tmp Variable");

            //Clear Data: 
            DataHelper.ClearProductDataList(_viewModel.CurrentMasterProducts);
            DataHelper.ClearProductDataList(_viewModel.ImportFullData);
            DataHelper.ClearProductDataList(_viewModel.ImportNativeData);
            DataHelper.ClearProductDataList(_viewModel.ImportAliasFoundData);
            DataHelper.ClearProductDataList(_viewModel.ImportNAData);
            DataHelper.ClearProductDataList(_viewModel.Import_NEEDS_REVIEW);
            DataHelper.ClearProductDataList(_viewModel.ImportExpandedUPCData);
            DataHelper.ClearProductAliasList(_viewModel.ImportMasterProducts);
            DataHelper.ClearProductAliasList(_viewModel.CurrentMasterAliases);
            DataHelper.ClearProductDataList(DuplicateData);
            DataHelper.ClearProductDataList(NonDuplicateData);
            
            await LogHelper.UpdateStatusAndLog(_viewModel,
                "Duplicate Hunter Completed ",
                "Duplicate Hunter Completed ");
        }

        #endregion //Tab functions

        #endregion //Functions
    }
}