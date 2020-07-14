using DermsUI.MediatorPattern;
using DermsUI.Model;
using DermsUI.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DermsUI.ViewModel
{
    public class AddNewTerminalViewModel:BindableBase
    {
        private string descriptionString;
        private string seqNumberString;
        private bool connectedBoolselected;
        private List<bool> connectedBoolList;
        private TerminalHelper terminal;
        private string errorString;
        private string name;

        public MyICommand<string> AddCommand { get; private set; }

        public AddNewTerminalViewModel()
        {
           

            name = "";
            connectedBoolList = new List<bool>();
            connectedBoolList.Add(true);
            connectedBoolList.Add(false);
            connectedBoolselected = false;
            terminal = new TerminalHelper();

            AddCommand = new MyICommand<string>(OnAddCommand);
        }

       

        public string DescriptionString { get { return descriptionString; } set { { SetProperty(ref descriptionString, value); } } }
        public string Name { get { return name; } set { { SetProperty(ref name, value); } } }
        public string SeqNumberString { get { return seqNumberString; } set { { SetProperty(ref seqNumberString, value); } } }
        public string ErrorString { get { return errorString; } set { { SetProperty(ref errorString, value); } } }
        public bool ConnectedBoolselected { get { return connectedBoolselected; } set { connectedBoolselected = value; } }
        public List<bool> ConnectedBoolList { get { return connectedBoolList; } set { connectedBoolList = value; } }

        public void OnAddCommand(string parameter)
        {
            if (Validate())
            {
                terminal.Connected = connectedBoolselected;
                terminal.Description = descriptionString;
                terminal.SeqNumber = Int32.Parse(seqNumberString);
                //SEND NEW TERMINAL FUNCTION
            }
            else
            {
                ErrorString = "Sva polja moraju biti popunjena!";
            }
        }

        public bool Validate()
        {
            ErrorString = "";
            if (descriptionString.Length != 0 && descriptionString.Trim(' ').Count()!=0 && seqNumberString.Length!=0 && seqNumberString.Trim(' ').Count() !=0)
            {
                int pom = 0;
                try {
                    pom = Int32.Parse(seqNumberString);
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
