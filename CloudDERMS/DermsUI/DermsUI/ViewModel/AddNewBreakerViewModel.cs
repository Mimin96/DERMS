using DermsUI.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DermsUI.ViewModel
{
    public class AddNewBreakerViewModel:BindableBase
    {
        private string description;
        private string name;
        private string customType;
        private List<bool> bools;
        private bool selectedBool;
        private string feederID1;
        private string feederID2;
        private bool normalOpen;
        private string errorString;

        public AddNewBreakerViewModel()
        {
            name = "";
            selectedBool = false;
            normalOpen = false;
            errorString = "";
            AddCommand = new MyICommand<string>(OnAddCommand);
            bools = new List<bool>();
            bools.Add(true);
            bools.Add(false);
        }

        public MyICommand<string> AddCommand { get; private set; }
        public string CustomType { get { return customType; } set { { SetProperty(ref customType, value); } } }
        public string Name { get { return name; } set { { SetProperty(ref name, value); } } }
        public string Description { get { return description; } set { { SetProperty(ref description, value); } } }
        public bool SelectedBool { get { return selectedBool; } set { { SetProperty(ref selectedBool, value); } } }
        public string ErrorString { get { return errorString; } set { { SetProperty(ref errorString, value); } } }
        public string FeederID1 { get { return feederID1; } set { { SetProperty(ref feederID1, value); } } }
        public string FeederID2 { get { return feederID2; } set { { SetProperty(ref feederID2, value); } } }
        public bool NormalOpen { get { return normalOpen; } set { { SetProperty(ref normalOpen, value); } } }
        public List<bool> Bools { get { return bools; } set { bools = value; } }

        public void OnAddCommand(string parameter)
        {
            if (Validate())
            {
                //SEND NEW ENERGY CONSUMER FUNCTION
            }
            else
            {
                ErrorString = "Sva polja moraju biti popunjena!";
            }
        }

        public bool Validate()
        {
            ErrorString = "";
            if (description.Length != 0 && description.Trim(' ').Count() != 0 && customType.Length != 0 && customType.Trim(' ').Count() != 0
                && FeederID1.Length != 0 && FeederID1.Trim(' ').Count() != 0 && FeederID2.Length != 0 && FeederID2.Trim(' ').Count() != 0)
            {
                double help1 = 0;
                double help2 = 0;
                double help3 = 0;
                double help4 = 0;
                double help5 = 0;
                try
                {
                    /*help1 = Convert.ToDouble(MaxQ);
                    help2 = Convert.ToDouble(MinQ);
                    help3 = Convert.ToDouble(MaxU);
                    help4 = Convert.ToDouble(MinU);
                    help5 = Convert.ToDouble(CondenserU);*/
                }
                catch
                {
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
