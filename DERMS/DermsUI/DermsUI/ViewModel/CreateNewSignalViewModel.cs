using DermsUI.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace DermsUI.ViewModel
{
    public class CreateNewSignalViewModel:BindableBase
    {
        private List<FTN.ESI.SIMES.CIM.CIMAdapter.Manager.SupportedProfiles> types;
        private FTN.ESI.SIMES.CIM.CIMAdapter.Manager.SupportedProfiles selectedType;

        public CreateNewSignalViewModel()
        {
            types = new List<FTN.ESI.SIMES.CIM.CIMAdapter.Manager.SupportedProfiles>();
            types.Add(FTN.ESI.SIMES.CIM.CIMAdapter.Manager.SupportedProfiles.PowerTransformer);
            selectedType = FTN.ESI.SIMES.CIM.CIMAdapter.Manager.SupportedProfiles.PowerTransformer;

            CreateCommand = new MyICommand<string>(OnCreate);
        }

        public List<FTN.ESI.SIMES.CIM.CIMAdapter.Manager.SupportedProfiles> Types { get { return types; } set { types = value; } }
        public FTN.ESI.SIMES.CIM.CIMAdapter.Manager.SupportedProfiles SelectedType { get { return selectedType; } set { { SetProperty(ref selectedType, value); } } }
        public MyICommand<string> CreateCommand { get; private set; }

        public void OnCreate(string parameter)
        {
            switch (SelectedType)
            {

                case FTN.ESI.SIMES.CIM.CIMAdapter.Manager.SupportedProfiles.PowerTransformer:
                    break;
                case FTN.ESI.SIMES.CIM.CIMAdapter.Manager.SupportedProfiles.ProtectionDevices:
                    break;
                case FTN.ESI.SIMES.CIM.CIMAdapter.Manager.SupportedProfiles.SwitchingEquipment:
                    break;
                case FTN.ESI.SIMES.CIM.CIMAdapter.Manager.SupportedProfiles.UndergroundCables:
                    break;
                case FTN.ESI.SIMES.CIM.CIMAdapter.Manager.SupportedProfiles.VoltageRegulator:
                    break;
            }
        }


    }
}
