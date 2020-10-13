using Rocket.API;

namespace Lafalafa.AutoBus
{
    public class AutoBusConfiguration : IRocketPluginConfiguration
    {

        public int Payment { get; set; }
        public int BusID { get; set; }
        public string BusGroupID { get; set; }
        public string ImageUrl { get; set; }
        public bool UseXP { get; set; }



        public void LoadDefaults()
        {

            BusID = 86;
            BusGroupID = "Colectivero";
            ImageUrl = "https://cdn.discordapp.com/attachments/707443525913935943/750888002187821086/autobus.png";
            Payment = 40;
            UseXP = false;
        }
    }
}