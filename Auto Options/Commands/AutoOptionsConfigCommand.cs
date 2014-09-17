using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using DougKlassen.Revit.AutoOptions.ConfigRepo;
using DougKlassen.Revit.AutoOptions.DomainModels;
using DougKlassen.Revit.AutoOptions.Interface;
using DougKlassen.Revit.AutoOptions.StartUp;

namespace DougKlassen.Revit.AutoOptions.Commands
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class AutoOptionsConfigCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Application revitApp = uiApp.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document dbDoc = null;
            if (uiDoc!=null)
            {
                dbDoc = uiDoc.Document;
            }

            IAutoOptionsRepository settingsRepo = new AutoOptionsConfigFileRepo();
            AutoOptionsSettings aOSettings = settingsRepo.LoadAutoOptions();

            AutoOptionsConfigWindow configWindow = new AutoOptionsConfigWindow(aOSettings, commandData);
            Boolean? saveChanges = configWindow.ShowDialog();

            if (saveChanges.Value)
            {
                aOSettings.LastUpdate = DateTime.Now;
                settingsRepo.WriteAutoOptions(aOSettings);
                //Access the dispatcher and point it to the new settings
                AutoOptionsDispatcher dispatcher = AutoOptionsDispatcher.Instance;
                dispatcher.SetOptions(aOSettings);
            }

            return Result.Succeeded;
        }
    }

    public class AutoOptionsConfigCommandAvailability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            //Set the command to always be available, including in a zero doc state
            return true;
        }
    }
}
