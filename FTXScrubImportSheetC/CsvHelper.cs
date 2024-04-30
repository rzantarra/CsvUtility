using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace FTXScrubImportSheetC
{
    public class CsvHelper
    {

        #region Constructor

        public CsvHelper(string installDirectory, string csvColumnHeaders, string printFilePath)
        {
            InstallDirectory = installDirectory;
            CSVColumnHeaders = csvColumnHeaders;
            PrintFilePath = printFilePath;
        }


        #endregion
        #region Variables
        public string InstallDirectory { get; private set; }
        public string CSVColumnHeaders { get; private set; }
        public string PrintFilePath { get; private set; }
        #endregion

        #region Functions

         

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

        #endregion


    }
}