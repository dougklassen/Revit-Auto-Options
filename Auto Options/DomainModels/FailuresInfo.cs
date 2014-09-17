using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace DougKlassen.Revit.AutoOptions.DomainModels
{
    /// <summary>
    /// Retrieve information about specific failures and about BuiltInFailures in Revit
    /// </summary>
    public static class FailuresInfo
    {
        //keep this as a static field so that it doesn't have to be reinitialized everytime a failure is checked against it
        private static List<BuiltInFailureDescription> builtInFailuresList;

        //initialize the object with a list of all registered BuiltInFailures found
        static FailuresInfo()
        {
            builtInFailuresList = GetBuiltInFailures().ToList();
        }

        /// <summary>
        /// Retrieves a list of all Failures found in BuiltInFailures using reflection
        /// </summary>
        /// <returns>An enumeration of Failures by FailureDefinitionId</returns>
        private static IEnumerable<BuiltInFailureDescription> GetBuiltInFailures()
        {
            List<BuiltInFailureDescription> builtInFailuresFoundList = new List<BuiltInFailureDescription>();
            foreach (Type catType in typeof(BuiltInFailures).GetNestedTypes())
            {
                foreach (PropertyInfo failPropInfo in catType.GetProperties())
                {
                    if (failPropInfo.PropertyType == typeof(FailureDefinitionId))
                    {
                        FailureDefinitionId propValue = failPropInfo.GetValue(null, null) as FailureDefinitionId;
                        builtInFailuresFoundList.Add(new BuiltInFailureDescription(propValue.Guid, failPropInfo.Name, catType.Name));
                    }
                }
            }

            return builtInFailuresFoundList;
        }

        /// <summary>
        /// Retrieves a list of all registered Failures that are also found in BuiltInFailures with handling options set to default.
        /// This is the primary factory method for obtaining a new AutoOptionsSettings
        /// </summary>
        /// <returns>A strongly typed List of failure handling info</returns>
        public static AutoOptionsSettings AsAutoOptionsSettings()
        {
            IEnumerable<FailureDefinitionAccessor> builtInFailures = Application.GetFailureDefinitionRegistry().ListAllFailureDefinitions()
                .Where(f => f.IsBuiltInFailure());
            
            AutoOptionsSettings aOSettings = new AutoOptionsSettings()
            {
                WarningOptions = builtInFailures
                    .Where(x => x.GetSeverity() == FailureSeverity.Warning)
                    .Select(x => new WarningHandlingOptions(x))
                    .ToList(),
                ErrorOptions = builtInFailures
                    .Where(x => x.GetSeverity() == FailureSeverity.Error)
                    .Select(x => new ErrorHandlingOptions(x))
                    .ToList(),
                CorruptOptions = builtInFailures
                    .Where(x => x.GetSeverity() == FailureSeverity.DocumentCorruption)
                    .Select(x => new CorruptDocumentHandlingOptions(x))
                    .ToList()
            };

            return aOSettings;
        }

        /// <summary>
        /// Utility method to determine whether a failure is part of Revit's BuiltInFailures
        /// </summary>
        /// <param name="failure">The failure to evalutate</param>
        /// <returns>Whether the Fauilure was found in the API's BuiltInFailures object</returns>
        public static Boolean IsBuiltInFailure(this FailureDefinitionAccessor failure)
        {
            //Contains() wasn't recognizing equal FailureDefintionId values, so I compared values by FailureDefinitionId.Guid instead
            if (builtInFailuresList.Select(x => x.Guid).Contains(failure.GetId().Guid)) return true;
            else return false;
        }

        /// <summary>
        /// Utility method to return the SubCategory (expressed as an inner class in BuiltInFailures) of a failure if it is found in BuiltInFailures
        /// </summary>
        /// <param name="failure">The failure to evaluate</param>
        /// <returns>The SubCategory, or null if the failure was not found in BuiltInFailures</returns>
        public static String GetBuiltInFailuresSubCategory(this FailureDefinitionAccessor failure)
        {
            return builtInFailuresList
                .Where(x => x.Guid == failure.GetId().Guid)
                .Select(x => x.Category)
                .FirstOrDefault();  //this specifies that if no matching failure was found, null is returned, based on null being the default value for String because it is a reference type
        }

        /// <summary>
        /// Utility method to return the internal Name of the failure in the BuiltInFailures class
        /// </summary>
        /// <param name="failure">the failure to evaluate</param>
        /// <returns>The String value used to internally represent the BuiltInFailure in Revit</returns>
        public static String GetBuiltInFailuresInternalName(this FailureDefinitionAccessor failure)
        {
            return builtInFailuresList
                .Where(x => x.Guid == failure.GetId().Guid)
                .Select(x => x.Name)
                .FirstOrDefault();
        }

        /// <summary>
        /// Extension method to return a List of all FailureResolutionTypes encapsulated as FailureResolutionOptions
        /// </summary>
        /// <param name="fda"></param>
        /// <returns></returns>
        public static List<FailureResolutionOption> GetRevitFailureResolutionOptions(this FailureDefinitionAccessor fda)
        {
            List<FailureResolutionOption> result = new List<FailureResolutionOption>();
            foreach (FailureResolutionType frt in fda.GetApplicableResolutionTypes())
            {
                result.Add(new RevitResolution()
                {
                    Resolution = frt,
                    FriendlyCaption = fda.GetResolutionCaption(frt) + " (" + frt + ")",
                    //todo: add FriendlyDescriptions for all the FailureResolutionTypes
                    FriendlyDescription = "Revit Internal Name: " + frt.ToString() + "\nRevit Caption: " + fda.GetResolutionCaption(frt)
                });
            }

            return result;
        }

        //An inner class used in the maintained list of BuiltInFailures
        private class BuiltInFailureDescription
        {
            //re. access modifiers, public is required so that the outer class (BuiltInFailuresInfo) has access
            public Guid Guid { get; set; }
            public String Name { get; set; }
            public String Category { get; set; }

            public BuiltInFailureDescription(Guid guidParam, String nameParam, String catParam)
            {
                Name = nameParam;
                Guid = guidParam;
                Category = catParam;
            }
        }
    }
}
