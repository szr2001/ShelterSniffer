using Residence_Web_Scraper.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Residence_Web_Scraper.WPFpages.PopUpsPages
{
    /// <summary>
    /// Interaction logic for WarningTextPopUp.xaml
    /// </summary>
    public partial class WarningTextPopUp : Page,IPopUp
    {
        public WarningTextPopUp(string warning)
        {
            InitializeComponent();
            WarningTxt.Text = warning;
        }

        public bool CanContinue()
        {
            return true;
        }
    }
}
