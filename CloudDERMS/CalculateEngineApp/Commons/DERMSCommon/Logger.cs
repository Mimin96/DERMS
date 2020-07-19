using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DERMSCommon
{
    public static class Logger
    {
        private static string path;

        static Logger()
        {
            path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, @"Loggs\DERMS_Log.txt");
        }

        public static void Log(string message,Enums.Component component, Enums.LogLevel logLevel)
        {
            try
            {
                if (!File.Exists(path))
                {
                    using (FileStream fs = File.Create(path))
                    {
                        fs.Close();
                    }
                }

                using (StreamWriter writter = File.AppendText(path))
                {
                    writter.Write(DateTime.Now + " << " + component.ToString() + " >> [" + logLevel.ToString() + "] -> " + message + "\r\n");
                    writter.Close();
                }
            }
            catch (Exception e)
            {

                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
