using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Residence_Web_Scraper.SuportedWebsites
{
    //This is the base class for any future webscraper, each webscraper can target one website
    //each webscraper could have more variables to handle that specific websites, but here are the main
    //methods.
    
    public abstract class ResidenceWebScraperBase
    {
        //main pages from where the residences are collected
        protected List<HtmlDocument> WebSiteMainPages = new();
        //detected main pages (A Main page is a page that stores listings)
        protected int DetectedMainPages = 0;

        //Url modifier Location for the url
        protected string? MainWebSiteLocation;
        //Url modifier  type of residence House or Aparts
        protected string? MainWebSiteResidenceType;
        //Url modifier  type of acquisition rent or buy
        protected string? MainWebSiteBuyOrRent;

        //Url modifier PAge (The string that once added to the main link can change the page location)
        protected string? WebSitePageModifier;
        //active Main Page Link
        protected string? SelectedWebsiteLink;
        //link for Houses BUY
        protected string? MainWebSiteHousesPageLink;
        //Link for Houses RENT
        protected string? MainWebSiteHousesRentPageLink;
        //link for Apartaments BUY
        protected string? MainWebSiteApartamentsPageLink;
        //link for Apartaments RENT
        protected string? MainWebSiteApartamentsRentPageLink;

        //webclient for downloading pages
        protected static HtmlWeb hWeb = new();
        //main window refference
        protected MainWindow AppMainWindow;
        protected ResidenceWebScraperBase(MainWindow appMainWindow)
        {
            AppMainWindow = appMainWindow;
        }
        //change active Main website link based on residence type and buy or rent status
        protected void SelectTargetLink(string ResidenceType, string BuyOrRentStatus)
        {
            //if the residence type is a house
            if(ResidenceType == "Houses")
            {
                //asign the corect house link based on buy or rent
                if(BuyOrRentStatus == "Buy")
                {
                    SelectedWebsiteLink = MainWebSiteHousesPageLink;
                }
                else
                {
                    SelectedWebsiteLink = MainWebSiteHousesRentPageLink;
                }
            }
            //if its an apartament
            else
            {
                //asign the corect house link based on buy or rent
                if (BuyOrRentStatus == "Buy")
                {
                    SelectedWebsiteLink = MainWebSiteApartamentsPageLink;
                }
                else
                {
                    SelectedWebsiteLink = MainWebSiteApartamentsRentPageLink;
                }
            }
        }
        /// <summary>
        /// Main method called from the handler to get the residence
        /// </summary>
        /// <returns></returns>
        public abstract Task<bool> GetWebsiteResidencesAsync(string City,string ResidenceType,string BuyOrRentStatus,CancellationToken token);
        /// <summary>
        /// Downloads the first page and gets the number of remaining available pages
        /// </summary>
        /// <returns></returns>
        protected abstract Task<bool> DownloadFirstPageAsync(CancellationToken token);
        /// <summary>
        /// Download the rest of available pages 
        /// </summary>
        /// <returns></returns>
        protected abstract Task<bool> DownloadLeftPagesParallelAsync(CancellationToken token);
        /// <summary>
        /// Gets all the residence links from available downloaded pages
        /// </summary>
        /// <returns></returns>
        protected abstract Task<bool> ProcessMainPagesParallelAsync();
        /// <summary>
        /// Downloads all found residence pages from founded links
        /// </summary>
        /// <param name="startingIndex"></param>
        /// <returns></returns>
        protected abstract Task<bool> DownloadResidencePagesParallelAsync(CancellationToken token);
        /// <summary>
        /// Clear the links that dont have a page downloaded in case of user canceling the download
        /// </summary>
        /// <returns></returns>
        protected abstract Task<bool> ClearUndownloadedLinksAsync();
        /// <summary>
        /// Process each residence downloaded page and make a residence class instance with founded info then add to list
        /// </summary>
        /// <returns></returns>
        protected abstract Task<bool> ProcessResidenceInfoAsync(CancellationToken token);
        /// <summary>
        /// Processes a residence page and returns it
        /// </summary>
        /// <param name="progress"></param>
        /// <returns></returns>
        protected abstract Task<Residence> ProcessSingleResidence(int index,HttpClient Client, CancellationToken token);
        /// <summary>
        /// Clear downloaded information
        /// </summary>
        public abstract void ClearWebScraperInfo();

        /// <summary>
        /// Downloads a document from the web using Url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        protected static async Task<HtmlDocument> GetDocument(string url,string? message)
        {
            //creates a new document
            HtmlDocument doc = new();
            try
            {
                //tries to download it
                doc =  await hWeb.LoadFromWebAsync(url).ConfigureAwait(false); 
                //when finished log a message or a default message
                if(message is null)
                {
                    Console.WriteLine("Download Page Finished");
                }
                else
                {
                    Console.WriteLine($"Download Finished: {message}");
                }
            }
            catch(Exception e)
            {
                //if failed  debug
                Console.WriteLine($"GetDocument Error: {e.Message}");
            }
            return doc;
        }
    }
}
