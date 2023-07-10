using Residence_Web_Scraper.Interfaces;
using Residence_Web_Scraper.SuportedWebsites;
using Residence_Web_Scraper.WPFpages.PopUpsPages;
using Residence_Web_Scraper.WPFpages;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Residence_Web_Scraper.HandlerClasses
{
    public static class ResidenceWebScrapinghandler
    {
        //reff to the main window
        private static MainWindow? AppMainWindow;
        //list of webscrapers
        private static List<ResidenceWebScraperBase> ResidenceScrapers = new();
        //initialize component
        public static void InitializeComponent(MainWindow appMainWindow)
        {
            //asign the main window
            AppMainWindow = appMainWindow;
            //ad the test webscraper to the list
            //Each webscraper made should be added here to be ran.
            ResidenceScrapers.Add
                (
                    new EXAMPLEWebScraper(appMainWindow: AppMainWindow)

                );
        }
        //main method for webscraping for residences
        public static async Task GetResidences(string City, string ResidenceType, string BuyOrRentStatus,CancellationToken token)
        {
            //updates the status bar to 0
            StatusHandler.UpdateStatusBar(0);

            //loops trough the webscrapers and calls their main method
            foreach(ResidenceWebScraperBase Scraper in ResidenceScrapers)
            {
                //Make a bool to store the succes of the main nmethod.
                //if the succese is false call again the the webscraper
                //maybe add popup to continue or not
                bool NextScraper = false;

                while(NextScraper ==  false)
                {
                    NextScraper = await Scraper.GetWebsiteResidencesAsync(City, ResidenceType, BuyOrRentStatus, token);
                    //reset scraper.
                    Scraper.ClearWebScraperInfo();

                    //if webscraper failed show popup 
                    if (!NextScraper)
                    {
                        //create a new popup content
                        IPopUp PopUpContent = new WarningTextPopUp
                            (
                                @"Webscraper failed, do you want to skip? This can happen because of bugs or blocked ip, try restarting the app and chose another vpn location"
                            );
                        //create a new popup and pass the content of the popup and the frame is atached to
                        PopUpPage WarningPopUpPage = new

                            (
                                AppMainWindow.PopUpFrame,
                                PopUpContent
                            );
                        //asign popup to the popup frame
                        AppMainWindow.PopUpFrame.Content = WarningPopUpPage;

                        //wait for imput from popup to continue or break
                        if((bool)await WarningPopUpPage.WaitForAnswerAsync())
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}
