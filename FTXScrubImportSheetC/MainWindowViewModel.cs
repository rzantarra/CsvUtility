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

        private bool _isUpdateDescriptionsEnabled;

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

        private bool _isUpdateDepartmentsEnabled;

        public bool IsUpdateDepartmentsEnabled
        {
            get { return _isUpdateDepartmentsEnabled; }
            set
            {
                _isUpdateDepartmentsEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _isUpdateManufBrandEnabled;

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


        public async void AddLogMessage(string message)
        {
            LogHelper.AddLogMessage(message);
            OnPropertyChanged(nameof(LogListBox));

            await Task.Delay(TimeSpan.FromMilliseconds(0.5));
        }

        #endregion


        #region TmpTrash

        //TODO Remove if nessessary 


        #endregion
    }
}