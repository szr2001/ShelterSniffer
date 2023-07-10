
namespace Residence_Web_Scraper
{
    public class Residence
    {
        public string? Url { get; set; }
        public byte[]? Image { get; set; }
        public string? Title { get; set; }
        public int Price { get; set; }
        public string? City { get; set; }
        public string? County { get; set; }
        public string? ResidenceTye { get; set; }
        public string? BuyOrRent { get; set; }
        public int? M2 { get; set; }
        public string? Website { get; set; }
        public Residence()
        {

        }
        public Residence(string? url, byte[]? image, string? title, int price, string? residenceTye,string buyOrRent,int m2, string? website, string? city, string? county)
        {
            Url = url;
            Image = image;
            Title = title;
            Price = price;
            ResidenceTye = residenceTye;
            BuyOrRent = buyOrRent;
            Website = website;
            City = city;
            M2 = m2;
            County = county;
        }
    }
}
