using DermsUI.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DermsUI.ViewModel
{
    public class AddNewDigitalSignalViewModel:BindableBase
    {
        private string description;
        private string measurementType;
        private bool currentState;
        private int maxValue;
        private int minValue;
        private int normalValue;
        private string errorString;
        private string name;
        private List<bool> bools;

        public AddNewDigitalSignalViewModel()
        {
            errorString = "";
            name = "";
            AddCommand = new MyICommand<string>(OnAddCommand);
            currentState = true;
            bools = new List<bool>();
            bools.Add(true);
            bools.Add(false);
        }

        public MyICommand<string> AddCommand { get; private set; }
        public string MeasurementType { get { return measurementType; } set { { SetProperty(ref measurementType, value); } } }
        public string Name { get { return name; } set { { SetProperty(ref name, value); } } }
        public string Description { get { return description; } set { { SetProperty(ref description, value); } } }
        public bool CurrentState { get { return currentState; } set { { SetProperty(ref currentState, value); } } }
        public int MaxValue { get { return maxValue; } set { { SetProperty(ref maxValue, value); } } }
        public int MinValue { get { return minValue; } set { { SetProperty(ref minValue, value); } } }
        public int NormalValue { get { return normalValue; } set { { SetProperty(ref normalValue, value); } } }
        public string ErrorString { get { return errorString; } set { { SetProperty(ref errorString, value); } } }
        public List<bool> Bools { get { return bools; } set { { SetProperty(ref bools, value); } } }

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
            if (description.Length != 0 && description.Trim(' ').Count() != 0 && MeasurementType.Length != 0 && MeasurementType.Trim(' ').Count() != 0 && MaxValue >= 0 && MaxValue >= 0 && MinValue >= 0 && MinValue >= 0 && NormalValue >= 0 && NormalValue >= 0)
            {
                double help1 = 0;
                double help2 = 0;
                double help3 = 0;
                double help4 = 0;
                double help5 = 0;
                try
                {
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
