using HtmlAgilityPack;
using Residence_Web_Scraper.HandlerClasses;
using Residence_Web_Scraper.Interfaces;
using Residence_Web_Scraper.WPFpages;
using Residence_Web_Scraper.WPFpages.PopUpsPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Residence_Web_Scraper.SuportedWebsites
{
    public class EXAMPLEWebScraper : ResidenceWebScraperBase
    {
        //This is an example of a real Webscraper class that was configurated for a specific website
        //but with details deleted for legal reasons.
        //it uses the default order of methods that is visible in the parent.
        //But a little different and with some more methods specific for this case
        //for example residence listings on this website might redirect
        //the user to another website so it needs special checks and more xpaths to handle the link page being from
        //2 websites, also the Main List that holds the link and the document page has another string
        //to hold the county for that specific page because the county is only visible in the main page 
        //the default way would be to get all the residence listings from one main page.
        //then download all the residence listings page and get all the info needed from those pages
        //but in this case the county is not visible anymore in the downloaded residence page.

        //Each website is a little different, so each webscrape would be a little bit different
        //so once you made the webscrapers for each website you want you can further break down the methods
        //and move them in to the parent.

        //Also for a better webscraper you could try to implement a headles broswer, that would give you
        //the posibility to also get the javascript not just the html.
        //So you would have to download a lot less pages and in general would be a lot faster

        //dictionary that holds the url, and a touple with the downloaded residence page document and the County that is specific for this case
        //in a normal scenario it would only be a dictionary with a string and a htmldocument
        private Dictionary<string, (HtmlDocument?, string)> ResidencePages = new();
        public EXAMPLEWebScraper(MainWindow appMainWindow) : base(appMainWindow) { }

        //main method called from handler
        public override async Task<bool> GetWebsiteResidencesAsync(string city, string residenceType, string buyOrRent,CancellationToken token)
        {

            //----------Creating pop-up warning----------------
            //delay so the previous popup has time to close
            await Task.Delay(1000);
            StatusHandler.UpdateAppStatus("Waiting for popup...");
            IPopUp PopUpContent = new WarningTextPopUp
            (
                @"The app cant and should not run with the example webscraper,make your own webscraper class using the example and add it inside the ResidenceWebScrapingHandler"

            );
            //creates a new popup with the content above
            PopUpPage PopUp = new
                (
                 AppMainWindow.PopUpFrame,
                 PopUpContent
                );
            //attach the popup to the main window popup holder
            AppMainWindow.PopUpFrame.Content = PopUp;
            //pause the code and wait for popup result
            bool? popupResult = await PopUp.WaitForAnswerAsync();
            //handle the result of the popup
            //if the proceede button was pressed continue the download cycle.
            
            //----------Creating pop-up warning----------------

            //Returning because this is an example class, executing will give errors 
            return true;


            //asign the correct variables for this webscraper instance
            WebSitePageModifier = "&page=";
            MainWebSiteLocation = city.ToLower();
            MainWebSiteResidenceType = residenceType;
            MainWebSiteBuyOrRent = buyOrRent;

            //asign the correct links, this are examples for how the links should look like on the target website
            MainWebSiteHousesPageLink = $"https://www.examplewebsite/houses-for-sale{MainWebSiteLocation}-judet/?currency=EUR";
            MainWebSiteHousesRentPageLink= $"https://www.examplewebsite/houses-for-rent{MainWebSiteLocation}-judet/?currency=EUR";
            MainWebSiteApartamentsPageLink = $"https://www.examplewebsite/appartaments-for-sale{MainWebSiteLocation}-judet/?currency=EUR";
            MainWebSiteApartamentsRentPageLink = $"https://www.examplewebsite/appartaments-for-rent{MainWebSiteLocation}-judet/?currency=EUR";

            //calls parent method to select the active link based on 2 filter options
            SelectTargetLink(residenceType,buyOrRent);

            try //return bool on the main method, and the smaller methods and use them for retry, return false if the methods faill
            {
                //starts the sequence of webscraping the targeted website
                //downloads first page add the url and the page in the list
                //and then look for the max number of pages at the bottom of the page
                //Example: 1,2,3.......15
                //Max number of pages in this example is 15
                if(!await DownloadFirstPageAsync(token)) return false;
                //download left pages,page 2,3,4,5...15 then add the url and the page in the list
                if (!await DownloadLeftPagesParallelAsync(token)) return false;
                //loop trough each downloaded main page and get the link and county then adds them to the main list 
                //with NULL for the htmldocument because it will be recived in the next steps
                if(!await ProcessMainPagesParallelAsync()) return false;
                //Loops trough the main list, get the links then downloads and asigns the htmldocument to the link
                if(!await DownloadResidencePagesParallelAsync(token)) return false;
                //the download process was skiped after some time, there would be links with null as htmldocument
                //this method clears those and leave only items that dosent have a null htmldocument
                if(!await ClearUndownloadedLinksAsync()) return false;
                //loops trough each htmldocument in the main list and collects the needed data from each page
                //creates residence classes with the collected data and adds them to a list.
                if(!await ProcessResidenceInfoAsync(token)) return false;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Canceled Downloading Data: {e.Message}");
            }

            //change the status of the app to Idle
            StatusHandler.UpdateAppStatus("Idle...");
            Console.WriteLine("Example Scraper Finished");
            //returns true so the webscraping handler will continuie with the next webnscraper class in its list
            return true;
        }
        protected override async Task<bool> DownloadFirstPageAsync(CancellationToken token)
        {
            //updates the app status and sets the status bar at 40
            Console.WriteLine("--------------Download First page--------------");
            StatusHandler.UpdateAppStatus("Download & Process First ExampleWebsite Page...");
            StatusHandler.UpdateStatusBar(40);
            Console.WriteLine($"Downloading: {SelectedWebsiteLink}");
            //downloads and adds the main page to the main list
            WebSiteMainPages.Add(await GetDocument(SelectedWebsiteLink, $"Finished: {SelectedWebsiteLink} 0"));
            //return false if it was not able to get the first page
            if(WebSiteMainPages.Count == 0)
            {
                return false;
            }
            //sets the status bar at 90
            StatusHandler.UpdateStatusBar(90);
            //Gets the total number of pages passing the just downloaded page and return if it succeded.
            //if it didnt succeed then stop webscraper and return false
            if(!GetNumberOfTotalVisiblePages(WebSiteMainPages[0])) return false;
            //checks for cancelation token
            token.ThrowIfCancellationRequested();
            return true;
        }
        protected override async Task<bool> DownloadLeftPagesParallelAsync(CancellationToken token)
        {
            //logs and uptates the status of the app
            Console.WriteLine("--------------Download left pages--------------");
            StatusHandler.UpdateAppStatus("Download Left Example Pages..");

            //A list of tasks for multithreading that each retuns a htmldocument
            List<Task<HtmlDocument>> DownloadPagesTasks = new List<Task<HtmlDocument>>();

            //Downloads all the pages at the same time
            for (int i = 2; i <= DetectedMainPages; i++)
            { 
                //stores the index to avoid a bug
                int index = i;
                //Creates the link with the selected main page + page modifier and + the index 
                //to get the correct link
                string link = $"{SelectedWebsiteLink}{WebSitePageModifier}{index}";
                //Creates a task that downloads the targeted link and adds it to the list
                DownloadPagesTasks.Add(Task.Run(() => GetDocument(link, link)));
                Console.WriteLine($"Downloading: {SelectedWebsiteLink}{WebSitePageModifier}{i}");//dell
            }
            //Calls the status handler and waits for all tasks tu finish while also updates the status bar
            List<HtmlDocument> result = await StatusHandler.UpdateStatusBasedOnTaskList(DownloadPagesTasks).ConfigureAwait(false);
            //checks for cancelation token
            token.ThrowIfCancellationRequested();
            //adds the result list of the pages in to the Main Page list
            WebSiteMainPages.AddRange(result);
            Console.WriteLine($"Total Example main Pages = {WebSiteMainPages.Count}");
            return true;
        }
        protected override async Task<bool> ProcessMainPagesParallelAsync() 
        {
            //updates and logs the status
            Console.WriteLine("------------Proces residence links----------------");
            Console.WriteLine("Download Residence Page Links");
            StatusHandler.UpdateAppStatus("Process Residence Page Links...");

            //creates another list of tasks for multithreading that each returns a htmlnodecollection 
            List<Task<HtmlNodeCollection>> SelectNodesTasks = new();
            //foreach main page adds a task to select all the residence listings on that page.
            foreach(HtmlDocument hd in WebSiteMainPages)
            {
                SelectNodesTasks.Add(Task.Run(() =>
                {
                    return hd.DocumentNode.SelectNodes("//div[@class='css-1sw7q4x']/a");
                }));
            }
            //Calls the status handler and waits for all tasks tu finish while also updates the status bar
            List<HtmlNodeCollection> result = await StatusHandler.UpdateStatusBasedOnTaskList(SelectNodesTasks).ConfigureAwait(false);
            //for each colection of residence listings on each page
            foreach (HtmlNodeCollection NodeColl in result) //add null check
            {
                Console.WriteLine($"Total Residence links: {ResidencePages.Count}");
                //for each residence listing in each collection of residence
                foreach(HtmlNode Node in NodeColl)
                {
                    //retrives the link of the residence listing
                    string Link = Node.Attributes[name: "href"].Value;
                    //if the link is not complete ,completes the link by adding the website domanin (Special case)
                    if (Link.StartsWith("/d/"))
                    {
                        Link = Link.Insert(0, "https://www.ExamplePage.com");
                    }
                    //adds the link and null for the htmldocument it will be downloaded later
                    //then adds the county (special case)
                    ResidencePages[Link] = (null, Node.SelectSingleNode(".//div[3]/p").InnerText.Split(" ")[0]);//null
                }
            }
            return true;
        }
        //total number of downloaded residence listing pages.
        int TotalDownloadedPages = 0;
        protected override async Task<bool> DownloadResidencePagesParallelAsync(CancellationToken token)
        {
            //updates and logs the status of the app
            Console.WriteLine("---------------Download Residence Pages-------------");
            StatusHandler.UpdateAppStatus("Download Residence Pages...");
            //calls the nested medhod to download the pages, and passes the index where to start in the list
            await DownloadNextPages(0, token);

            //while the total downloded pages is smaller then the total pages that needs to be downloaded
            while (TotalDownloadedPages < ResidencePages.Count)
            {
                //Check the settings if it should throw a popup each downloading cycle.
                if(SettingsHandler.IsMbConsumtionWarningActive) 
                {   
                    //update app status and create a new popup content
                    StatusHandler.UpdateAppStatus("Waiting for popup...");
                    IPopUp PopUpContent = new WarningTextPopUp
                    (
                        @"Check if your vpn has around 200 mb of remaining data, 
                          if you have a subscription with unlimited data you can disable this popup in settings"

                    );
                    //creates a new popup with the content above
                    PopUpPage PopUp = new
                        (
                         AppMainWindow.PopUpFrame,
                         PopUpContent
                        );
                    //attach the popup to the main window popup holder
                    AppMainWindow.PopUpFrame.Content = PopUp;
                    //pause the code and wait for popup result
                    bool? popupResult = await PopUp.WaitForAnswerAsync();
                    //handle the result of the popup
                    //if the proceede button was pressed continue the download cycle.
                    if (popupResult.Value)
                    {
                        //continue the download cycle and pass the number of  downloaded pages
                        //to continue from that index
                        await DownloadNextPages(TotalDownloadedPages,token);
                    }
                    else
                    {
                        //else break
                        break;
                    }
                }
                //if the popup setting was disabled just continue the download cycle
                else
                {
                    //continue the download cycle and pass the number of  downloaded pages
                    //to continue from that index
                    await DownloadNextPages(TotalDownloadedPages, token);
                }
            }

            //download method that asks for a starting index from where to start to download
            //from the list of residence page links
            async Task DownloadNextPages(int startingIndex, CancellationToken token)
            {
                //create a tasks list 
                List<Task> DownResTasks = new();
                //set a loops to keep count of how many loops this method ran
                //also set the max numbers of loops
                //in this case the cycle will download 50 websites at the same time until
                //the total number of websites downloaded is the same as the total availabe links
                int Loops = 0;
                int MaxLoops = 50;

                //starts at the starting index and run until the max loops is reached,
                //or there is no links to be downloaded anymore
                for (int x = startingIndex; x < ResidencePages.Count; x++)
                {
                    //save the index and link to avoid a bug
                    int index = x;
                    string link = ResidencePages.Keys.ElementAt(index);

                    //debug thing
                    Console.WriteLine($"Downloading page number {index}");//dell

                    //add the task that downloads the page to the list
                    DownResTasks.Add(Task.Run(async () =>
                    {
                        HtmlDocument htmlDoc = await GetDocument(link, $"{index}").ConfigureAwait(false);
                        //lock the residence pages dictionary so there is no multiple writes at the same time
                        lock (ResidencePages)
                        {
                            ResidencePages[link] = (htmlDoc, ResidencePages[link].Item2);
                        }
                    }));
                    //increase the loop
                    Loops++;
                    //if the loops are the same as the max loops then break 
                    //to that the cycle continues with 50 downloads at a time.
                    if (Loops == MaxLoops)
                    {
                        break;
                    }
                }
                //add the loops to the total downloaded pages
                TotalDownloadedPages += Loops;
                //update the status bar based on total downloaded pages and the available links
                StatusHandler.UpdateStatusBar((TotalDownloadedPages * 100) / ResidencePages.Count);

                //wait for all tasks to finish
                await Task.WhenAll(DownResTasks);

                //check for cancelation token
                token.ThrowIfCancellationRequested();

                //debug stuff
                Console.WriteLine($"ExampleWebsite Residence Total Links {ResidencePages.Count}");//dell
                Console.WriteLine($"ExampleWebsite Residence Total Page Downloaded {TotalDownloadedPages}");//dell
            }
            return true;
        }
        protected override async Task<bool> ClearUndownloadedLinksAsync()
        {
            Console.WriteLine("-------------Clearing Undownloaded Links---------------");
            //clear the undownloaded links from the dictionary toa void null refferences.
            //this happens because the user can choose to not download all available pages
            await Task.Run(() => 
            {
                //creates a new dictionary
                Dictionary<string, (HtmlDocument?,string)> CleanResidencePages = new();
                //clear any element that has a nulll htmldocument
                foreach (var item in ResidencePages)
                {
                    if (item.Value.Item1 is not null)
                    {
                        CleanResidencePages.Add(item.Key, item.Value);
                    }
                }
                //asign the cleaned dictionary to the main dictionary
                ResidencePages = CleanResidencePages;
            });
            return true;
        }
        //an int to save the total number of downloaded residence pages that has succesfuly convertet to Residence class
        int TotalProcessedPages = 0;
        protected override async Task<bool> ProcessResidenceInfoAsync(CancellationToken token)
        {
            //debug and update app status
            Console.WriteLine("-------------Process Residence Info---------------");
            StatusHandler.UpdateAppStatus("Process Residence Info...");
            //while the total processed residence pages is smaller then the available residence pages
            //call the process residence method to process 50 residences at the same time
            while(TotalProcessedPages < TotalDownloadedPages)
            {
                //calls the nested method with the amount of processed pages as a index to continue processing 
                await ProcessNextResidences(TotalProcessedPages, token);
            }
            //process a single residence page using the index to acces the page in the dictionary
            async Task ProcessNextResidences(int startingIndex, CancellationToken token)
            {
                //create a list of tasks to process multiple pages at once
                List<Task<Residence>> ResidenceTasks = new();
                //debug
                Console.WriteLine($"Starting Index {startingIndex}");
                //store the loops and the max loops to make a cicle of 50 processed pages at once
                int Loops = 0;
                int MaxLoops = 50;
                //use a httpclient
                using (HttpClient Client = new())
                {
                    //starts at the starting index and continue untill them ax loops is reached
                    //or there is no available residence pages to process
                    for (int x = startingIndex; x < TotalDownloadedPages; x++)
                    {
                        //save index to avoid bg=ug
                        int index = x;
                        //add the method to process a single page to the list passing the index
                        ResidenceTasks.Add(Task.Run(() =>ProcessSingleResidence(index, Client, token)));
                        //increase loops
                        Loops++;
                        //stop if the loops reached the max loops
                        if (Loops == MaxLoops)
                        {
                            break;
                        }
                    }
                    //wait for the result
                    var result = await Task.WhenAll(ResidenceTasks);
                    //check if the cancelation was reuqested
                    token.ThrowIfCancellationRequested();
                    //convert the result to a list of residences
                    List<Residence> FinishedRes = result.ToList();
                    //waits for the databasehandler to write or update the residence to the db
                    await DataBaseHandler.AddResidences(FinishedRes);
                }
                //increase the total processed with the loops
                TotalProcessedPages += Loops;
                //update status bar with the total processed pages and the total number of available pages
                StatusHandler.UpdateStatusBar((TotalProcessedPages * 100) / TotalDownloadedPages);
                //debug
                Console.WriteLine($"Total Res count = {TotalProcessedPages} Available Res = {TotalDownloadedPages}");
            }
            return true;
        }




        //---------------------------------------Secondary Methods-------------------------------------

        //process a single residence page
        protected override async Task<Residence> ProcessSingleResidence(int index,HttpClient Client, CancellationToken token)
        {
            //Acceses the url using the index 
            string? url = $"{ResidencePages.Keys.ElementAt(index)}";
            //Downloads the image from the page
            byte[]? image = await GetRImage(url, Client);
            //get the title from the page
            string? title =  GetRTitle(url);
            //get the price from the page
            int price =  GetRPrice(url);
            //use the main website residence type as the residence type for this residence
            string? residenceType = MainWebSiteResidenceType;
            //use the main website buy or rent as the residence buy rent for this residence
            string? buyOrRent = MainWebSiteBuyOrRent;
            //sets the website name for this webscraper class
            string? website = "ExampleWebsite";
            //use the main website location for the city of this residence convert the first letter to upper, needs more work
            string? City = MainWebSiteLocation[0].ToString().ToUpper() + MainWebSiteLocation.Substring(1); //problm with 2 words places
            //use the county ghatered at the same time with the link for this specific example website
            string? County= ResidencePages[url].Item2;
            //get square metters
            int M2 = GetRM2(url);
            //assemble everything in a residence and return it
            Residence Res = new(url,image,title,price, residenceType,buyOrRent,M2,website,City,County);
            return Res;
        }
        
        #region Get residence info GetR methods

        private async Task<byte[]> GetRImage(string pageli,HttpClient client)
        {
            //saves the url
            string residencePageLink = pageli;
            //url for the photo
            string photoLink = string.Empty;
            //html node where the link of the photo is
            HtmlNode? ImageNode = null;

            //find witch xpath to use depending on where the specific residence listing is hosted, sometimes
            //the listing would apear on the targeted website, but once accesed the link it would redirect the app to another website
            //so the listing is shown in a website, but hosted on another website
            //so here we check if the downloaded page is from the normal webpage, or if it was redirected to another website
            // to be sure witch xpaths to use
            if (residencePageLink.Contains("https://www.ExampleWebsite.com"))
            {
                //checks if the page htmldocument is null
                if(ResidencePages[residencePageLink].Item1 is null)
                {
                    Console.WriteLine("ERROR geting Image element, HtmlDocument null");
                    //return an empty byte array for the image
                    return Array.Empty<byte>();
                }
                //selects the image node from the downloaded page
                ImageNode = ResidencePages[residencePageLink].Item1?.DocumentNode.SelectSingleNode(".//div[1]/div[@class='swiper-zoom-container']/img");
                //check if its null
                if(ImageNode is null)
                {
                    //if yes return an empty byte array
                    Console.WriteLine("ERROR geting Image Link, ImageNode null ExampleWebsite");
                    return Array.Empty<byte>();
                }
                //if not null get the link from the src tag
                photoLink = ImageNode.Attributes[name: "src"].Value;
            }
            //handle the residence page listing being hosted on another website
            else
            {
                //checks if the page htmldocument is null
                if (ResidencePages[residencePageLink].Item1 is null)
                {
                    Console.WriteLine("ERROR geting Image element, HtmlDocument null");
                    //return an empty byte array for the image
                    return Array.Empty<byte>();
                }
                //selects the image node from the downloaded page
                ImageNode = ResidencePages[residencePageLink].Item1?.DocumentNode.SelectSingleNode(".//div/picture/img");
                //check if its null
                if (ImageNode is null)
                {
                    ImageNode = ResidencePages[residencePageLink].Item1?.DocumentNode.SelectSingleNode(".//div/div/div[1]/div[2]/div/div[1]/img");
                    if(ImageNode is null)
                    {
                        Console.WriteLine("ERROR geting Image Link, ImageNode null Storia");
                        //if yes return an empty byte array
                        return Array.Empty<byte>();
                    }
                }
                //if not null get the link from the src tag
                photoLink = ImageNode.Attributes[name: "src"].Value;
            }
            //creates a httm response to recive the image
            HttpResponseMessage img;
            try
            {
                //try to download the image using the url from the photolink variable
                img = await client.GetAsync(photoLink);
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR Download Residance Image: + {e.Message}");
                //if it failed return an empty byte array
                return Array.Empty<byte>();
            }
            //if it succesed convert the image to byte array to store in the database
            byte[] Image = await img.Content.ReadAsByteArrayAsync();
            //returns the image byte array
            return Image;
        }
        private string GetRTitle(string pageli)
        {
            //saves the page link
            string PageLink = pageli;
            //retrive the residence page from the list using the url as index
            HtmlDocument? Page = ResidencePages[PageLink].Item1;
            //creates an empty title string
            string title = "Title ERROR";
            //create a null node that contains the title
            HtmlNode? TitleNode;
            //tries to find what website the page came from using the page url to know witch xpath to use
            if (PageLink.Contains("https://www.ExampleWebsite.com"))
            {
                //uses a xpath to try to find the title in the page
                TitleNode = Page.DocumentNode.SelectSingleNode(".//div[2]/h1");
                //checks to see if it found the correct node
                if(TitleNode is null)
                {
                    //if not, remove and return the empty title
                    Console.WriteLine($"ERROR Geting Title, TitleHtmlNode null ExampleWebsite");
                    return title;
                }
                //if the title node is not null get the inner text as the title
                title = TitleNode.InnerText;
            }
            //handle the residence page listing being hosted on another website
            else
            {
                //use anoter xpath for the specific website
                TitleNode = Page.DocumentNode.SelectSingleNode(".//header/h1");
                //if the title node was not found
                if (TitleNode is null)
                {
                    Console.WriteLine($"ERROR Geting Title, TitleHtmlNode null Storia");
                    //return the empty title
                    return title;
                }
                //else retrive the inner text of the node as the title
                title = TitleNode.InnerText;
            }
            //return title
            return title;
        }
        private int GetRPrice(string pageli)
        {
            //save the page url
            string PageLink = pageli;
            //set the page price to -1 meaning the price was not found as a default
            int price = -1;
            //gets the page from the list as a variable
            HtmlDocument? Page = ResidencePages[PageLink].Item1;
            //creates a string that would hold the returned text with the price from the websites
            string pricetext = "Price ERROR";
            //creates an empty node for the price
            HtmlNode? PriceNode;
            //checks witch xpath to use depending on where is the residence listing hosted

            if (PageLink.Contains("https://www.ExampleWebsite.com"))
            {
                //the retrive the node containing the price using the xpath for this case
                PriceNode = Page.DocumentNode.SelectSingleNode(".//div[3]/h3");
                //checks to see if a node was found
                if(PriceNode is null)
                {
                    Console.WriteLine($"ERROR Getting Price, PriceNode is null ExampleWebsite");
                    //return the default value as a price (-1) meaning no price found
                    return price;
                }
                //if an node was found get the inner text
                pricetext = PriceNode.InnerText;
                //Try replaceing characters to get only numberss
                pricetext = pricetext.Replace("€", "").Replace(" ", "").Replace("RON", "").Replace("lei", "").Replace("Schimb","0");
                try
                {
                    //tries parsing the text in an int
                    price = int.Parse(pricetext);
                }
                catch (Exception e)
                {
                    //debug
                    Console.WriteLine($"ERROR Converting ExampleWebsite Price {e.Message} Price: {pricetext}");
                }
            }
            //handle the residence page listing being hosted on another website
            else
            {
                //the retrive the node containing the price using the xpath for this case
                PriceNode = Page.DocumentNode.SelectSingleNode(".//header/strong");
                //checks to see if a node was found
                if (PriceNode is null)
                {
                    Console.WriteLine($"ERROR Getting Price, PriceNode is null Storia");
                    //return the default value as a price (-1) meaning no price found
                    return price;
                }
                //if an node was found get the inner text
                pricetext = PriceNode.InnerText;
                //Try replaceing characters to get only numberss
                pricetext = pricetext.Replace("€", "").Replace(" ", "").Replace("RON", "").Replace("lei", "").Replace("Schimb", "0");
                try
                {
                    //tries parsing the text in an int
                    price = int.Parse(pricetext);
                }
                catch (Exception e)
                {
                    //debug
                    Console.WriteLine($"ERROR Converting Storia Price {e.Message} Price: {pricetext}");
                }
            }
            //return the price
            return price;
        }
        private int GetRM2(string pageli)
        {
            //saves the page url
            string PageLink = pageli;
            //gets the htmldocument from the list using the url
            HtmlDocument? Page = ResidencePages[PageLink].Item1;
            //an int with the square metters with -1 as default meaning nothing found
            int m2 = -1;
            //creates a empty htmlnode
            HtmlNode? M2Node;
            //creates an empty 
            string m2string = "";

            //checks witch xpath to use depending on where is the residence listing hosted
            if (PageLink.Contains("https://www.ExampleWebsite.com"))    
            {
                //the node containing the price is changing depending on the number of provided informations
                //in the website there can be a maximum of 5 boxes in the page with information
                //if only 2 informations are specified by the sellar then only 2 nodes will be
                //if everything is specified by the seller then all 5 places will contain information about the residence
                //so we check each node ot of 5 for the "m2" text specifying the square metters of the residence

                //we first check the first node with this xpath
                //we check if the node was found and if the text inside contains "m2"
                //if not, we continue with the next xpath
                M2Node = Page.DocumentNode.SelectSingleNode(".//ul/li[1]/p");
                if(M2Node is null || !M2Node.InnerHtml.Contains("m²"))
                {
                    M2Node = Page.DocumentNode.SelectSingleNode(".//ul/li[2]/p");
                    if (M2Node is null || !M2Node.InnerHtml.Contains("m²"))
                    {
                        M2Node = Page.DocumentNode.SelectSingleNode(".//ul/li[3]/p");
                        if (M2Node is null || !M2Node.InnerHtml.Contains("m²"))
                        {
                            M2Node = Page.DocumentNode.SelectSingleNode(".//ul/li[4]/p");
                            if (M2Node is null || !M2Node.InnerHtml.Contains("m²"))
                            {
                                M2Node = Page.DocumentNode.SelectSingleNode(".//ul/li[5]/p");
                                if (M2Node is null || !M2Node.InnerHtml.Contains("m²"))
                                {
                                    //if all xpath failed we return the default m2 variable (-1) meaning no  square metters detected
                                    //this can happen if the seller didnt provide that information on the page
                                    //or a random difference in the page layout that makes the xpaths location incorect
                                    Console.WriteLine($"ERROR Getting M2, M2Node is null ExampleWebsite");
                                    return m2;
                                }
                            }
                        }
                    }
                }
                //if we found a node that contains the "m2" text inside we save it in to the m2node variavble
                //We then loop trough all characters in the node inner text to find numbers
                foreach (char c in M2Node.InnerText)
                {
                    if (Char.IsDigit(c))
                    {
                        //if a number is detected in the inner text of the node we add it to the string
                        //that represents the number
                        m2string += c;
                    }
                }
            }
            //checks witch xpath to use depending on the where is the residence listing hosted
            else
            {
                //if the residence is hosted on the other website, then is more simpler because the square metters
                //is always shown in the same xpath not like the other case above where it could be in 5 xpath depending on the
                //number of information provided by the seller

                //selects the node that contains the square metters number
                M2Node = Page.DocumentNode.SelectSingleNode(".//div[3]/div[2]/div[1]/div/div[2]/div[2]/div");
                //check if the node was found
                if(M2Node is null)
                {
                    //if not return the default square metters (-1) meaning nothing found
                    Console.WriteLine($"ERROR Getting M2, M2Node is null Storia");
                    return m2;
                }
                //if found check again the inner text for numbers
                foreach (char c in M2Node.InnerText)
                {
                    if (Char.IsDigit(c))
                    {
                        //add the numbers to the string
                        m2string += c;
                    }
                }
            }
            //convert the string to int
            m2 = int.Parse(m2string);
            //return the square metters
            return m2;
        }
        #endregion
        private bool GetNumberOfTotalVisiblePages(HtmlDocument page)
        {
            //gets the node that contains the number of pages from the given Main Page
            HtmlNode FirstExampleWebsitePageNumbers = page.DocumentNode.SelectSingleNode("//section/div/ul");
            if(FirstExampleWebsitePageNumbers == null)
            {
                return false;
            }
            //creates a  colection of node that each contains a number from the pages
            //for example at the bottom of the page might look like this
            //1 2 3......15
            //each number is a different node that can be pressed and redirects the user to the corect Main Page
            //the htmlnode colection contains all those nodes that can be pressed and contains a number
            //in this case 1,2,3,.,.,.,.,.,15 
            HtmlNodeCollection FirstExampleWebsitePageNumbersNodes = FirstExampleWebsitePageNumbers.SelectNodes(".//li");//addcheck for null

            //foreach node inside the node collection
            foreach (HtmlNode ExampleWebsitePageNumber in FirstExampleWebsitePageNumbersNodes)
            {
                //creates a int to store the bigest number in the list,initialized with 0
                //the biggest number would be the total number of available Main Pages
                int DetectedPageNr = 0;
                //placed in a try so it wont trhow an error when trying to parse points to numbers
                try
                {
                    //checks if the number is bigger the detected main page number
                    //if yes, sets that number as the Main page number
                    if (DetectedPageNr < int.Parse(ExampleWebsitePageNumber.InnerText))
                    {
                        DetectedPageNr = int.Parse(ExampleWebsitePageNumber.InnerText);
                    }
                }
                catch
                {
                }
                //sets the Main Pages number to the detected one
                DetectedMainPages = DetectedPageNr;
            }
            return true;
        }
        //cleares the webscraper 
        public override void ClearWebScraperInfo()
        {
            WebSiteMainPages.Clear();
            ResidencePages.Clear();
            DetectedMainPages = 0;
            TotalDownloadedPages = 0;
            TotalProcessedPages = 0;
            //maybe force a garbace collection?
        }

    }
}
