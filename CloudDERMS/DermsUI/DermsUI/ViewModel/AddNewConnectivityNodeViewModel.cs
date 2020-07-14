using DermsUI.Model;
using DermsUI.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DermsUI.ViewModel
{
    public class AddNewConnectivityNodeViewModel:BindableBase
    {
        private string descriptionString;
        private string errorString;
        private string name;
        private ConnectivityNodeHelper connectivityNode;

        public AddNewConnectivityNodeViewModel()
        {
            descriptionString = "";
            errorString = "";
            name = "";
            AddCommand = new MyICommand<string>(OnAddCommand);
            connectivityNode = new ConnectivityNodeHelper();
        }

        public string DescriptionString { get { return descriptionString; } set { { SetProperty(ref descriptionString, value); } } }
        public string ErrorString { get { return errorString; } set { { SetProperty(ref errorString, value); } } }
        public string Name { get { return name; } set { { SetProperty(ref name, value); } } }
        public ConnectivityNodeHelper ConnectivityNode { get { return connectivityNode; } set { connectivityNode = value; } }
        public MyICommand<string> AddCommand { get; private set; }

        public void OnAddCommand(string parameter)
        {
            if (Validate())
            {
                connectivityNode.Description = DescriptionString;
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
