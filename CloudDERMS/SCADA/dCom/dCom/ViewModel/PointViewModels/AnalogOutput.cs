using Common;
using System;

namespace dCom.ViewModel
{
    internal class AnalogOutput : AnalogBase
    {
        IConfigItem cc;
        public AnalogOutput(IConfigItem c, IFunctionExecutor commandExecutor, IStateUpdater stateUpdater, IConfiguration configuration, int i)
            : base(c, commandExecutor, stateUpdater, configuration, i)
        {
            cc = c;
        }

        protected override bool WriteCommand_CanExecute(object obj)
        {
            // ovde izmenio
            if (CommandedValue < cc.LowAlarm || CommandedValue > cc.HighAlarm)
                return false;

            return true;
        }
    }
}