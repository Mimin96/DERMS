using DERMSCommon.TransactionManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace TransactionCoordinator
{
    public class TransactionCoordinator : ITransactionListing
    {
        private List<string> _activeServices;
        bool result = false;

        public TransactionCoordinator()
        {
            this._activeServices = new List<string>();
        }        

        public void Enlist(string adress)
        {
            _activeServices.Add(adress);
        }

        public void FinishList(bool IsSuccessfull)
        {
            if (IsSuccessfull)
            {
                bool prepareSuccessfull = true;
                List<string> failedServices = new List<string>();
                TranscationCoordinatorCheck proxy = new TranscationCoordinatorCheck("net.tcp://localhost:19506/ITransactionCheck");
                Console.WriteLine("Prepare in client: " + "net.tcp://localhost:19506/ITransactionCheck/NMS");
                result = proxy.Prepare();

                if (result == false)
                {
                    prepareSuccessfull = false;
                    failedServices.Add("net.tcp://localhost:19506/ITransactionCheck");
                }

                if (!prepareSuccessfull)
                {
                    Console.WriteLine(DateTime.Now + ": Distributed transaction failed. Roolback in progress.");
                    Rollback();
                }
                else
                {
                    Console.WriteLine(DateTime.Now + ": Distributed transaction successfull. Commit in progress.");
                    Commit();
                }
            } 
        }

        private void Rollback()
        {           
            TranscationCoordinatorCheck proxy = new TranscationCoordinatorCheck("net.tcp://localhost:19506/ITransactionCheck");
            proxy.Rollback();
            Console.WriteLine(DateTime.Now + ": Distributed transaction successfull. Rollback on NMS is done.");            
        }

        private void Commit()
        {
            TranscationCoordinatorCheck proxy = new TranscationCoordinatorCheck("net.tcp://localhost:19506/ITransactionCheck");
            proxy.Commit();
            Console.WriteLine(DateTime.Now + ": Distributed transaction successfull. Commit on NMS is done.");            
        }
    }
}
