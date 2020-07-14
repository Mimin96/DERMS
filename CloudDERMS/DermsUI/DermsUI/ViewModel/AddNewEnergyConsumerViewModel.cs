using DermsUI.Model;
using DermsUI.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DermsUI.ViewModel
{
    public class AddNewEnergyConsumerViewModel:BindableBase
    {
        private string customType;
        private string description;
        private string consumerCountsString;
        private List<bool> bools;
        private bool selectedBool;
        private string errorString;
        private string name;
        private EnergyConsumerHelper energyConsumer;


        public AddNewEnergyConsumerViewModel()
        {
            customType = "";
            description = "";
            name = "";
            consumerCountsString = "";
            errorString = "";
            bools = new List<bool>();
            bools.Add(true);
            bools.Add(false);
            selectedBool = true;
            energyConsumer = new EnergyConsumerHelper();

            AddCommand = new MyICommand<string>(OnAddCommand);
        }

        public MyICommand<string> AddCommand { get; private set; }
        public string CustomType { get { return customType; } set { { SetProperty(ref customType, value); } } }
        public string Name { get { return name; } set { { SetProperty(ref name, value); } } }
        public string Description { get { return description; } set { { SetProperty(ref description, value); } } }
        public string ConsumerCount { get { return consumerCountsString; } set { { SetProperty(ref consumerCountsString, value); } } }
        public string ErrorString { get { return errorString; } set { { SetProperty(ref errorString, value); } } }
        public bool SelectedBool { get { return selectedBool; } set { selectedBool = value; } }
        public List<bool> Bools { get { return bools; } set { bools = value; } }

        public void OnAddCommand(string parameter)
        {
            if (Validate())
            {
                energyConsumer.ConsumerCount = Int32.Parse(consumerCountsString);
                energyConsumer.CustomType = customType;
                energyConsumer.Description = Description;
                energyConsumer.PrivacyState = selectedBool;
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
                && consumerCountsString.Length!=0 && consumerCountsString.Trim(' ').Count()!=0)
            {
                int pom = 0;
                try
                {
                    pom = Int32.Parse(consumerCountsString);
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
