using DermsUI.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DermsUI.ViewModel
{
    public class AddNewGeneratorViewModel:BindableBase
    {
        private string description;
        private string name;
        private List<bool> bools;
        private bool selectedBool;
        private string customType;
        private double baseQ;
        private double maxQ;
        private double minQ;
        private double maxU;
        private double minU;
        private double condenserU;
        private string errorString;

        public AddNewGeneratorViewModel()
        {
            errorString = "";
            name = "";
            bools = new List<bool>();
            bools.Add(true);
            bools.Add(false);
            AddCommand = new MyICommand<string>(OnAddCommand);
        }

        public MyICommand<string> AddCommand { get; private set; }
        public string CustomType { get { return customType; } set { { SetProperty(ref customType, value); } } }
        public string Description { get { return description; } set { { SetProperty(ref description, value); } } }
        public string Name { get { return name; } set { { SetProperty(ref name, value); } } }
        public bool SelectedBool { get { return selectedBool; } set { { SetProperty(ref selectedBool, value); } } }
        public double BaseQ { get { return baseQ; } set { { SetProperty(ref baseQ, value); } } }
        public double MaxQ { get { return maxQ; } set { { SetProperty(ref maxQ, value); } } }
        public double MinQ { get { return minQ; } set { { SetProperty(ref minQ, value); } } }
        public double MaxU { get { return maxU; } set { { SetProperty(ref maxU, value); } } }
        public double MinU { get { return minU; } set { { SetProperty(ref minU, value); } } }
        public double CondenserU { get { return condenserU; } set { { SetProperty(ref condenserU, value); } } }
        public string ErrorString { get { return errorString; } set { { SetProperty(ref errorString, value); } } }
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
            if (description.Length != 0 && description.Trim(' ').Count() != 0 && customType.Length != 0 && customType.Trim(' ').Count() != 0 && baseQ>=0 &&
                maxQ >= 0 && minQ >= 0 && maxU >= 0 && minU >= 0 && condenserU >= 0)
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
