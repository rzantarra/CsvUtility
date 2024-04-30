using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;

namespace FTXScrubImportSheetC
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _clipboardText;
        private CsvHelper _csvHelper;

        public ICommand CopyCommand { get; }
        public ICommand PasteCommand { get; }

        public MainWindowViewModel()
        {
            // Initialize 
            CurrentMasterProducts = new List<clsProductData>();
            CurrentMasterAliases = new List<clsProductAlias>();
            ImportMasterProducts = new List<clsProductAlias>();
            ImportNAData = new List<clsProductData>();
            ImportFullData = new List<clsProductData>();
            ImportNotFoundData = new List<clsProductData>();
            ImportNativeData = new List<clsProductData>();
            ImportExpandedUPCData = new List<clsProductData>();
            ImportAliasFoundData = new List<clsProductData>();
            CopyCommand = new RelayCommand(CopyToClipboard);
            PasteCommand = new RelayCommand(PasteFromClipboard);
            _csvHelper = new CsvHelper(InstallDirectory, CSVColumnHeaders);
            // Initialize LogHelper (assuming LogHelper has a static method for initialization)
            LogHelper.Initialize();
        }

        #region Variables

        public static string InstallDirectory { get; } = AppDomain.CurrentDomain.BaseDirectory;

        public const string CSVColumnHeaders =
            "upc,name,description,department,department_number,category,manufacturer,brand,is_active,cost,price,vendor,part_num,part_num_units,part_cost,child_upc,num_units";

        public List<clsProductData>
            ImportNativeData = new List<clsProductData>(); //this is the straight import data with NO modification

        public List<clsProductData>
            ImportExpandedUPCData = new List<clsProductData>(); // this will hold from an expanded UPC 

        public List<clsProductData> ImportNAData = new List<clsProductData>();
        public List<clsProductData> ImportFullData = new List<clsProductData>();
        public List<clsProductData> ImportNotFoundData = new List<clsProductData>();
        public List<clsProductData> ImportAliasFoundData = new List<clsProductData>();
        
        private ObservableCollection<string> logListBox = new ObservableCollection<string>();

        public ObservableCollection<string> LogListBox
        {
            get { return logListBox; }
            set
            {
                logListBox = value;
                OnPropertyChanged(nameof(LogListBox));
            }
        }

        #endregion Variables


        #region Properties

        public string ClipboardText
        {
            get { return _clipboardText; }
            set
            {
                if (_clipboardText != value)
                {
                    _clipboardText = value;
                    OnPropertyChanged(nameof(ClipboardText));
                }
            }
            //SetProperty(ref _clipboardText, value); }
        }

        private void CopyToClipboard()
        {
            Clipboard.SetText(ClipboardText);
        }

        private void PasteFromClipboard()
        {
            ClipboardText = Clipboard.GetText();
        }

        private string _updateStatusTxt;

        public string UpdateStatusTxt
        {
            get { return _updateStatusTxt; }
            set
            {
                if (_updateStatusTxt != value)
                {
                    _updateStatusTxt = value;
                    OnPropertyChanged();
                    // OnPropertyChanged(nameof(UpdateStatusTxt)); //notify the binding 
                }
            }
        }
        
        private List<clsProductAlias> _importMasterProducts;

        public List<clsProductAlias> ImportMasterProducts
        {
            get { return _importMasterProducts; }
            set
            {
                _importMasterProducts = value;
                OnPropertyChanged();
            }
        }

        private List<clsProductData> _currentMasterProducts;

        public List<clsProductData> CurrentMasterProducts
        {
            get { return _currentMasterProducts; }
            set
            {
                _currentMasterProducts = value;
                OnPropertyChanged();
            }
        }

        private List<clsProductAlias> _currentMasterAliases;

        public List<clsProductAlias> CurrentMasterAliases
        {
            get { return _currentMasterAliases; }
            set
            {
                _currentMasterAliases = value;
                OnPropertyChanged();
            }
        }
        
        private string _productsFilePath;

        public string ProductsFilePath
        {
            get { return _productsFilePath; }
            set
            {
                _productsFilePath = value;
                OnPropertyChanged();
            }
        }

        private string _aliasFilePath;

        public string AliasFilePath
        {
            get { return _aliasFilePath; }
            set
            {
                _aliasFilePath = value;
                OnPropertyChanged();
            }
        }

        private string _importSheetFilePath;

        public string ImportSheetFilePath
        {
            get { return _importSheetFilePath; }
            set
            {
                _importSheetFilePath = value;
                OnPropertyChanged();
            }
        }

        private bool _ckUpdateDescriptions;

        public bool CKUpdateDescriptions
        {
            get { return _ckUpdateDescriptions; }
            set
            {
                if (_ckUpdateDescriptions != value)
                {
                    _ckUpdateDescriptions = value;
                    OnPropertyChanged(nameof(CKUpdateDescriptions));
                }
            }
        }

        private bool _ckUpdateCategories;

        public bool CKUpdateCategories
        {
            get { return _ckUpdateCategories; }
            set
            {
                if (_ckUpdateCategories != value)
                {
                    _ckUpdateCategories = value;
                    OnPropertyChanged(nameof(CKUpdateCategories));
                }
            }
        }

        private bool _ckUpdateDept;

        public bool CKUpdateDept
        {
            get { return _ckUpdateDept; }
            set
            {
                if (_ckUpdateDept != value)
                {
                    _ckUpdateDept = value;
                    OnPropertyChanged(nameof(CKUpdateDept));
                }
            }
        }

        private bool _ckUpdateManufBrand;

        public bool CKUpdateManufBrand
        {
            get { return _ckUpdateManufBrand; }
            set
            {
                if (_ckUpdateManufBrand != value)
                {
                    _ckUpdateManufBrand = value;
                    OnPropertyChanged(nameof(CKUpdateManufBrand));
                }
            }
        }

        private bool _ckExpandUPC;

        public bool CKExpandUPC
        {
            get { return _ckExpandUPC; }
            set
            {
                if (_ckExpandUPC != value)
                {
                    _ckExpandUPC = value;
                    OnPropertyChanged(nameof(CKExpandUPC));
                    // Update the IsExpandUPCOnlyChecked property when CKExpandUPC is changed
                    IsExpandUPCOnlyChecked = value;
                }
            }
        }

        private bool _isExpandUPCOnlyChecked;

        public bool IsExpandUPCOnlyChecked
        {
            get { return _isExpandUPCOnlyChecked; }
            set
            {
                if (_isExpandUPCOnlyChecked != value)
                {
                    _isExpandUPCOnlyChecked = value;
                    OnPropertyChanged(nameof(IsExpandUPCOnlyChecked));

                    // If ExpandUPC Only is checked, uncheck and disable other checkboxes
                    if (_isExpandUPCOnlyChecked)
                    {
                        CKUpdateDescriptions = false;
                        CKUpdateCategories = false;
                        CKUpdateDept = false;
                        CKUpdateManufBrand = false;
                    }
                    else
                    {
                        // Re-enable and uncheck other checkboxes when "ExpandUPC Only" is unchecked
                        CKUpdateDescriptions = false;
                        CKUpdateCategories = false;
                        CKUpdateDept = false;
                        CKUpdateManufBrand = false;
                    }
                }
            }
        }

        private bool _isUpdateDescriptionsEnabled = true;

        public bool IsUpdateDescriptionsEnabled
        {
            get { return _isUpdateDescriptionsEnabled; }
            set
            {
                _isUpdateDescriptionsEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _isUpdateCategoriesEnabled;

        public bool IsUpdateCategoriesEnabled
        {
            get { return _isUpdateCategoriesEnabled; }
            set
            {
                _isUpdateCategoriesEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _isUpdateDepartmentsEnabled = true;

        public bool IsUpdateDepartmentsEnabled
        {
            get { return _isUpdateDepartmentsEnabled; }
            set
            {
                _isUpdateDepartmentsEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _isUpdateManufBrandEnabled = true;

        public bool IsUpdateManufBrandEnabled
        {
            get { return _isUpdateManufBrandEnabled; }
            set
            {
                _isUpdateManufBrandEnabled = value;
                OnPropertyChanged();
            }
        }


        private string _statusMessage;

        #endregion

        #region Commands

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Methods

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

        public List<clsProductData> ExpandImportUPCProducts(List<clsProductData> ImportNativeData)
        {
            foreach (clsProductData productData in ImportNativeData)
            {
                clsProductData expandedProductData = new clsProductData()
                {
                    upc = CsvHelper.ExpandUPC(productData.upc),
                    brand = productData.brand,
                    category = productData.category,
                    child_upc = productData.child_upc,
                    cost = productData.cost,
                    department = productData.cost,
                    department_number = productData.department_number,
                    description = productData.description,
                    is_active = productData.is_active,
                    manufacturer = productData.manufacturer,
                    name = productData.name,
                    num_units = productData.num_units,
                    part_cost = productData.part_cost,
                    part_num = productData.part_num,
                    part_num_units = productData.part_num_units,
                    price = productData.price,
                    vendor = productData.vendor
                };
                ImportExpandedUPCData.Add(expandedProductData);
            }

            return ImportExpandedUPCData;
        }

        public async Task ScrubImport(MainWindowViewModel _viewModel)
        {
            string tmpUpdateTxt;

            // Step 1: Wildcard search of ImportNativeData UPC field against CurrentMasterProducts
            _viewModel.UpdateStatusTxt = $"Scrub Step 1: Wildcard Search of Import Data Against Master Products";
            await Task.Delay(TimeSpan.FromMilliseconds(0.5));
            _viewModel.AddLogMessage(tmpUpdateTxt = "Scrub Step 1: Wildcard Search of Import Data...");
            await Task.Delay(TimeSpan.FromMilliseconds(0.5));

            foreach (var importData in ImportNativeData)
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
                var matchingUPCs = CurrentMasterProducts.Where(p => Regex.IsMatch(p.upc, pattern))
                    .Select(p => p.upc)
                    .ToList();

                if (matchingUPCs.Any())
                {
                    // Add all variables of matching UPCs to ImportNAData
                    ImportNAData.AddRange(CurrentMasterProducts.Where(p => matchingUPCs.Contains(p.upc)));
                }
                else
                {
                    // Add all variables of non-matching UPCs to ImportNotFoundData
                    ImportNotFoundData.Add(importData);
                }
            }

            // Step 2: Search CurrentMasterProducts against ImportExpandedUPCData
            _viewModel.UpdateStatusTxt = $"Scrub Step 2: Compare Master Products against Import Expanded UPC";
            await Task.Delay(TimeSpan.FromMilliseconds(0.5));
            _viewModel.AddLogMessage(tmpUpdateTxt = "Scrub Step 2: Compare Mster Products against Expanded UPC...");
            await Task.Delay(TimeSpan.FromMilliseconds(0.5));

            var expandedUPCs = ImportExpandedUPCData.Select(p => p.upc).ToList();
            var matchedUPCs = CurrentMasterProducts.Where(p => expandedUPCs.Contains(p.upc)).Select(p => p.upc)
                .ToList();

            foreach (var matchUPC in matchedUPCs)
            {
                var matchedProducts = CurrentMasterProducts.Where(p => p.upc == matchUPC).ToList();

                foreach (var product in matchedProducts)
                {
                    var updatedProduct = new clsProductData
                    {
                        upc = product.upc,
                        category = CKUpdateCategories
                            ? product.category
                            : ImportFullData.FirstOrDefault(d => d.upc == product.upc)?.category,
                        description = CKUpdateDescriptions
                            ? product.description
                            : ImportFullData.FirstOrDefault(d => d.upc == product.upc)?.description,
                        department = CKUpdateDept
                            ? product.department
                            : ImportFullData.FirstOrDefault(d => d.upc == product.upc)?.department,
                        manufacturer = CKUpdateManufBrand
                            ? product.manufacturer
                            : ImportFullData.FirstOrDefault(d => d.upc == product.upc)?.manufacturer,
                        brand = CKUpdateManufBrand
                            ? product.brand
                            : ImportFullData.FirstOrDefault(d => d.upc == product.upc)?.brand,
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

                    ImportFullData.Add(updatedProduct);
                }

                ImportNAData.RemoveAll(p => p.upc == matchUPC);
            }

            // Step 3: Search CurrentMasterAlias for aliases in ImportNotFoundData
            _viewModel.UpdateStatusTxt = $"Scrub Step 3: ImportNotFound UPC search in Current Master Alias";
            await Task.Delay(TimeSpan.FromMilliseconds(0.5));
            _viewModel.AddLogMessage(tmpUpdateTxt = "Scrub Step : ImportNotFound UPC search...");
            await Task.Delay(TimeSpan.FromMilliseconds(0.5));

            foreach (var importData in ImportNotFoundData)
            {
                var originUpc = importData.upc;
                if (string.IsNullOrEmpty(originUpc))
                {
                    continue; // Skip if UPC is empty
                }

                var matchingAlias = CurrentMasterAliases.FirstOrDefault(p => p.upc == originUpc);
                if (matchingAlias != null)
                {
                    // Update UPC to Alias
                    importData.upc = matchingAlias.alias;

                    var matchingProduct = CurrentMasterProducts.FirstOrDefault(p => p.upc == importData.upc);

                    if (matchingProduct != null)
                    {
                        // Update other data based on flags
                        if (CKUpdateCategories)
                        {
                            importData.category = matchingProduct.category;
                        }

                        if (CKUpdateDescriptions)
                        {
                            importData.description = matchingProduct.description;
                        }

                        if (CKUpdateDept)
                        {
                            importData.department = matchingProduct.department;
                        }

                        if (CKUpdateManufBrand)
                        {
                            importData.manufacturer = matchingProduct.manufacturer;
                            importData.brand = matchingProduct.brand;
                        }

                        ImportFullData.Add(importData);
                        // Add to ImportAliasFoundData
                        ImportAliasFoundData.Add(importData);
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
            
            _csvHelper.WriteToCSV(ImportNAData, $"ImportNeedsAttentionFound_{timestamp}");
            _csvHelper.WriteToCSV(ImportNotFoundData, $"ImportNotFound_{timestamp}");
            _csvHelper.WriteToCSV(ImportFullData, $"ImportFullFound_{timestamp}");
            _csvHelper.WriteToCSV(ImportAliasFoundData, $"ImportAliasFoundData_{timestamp}");

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
            DataHelper.ClearProductDataList(CurrentMasterProducts);
            DataHelper.ClearProductDataList(ImportFullData);
            DataHelper.ClearProductDataList(ImportNativeData);
            DataHelper.ClearProductDataList(ImportAliasFoundData);
            DataHelper.ClearProductDataList(ImportNAData);
            DataHelper.ClearProductDataList(ImportNotFoundData);
            DataHelper.ClearProductDataList(ImportExpandedUPCData);
            DataHelper.ClearProductAliasList(ImportMasterProducts);
            DataHelper.ClearProductAliasList(CurrentMasterAliases);

            Console.WriteLine("testingfor Clearing of Variables");

            _viewModel.UpdateStatusTxt = $"Idle...";
            await Task.Delay(TimeSpan.FromMilliseconds(0.5));
            
            _viewModel.AddLogMessage(tmpUpdateTxt = "Scrubbing Complete");
            await Task.Delay(TimeSpan.FromMilliseconds(0.5));
        }

        public async void AddLogMessage(string message)
        {
            LogHelper.AddLogMessage(message);
            OnPropertyChanged(nameof(LogListBox));

            await Task.Delay(TimeSpan.FromMilliseconds(0.5));
        }
        
        #endregion
        

        #region TmpTrash
        //TODO Remove if nessessary 

        // public void ClearProductDataList(List<clsProductData> List)
        // {
        //     if (List != null)
        //     {
        //         List.Clear();
        //     }
        // }
        //
        // public void ClearProductAliasList(List<clsProductAlias> List)
        // {
        //     if (List != null)
        //     {
        //         List.Clear();
        //     }
        // }
        // public async void AddLogMessage(string message)
        // {
        //     LogListBox.Add(message);
        //     OnPropertyChanged(nameof(LogListBox));
        //
        //     await Task.Delay(TimeSpan.FromMilliseconds(0.5));
        // }


        
        //This was moved to CsvHelper Class 
        // public void WriteToCSV(List<clsProductData> data, string fileNamePrefix)
        // {
        //     try
        //     {
        //         // Create a file name using the specified prefix and the current date and time
        //         string fileName = $"{fileNamePrefix}.csv";
        //
        //         // Combine the directory path and file name to get the full file path
        //         string filePath = Path.Combine(InstallDirectory, fileName);
        //         PrintFilePath = filePath;
        //
        //         // Open the CSV file for writing
        //         using (StreamWriter sw = new StreamWriter(filePath))
        //         {
        //             // Write the column headers as the initial top line
        //             sw.WriteLine(CSVColumnHeaders);
        //
        //             // Write each string in the list to the CSV file
        //             foreach (clsProductData productData in data)
        //             {
        //                 // Format the product data as a CSV line
        //                 string line =
        //                     $"{EscapeCsvField(productData.upc)},{EscapeCsvField(productData.name)},{EscapeCsvField(productData.description)},{EscapeCsvField(productData.department_number)},{EscapeCsvField(productData.category)},{EscapeCsvField(productData.manufacturer)},{EscapeCsvField(productData.brand)},{EscapeCsvField(productData.is_active)},{EscapeCsvField(productData.cost)},{EscapeCsvField(productData.price)},{EscapeCsvField(productData.vendor)},{EscapeCsvField(productData.part_num)},{EscapeCsvField(productData.part_num_units)},{EscapeCsvField(productData.part_num_units)}{EscapeCsvField(productData.part_cost)},{EscapeCsvField(productData.child_upc)},{EscapeCsvField(productData.num_units)}";
        //                 sw.WriteLine(line);
        //             }
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         // Handle any exceptions that occur during writing
        //         Console.WriteLine("Error writing to CSV file: " + ex.Message);
        //         MessageBox.Show("Erorr Writing to CSV File: " + ex.Message + ex.StackTrace);
        //     }
        // }
        
        
        //Function was moved to CsvHelper.cs
        // private string EscapeCsvField(string field)
        // {
        //     // If the field contains double quotes, escape them by doubling them
        //     if (field.Contains("\""))
        //     {
        //         field = field.Replace("\"", "\"\"");
        //     }
        //
        //     // If the field contains commas or double quotes, enclose it in double quotes
        //     if (field.Contains(",") || field.Contains("\""))
        //     {
        //         field = $"\"{field}\"";
        //     }
        //
        //     return field;
        // }
        
        // public static string ExpandUPC(string UPC)
        // {
        //     int OddSum, EvenSum, TotalSum, CheckDigit;
        //     string Char1, Char2, Char3, Char4, Char5, Char6;
        //     string ReturnUPC, tmpUPC;
        //     // int something = 5;
        //     UPC = UPC.Trim();
        //     try
        //     {
        //         if (Int64.TryParse(UPC, out _) == false)
        //         {
        //             return UPC;
        //         }
        //
        //         UPC = UPC.Trim();
        //
        //         if (UPC.StartsWith("a"))
        //         {
        //             UPC = UPC.Substring(1);
        //         }
        //
        //         if (UPC.EndsWith("b"))
        //         {
        //             UPC = UPC.Substring(0, UPC.Length - 1);
        //         }
        //
        //         if (UPC.Length > 12)
        //         {
        //             return UPC;
        //         }
        //         else if (UPC.Length == 12)
        //         {
        //             return UPC;
        //         }
        //         else if (UPC.Length == 11)
        //         {
        //             UPC = UPC.Trim();
        //             OddSum = 0;
        //             for (int i = 0; i < UPC.Length; i += 2)
        //             {
        //                 OddSum += int.Parse(UPC[i].ToString());
        //             }
        //
        //             OddSum *= 3;
        //
        //             EvenSum = 0;
        //             for (int i = 1; i < UPC.Length; i += 2)
        //             {
        //                 EvenSum += int.Parse(UPC[i].ToString());
        //             }
        //
        //             TotalSum = EvenSum + OddSum;
        //
        //             CheckDigit = TotalSum % 10;
        //             if (CheckDigit != 0)
        //             {
        //                 CheckDigit = 10 - CheckDigit;
        //             }
        //
        //             UPC += CheckDigit;
        //             ReturnUPC = UPC;
        //         }
        //         else if (UPC.Length == 10)
        //         {
        //             UPC = "0" + UPC;
        //             UPC = UPC.Trim();
        //             OddSum = 0;
        //             for (int i = 0; i < UPC.Length; i += 2)
        //             {
        //                 OddSum += int.Parse(UPC[i].ToString());
        //             }
        //
        //             OddSum *= 3;
        //
        //             EvenSum = 0;
        //             for (int i = 1; i < UPC.Length; i += 2)
        //             {
        //                 EvenSum += int.Parse(UPC[i].ToString());
        //             }
        //
        //             TotalSum = EvenSum + OddSum;
        //
        //             CheckDigit = TotalSum % 10;
        //             if (CheckDigit != 0)
        //             {
        //                 CheckDigit = 10 - CheckDigit;
        //             }
        //
        //             UPC += CheckDigit;
        //             ReturnUPC = UPC;
        //         }
        //         else if (UPC.Length == 9)
        //         {
        //             UPC = "00" + UPC;
        //             UPC = UPC.Trim();
        //             OddSum = 0;
        //             for (int i = 0; i < UPC.Length; i += 2)
        //             {
        //                 OddSum += int.Parse(UPC[i].ToString());
        //             }
        //
        //             OddSum *= 3;
        //
        //             EvenSum = 0;
        //             for (int i = 1; i < UPC.Length; i += 2)
        //             {
        //                 EvenSum += int.Parse(UPC[i].ToString());
        //             }
        //
        //             TotalSum = EvenSum + OddSum;
        //
        //             CheckDigit = TotalSum % 10;
        //             if (CheckDigit != 0)
        //             {
        //                 CheckDigit = 10 - CheckDigit;
        //             }
        //
        //             UPC += CheckDigit;
        //             ReturnUPC = UPC;
        //         }
        //         else if (UPC.Length == 8)
        //         {
        //             if (UPC.StartsWith("0"))
        //             {
        //                 UPC = UPC.Substring(1, 6);
        //
        //                 tmpUPC = UPC;
        //                 Char1 = UPC[0].ToString();
        //                 Char2 = UPC[1].ToString();
        //                 Char3 = UPC[2].ToString();
        //                 Char4 = UPC[3].ToString();
        //                 Char5 = UPC[4].ToString();
        //                 Char6 = UPC[5].ToString();
        //
        //                 if (Char6 == "0")
        //                 {
        //                     UPC = "0" + Char1 + Char2 + "00000" + Char3 + Char4 + Char5;
        //                 }
        //                 else if (Char6 == "1")
        //                 {
        //                     UPC = "0" + Char1 + Char2 + "10000" + Char3 + Char4 + Char5;
        //                 }
        //                 else if (Char6 == "2")
        //                 {
        //                     UPC = "0" + Char1 + Char2 + "20000" + Char3 + Char4 + Char5;
        //                 }
        //                 else if (Char6 == "3")
        //                 {
        //                     UPC = "0" + Char1 + Char2 + Char3 + "00000" + Char4 + Char5;
        //                 }
        //                 else if (Char6 == "4")
        //                 {
        //                     UPC = "0" + Char1 + Char2 + Char3 + Char4 + "00000" + Char5;
        //                 }
        //                 else if (Char6 == "5")
        //                 {
        //                     UPC = "0" + Char1 + Char2 + Char3 + Char4 + Char5 + "00005";
        //                 }
        //                 else if (Char6 == "6")
        //                 {
        //                     UPC = "0" + Char1 + Char2 + Char3 + Char4 + Char5 + "00006";
        //                 }
        //                 else if (Char6 == "7")
        //                 {
        //                     UPC = "0" + Char1 + Char2 + Char3 + Char4 + Char5 + "00007";
        //                 }
        //                 else if (Char6 == "8")
        //                 {
        //                     UPC = "0" + Char1 + Char2 + Char3 + Char4 + Char5 + "00008";
        //                 }
        //                 else if (Char6 == "9")
        //                 {
        //                     UPC = "0" + Char1 + Char2 + Char3 + Char4 + Char5 + "00009";
        //                 }
        //                 else
        //                 {
        //                     UPC = "111111111111";
        //                 }
        //
        //                 OddSum = 0;
        //                 for (int i = 0; i < UPC.Length; i += 2)
        //                 {
        //                     OddSum += int.Parse(UPC[i].ToString());
        //                 }
        //
        //                 OddSum *= 3;
        //
        //                 EvenSum = 0;
        //                 for (int i = 1; i < UPC.Length; i += 2)
        //                 {
        //                     EvenSum += int.Parse(UPC[i].ToString());
        //                 }
        //
        //                 TotalSum = EvenSum + OddSum;
        //
        //                 CheckDigit = TotalSum % 10;
        //                 if (CheckDigit != 0)
        //                 {
        //                     CheckDigit = 10 - CheckDigit;
        //                 }
        //
        //                 UPC += CheckDigit;
        //                 ReturnUPC = UPC;
        //             }
        //             else
        //             {
        //                 ReturnUPC = UPC;
        //             }
        //         }
        //         else if (UPC.Length == 7)
        //         {
        //             tmpUPC = UPC;
        //             Char1 = UPC[0].ToString();
        //             Char2 = UPC[1].ToString();
        //             Char3 = UPC[2].ToString();
        //             Char4 = UPC[3].ToString();
        //             Char5 = UPC[4].ToString();
        //             Char6 = UPC[5].ToString();
        //
        //             if (Char6 == "0")
        //             {
        //                 UPC = "0" + Char1 + Char2 + "00000" + Char3 + Char4 + Char5;
        //             }
        //             else if (Char6 == "1")
        //             {
        //                 UPC = "0" + Char1 + Char2 + "10000" + Char3 + Char4 + Char5;
        //             }
        //             else if (Char6 == "2")
        //             {
        //                 UPC = "0" + Char1 + Char2 + "20000" + Char3 + Char4 + Char5;
        //             }
        //             else if (Char6 == "3")
        //             {
        //                 UPC = "0" + Char1 + Char2 + Char3 + "00000" + Char4 + Char5;
        //             }
        //             else if (Char6 == "4")
        //             {
        //                 UPC = "0" + Char1 + Char2 + Char3 + Char4 + "00000" + Char5;
        //             }
        //             else if (Char6 == "5")
        //             {
        //                 UPC = "0" + Char1 + Char2 + Char3 + Char4 + Char5 + "00005";
        //             }
        //             else if (Char6 == "6")
        //             {
        //                 UPC = "0" + Char1 + Char2 + Char3 + Char4 + Char5 + "00006";
        //             }
        //             else if (Char6 == "7")
        //             {
        //                 UPC = "0" + Char1 + Char2 + Char3 + Char4 + Char5 + "00007";
        //             }
        //             else if (Char6 == "8")
        //             {
        //                 UPC = "0" + Char1 + Char2 + Char3 + Char4 + Char5 + "00008";
        //             }
        //             else if (Char6 == "9")
        //             {
        //                 UPC = "0" + Char1 + Char2 + Char3 + Char4 + Char5 + "00009";
        //             }
        //             else
        //             {
        //                 UPC = "111111111111";
        //             }
        //
        //             OddSum = 0;
        //             for (int i = 0; i < UPC.Length; i += 2)
        //             {
        //                 OddSum += int.Parse(UPC[i].ToString());
        //             }
        //
        //             OddSum *= 3;
        //
        //             EvenSum = 0;
        //             for (int i = 1; i < UPC.Length; i += 2)
        //             {
        //                 EvenSum += int.Parse(UPC[i].ToString());
        //             }
        //
        //             TotalSum = EvenSum + OddSum;
        //
        //             CheckDigit = TotalSum % 10;
        //             if (CheckDigit != 0)
        //             {
        //                 CheckDigit = 10 - CheckDigit;
        //             }
        //
        //             UPC += CheckDigit;
        //             ReturnUPC = UPC;
        //         }
        //         else
        //         {
        //             ReturnUPC = UPC.PadLeft(12, '0');
        //         }
        //
        //         return ReturnUPC;
        //     }
        //     catch (Exception ex)
        //     {
        //         return "";
        //     }
        // }
        #endregion
    }
}