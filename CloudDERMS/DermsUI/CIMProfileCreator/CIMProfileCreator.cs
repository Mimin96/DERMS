using FTN.ESI.SIMES.CIM.Core;
using FTN.ESI.SIMES.CIM.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CimProfileCreator
{
    public class CIMProfileCreator : ICIMProfileCreator
    {
        private Profile profile = null;
        private FileStream fs;

        public string GenerateDLL(string name, string version, string productName, string nameSpace)
        {
            try
            {
                if(profile == null)
                    return "ERROR >> Profile doesn't exist!";

                ProfileCreator pc = new ProfileCreator();
                StringBuilder sb = pc.CreateProfile(nameSpace, name, productName, version, profile);

                return sb.ToString();
            }
            catch (Exception e)
            {
                return "An error occurred.\n\n{0}"+e.Message;
            }
        }

        public string LoadCIMRDFSFile(string location)
        {
            try
            {
                profile = null;
                using (fs = File.Open(location, FileMode.Open))
                {
                    ProfileLoader rdfParser = new ProfileLoader();
                    profile = rdfParser.LoadProfileDocument(fs, location);
                }
            }
            catch (Exception e)
            {
                return "An error occurred.\n\n{0}"+e.Message;
            }
            return profile.ToString();
        }
    }
}
