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

using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;

using DougKlassen.Revit.AutoOptions.DomainModels;

namespace DougKlassen.Revit.AutoOptions.Interface
{
    /// <summary>
    /// Interaction logic for FailureCatcherWindow.xaml
    /// </summary>
    public partial class FailureCatcherWindow : Window
    {
        private FailureCatcherWindow()
        {
            InitializeComponent();
        }

        public FailureCatcherWindow(AutoOptionsSettings aOSettings, AutoFailureHandlingOptions caughtFailOpts, UIApplication uiApp = null) : this()
        {
            DataContext = aOSettings;
            CaughtFailurePanel.DataContext = caughtFailOpts;

            //Center the window on the main Revit window. uiApp is not guaranteed to be set
            if (uiApp != null)
            {
                Autodesk.Revit.UI.Rectangle revitWindow = uiApp.MainWindowExtents;
                Double centerWindowX = (revitWindow.Left + revitWindow.Right) / 2;
                Double centerWindowY = (revitWindow.Top + revitWindow.Bottom) / 2;
                Left = centerWindowX - Width / 2;
                Top = centerWindowY - Height / 2;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
