using System.Collections.Generic;
using FTXScrubImportSheetC;

namespace FTXScrubImportSheetC
{
    
    public class clsProductData
    {
        public string upc { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string department { get; set; }
        public string department_number { get; set; }
        public string category { get; set; }
        public string manufacturer { get; set; }
        public string brand { get; set; }
        public string is_active { get; set; }
        public string cost { get; set; }
        public string price { get; set; }
        public string vendor { get; set; }
        public string part_num { get; set; }
        public string part_num_units { get; set; }
        public string part_cost { get; set; }
        public string child_upc { get; set; }
        public string num_units { get; set; }
    }

    public class clsProductAlias
    {
        public string upc { get; set; }
        public string alias { get; set; }
    }
    
    

    
    
}
    public class DataHelper
    {
        public static void ClearProductDataList(List<clsProductData> List)
        {
            
            if (List != null)
            {
                List.Clear();
            }
        }
        
        public static void ClearProductAliasList(List<clsProductAlias> List)
        {
            if (List != null)
            {
                List.Clear();
            }
        }

}