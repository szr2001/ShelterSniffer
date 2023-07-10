using Residence_Web_Scraper.HandlerClasses;
using Residence_Web_Scraper.WPFpages;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Residence_Web_Scraper.WPFpages.PopUpsPages;
using Residence_Web_Scraper.Interfaces;
using HtmlAgilityPack;
using System.IO;
using System.Data;

namespace Residence_Web_Scraper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //a list of Seen residences, Url and Price,a new element is added to the list each time
        //the user interacts with a Redisence Slot
        //On refreash the list is sent to the database handler and stored in the database
        //after that the list is cleared.
        //it was made like this to limit the write calls to the database
        public HashSet<(string,int)> SeenResidenceTemp = new();

        //Cancelation source used to cancel refreash and webscrape
        public static CancellationTokenSource? CancelTokenSource;

        //Settings and Residencehistory Windows 
        public SettingsWindow SettingsW;
        public ResidenceFavorites RFavorites;

        //A list with BUY prices, 'K' will be converted in 3 zeros
        public List<string> BuyPrices = new() 
        {
            "0k€","10k€","20k€","30k€", "40k€","50k€", "60k€", "70k€", "80k€", "90k€", "100k€"
            , "110k€", "120k€", "130k€", "140k€", "150k€", "160k€", "170k€", "180k€", "190k€", "200k€"
            , "210k€", "210k€", "220k€", "230k€", "240k€", "250k€", "260k€", "270k€", "280k€", "290k€", "300k€"
            , "210k€", "210k€", "220k€", "230k€", "240k€", "250k€", "260k€", "270k€", "280k€", "290k€", "300k€"
            , "310k€", "310k€", "320k€", "330k€", "340k€", "350k€", "360k€", "370k€", "380k€", "390k€", "400k€"
            , "410k€", "410k€", "420k€", "430k€", "440k€", "450k€", "460k€", "470k€", "480k€", "490k€", "500k€"
            , "510k€", "510k€", "520k€", "530k€", "540k€", "550k€", "560k€", "570k€", "580k€", "590k€", "600k€"
            , "610k€", "610k€", "620k€", "630k€", "640k€", "650k€", "660k€", "670k€", "680k€", "690k€", "700k€"
            , "710k€", "710k€", "720k€", "730k€", "740k€", "750k€", "760k€", "770k€", "780k€", "790k€", "800k€"
            , "810k€", "810k€", "820k€", "830k€", "840k€", "850k€", "860k€", "870k€", "880k€", "890k€", "900k€"
            , "910k€", "910k€", "920k€", "930k€", "940k€", "950k€", "960k€", "970k€", "980k€", "990k€", "1000k€"
        };

        //a list for RENT Prices
        public List<string> RentPrices = new() 
        {
            "0€", "50€", "100€", "150€", "200€", "250€", "300€", "350€", "400€", "450€", "500€",
            "550€", "600€", "650€", "700€", "750€", "800€", "850€", "900€", "950€", "1000€", "1200€",
            "1400€", "1600€", "1800€", "2000€",
        };

        //Available cities in Romania
        public List<string> AvailableCitys = new() 
        { 
            "Alba", 
            "Arad", 
            "Arges", 
            "Bacau", 
            "Bihor",
            "Bistrita-Nasaud", 
            "Botosani", 
            "Braila", 
            "Brasov",
            "Bucuresti-Ilfov",
            "Buzau",
            "Calarasi",
            "Caras-Severin",
            "Cluj",
            "Constanta",
            "Covasna",
            "Dambovita",
            "Dolj",
            "Galati",
            "Giurgiu",
            "Gorj",
            "Hargita",
            "Hunedoara",
            "Ialomita",
            "Iasi",
            "Maramures",
            "Mehedinti",
            "Mures",
            "Neamt",
            "Olt",
            "Prahova",
            "Salaj",
            "Satu-mare",
            "Sibiu",
            "Suceava",
            "Teleorman",
            "Timis",
            "Tulcea",
            "Valcea",
            "Vaslui",
            "Vrancea",
        };
        
        //Dinamically created Counties depending on active listings, so uoi cant select a countie that has no available listings 
        public List<string> AvailableCountys = new();

        //Types of residences
        public List<string> ResidenceTyes = new() { "Houses", "Apparts" };

        //Types of acquisitions
        public List<string> BuyOrRent = new() { "Buy", "Rent" };

        //Types of ways to order the selected residence list
        public List<string> OrderBy = new() { "Price", "m2" };

        //Ascending or descending 
        public List<string> Order = new() { "desc", "asc" };

        //selected filters
        public string SelectedCity = "Alba";
        public string SelectedCounty = "All";
        public string SelectedResidenceType = "Houses";
        public string SelectedBuyOrRent = "Buy";
        public int SelectedMinPrice = 0;
        public int SelectedMaxPrice = 90000;
        public string SelectedOrderBy = "Price";
        public string SelectedOrder = "desc";

        //list that holds the filtered residences
        private List<Residence> FilteredResidences = new();
        //list that holds the residences that changed in price sience the last check
        private List<(Residence,int)> ChangedResidences = new();

        public MainWindow()
        {
            InitializeComponent();
            //creating the settings and favorite windows and adding a refference to this Main Window.
            SettingsW = new();
            RFavorites = new ResidenceFavorites(mainWindow: this);
            StatusHandler.InitializeComponent(appMainWindow: this);
            ResidenceWebScrapinghandler.InitializeComponent(appMainWindow: this);

            //activate console for debuging
            AllocConsole();
        }
        //Console Dll Inport for debuging
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
        //

        //Apply the filters and refreas the list of residences
        private async void RefreshLocalInfo(object sender, RoutedEventArgs e)
        {
            //clears the spawned residence slots
            ClearSpawnedResidences();
            //invoke cancel refreash if its already running
            InvokeCancelationToken(sender,e);
            //add a small delay so if the refreash code was already runing it has time to get the cancel request and stop
            await Task.Delay(500);

            //disable buttons so there is no multiple requests at the same time
            RefreshLocalInfoBTN.IsEnabled = false;
            UpdateLocalInfoBtn.IsEnabled = false;

            //create a new cancel token and turn on the cancel button
            CancelTokenSource = new CancellationTokenSource();
            CancelWebScrapingBtn.IsEnabled = true;

            //call refresh method and await until the data is requested from the db and it is shown to screen
            await RefreshResidenceSlots(CancelTokenSource.Token);

            //after finish clear the cancelation token
            CancelTokenSource = null;

            //enable refreash buttons and disable Cancel button
            RefreshLocalInfoBTN.IsEnabled = true;
            UpdateLocalInfoBtn.IsEnabled = true;
            CancelWebScrapingBtn.IsEnabled = false;

        }
        //clear spawned residence by clearing children inside the holder
        private void ClearSpawnedResidences()
        {
            ResidencesHolder.Children.Clear();
        }
        //Request residences from db that follows the filters and show them  to the screen.
        private async Task RefreshResidenceSlots(CancellationToken ctoken)
        {
            //updating the app status and status bar to 0
            StatusHandler.UpdateAppStatus("Applying Filters...");
            StatusHandler.UpdateStatusBar(0);

            //writing to console that the method starts
            Console.WriteLine("--------------Apply Filters-----------------");

            //get all the cities from the selected county to update the combobox
            await RequestCountysFromCity();

            //save seen residences
            await SaveSeenResidences();

            //max active residence slots on screen at one time
            int MaxVisibleRes = 55;
            
            //get residences that changed  in price (Compares seenresidence table
            //Oldprice with the totalResidence NewPrice, if its not the same then it changed)
            ChangedResidences = await DataBaseHandler.GetChangedResidences(MaxVisibleRes);
            //subtract the total changed residences from the max visible elements so in the end it wont be more then the max visible elements
            MaxVisibleRes -= ChangedResidences.Count;

            //gets the filtered residences from the database with the active filters
            FilteredResidences = await DataBaseHandler.GetFilteredResidences
                (
                    SelectedMinPrice,
                    SelectedMaxPrice,
                    SelectedCity,
                    SelectedCounty,
                    SelectedResidenceType,
                    SelectedBuyOrRent,
                    SelectedOrderBy,
                    SelectedOrder,
                    MaxVisibleRes
                );
            
            //Write to console for debuging
            Console.WriteLine($"Residence Total Count: {await DataBaseHandler.GetTotalResidenceCount()}");
            Console.WriteLine($"Residence Filtered Count: {FilteredResidences.Count}");
            Console.WriteLine($"Residence Changed Count: {ChangedResidences.Count}");

            //amount of loops the foreach ran
            int loops = 0;
            //delay between spawning residence slots in miliseconds
            int delay = 50;
            try
            {
                //loop trough the changedResidence and spawn a ResidenceSlot with the residence's info.
                foreach ((Residence,int) item in ChangedResidences)
                {
                    //check for cancelation
                    ctoken.ThrowIfCancellationRequested();
                    //create residenceslot
                    ResidenceSlot ResSlot = new(item.Item1, this, item.Item2,false);
                    //call dispatcher to add the Slot to the Holder
                    await Dispatcher.InvokeAsync(() =>
                    {
                        ResidencesHolder.Children.Add(ResSlot);
                    });
                    //add a delay so the ui can update
                    await Task.Delay(delay);
                    //increase the loops so it can stop when it reaches the max spawned slots
                    loops++;
                    //update the app statusbar
                    StatusHandler.UpdateStatusBar((loops * 100) / MaxVisibleRes);
                }

                foreach (Residence item in FilteredResidences)
                {
                    //check for cancelation
                    ctoken.ThrowIfCancellationRequested();
                    //create residenceslot
                    ResidenceSlot ResSlot = new(item,this,null,false);
                    //call dispatcher to add the Slot to the Holder
                    await Dispatcher.InvokeAsync(() =>
                    {
                        ResidencesHolder.Children.Add(ResSlot);
                    });
                    //add a delay so the ui can update
                    await Task.Delay(delay);
                    //increase the loops so it can stop when it reaches the max spawned slots
                    loops++;
                    //update the app statusbar
                    StatusHandler.UpdateStatusBar((loops * 100) / MaxVisibleRes);
                }
            }
            catch(Exception e)
            {
                //if cancelation requested log to console
                Console.WriteLine($"Canceled Applying Filters: {e.Message}");
            }
            //update status and set status bar to 0%
            StatusHandler.UpdateAppStatus("Idle...");
            StatusHandler.UpdateStatusBar(0);
        }
        //Saves the SeenResidences list
        private async Task SaveSeenResidences()
        {
            //call the database to save seen residence and pass the seenresidence list
            await DataBaseHandler.SaveSeenResidenceAsync(SeenResidenceTemp);
            //clear the seenresidence list when it was stored in the db
            SeenResidenceTemp.Clear();
        }
        //toggles visibility of the Settings window 
        private async void DownloadSinglePageTest(object sender, RoutedEventArgs e)
        {
            //------------Download Page For XPath Tests--------------------
            Console.WriteLine("Type Page Link..............");
            string link = Console.ReadLine();

            if (!Directory.Exists(@"C:\Users\szr20\Desktop\WebScraperTest"))
            {
                Directory.CreateDirectory(@"C:\Users\szr20\Desktop\WebScraperTest");
            }
            using (StreamWriter str = new StreamWriter(@"C:\Users\szr20\Desktop\WebScraperTest\PageTest.txt"))
            {
                HtmlDocument dc = await GetDocument(link, null);
                await str.WriteAsync(dc.DocumentNode.InnerHtml);
            }

            async Task<HtmlDocument> GetDocument(string url, string? message)
            {
                //creates a new document
                HtmlDocument doc = new();
                try
                {
                    //tries to download it
                    HtmlWeb hWeb = new HtmlWeb();
                    doc = await hWeb.LoadFromWebAsync(url).ConfigureAwait(false);
                    //when finished log a message or a default message
                    if (message is null)
                    {
                        Console.WriteLine("Download Page Finished");
                    }
                    else
                    {
                        Console.WriteLine($"Download Finished: {message}");
                    }
                }
                catch (Exception e)
                {
                    //if failed  debug
                    Console.WriteLine($"GetDocument Error: {e.Message}");
                }
                return doc;
            }
            //------------Download Page For XPath Tests--------------------
        }
        private void OpenStettings(object sender, RoutedEventArgs e)
        {

            if (SettingsW.IsVisible)
            {
                SettingsW.Hide();
            }
            else
            {
                SettingsW.Show();
            }
        }
        //toggles visibility of the Favorites window 
        private void OpenFav(object sender, RoutedEventArgs e)
        {
            if (RFavorites.IsVisible)
            {
                RFavorites.Hide();
            }
            else
            {
                RFavorites.Show();
            }
        }
        //invoke cancelation token if not null
        private void InvokeCancelationToken(object sender, RoutedEventArgs e)
        {
            if(CancelTokenSource is not null)
            {
                if (!CancelTokenSource.IsCancellationRequested)
                {
                    CancelTokenSource.Cancel();
                    //disable cancel button
                    CancelWebScrapingBtn.IsEnabled = false;
                    //update status to stopping
                    StatusHandler.UpdateAppStatus("Stopping...");
                }
            }
        }
        //request counties from a specified City based on amount of residences for sale/rent in that area
        private async Task RequestCountysFromCity()
        {
            //wait for the list
            AvailableCountys = await DataBaseHandler.GetResidenceCountys(SelectedCity, "SeenResidences");
            //remove event from when the combo box is updated so it wont fire when updating the content now
            FilterLocationCounty.SelectionChanged -= UpdateActiveFilterLocationCounty;
            //update the combobox content 
            FilterLocationCounty.ItemsSource = AvailableCountys;
            if(FilterLocationCounty.SelectedIndex == -1)
            {
                FilterLocationCounty.Text = "All";
            }
            //asign the event on selection changed
            FilterLocationCounty.SelectionChanged += UpdateActiveFilterLocationCounty;
        }

        #region AppWindow Controls
        //On close app event
        private async void OnAppClose()
        {
            //update status to closing app, and wait to save seenresidences before closing
            StatusHandler.UpdateAppStatus("Closing App...");
            await SaveSeenResidences();
            App.Current.Shutdown();

        }
        //Close app
        private void CloseApp(object sender, RoutedEventArgs e)
        {
            //disable buttons
            CloseAppBtn.IsEnabled = false;
            MaximizeWindowBtn.IsEnabled = false;
            MinimizeWindowBtn.IsEnabled = false;
            OpenSettingsBtn.IsEnabled = false;
            OpenStatsBtn.IsEnabled = false;
            RefreshLocalInfoBTN.IsEnabled = false;
            UpdateLocalInfoBtn.IsEnabled = false;
            //call on app close
            OnAppClose();
        }
        //move window on click
        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        //maximize window
        private void MaximizeWindow(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }
        //minimize window
        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            if(this.WindowState == WindowState.Minimized)
            {
               this.WindowState = WindowState.Normal;
            }
            else
            {
               this.WindowState = WindowState.Minimized;
            }
        }
        #endregion

        #region Download New Info Async
        // Show popup on download new info button press
        private void ShowDownloadNewInfoPopUp(object sender, RoutedEventArgs e)
        {
            //create a new popup content
            IPopUp PopUpContent = new WarningTextPopUp
                (
                    @"Check if you have a vpn active and it has at least 200 mb remaining.
                    Proceeding without a vpn might get you banned from active websites"
                );
            //create a new popup and pass the content of the popup and the frame is atached to
            PopUpPage WarningPopUpPage = new

                (
                    PopUpFrame,
                    PopUpContent
                );
            //asign popup to the popup frame
            PopUpFrame.Content = WarningPopUpPage;
            //asign a method to On Popup Procede event
            WarningPopUpPage.OnPopUpProceed += StartDownloadNewInfo;
        }
        //cleares spawned residences and then calls download new info async and trhows the return
        private void StartDownloadNewInfo()
        {
            ClearSpawnedResidences();

            _ = DownloadNewInfoAsync();
        }
        //download new info
        private async Task DownloadNewInfoAsync()
        {
            //disable buttons
            UpdateLocalInfoBtn.IsEnabled = false;
            RefreshLocalInfoBTN.IsEnabled = false;

            //new cancelation token and enable cancel button
            CancelTokenSource = new CancellationTokenSource();

            CancelWebScrapingBtn.IsEnabled = true;

            //await for the handler to run each webscraper to target different websites, passing the filters
            await ResidenceWebScrapinghandler.GetResidences(SelectedCity,SelectedResidenceType,SelectedBuyOrRent,CancelTokenSource.Token);

            //once finished clear the token source
            CancelTokenSource = null;

            //refreashes the local info after the new info has been downloaded
            RefreshLocalInfo(new object(), new RoutedEventArgs());

            //enable buttons and disable cancel button
            UpdateLocalInfoBtn.IsEnabled = true;
            RefreshLocalInfoBTN.IsEnabled = true;

            CancelWebScrapingBtn.IsEnabled = false;
        }
        #endregion

        #region Combo boxes Controls
        //Combo boxes for OnLoad event and OnChanged event to initliaize each combo box with default values
        //and handle update filters
        private void LocationCityComboBoxLoaded(object sender, RoutedEventArgs e)
        {
            FilterLocationCity.SelectionChanged -= UpdateActiveFilterLocationCity;

            FilterLocationCity.ItemsSource = AvailableCitys;
            FilterLocationCity.SelectedIndex = 0;

            FilterLocationCity.SelectionChanged += UpdateActiveFilterLocationCity;

        }
        private void UpdateActiveFilterLocationCity(object sender, SelectionChangedEventArgs e)
        {
            if (FilterLocationCity.SelectedItem == null)
            {
                return;
            }

            SelectedCity = FilterLocationCity.SelectedItem.ToString();
            RefreshLocalInfo(sender, new RoutedEventArgs());
        }

        private void UpdateActiveFilterLocationCounty(object sender, SelectionChangedEventArgs e)
        {
            if (FilterLocationCounty.SelectedItem == null)
            {
                return;
            }

            SelectedCounty = FilterLocationCounty.SelectedItem.ToString();
            RefreshLocalInfo(sender, new RoutedEventArgs());
        }

        private void BuyOrRentComboBoxLoaded(object sender, RoutedEventArgs e)
        {
            FilterResidenceBuyOrRent.SelectionChanged -= UpdateActiveFilterBuyOrRent;

            FilterResidenceBuyOrRent.ItemsSource = BuyOrRent;
            FilterResidenceBuyOrRent.SelectedIndex = 0;

            FilterResidenceBuyOrRent.SelectionChanged += UpdateActiveFilterBuyOrRent;
        }
        private void UpdateActiveFilterBuyOrRent(object sender, SelectionChangedEventArgs e)
        {
            if (FilterResidenceBuyOrRent.SelectedItem == null)
            {
                return;
            }
            SelectedBuyOrRent = FilterResidenceBuyOrRent.SelectedItem.ToString();
            //on buy or rent changed update price to use the correct price list
            MaxPriceComboBoxLoaded(sender, e);
            MinPriceComboBoxLoaded(sender, e);
            RefreshLocalInfo(sender, new RoutedEventArgs());
        }

        private void MinPriceComboBoxLoaded(object sender, RoutedEventArgs e)
        {
            FilterMinPrice.SelectionChanged -= UpdateActiveFilterMinPrice;

            //asign the right list of prices based on selected acquisition type
            if (SelectedBuyOrRent == "Buy")
            {
                FilterMinPrice.ItemsSource = BuyPrices;
            }
            else
            {
                FilterMinPrice.ItemsSource = RentPrices;
            }
            FilterMinPrice.SelectedIndex = 0;
            string price = "-1";
            //convert string from list to correct Int
            try
            {
                price = FilterMinPrice.SelectedItem.ToString().Replace("k", "000").Replace("€", "");
            }
            catch
            {
                price = FilterMinPrice.SelectedItem.ToString().Replace("€", "");
            }
            SelectedMinPrice = int.Parse(price);
            FilterMinPrice.SelectionChanged += UpdateActiveFilterMinPrice;
        }
        private void UpdateActiveFilterMinPrice(object sender, SelectionChangedEventArgs e)
        {
            if (FilterMinPrice.SelectedItem == null)
            {
                return;
            }
            string price = "-1";
            //convert string from list to correct Int
            try
            {
                price = FilterMinPrice.SelectedItem.ToString().Replace("k", "000").Replace("€", "");
            }
            catch
            {
                price = FilterMinPrice.SelectedItem.ToString().Replace("€", "");
            }
            SelectedMinPrice = int.Parse(price);
            RefreshLocalInfo(sender, new RoutedEventArgs());
        }

        private void MaxPriceComboBoxLoaded(object sender, RoutedEventArgs e)
        {
            FilterMaxPrice.SelectionChanged -= UpdateActiveFilterMaxPrice;

            if (SelectedBuyOrRent == "Buy")
            {
                FilterMaxPrice.ItemsSource = BuyPrices;
            }
            else
            {
                FilterMaxPrice.ItemsSource = RentPrices;
            }
            FilterMaxPrice.SelectedIndex = 9;
            string price = "-1";
            //convert string from list to correct Int
            try
            {
                price = FilterMaxPrice.SelectedItem.ToString().Replace("k", "000").Replace("€", "");
            }
            catch
            {
                price = FilterMaxPrice.SelectedItem.ToString().Replace("€", "");
            }
            SelectedMaxPrice = int.Parse(price);
            FilterMaxPrice.SelectionChanged += UpdateActiveFilterMaxPrice;
        }
        private void UpdateActiveFilterMaxPrice(object sender, SelectionChangedEventArgs e)
        {
            if (FilterMaxPrice.SelectedItem == null)
            {
                return;
            }
            string price = "-1";
            //convert string from list to correct Int
            try
            {
                price = FilterMaxPrice.SelectedItem.ToString().Replace("k", "000").Replace("€", "");
            }
            catch
            {
                price = FilterMaxPrice.SelectedItem.ToString().Replace("€", "");
            }
            SelectedMaxPrice = int.Parse(price);
            RefreshLocalInfo(sender, new RoutedEventArgs());
        }

        private void ResidenceTypeComboBoxLoaded(object sender, RoutedEventArgs e)
        {
            FilterResidenceType.SelectionChanged -= UpdateActiveFilterResidenceType;

            FilterResidenceType.ItemsSource = ResidenceTyes;
            FilterResidenceType.SelectedIndex = 0;

            FilterResidenceType.SelectionChanged += UpdateActiveFilterResidenceType;
        }
        private void UpdateActiveFilterResidenceType(object sender, SelectionChangedEventArgs e)
        {
            if (FilterResidenceType.SelectedItem == null)
            {
                return;
            }
            SelectedResidenceType = FilterResidenceType.SelectedItem.ToString();
            RefreshLocalInfo(sender, new RoutedEventArgs());
        }
       
        private void OrderByComboBoxLoaded(object sender, RoutedEventArgs e)
        {
            FilterOrderBy.SelectionChanged -= UpdateActiveFilterOrderBy;

            FilterOrderBy.ItemsSource = OrderBy;
            FilterOrderBy.SelectedIndex = 0;

            FilterOrderBy.SelectionChanged += UpdateActiveFilterOrderBy;
        }
        private void UpdateActiveFilterOrderBy(object sender, SelectionChangedEventArgs e)
        {
            if (FilterOrderBy.SelectedItem == null)
            {
                return;
            }
            SelectedOrderBy = FilterOrderBy.SelectedItem.ToString();
            RefreshLocalInfo(sender, new RoutedEventArgs());
        }

        private void OrderComboBoxLoaded(object sender, RoutedEventArgs e)
        {
            FilterOrder.SelectionChanged -= UpdateActiveFilterOrder;

            FilterOrder.ItemsSource = Order;
            FilterOrder.SelectedIndex = 0;

            FilterOrder.SelectionChanged += UpdateActiveFilterOrder;
        }
        private void UpdateActiveFilterOrder(object sender, SelectionChangedEventArgs e)
        {
            if (FilterOrder.SelectedItem == null)
            {
                return;
            }
            SelectedOrder = FilterOrder.SelectedItem.ToString();
            RefreshLocalInfo(sender, new RoutedEventArgs());
        }
        #endregion
    }
}
