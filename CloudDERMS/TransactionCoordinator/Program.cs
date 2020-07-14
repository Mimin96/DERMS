using FTN.Common;
using System;
using TransactionCoordinatorService;

namespace TransactionCoordinator
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceManager serviceManager = new ServiceManager();
            string message = "Press <Enter> to stop the service.";
            CommonTrace.WriteTrace(CommonTrace.TraceInfo, message);
            Console.WriteLine(message);
            Console.ReadLine();
        }
    }
}
