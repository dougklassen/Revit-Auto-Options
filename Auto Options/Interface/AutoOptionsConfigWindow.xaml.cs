using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using DougKlassen.Revit.AutoOptions.DomainModels;

namespace DougKlassen.Revit.AutoOptions.Interface
{
    /// <summary>
    /// Interaction logic for AutoOptionsConfig.xaml
    /// </summary>
    public partial class AutoOptionsConfigWindow : Window
    {
        AutoOptionsSettings aOSettings;
        AutoFailureHandlingOptions selWarnOpts = new AutoFailureHandlingOptions();
        AutoFailureHandlingOptions selErrorOpts = new AutoFailureHandlingOptions();

        private AutoOptionsConfigWindow()
        {
            InitializeComponent();
        }

        public AutoOptionsConfigWindow(AutoOptionsSettings aOSettingsParam, ExternalCommandData cDParam) : this()
        {
            aOSettings = aOSettingsParam;
            DataContext = aOSettings;

            ////todo: this data binding doesn't work, perhaps because of reference value levels
            //System.Windows.Data.Binding warningBinding = new System.Windows.Data.Binding();
            //warningBinding.Mode = BindingMode.OneWay;
            //warningBinding.Source = selWarnOpts;
            //WarningOptionsPanel.DataContext = warningBinding;

            //System.Windows.Data.Binding errorBinding = new System.Windows.Data.Binding();
            //errorBinding.Mode = BindingMode.OneWay;
            //errorBinding.Source = selErrorOpts;
            //ErrorOptionsPanel.DataContext = errorBinding;

            //Set DataContext to null or it will be inherited from parent (set to aOSettings) till something is selected
            WarningOptionsPanel.DataContext = null;
            ErrorOptionsPanel.DataContext = null;

            //Center the window on the main Revit window
            var revitWindow = cDParam.Application.MainWindowExtents;
            Double centerWindowX = (revitWindow.Left + revitWindow.Right) / 2;
            Double centerWindowY = (revitWindow.Top + revitWindow.Bottom) / 2;
            Left = centerWindowX - Width / 2;
            Top = centerWindowY - Height / 2;

            GenerateWarningsTree();
            GenerateErrorTree();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            //indicates all changes should be discarded
            DialogResult = false;
            Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            //indicates all changes should be applied and written to the config file
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Updates TreeViewWarningOptions filtered for the search string
        /// </summary>
        private void GenerateWarningsTree()
        {
            TreeViewWarningOptions.Items.Clear();

            AutoOptionsSettings subList = aOSettings.GetFilteredList();

            var warningCats = subList.WarningOptions
                .Select(x => x.BuiltInFailuresSubCategory)
                .Distinct();
            
            foreach (var cat in warningCats)
            {
                Int32 itemIndex = TreeViewWarningOptions.Items.Add(new TreeViewItem() { Header = cat });
                ((TreeViewItem)TreeViewWarningOptions.Items[itemIndex]).ItemsSource = subList.WarningOptions
                    .Where(x => x.BuiltInFailuresSubCategory == cat)
                    .Select(x => x.BuiltInFailuresInternalName);
            }
        }

        /// <summary>
        /// Updates TreeViewErrorOptions filtered for the search string
        /// </summary>
        private void GenerateErrorTree()
        {
            //todo: create strongly typed TreeItems
            TreeViewErrorOptions.Items.Clear();

            AutoOptionsSettings subList = aOSettings.GetFilteredList();
            var errorCats = subList.ErrorOptions
                .Select(x => x.BuiltInFailuresSubCategory)
                .Distinct();

            foreach (var cat in errorCats)
            {
                Int32 itemIndex = TreeViewErrorOptions.Items.Add(new TreeViewItem() { Header = cat });
                ((TreeViewItem)TreeViewErrorOptions.Items[itemIndex]).ItemsSource = subList.ErrorOptions
                    .Where(x => x.BuiltInFailuresSubCategory == cat)
                    .Select(x => x.BuiltInFailuresInternalName);
            }
        }

        private void TextBoxSearchString_TextChanged(object sender, TextChangedEventArgs e)
        {
            GenerateWarningsTree();
            GenerateErrorTree();
        }

        private void TreeViewWarningOptions_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //String indicates that the lowest level of the tree has been reached
            if (e.NewValue is String)
            {
                ////todo: use data binding here
                //selWarnOpts = GetFailureHandlingOptionFromInternalName(e.NewValue as String);
                WarningOptionsPanel.SetTargetFailure( GetFailureHandlingOptionFromInternalName(e.NewValue as String) );
            }
        }

        private void TreeViewErrorOptions_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //String indicates that the lowest level of the tree has been reached
            if (e.NewValue is String)
            {
                ////todo: use data binding here
                //selErrorOpts = GetFailureHandlingOptionFromInternalName(e.NewValue as String);
                ErrorOptionsPanel.SetTargetFailure( GetFailureHandlingOptionFromInternalName(e.NewValue as String) );
            }
        }

        /// <summary>
        /// Reset all failures to be handled with "No Action"
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The Event arguements</param>
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            aOSettings.ResetAllSelectedOptions();

            //todo: use data binding here
            if (WarningOptionsPanel.DataContext != null)
            {
                WarningOptionsPanel.ComboBoxResolutions.SelectedIndex = ((AutoFailureHandlingOptions)WarningOptionsPanel.DataContext).SelectedResIndex;
            }
            if (ErrorOptionsPanel.DataContext != null)
            {
                ErrorOptionsPanel.ComboBoxResolutions.SelectedIndex = ((AutoFailureHandlingOptions)ErrorOptionsPanel.DataContext).SelectedResIndex;
            } 
        }

        /// <summary>
        /// Get strongly type FailureHandlingOptions from the InternalName string
        /// </summary>
        /// <param name="selectedFailure">The name of the failure taken from BuiltInFailures InternalName</param>
        /// <returns>A FailureHandlingOptions representing the failure</returns>
        private AutoFailureHandlingOptions GetFailureHandlingOptionFromInternalName(String selectedFailure)
        {
            return aOSettings.AllFailureOptions
                .Where(x => x.BuiltInFailuresInternalName == selectedFailure)
                .FirstOrDefault();
        }
    }
}
