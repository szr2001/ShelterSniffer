using Residence_Web_Scraper.HandlerClasses;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Residence_Web_Scraper.WPFpages
{
    /// <summary>
    /// Interaction logic for ResedinceSlot.xaml
    /// </summary>
    public partial class ResidenceSlot : UserControl
    {
        //ref for main window
        private static MainWindow AppMainWindow;
        //residence info used to set up the slot
        private readonly Residence ResidenceInfo;
        //old price used for comparing prices
        int? OldPrice = null;
        //is vaforite used to change apperance 
        bool IsFavorite;
        public ResidenceSlot(Residence residenceInfo,MainWindow appwindow, int? oldPrice, bool isFavorite) //System.InvalidOperationException: 'The calling thread must be STA, because many UI components require this.'
        {
            InitializeComponent();
            //asign passed values
            ResidenceInfo = residenceInfo;
            AppMainWindow = appwindow;
            OldPrice = oldPrice;
            IsFavorite = isFavorite;

            if (!isFavorite)
            {
                //asign NEW if there is no old price detected 
                if (OldPrice is null) //problem, i moved everything to Seenresidence,stuff that wount appear in normal
                {
                    SlotStatusText.Text = "New";
                }
                else
                {
                    //if the old price was detected asign Changed
                    SlotStatusText.Text = "Changed";

                    //set apperance depending on the old price is higher or lower then the new price 
                    if (OldPrice < residenceInfo.Price)
                    {
                        SlotStatusBorder.Background = Application.Current.Resources["RRed"] as Brush;
                        SlotOldPrice.Foreground = Application.Current.Resources["RRed"] as Brush;
                        BitmapImage thumbnail = new BitmapImage(new Uri(@"pack://application:,,,/Assets/Images/RedArrow.png"));
                        SlotPriceArrow.Source = thumbnail;
                    }
                    else
                    {
                        SlotStatusBorder.Background = Application.Current.Resources["RGreen"] as Brush;
                        SlotOldPrice.Foreground = Application.Current.Resources["RGreen"] as Brush;
                        BitmapImage thumbnail = new BitmapImage(new Uri(@"pack://application:,,,/Assets/Images/GreenArrow.png"));
                        SlotPriceArrow.Source = thumbnail;
                    }
                    SlotOldPrice.Text = oldPrice.ToString();
                }
            }
            else
            {
                SlotStatusText.Text = "Favorite";
                ResidenceSlotBorder.Background = Application.Current.Resources["RGold"] as Brush;
            }

            if (residenceInfo.Image.Length == 0)
            {
                //if the image binary was 0 set the default image
                Console.WriteLine($"Missing Residence Image, using Default one");
                try
                {
                    BitmapImage thumbnail = new BitmapImage(new Uri(@"pack://application:,,,/Assets/Images/NoImageError.png"));
                    SlotImage.Source = thumbnail;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"ERROR Set image to 'NoImageError.Png': {e.Message}");
                }

            }
            else
            {
                //convert binary to img
                using (var memoryStream = new MemoryStream(ResidenceInfo.Image))
                {
                    try
                    {
                        BitmapImage thumbnail = new BitmapImage();
                        thumbnail.BeginInit();
                        thumbnail.CacheOption = BitmapCacheOption.OnLoad;
                        thumbnail.StreamSource = memoryStream;
                        thumbnail.EndInit();

                        SlotImage.Source = thumbnail;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"ERROR Set image to binary: {e.Message}");
                    }
                }
            }
            //asign title and city  and county
            SlotTitle.Text = ResidenceInfo.Title;

            SlotCity.Text = residenceInfo.City;

            SlotCounty.Text = residenceInfo.County;
            if (residenceInfo.M2 == -1)
            {
                SlotM2.Text = "M2 Error";

            }
            else
            {
                SlotM2.Text = residenceInfo.M2.ToString();
            }

            if (ResidenceInfo.Price == -1)
            {
                SlotPrice.Text = "Price ERROR";
            }
            else if (ResidenceInfo.Price == 0)
            {
                SlotPrice.Text = "Schimb";
            }
            else if (residenceInfo.Price >= 0)
            {
                SlotPrice.Text = $"{ResidenceInfo.Price}€";
            }
        }
        //if the user presses the favorites button
        private void AddResidenceToFavorites(object sender, RoutedEventArgs e)
        {
            //renmove or add this slot residence info to facvorites
            if(IsFavorite)
            {
                ResidenceSlotBorder.Background = Application.Current.Resources["RBrightBlueLight"] as Brush;
                FavoriteBtn.IsEnabled = false;
                FavoriteBtn.Width = 0;
                _ = DataBaseHandler.RemoveFavResidence(ResidenceInfo.Url);
            }
            else
            {
                ResidenceSlotBorder.Background = Application.Current.Resources["RGold"] as Brush;
                SetSeenResidence();
                _ = DataBaseHandler.AddFavResidence(ResidenceInfo);
            }
        }
        //open residence listing page in browser
        private void OpenResidencePage(object sender, RoutedEventArgs e)
        {
            SetSeenResidence();
            string url = ResidenceInfo.Url;
            try
            {
                Process.Start(url);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else
                {
                    throw;
                }
            }
        }
        //if user interacts with slot add slot residence to seen residences
        private void SlotClicked(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                SetSeenResidence();
            }
        }
        //adds the price and url from Residence variable to the main page SeenResidenceTemp List
        private void SetSeenResidence()
        {
            if (!IsFavorite)
            {
                if (SlotStatusBorder.Width == 0)
                {
                    return;
                }
                SlotStatusBorder.Width = 0;

                bool succes = AppMainWindow.SeenResidenceTemp.Add((ResidenceInfo.Url, ResidenceInfo.Price));

                if (!succes)
                {
                    Console.WriteLine($"Error adding Link to Seen Residences");
                }
            }
        }
    }
}
