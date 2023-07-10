using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Residence_Web_Scraper.HandlerClasses;

namespace Residence_Web_Scraper.WPFpages
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
        private void MoveApplication(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void ToggleDownloadMbWarning(object sender, RoutedEventArgs e)
        {
            //toggles the settings using the settingshandler and updates the photo in this window
            if (SettingsHandler.IsMbConsumtionWarningActive)
            {
                SettingsHandler.IsMbConsumtionWarningActive = false;
                EnableDownWarnImage.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/ThumbDown.png"));
            }
            else
            {
                SettingsHandler.IsMbConsumtionWarningActive = true;
                EnableDownWarnImage.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/ThumbUp.png"));
            }
        }
    }
}
