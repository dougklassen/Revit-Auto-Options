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
using System.Windows.Navigation;
using System.Windows.Shapes;

using DougKlassen.Revit.AutoOptions.DomainModels;

namespace DougKlassen.Revit.AutoOptions.Interface
{
    /// <summary>
    /// Interaction logic for FailureOptionsControl.xaml
    /// </summary>
    public partial class FailureOptionsControl : UserControl
    {
        public FailureOptionsControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Set the FailureResolutionType for the current ErrorOption by retrieving it from the friendly description used for the ComboBox item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBoxResolutions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AutoFailureHandlingOptions fOpts = (AutoFailureHandlingOptions)DataContext;
            if (ComboBoxResolutions.SelectedItem != null)
            {
                //todo: data binding works for everything but this text. Can it be invalidated or otherwise updated?
                ResolutionDescriptionText.Text = fOpts.SelectedResolution.FriendlyDescription;
            }
        }

        public void SetTargetFailure( AutoFailureHandlingOptions targetFail )
        {
            DataContext = targetFail;            
        }
    }
}
