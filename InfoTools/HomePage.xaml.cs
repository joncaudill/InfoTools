using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InfoTools
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HomePage"/> class.
        /// </summary>
        public HomePage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Applies the specified color to the alert bar background.
        /// </summary>
        /// <param name="colorHex">Hex color string (e.g., "#FF0000").</param>
        public void ApplyAlertBarColor(string colorHex)
        {
            AlertBar.ApplyAlertBarColor(colorHex);
        }

        /// <summary>
        /// Applies font face and scale settings from configuration to the alert bar.
        /// </summary>
        public void ApplyAlertBarFontAndScale()
        {
            AlertBar.ApplyAlertBarFontAndScale();
        }
    }
}