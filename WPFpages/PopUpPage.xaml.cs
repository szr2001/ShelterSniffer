using Residence_Web_Scraper.Interfaces;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Residence_Web_Scraper.WPFpages
{
    /// <summary>
    /// Interaction logic for MBWarningPopUp.xaml
    /// </summary>
    public partial class PopUpPage : Page
    {
        //The frame this popup will be atached to
        private Frame FrameParent;
        private bool? IsProceeding;
        //popup content
        private IPopUp PopUpContent;

        //Event OnCancel
        public delegate void PopUpCancel();
        public event PopUpCancel? OnPopUpCancel;

        //event OnProceede
        public delegate void PopUpProceed();
        public event PopUpProceed? OnPopUpProceed;
        public PopUpPage(Frame frameparent,IPopUp popupContent)
        {
            InitializeComponent();
            PopUpContent = popupContent;
            FrameParent = frameparent;
            PopUpContentFrrame.Content = PopUpContent;
        }
        //A method for puting code on pause until the cancel or proceede button is pressed
        public async Task<bool?> WaitForAnswerAsync()
        {
            while (IsProceeding == null)
            {
                await Task.Delay(100);
            }
            return IsProceeding;
        }
        //proceede button method
        private void Proceed(object sender, RoutedEventArgs e)
        {
            if (PopUpContent.CanContinue())
            {
                if (OnPopUpProceed != null)
                {
                    OnPopUpProceed.Invoke();
                }
                IsProceeding = true;
                ClosePopUp();
            }
        }
        //Cancel button method
        private void Cancel(object sender, RoutedEventArgs e)
        {
            if(OnPopUpCancel != null)
            {
                OnPopUpCancel.Invoke();
            }
            IsProceeding = false;
            ClosePopUp();
        }
        //Destroy self
        private void ClosePopUp()
        {
            FrameParent.Content = null;
            FrameParent.NavigationService.RemoveBackEntry();
        }
    }
}
