using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

namespace DougKlassen.Revit.AutoOptions.DomainModels
{
    public abstract class FailureResolutionOption
    {
        public String FriendlyCaption { get; set; }
        public String FriendlyDescription { get; set; }
    }

    [Serializable]
    public class AutoOptionsResolution : FailureResolutionOption
    {
        public AutoOptionsResolutionType Resolution { get; set; }

        public static FailureResolutionOption NoActionRes, CancelRes, DeleteRes, HideRes;
        public static List<FailureResolutionOption> ErrorResolutions, WarningResolutions;

        /// <summary>
        /// Public default constructor for serialization
        /// </summary>
        public AutoOptionsResolution() {}

        /// <summary>
        /// Default constructor, Options should be obtained from public properties
        /// </summary>
        static AutoOptionsResolution()
        {
            NoActionRes = new AutoOptionsResolution()
                {
                    Resolution = AutoOptionsResolutionType.NoAction,
                    FriendlyCaption = "No Action",
                    FriendlyDescription = "Continue with default Revit failure behaivor"
                };
            //todo: add default res
            CancelRes = new AutoOptionsResolution()
                {
                    Resolution = AutoOptionsResolutionType.CancelTransaction,
                    FriendlyCaption = "Cancel Transaction",
                    FriendlyDescription = "Cancel the action that produced the failure"
                };
            DeleteRes = new AutoOptionsResolution()
                {
                    Resolution = AutoOptionsResolutionType.DeleteAffected,
                    FriendlyCaption = "Delete Elements",
                    FriendlyDescription = "Delete all affected elements"
                };
            HideRes = new AutoOptionsResolution()
                {
                    Resolution = AutoOptionsResolutionType.HideWarning,
                    FriendlyCaption = "Hide Warning",
                    FriendlyDescription = "Hide the warning"
                };

            ErrorResolutions = new List<FailureResolutionOption> { NoActionRes, CancelRes, DeleteRes };
            WarningResolutions = new List<FailureResolutionOption> { NoActionRes, CancelRes, DeleteRes, HideRes };
        }
    }

    [Serializable]
    public class RevitResolution : FailureResolutionOption
    {
        public FailureResolutionType Resolution { get; set; }

        /// <summary>
        /// Default constructor for serialization purposes
        /// </summary>
        public RevitResolution() {}
    }
}

public enum AutoOptionsResolutionType
{
    NoAction,
    CancelTransaction,
    DeleteAffected,
    HideWarning
}
