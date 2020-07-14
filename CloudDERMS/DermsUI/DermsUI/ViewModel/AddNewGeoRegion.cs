using DermsUI.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DermsUI.ViewModel
{
    public class AddNewGeoRegion:BindableBase
    {
        private string descriptionString;
        private string errorString;
        private string name;

        public AddNewGeoRegion()
        {
            descriptionString = "";
            errorString = "";
            name = "";
            AddCommand = new MyICommand<string>(OnAddCommand);
        }

        public string DescriptionString { get { return descriptionString; } set { { SetProperty(ref descriptionString, value); } } }
        public string Name { get { return name; } set { { SetProperty(ref name, value); } } }
        public string ErrorString { get { return errorString; } set { { SetProperty(ref errorString, value); } } }
        public MyICommand<string> AddCommand { get; private set; }

        public void OnAddCommand(string parameter)
        {
            if (Validate())
            {
                //SEND NEW CONNECTIVITY NODE FUNCTION
            }
            else
            {
                ErrorString = "Sva polja moraju biti popunjena!";
            }
        }

        public bool Validate()
        {
            ErrorString = "";
            if (descriptionString.Length != 0 && descriptionString.Trim(' ').Count() != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
