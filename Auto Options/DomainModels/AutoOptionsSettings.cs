using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;

namespace DougKlassen.Revit.AutoOptions.DomainModels
{
    /// <summary>
    /// A class to encapsulate all configurable options in AutoOptions
    /// </summary>
    [Serializable]
    public class AutoOptionsSettings
    {
        /// <summary>
        /// The last time settings were written to the config file
        /// </summary>
        public DateTime LastUpdate { get; set; }

        /// <summary>
        /// Is failure handling active and will failures be handled by AutoOptionsDispatcher.AutoOptionsFailureHandler()
        /// </summary>
        public Boolean HandlingActive { get; set; }
        /// <summary>
        ///  Is interactive mode enabled and will a dialog be displayed when a failure occurs
        /// </summary>
        public Boolean InteractiveModeEnabled { get; set; }
        /// <summary>
        /// The string used to filter the list of built in failures in the config dialog
        /// </summary>
        public String SearchString { get; set; }
        /// <summary>
        /// The settings for all handled warnings
        /// </summary>
        public List<WarningHandlingOptions> WarningOptions { get; set; }
        /// <summary>
        /// The settings for all handled errors
        /// </summary>
        public List<ErrorHandlingOptions> ErrorOptions { get; set; }
        /// <summary>
        /// The settings for all handled corrupt document failures. There are no corrupt document failures
        /// in builtInFailures and this is currently not used
        /// </summary>
        public List<CorruptDocumentHandlingOptions> CorruptOptions { get; set; }
        /// <summary>
        /// a concatenated list of warnings plus errors
        /// </summary>
        public List<AutoFailureHandlingOptions> AllFailureOptions
        {
            get
            {
                return ErrorOptions.Cast<AutoFailureHandlingOptions>().Concat(WarningOptions.Cast<AutoFailureHandlingOptions>()).ToList();
            }
        }

        /// <summary>
        /// Automatically apply temporary view settings when a working view is opened
        /// </summary>
        public Boolean UseTempViewPropsForWorkingViews { get; set; }
        /// <summary>
        /// The regular expression identifying a working view
        /// </summary>
        public String WorkViewNameFilter { get; set; }

        //Default settings in the constructor will be overridden when failures are deserialized from the config file
        public AutoOptionsSettings()
        {
            LastUpdate = DateTime.Now;
            HandlingActive = true;
            InteractiveModeEnabled = false;
            SearchString = String.Empty;
            UseTempViewPropsForWorkingViews = false;
            WorkViewNameFilter = String.Empty;
        }

        /// <summary>
        /// Returns a filtered AutoOptionsList where all UserDescriptions match the search string
        /// </summary>
        /// <param name="searchString">A search string formatted as a regular expresseion</param>
        /// <returns>The filtered AutoOptionsList</returns>
        public AutoOptionsSettings GetFilteredList()
        {
            if (SearchString == String.Empty)
	        {
		        return this;
	        }
            else
	        {
                Regex searchEx = new Regex(SearchString, RegexOptions.IgnoreCase );
                List<WarningHandlingOptions> filteredWarnings = this.WarningOptions
                    .Where(x => searchEx.IsMatch(x.UserDescription))
                    .ToList();
                List<ErrorHandlingOptions> filteredErrors = this.ErrorOptions
                    .Where(x => searchEx.IsMatch(x.UserDescription))
                    .ToList();

                return new AutoOptionsSettings()
                    {
                        WarningOptions = filteredWarnings,
                        ErrorOptions = filteredErrors
                    };
	        }
        }

        /// <summary>
        /// Sets all FailureOptions to do nothing
        /// </summary>
        public void ResetAllSelectedOptions()
        {
            foreach (AutoFailureHandlingOptions opt in AllFailureOptions)
            {
                FailureResolutionOption noActionRes = opt.AvailableResolutions
                    .Where(x => x is AutoOptionsResolution)
                    .Cast<AutoOptionsResolution>()
                    .Where(x => x.Resolution == ((AutoOptionsResolution)AutoOptionsResolution.NoActionRes).Resolution)
                    .FirstOrDefault();
                Int32 SelectedResIndex =
                opt.SelectedResIndex = opt.AvailableResolutions.IndexOf(noActionRes);
            }
        }
    }
}
