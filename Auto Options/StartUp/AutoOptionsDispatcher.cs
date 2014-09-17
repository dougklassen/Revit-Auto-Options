using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;

using DougKlassen.Revit.AutoOptions.ConfigRepo;
using DougKlassen.Revit.AutoOptions.DomainModels;
using DougKlassen.Revit.AutoOptions.Interface;

namespace DougKlassen.Revit.AutoOptions.StartUp
{
    public sealed class AutoOptionsDispatcher
    {
        //the currently applicable settings
        private static AutoOptionsSettings currentSettings;
        //The Revit UI for use in managing interface elements. This is not guaranteed to be set at any point
        private UIApplication uiApp = null;

        //the private Singleton instance and the public property
        private static readonly AutoOptionsDispatcher instance = new AutoOptionsDispatcher();
        public static AutoOptionsDispatcher Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// private constructor used only by the static field instance
        /// </summary>
        private AutoOptionsDispatcher() { }

        /// <summary>
        /// Sets the AutoOptionsSettings currently in use. The Dispatcher won't be properly initialized till this is called
        /// </summary>
        /// <param name="aOListParam"><The Settings to use/param>
        public void SetOptions(AutoOptionsSettings aOListParam)
        {
            currentSettings = aOListParam;
        }

        /// <summary>
        /// This is an event handler that responds to a failure according to the current settings
        /// </summary>
        /// <param name="sender">The sending object, i.e. the Revit application</param>
        /// <param name="e">The event arguments</param>
        public void AutoOptionsFailureHandler(Object sender, Autodesk.Revit.DB.Events.FailuresProcessingEventArgs e)
        {
            //don't process if handling is turned off
            if (currentSettings.HandlingActive == false)
            {
                return;
            }

            //update the uiApp reference
            Application RevitApp = sender as Application;
            uiApp = new UIApplication(RevitApp);

            FailuresAccessor fa = e.GetFailuresAccessor();
            //this is the action that will be taken to attempt resolution. Default is to continue the default Revit failure processing
            FailureProcessingResult action = FailureProcessingResult.Continue;

            foreach (FailureMessageAccessor fma in fa.GetFailureMessages())
            {
                AutoFailureHandlingOptions aFOpts = currentSettings.AllFailureOptions
                    .Where(x => x.FailureGuid == fma.GetFailureDefinitionId().Guid)
                    .FirstOrDefault();

                if (aFOpts != null)
                {
                    //Show the CatchFailures dialog if a failure was caught
                    if (currentSettings.InteractiveModeEnabled)
                    {
                        try
                        {
                            //todo: clone to allow roll-back?
                            FailureCatcherWindow failWin = new FailureCatcherWindow(currentSettings, aFOpts, uiApp);

                            Boolean? result = failWin.ShowDialog();

                            if (result.Value)
                            {
                                //Write changes to the .ini
                                currentSettings.LastUpdate = DateTime.Now;
                                IAutoOptionsRepository settingsRepo = new AutoOptionsConfigFileRepo();
                                settingsRepo.WriteAutoOptions(currentSettings);
                            }
                        }
                        catch (Exception ex)
                        {
                            TaskDialog.Show("ex", ex.Message + "\n" + ex.StackTrace);
                            throw;
                        }
                    }

                    FailureResolutionOption selectedResolution = aFOpts.SelectedResolution;
                    if (selectedResolution is AutoOptionsResolution)
	                {
                        try 
	                    {	        
		                    switch (((AutoOptionsResolution)selectedResolution).Resolution)
                            {
                                case AutoOptionsResolutionType.NoAction:
                                    break;
                                case AutoOptionsResolutionType.DeleteAffected:
                                    fa.DeleteElements(fma.GetFailingElementIds().ToList());
                                    action = FailureProcessingResult.ProceedWithCommit;
                                    break;
                                case AutoOptionsResolutionType.CancelTransaction:
                                    //todo: not working, being overwritten?
                                    action = FailureProcessingResult.ProceedWithRollBack;
                                    e.SetProcessingResult(action);
                                    fa.RollBackPendingTransaction();
                                    return;
                                    break;
                                case AutoOptionsResolutionType.HideWarning:
                                    //todo: check if actually is a warning?
                                    fa.DeleteWarning(fma);
                                    break;
                                default:
                                    break;
                            }
	                    }
	                    catch (Exception ex)
	                    {
                            TaskDialog.Show("Dispatcher", ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace);
	                    }
                    }
                    else if (selectedResolution is RevitResolution)
                    {
                        try
                        {
                            FailureResolutionType fRT = ((RevitResolution)selectedResolution).Resolution;

                            if (fma.HasResolutionOfType(fRT))
                            {
                                fma.SetCurrentResolutionType(fRT);
                                fa.ResolveFailure(fma);
                                action = FailureProcessingResult.ProceedWithCommit;
                            }
                            else
                            {
                                TaskDialog.Show("AutoOptions", "The selected automatic resolution \n***" + aFOpts.SelectedResolution.FriendlyCaption + " (" + fRT + ")***\ncan't be used");
                            }
                        }
                        catch(Exception ex)
                        {
                            TaskDialog.Show("Dispatcher", ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace);
                        }
                    }
                }
            }

            e.SetProcessingResult(action);
        }
    }
}
