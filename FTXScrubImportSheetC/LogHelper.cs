using System.Collections.ObjectModel;

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
    }
}