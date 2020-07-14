using DermsUI.Resources;
using FTN.Common;
using FTN.ESI.SIMES.CIM.CIMAdapter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace DermsUI.ViewModel
{
    public class CreateApplyDeltaViewModel:BindableBase
    {
        private List<FTN.ESI.SIMES.CIM.CIMAdapter.Manager.SupportedProfiles> supportedProfiles;
        private FTN.ESI.SIMES.CIM.CIMAdapter.Manager.SupportedProfiles selectedProfile;
        private string cimLocationString;
        private string logResultString;
        //private CIMAdapter adapter;
        private Delta nmsDelta;


        public CreateApplyDeltaViewModel()
        {
            nmsDelta = null;
            //adapter = new CIMAdapter();
            cimLocationString = "";
            SearchCommand = new MyICommand<string>(OnSearchCommand);
            ConvertDeltaCommand = new MyICommand<string>(OnConvertCommand);
            ApplyDeltaCommand = new MyICommand<string>(OnSearchCommand);

            supportedProfiles = new List<FTN.ESI.SIMES.CIM.CIMAdapter.Manager.SupportedProfiles>();
            supportedProfiles.Add(FTN.ESI.SIMES.CIM.CIMAdapter.Manager.SupportedProfiles.PowerTransformer);
            supportedProfiles.Add(FTN.ESI.SIMES.CIM.CIMAdapter.Manager.SupportedProfiles.OverheadLines);
            supportedProfiles.Add(FTN.ESI.SIMES.CIM.CIMAdapter.Manager.SupportedProfiles.ProtectionDevices);
            supportedProfiles.Add(FTN.ESI.SIMES.CIM.CIMAdapter.Manager.SupportedProfiles.SwitchingEquipment);
            supportedProfiles.Add(FTN.ESI.SIMES.CIM.CIMAdapter.Manager.SupportedProfiles.UndergroundCables);
            supportedProfiles.Add(FTN.ESI.SIMES.CIM.CIMAdapter.Manager.SupportedProfiles.VoltageRegulator);
            selectedProfile = FTN.ESI.SIMES.CIM.CIMAdapter.Manager.SupportedProfiles.PowerTransformer;
        }

        public MyICommand<string> SearchCommand { get; private set; }
        public MyICommand<string> ConvertDeltaCommand { get; private set; }
        public MyICommand<string> ApplyDeltaCommand { get; private set; }

        public List<FTN.ESI.SIMES.CIM.CIMAdapter.Manager.SupportedProfiles> SupportedProfiles { get { return supportedProfiles; } set { supportedProfiles = value; } }
        public FTN.ESI.SIMES.CIM.CIMAdapter.Manager.SupportedProfiles SelectedProfile { get { return selectedProfile; } set { { SetProperty(ref selectedProfile, value); } } }
        public string CimLocationString { get { return cimLocationString; } set { { SetProperty(ref cimLocationString, value); } } }
        public string LogResultString { get { return logResultString; } set { { SetProperty(ref logResultString, value); } } }

        public void OnSearchCommand(string parameter)
        {
            System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
            fileDialog.Title = "Select CIM profile";
            fileDialog.InitialDirectory = @"c:/";
            fileDialog.Filter = "CIM-RDFS Files|*.rdfs;*.legacy-rdfs|All Files|*.*";
            fileDialog.RestoreDirectory = true;
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                CimLocationString = fileDialog.FileName;
            }

        }

        public void OnConvertCommand(string parameter)
        {
            ////SEND CIM/XML to ADAPTER
            try
            {
                if (cimLocationString == string.Empty)
                {
                    MessageBox.Show("Must enter CIM/XML file.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string log="";
                nmsDelta = null;
                using (FileStream fs = File.Open(cimLocationString, FileMode.Open))
                {
                    //nmsDelta = adapter.CreateDelta(fs, selectedProfile, out log);
                    LogResultString = log;
                }
                if (nmsDelta != null)
                {
                    //// export delta to file
                    using (XmlTextWriter xmlWriter = new XmlTextWriter(".\\deltaExport.xml", Encoding.UTF8))
                    {
                        xmlWriter.Formatting = Formatting.Indented;
                        nmsDelta.ExportToXml(xmlWriter);
                        xmlWriter.Flush();
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("An error occurred.\n\n{0}", e.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
