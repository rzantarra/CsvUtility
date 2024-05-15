using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FTXScrubImportSheetC
{
    public class LogHelper
    {
        public static ObservableCollection<string> LogListBox { get; } = new ObservableCollection<string>();

        public static void Initialize()
        {
            
        }
        public static void AddLogMessage(string message)
        {
            LogListBox.Add(message);
        }
        
        public static async Task UpdateStatusAndLog(MainWindowViewModel viewModel, string statusText, string logMessage)
        {
            viewModel.UpdateStatusTxt = statusText;
            await Task.Delay(TimeSpan.FromMilliseconds(0.5));
            viewModel.AddLogMessage(logMessage);
            await Task.Delay(TimeSpan.FromMilliseconds(0.5));
        }
    }
}