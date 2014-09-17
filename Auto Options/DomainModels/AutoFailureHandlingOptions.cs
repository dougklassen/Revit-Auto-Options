using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;

namespace DougKlassen.Revit.AutoOptions.DomainModels
{
    [Serializable]
    public class AutoFailureHandlingOptions
    {
        public String BuiltInFailuresInternalName { get; set; }
        public String UserDescription { get; set; }
        public Guid FailureGuid;
        //todo: use path option in config dialog rather than this work around
        public String GuidString
        {
            get
            {
                return FailureGuid.ToString();
            }
        }
        public String BuiltInFailuresSubCategory { get; set; }
        public String SeverityDescription { get; set; }
        public Boolean HasResolutions { get; set; }
        public List<FailureResolutionOption> AvailableResolutions { get; set; }
        public Int32 SelectedResIndex { get; set; }
        public FailureResolutionOption SelectedResolution
        {
            get
            {
                return AvailableResolutions[SelectedResIndex];
            }
        }
        public String SelectedResolutionDescription
        {
            get
            {
                return SelectedResolution.FriendlyDescription;
            }
        }

        //need default constructor for serialization
        public AutoFailureHandlingOptions() { }

        public AutoFailureHandlingOptions(FailureDefinitionAccessor fda) : this()
        {
            FailureGuid = fda.GetId().Guid;
            BuiltInFailuresInternalName = fda.GetBuiltInFailuresInternalName();
            UserDescription = fda.GetDescriptionText();
            BuiltInFailuresSubCategory = fda.GetBuiltInFailuresSubCategory();

            switch (fda.GetSeverity())
            {
                case FailureSeverity.None:
                    SeverityDescription = "None";
                    break;
                case FailureSeverity.Warning:
                    SeverityDescription = "Warning";
                    break;
                case FailureSeverity.Error:
                    SeverityDescription = "Error";
                    break;
                case FailureSeverity.DocumentCorruption:
                    SeverityDescription = "Document Corruption";
                    break;
                default:
                    break;
            }
            
            HasResolutions = fda.HasResolutions();

            if (fda.GetSeverity() == FailureSeverity.Warning)
            {
                AvailableResolutions = AutoOptionsResolution.WarningResolutions.Concat(fda.GetRevitFailureResolutionOptions()).ToList();
            }
            else if (fda.GetSeverity() == FailureSeverity.Error)
            {
                AvailableResolutions = AutoOptionsResolution.ErrorResolutions.Concat(fda.GetRevitFailureResolutionOptions()).ToList();
            }
            else
            {
                AvailableResolutions = new List<FailureResolutionOption>();
            }

            HasResolutions = fda.HasResolutions();
            FailureResolutionOption noActionRes = AvailableResolutions
                .Where(x => x is AutoOptionsResolution)
                .Cast<AutoOptionsResolution>()
                .Where(x => x.Resolution == ((AutoOptionsResolution)AutoOptionsResolution.NoActionRes).Resolution)
                .FirstOrDefault();
            SelectedResIndex = AvailableResolutions.IndexOf(noActionRes);
        }

        /// <summary>
        /// Search the FailureDefinitionRegistry for a FailureDefinition that matches
        /// the FailureDefinitionId of this HandlingOption
        /// </summary>
        /// <returns></returns>
        private FailureDefinitionAccessor GetCorrespondingFailureDefinitionAccessor()
        {
            FailureDefinitionRegistry faReg = Application.GetFailureDefinitionRegistry();
            return faReg.ListAllFailureDefinitions()
                .Where(x => x.GetId().Guid == FailureGuid)
                .FirstOrDefault();
        }

        //todo: for debugging. delete
        public String GetAvailableResolutionsDescription()
        {
            String result = String.Empty;
            FailureDefinitionAccessor fa = GetCorrespondingFailureDefinitionAccessor();
            foreach (var fRT in fa.GetApplicableResolutionTypes())
            {
                result += "***" + fa.GetResolutionCaption(fRT) + " (" + fRT + ")***\n";
            }
            return result;
        }
    }

    [Serializable]
    public class WarningHandlingOptions : AutoFailureHandlingOptions
    {
        public WarningHandlingOptions() : base() { }

        public WarningHandlingOptions(FailureDefinitionAccessor fa) : base(fa) { }
    }

    [Serializable]
    public class ErrorHandlingOptions : AutoFailureHandlingOptions
    {

        public ErrorHandlingOptions() : base() { }
        
        public ErrorHandlingOptions(FailureDefinitionAccessor fa) : base(fa) { }
    }

    [Serializable]
    public class CorruptDocumentHandlingOptions : AutoFailureHandlingOptions
    {
        public CorruptDocumentHandlingOptions() : base() { }

        public CorruptDocumentHandlingOptions(FailureDefinitionAccessor fa) : base(fa) { }
    }
}
