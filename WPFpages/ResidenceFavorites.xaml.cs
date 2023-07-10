using Residence_Web_Scraper.HandlerClasses;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Residence_Web_Scraper.WPFpages
{
    /// <summary>
    /// Interaction logic for Stats.xaml
    /// </summary>
    public partial class ResidenceFavorites : Window
    {
        //page number
        private int PageNumber = 1;
        //offset used in sql statement 
        private int Offset = 0;
        //max visible slots per page
        private int MaxCount = 12;
        //list of residence
        private List<Residence> FavRes = new();
        //reff to main window and a cancel token
        private MainWindow MainWindow;
        private CancellationTokenSource CancelTokenSource;
        
        //filters
        public List<string> AvailableCountys = new();
        public string SelectedCity = "Alba";
        public string SelectedCounty = "All";
        public string SelectedResidenceType = "Houses";
        public string SelectedBuyOrRent = "Buy";
        public int SelectedMinPrice = 0;
        public int SelectedMaxPrice = 90000;
        public string SelectedOrderBy = "Price";
        public string SelectedOrder = "desc";

        public ResidenceFavorites(MainWindow mainWindow)
        {
            MainWindow = mainWindow;
            InitializeComponent();
        }
        private void MoveApplication(object sender, MouseButtonEventArgs e)
        {
            //move app
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        private void Refreash() 
        {
            //if there is an active token source call cancel and make a new cancel token source
            CancelTokenSource?.Cancel();
            CancelTokenSource = new CancellationTokenSource();
            //calls the method to refresh and forgets about it
            _ = RefreshFavorite(CancelTokenSource.Token);

            //debug to console the applied filters
            Console.WriteLine($"SelectedCity {SelectedCity}");
            Console.WriteLine($"SelectedResidenceType {SelectedResidenceType}");
            Console.WriteLine($"SelectedCounty {SelectedCounty}");
            Console.WriteLine($"SelectedBuyOrRent {SelectedBuyOrRent}");
            Console.WriteLine($"SelectedMinPrice {SelectedMinPrice}");
            Console.WriteLine($"SelectedMaxPrice {SelectedMaxPrice}");
            Console.WriteLine($"SelectedOrderBy {SelectedOrderBy}");
            Console.WriteLine($"SelectedOrder {SelectedOrder}");
        }
        private async Task RefreshFavorite(CancellationToken token)
        {
            //clear the spawned slots from the holder if it has any
            ResidencesHolder.Children.Clear();

            //waits to refresh the county combo using the selected city 
            await CountyComboBoxRefresh();

            //gets the fav residence passing the aplied filters
            FavRes = await DataBaseHandler.GetFavResidences
                (
                    Offset,
                    SelectedMinPrice,
                    SelectedMaxPrice,
                    SelectedCity,
                    SelectedCounty,
                    SelectedResidenceType,
                    SelectedBuyOrRent,
                    SelectedOrder,
                    MaxCount
                );

            try
            {
                //creates a slot for each residence
                foreach (Residence item in FavRes)
                {
                    token.ThrowIfCancellationRequested();
                    ResidenceSlot ResSlot = new(item, MainWindow, null, true);
                    await Dispatcher.InvokeAsync(() =>
                    {
                        ResidencesHolder.Children.Add(ResSlot);
                    });
                    await Task.Delay(5);
                }
            }
            catch (Exception ex)
            {
                //if failed, debug
                Console.WriteLine("Refreash Canceled");
            }
        }
        private void NextPage(object sender, RoutedEventArgs e)
        {
            //checl to see if there more residence available then the max count, that meaning it could be a second page
            if(FavRes.Count >= MaxCount)
            {
                //increase the page number and sets the page text
                PageNumber += 1;
                PageNumberT.Text = PageNumber.ToString();
                //increase the offset by the max count.
                //this way if there are 20 total residences and max 15
                //the second page would get all the residence starting from 15
                // so the second page would have 5 residences
                Offset += MaxCount;
            }
            //refreashes the slots
            Refreash();
        }
        private void Previous(object sender, RoutedEventArgs e)
        {
            //check if the offset is bigger then the max count
            //meaning if the max count is 15 offset is 20,the user is on the page 2, there are 30 favorite residences
            //there will be 10 residences on the page
            //if you want to go back a page, you will remove 15
            //from 20 to get at index 5, and read 15 pages from that index
            if (Offset >= MaxCount)
            {
                //remove 1 from the page
                PageNumber -= 1;
                //remove some offset
                Offset -= MaxCount;
            }
            else
            {
                //if there is less offset then the max count, we cant remove more
                //because we will go under 0 and we cant have a negative offset
                //so we set the page to 1 and the offset to 0
                PageNumber = 1;
                Offset = 0;
            }
            //updates the page number text
            PageNumberT.Text = PageNumber.ToString();
            //refreashes the residences
            Refreash();
        }

        private void CloseFav(object sender, RoutedEventArgs e)
        {
            //resets the offset and page number, clears the spawned slots to free up memory
            Offset = 0;
            PageNumber = 1;
            PageNumberT.Text = 1.ToString();
            ResidencesHolder.Children.Clear();
            //then hide the window
            MainWindow.RFavorites.Hide();
        }
        private void LoadPage(object sender, EventArgs e)
        {
            Refreash();
        }

        #region Combo boxes Controls
        //controlls the combo boxes,the same as in the main window
        private void LocationCityComboBoxLoaded(object sender, RoutedEventArgs e)
        {
            FilterLocationCity.SelectionChanged -= UpdateActiveFilterLocationCity;

            FilterLocationCity.ItemsSource = MainWindow.AvailableCitys;
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
            Refreash();
        }

        private async Task CountyComboBoxRefresh()
        {
            AvailableCountys = await DataBaseHandler.GetResidenceCountys(SelectedCity,"FavResidences");
            FilterLocationCounty.SelectionChanged -= UpdateActiveFilterLocationCounty;

            FilterLocationCounty.ItemsSource = AvailableCountys;
            FilterLocationCounty.SelectedIndex = 0;

            FilterLocationCounty.SelectionChanged += UpdateActiveFilterLocationCounty;

        }
        private void UpdateActiveFilterLocationCounty(object sender, SelectionChangedEventArgs e)
        {
            if (FilterLocationCounty.SelectedItem == null)
            {
                return;
            }
            SelectedCounty = FilterLocationCounty.SelectedItem.ToString();
            Refreash();
        }

        private void BuyOrRentComboBoxLoaded(object sender, RoutedEventArgs e)
        {
            FilterResidenceBuyOrRent.SelectionChanged -= UpdateActiveFilterBuyOrRent;

            FilterResidenceBuyOrRent.ItemsSource = MainWindow.BuyOrRent;
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
            MaxPriceComboBoxLoaded(sender, e);
            MinPriceComboBoxLoaded(sender, e);
            Refreash();
        }

        private void MinPriceComboBoxLoaded(object sender, RoutedEventArgs e)
        {
            FilterMinPrice.SelectionChanged -= UpdateActiveFilterMinPrice;

            if (SelectedBuyOrRent == "Buy")
            {
                FilterMinPrice.ItemsSource = MainWindow.BuyPrices;
            }
            else
            {
                FilterMinPrice.ItemsSource = MainWindow.RentPrices;
            }
            FilterMinPrice.SelectedIndex = 0;
            string price = "-1";
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
            try
            {
                price = FilterMinPrice.SelectedItem.ToString().Replace("k", "000").Replace("€", "");
            }
            catch
            {
                price = FilterMinPrice.SelectedItem.ToString().Replace("€", "");
            }
            SelectedMinPrice = int.Parse(price);
            Refreash();
        }

        private void MaxPriceComboBoxLoaded(object sender, RoutedEventArgs e)
        {
            FilterMaxPrice.SelectionChanged -= UpdateActiveFilterMaxPrice;

            if (SelectedBuyOrRent == "Buy")
            {
                FilterMaxPrice.ItemsSource = MainWindow.BuyPrices;
            }
            else
            {
                FilterMaxPrice.ItemsSource = MainWindow.RentPrices;
            }
            FilterMaxPrice.SelectedIndex = 9;
            string price = "-1";
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
            try
            {
                price = FilterMaxPrice.SelectedItem.ToString().Replace("k", "000").Replace("€", "");
            }
            catch
            {
                price = FilterMaxPrice.SelectedItem.ToString().Replace("€", "");
            }
            SelectedMaxPrice = int.Parse(price);
            Refreash();
        }

        private void ResidenceTypeComboBoxLoaded(object sender, RoutedEventArgs e)
        {
            FilterResidenceType.SelectionChanged -= UpdateActiveFilterResidenceType;

            FilterResidenceType.ItemsSource = MainWindow.ResidenceTyes;
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
            Refreash();
        }

        private void OrderComboBoxLoaded(object sender, RoutedEventArgs e)
        {
            FilterOrder.SelectionChanged -= UpdateActiveFilterOrder;

            FilterOrder.ItemsSource = MainWindow.Order;
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
            Refreash();
        }
        #endregion
    }
}
