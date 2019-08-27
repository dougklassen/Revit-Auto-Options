using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Reflection;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using DougKlassen.Revit.AutoOptions.ConfigRepo;
using DougKlassen.Revit.AutoOptions.DomainModels;

namespace DougKlassen.Revit.AutoOptions.StartUp
{
    public static class FileLocations
    {
        public static readonly String AddInDirectory = @"C:\ProgramData\AutoDesk\Revit\Addins\2018\AutoOptions\";
        public static String AssemblyFullPath
        {
            get
            {
                return AddInDirectory + "AutoOptions.dll";
            }
        }
    }

    public class StartUpApp : IExternalApplication
    {
        AutoOptionsDispatcher dispatcher;

        Result IExternalApplication.OnStartup(UIControlledApplication application)
        {
            ////todo: this sets the ResourceAssembly of Revit itself, so it will break other addins that try to set or access the ResourceAssembly
            if (System.Windows.Application.ResourceAssembly == null)
            {
                System.Windows.Application.ResourceAssembly = Assembly.GetExecutingAssembly();
            }

            //setup the ui on the Ribbon
            PushButtonData AutoOptionsConfigCommandPushButtonData =
                new PushButtonData(
                    "AutoOptionsConfigCommandButton", //name of the button
                    "Auto Options", //text on the button
                    FileLocations.AssemblyFullPath,
                    "DougKlassen.Revit.AutoOptions.Commands.AutoOptionsConfigCommand"
                );
            AutoOptionsConfigCommandPushButtonData.AvailabilityClassName =
                "DougKlassen.Revit.AutoOptions.Commands.AutoOptionsConfigCommandAvailability";
            AutoOptionsConfigCommandPushButtonData.LargeImage =
                new BitmapImage(new Uri("pack://application:,,,/resources/ao_large.jpg"));

            RibbonPanel AutoOptionsRibbonPanel = application.CreateRibbonPanel("Auto Options");
            AutoOptionsRibbonPanel.AddItem(AutoOptionsConfigCommandPushButtonData);

            //assign the dispatcher object using the singleton instance. The singleton will be initialized at this time
            dispatcher = AutoOptionsDispatcher.Instance;
            //load the settings to use in this session
            IAutoOptionsRepository settingsRepo = new AutoOptionsConfigFileRepo();
            dispatcher.SetOptions(settingsRepo.LoadAutoOptions());
            //register the event handler
            application.ControlledApplication.FailuresProcessing += dispatcher.AutoOptionsFailureHandler;

            return Result.Succeeded;
        }

        Result IExternalApplication.OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
