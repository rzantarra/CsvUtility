using System;
using System.Runtime.Remoting.Channels;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace FTXScrubImportSheetC
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;
        private CsvHelper _csvHelper;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainWindowViewModel();
            _csvHelper = new CsvHelper(MainWindowViewModel.InstallDirectory, MainWindowViewModel.CSVColumnHeaders);
            DataContext = _viewModel;
        }


        #region Variables

        private string masterProductsFilePath;

        public string MasterProductsFilePath
        {
            get { return masterProductsFilePath; }

            set { masterProductsFilePath = value; }
        }

        private string aliasProductsFilePath;

        public string AliasProdctsFilePath
        {
            get { return aliasProductsFilePath; }
            set { aliasProductsFilePath = value; }
        }

        private string importSheetFilePath;

        public string ImportSheetFilePath
        {
            get { return importSheetFilePath; }
            set { importSheetFilePath = value; }
        }

        #endregion

        #region Events

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the main window
            this.Close();
        }

        private void cmdBrowseProductFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV Files (*.csv)|*.csv";
            if (openFileDialog.ShowDialog() == true)
            {
                _viewModel.ProductsFilePath = openFileDialog.FileName;
                if (txtProductsFile != null) txtProductsFile.Text = openFileDialog.FileName;
                MasterProductsFilePath = openFileDialog.FileName;
            }
        }

        private void cmdBrowseAliasFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV Files (*.csv)|*.csv";
            if (openFileDialog.ShowDialog() == true)
            {
                MainWindowViewModel viewModel = (MainWindowViewModel)DataContext;
                viewModel.AliasFilePath = openFileDialog.FileName;
                AliasProdctsFilePath = openFileDialog.FileName;
            }
        }

        private void cmdBrowseImportSheetFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV Files (*.csv)|*.csv";
            if (openFileDialog.ShowDialog() == true)
            {
                MainWindowViewModel viewModel = (MainWindowViewModel)DataContext;
                viewModel.ImportSheetFilePath = openFileDialog.FileName;
                ImportSheetFilePath = openFileDialog.FileName;
            }
        }


        private bool OKToContinue_ScrubImport()
        {
            try
            {
                if (string.IsNullOrEmpty(txtProductsFile.Text)) throw new Exception("Invalid Products File");
                if (string.IsNullOrEmpty(txtAliasFile.Text)) throw new Exception("Invalid Alias File");
                if (string.IsNullOrEmpty(txtImportSheetFile.Text)) throw new Exception("Invalid Import Sheet File");

                string tmpFileCheck = "";
                tmpFileCheck = txtProductsFile.Text;
                if (!System.IO.File.Exists(tmpFileCheck)) throw new Exception("Invalid Products File");
                tmpFileCheck = txtAliasFile.Text;
                if (!System.IO.File.Exists(tmpFileCheck)) throw new Exception("Invalid Alias File");
                tmpFileCheck = txtImportSheetFile.Text;
                if (!System.IO.File.Exists(tmpFileCheck)) throw new Exception("Invalid Import Sheet File");

                int tmpNumChecked = 0;
                if (CKUpdateCategories.IsChecked == true) tmpNumChecked++;
                if (CKUpdateDept.IsChecked == true) tmpNumChecked++;
                if (CKUpdateDescriptions.IsChecked == true) tmpNumChecked++;
                if (CKUpdateManufBrand.IsChecked == true) tmpNumChecked++;
                if (CKExpandUPC.IsChecked == true) tmpNumChecked++;
                if (tmpNumChecked == 0) throw new Exception("No Options Chosen");

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Validating: " + ex.Message, "Validation Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }
        
        private bool OKToContinue_PrunerImport()
        {
            try
            {
                if (string.IsNullOrEmpty(txtProductsFile.Text)) throw new Exception("Invalid Products File");
                if (string.IsNullOrEmpty(txtAliasFile.Text)) throw new Exception("Invalid Alias File");
                if (string.IsNullOrEmpty(txtImportSheetFile.Text)) throw new Exception("Invalid Import Sheet File");

                string tmpFileCheck = "";
                tmpFileCheck = txtProductsFile.Text;
                if (!System.IO.File.Exists(tmpFileCheck)) throw new Exception("Invalid Products File");
                tmpFileCheck = txtAliasFile.Text;
                if (!System.IO.File.Exists(tmpFileCheck)) throw new Exception("Invalid Alias File");
                tmpFileCheck = txtImportSheetFile.Text;
                if (!System.IO.File.Exists(tmpFileCheck)) throw new Exception("Invalid Import Sheet File");

                int tmpNumChecked = 0;
                if (CKUpdateCategories.IsChecked == true) tmpNumChecked++;
                if (CKUpdateDept.IsChecked == true) tmpNumChecked++;
                if (CKUpdateDescriptions.IsChecked == true) tmpNumChecked++;
                if (CKUpdateManufBrand.IsChecked == true) tmpNumChecked++;
                if (CKExpandUPC.IsChecked == true) tmpNumChecked++;
                if (tmpNumChecked == 0) throw new Exception("No Options Chosen");

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Validating: " + ex.Message, "Validation Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }

        private async void btnGo_Click(object sender, RoutedEventArgs e)
        {
            TabItem selectedTab = tabControl.SelectedItem as TabItem;

            if (selectedTab != null)
            {
                if (selectedTab.Name == "Scrub_Importer") 
                {
                    if (OKToContinue_ScrubImport() &&
                        await CsvHelper.LoadMasterProducts(MasterProductsFilePath, _viewModel) &&
                        await CsvHelper.LoadMasterAliases(AliasProdctsFilePath, _viewModel) &&
                        await CsvHelper.LoadImportSheetProducts(ImportSheetFilePath, _viewModel))
                    {
                       // _viewModel.ExpandImportUPCProducts(_viewModel.ImportNativeData);
                    }

                    if (CKExpandUPC.IsChecked == true)
                    {
                        _csvHelper.ExpandUpcOnly(_viewModel);
                        return;
                    }
                    else
                    {
                        _csvHelper.ScrubImport(_viewModel);
                    }
                }
                else if (selectedTab.Name ==
                         "Pruner and Duplicate Hunter") // Replace "tab2" with the actual name of your tab
                {

                    OKToContinue_PrunerImport();
                }
            }
        }

        #endregion


        #region To Remove

        #endregion
    }
}