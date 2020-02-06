using DermsUI.Resources;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DermsUI.ViewModel
{
    public class CimProfileCreatorViewModel : BindableBase
    {
        private string cimLocationString;
        private string cimNamespaceString;
        private string cimFileNameString;
        private string cimVersionString;
        private string logResultString;
        private string cimProductNameString;

        public MyICommand<string> SearchCommand { get; private set; }
        public MyICommand<string> LoadCommand { get; private set; }
        public MyICommand<string> GenerateCommand { get; private set; }

        private CimProfileCreator.ICIMProfileCreator cimProfileCreator;

        public CimProfileCreatorViewModel()
        {
            cimLocationString = "";
            cimNamespaceString = "DERMS";
            cimFileNameString = "DistributedEnergyManagementSystem";
            cimProductNameString = "Labs";
            cimVersionString = "1.0.0";

            SearchCommand = new MyICommand<string>(OnSearchCommand);
            LoadCommand = new MyICommand<string>(OnLoadCommand);
            GenerateCommand = new MyICommand<string>(OnGenerateCommand);
            cimProfileCreator = new CimProfileCreator.CIMProfileCreator();
        }

        public string CimLocationString { get { return cimLocationString; } set { SetProperty(ref cimLocationString, value); } }
        public string CimNamespaceString { get { return cimNamespaceString; } set { SetProperty(ref cimNamespaceString, value); } }
        public string CimFileNameString { get { return cimFileNameString; } set { SetProperty(ref cimFileNameString, value); } }
        public string CimVersionString { get { return cimVersionString; } set { SetProperty(ref cimVersionString, value); } }
        public string CimProductNameString { get { return cimProductNameString; } set { SetProperty(ref cimProductNameString, value); } }
        public string LogResultString { get { return logResultString; } set { SetProperty(ref logResultString, value); } }


        public void OnSearchCommand(string parameter)
        {
            System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
            fileDialog.Title = "Select CIM profile";
            fileDialog.InitialDirectory = @"c:/";
            fileDialog.Filter = "CIM-RDFS Files|*.rdfs;*.legacy-rdfs|All Files|*.*";
            fileDialog.RestoreDirectory = true;
            if (fileDialog.ShowDialog()== DialogResult.OK)
            {
                CimLocationString = fileDialog.FileName;
            }

        }

        public void OnLoadCommand(string parameter)
        {
            LogResultString = cimProfileCreator.LoadCIMRDFSFile(CimLocationString);
        }

        public void OnGenerateCommand(string parameter)
        {
            if (CimNamespaceString.Trim() != "" && CimFileNameString.Trim() != "" && CimVersionString.Trim() != "" && CimProductNameString.Trim() != "") 
            LogResultString = cimProfileCreator.GenerateDLL(CimFileNameString, CimVersionString, CimProductNameString, CimNamespaceString);
            else
                LogResultString = "ERROR >> None of the values can be left out.";
        }
    }
}
