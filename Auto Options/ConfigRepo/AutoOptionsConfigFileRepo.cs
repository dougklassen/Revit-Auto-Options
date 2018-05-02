using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using Autodesk.Revit.UI;

using DougKlassen.Revit.AutoOptions.DomainModels;
using DougKlassen.Revit.AutoOptions.StartUp;

namespace DougKlassen.Revit.AutoOptions.ConfigRepo
{
    public class AutoOptionsConfigFileRepo : IAutoOptionsRepository
    {
        private static String configFileName = "AutoOptions.ini";
        private static String configFilePath = FileLocations.AddInDirectory + configFileName;

        public void WriteAutoOptions(AutoOptionsSettings aOListParam)
        {
            XmlSerializer xmlFormat = new XmlSerializer(typeof(AutoOptionsSettings), new Type[] {typeof(AutoOptionsResolution), typeof(RevitResolution)});
            if (!Directory.Exists(FileLocations.AddInDirectory))
            {
                Directory.CreateDirectory(FileLocations.AddInDirectory);
            }

            //todo: confirm overwrite of file
            using (Stream fstream = new FileStream(configFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                xmlFormat.Serialize(fstream, aOListParam);
            }
        }

        public AutoOptionsSettings LoadAutoOptions()
        {
            AutoOptionsSettings aOptList;

            if (File.Exists(configFilePath))
            {
                try
                {
                    XmlSerializer xmlFormat = new XmlSerializer(typeof(AutoOptionsSettings), new Type[] { typeof(AutoOptionsResolution), typeof(RevitResolution) });

                    using (Stream fStream = new FileStream(configFilePath, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        aOptList = (AutoOptionsSettings)xmlFormat.Deserialize(fStream);
                    }
                }
                catch (Exception e)
                {
                    //if the file can't be deserialized, create a new AutoOptionsList
                    TaskDialog.Show("AutoOptions", "Couldn't read " + configFilePath + "\nRecreating file\n\n" + e.Message);
                    aOptList = FailuresInfo.AsAutoOptionsSettings();
                }
            }
            else
            {
                //if the file isn't found, create a new AutoOptionsList
                aOptList = FailuresInfo.AsAutoOptionsSettings();
                //also create the file
                WriteAutoOptions(aOptList);
            }

            return aOptList;
        }
    }
}
