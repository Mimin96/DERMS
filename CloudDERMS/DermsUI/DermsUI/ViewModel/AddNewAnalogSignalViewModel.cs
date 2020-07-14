using DermsUI.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DermsUI.ViewModel
{
    public class AddNewAnalogSignalViewModel:BindableBase
    {
        private string description;
        private string measurementType;
        private float currentValue;
        private float maxValue;
        private float minValue;
        private float normalValue;
        private string errorString;
        private string name;


        public AddNewAnalogSignalViewModel()
        {
            name = "";
            errorString = "";
            AddCommand = new MyICommand<string>(OnAddCommand);
        }

        public MyICommand<string> AddCommand { get; private set; }
        public string MeasurementType { get { return measurementType; } set { { SetProperty(ref measurementType, value); } } }
        public string Description { get { return description; } set { { SetProperty(ref description, value); } } }
        public float CurrentValue { get { return currentValue; } set { { SetProperty(ref currentValue, value); } } }
        public float MaxValue { get { return maxValue; } set { { SetProperty(ref maxValue, value); } } }
        public float MinValue { get { return minValue; } set { { SetProperty(ref minValue, value); } } }
        public float NormalValue { get { return normalValue; } set { { SetProperty(ref normalValue, value); } } }
        public string ErrorString { get { return errorString; } set { { SetProperty(ref errorString, value); } } }
        public string Name { get { return name; } set { { SetProperty(ref name, value); } } }

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
            if (description.Length != 0 && description.Trim(' ').Count() != 0 && MeasurementType.Length != 0 && MeasurementType.Trim(' ').Count() != 0 && CurrentValue >= 0 &&
                CurrentValue >= 0 && MaxValue >= 0 && MaxValue >= 0 && MinValue >= 0 && MinValue >= 0 && NormalValue >= 0 && NormalValue >= 0)
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
