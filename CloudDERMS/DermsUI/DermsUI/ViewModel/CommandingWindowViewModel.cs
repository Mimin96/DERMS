using DERMSCommon.SCADACommon;
using DermsUI.MediatorPattern;
using DermsUI.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DermsUI.ViewModel
{
    internal class CommandingWindowViewModel:BindableBase
    {
        private DataPoint item;
        public DataPoint Item { get { return item; } set { item = value; } }
        public short CommandedValue { get; set; }

        public CommandingWindowViewModel(DataPoint selectedItem)
        {
            Item = selectedItem;
        }

        private ICommand _writeCommand;
        public ICommand WriteCommand { get { return _writeCommand ?? (_writeCommand = new CommandHandler(() => Write(), () => CanExecute)); } }

        public void Write()
        {
            SCADACommanding commanding = new SCADACommanding(Item.Gid, (ushort)CommandedValue, Item.Type);

            Mediator.NotifyColleagues("SCADACommanding", commanding);
        }

        public bool CanExecute 
        { 
            get
            {
                if (Item.Type.Equals(PointType.ANALOG_INPUT) || Item.Type.Equals(PointType.ANALOG_OUTPUT))
                {
                    if(CommandedValue > 0)
                        return true;
                }
                else
                {
                    if (CommandedValue.Equals(1) || CommandedValue.Equals(0))
                        return true;
                }

                return false;
            } 
        }
    }
}
