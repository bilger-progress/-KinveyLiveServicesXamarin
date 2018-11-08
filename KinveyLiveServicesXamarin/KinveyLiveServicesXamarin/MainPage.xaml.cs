using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Kinvey;

namespace KinveyLiveServicesXamarin
{
    public partial class MainPage : ContentPage
    {
        private Client kinveyClient = null;

        public MainPage()
        {
            InitializeComponent();

            // Set-up the Client Builder.
            Client.Builder builder = new Client.Builder("", "")
                .SetFilePath(DependencyService.Get<ISQLite>().GetPath());
            // .setOfflinePlatform(DependencyService.Get<ISQLite>().GetConnection());
            // What happens with the call from above?

            // Set-up the Kinvey Client.
            this.kinveyClient = builder.Build();
            this.KinveyPing();
        }

        /// <summary>
        /// 
        /// Ping the Kinvey Backend and output the response
        /// on the Console.
        /// 
        /// </summary>
        private async void KinveyPing()
        {
            try
            {
                PingResponse response = await kinveyClient.PingAsync();
                Console.WriteLine("Kinvey Ping Response: " + response.kinvey);
            }
            catch (Exception exc)
            {
                // Log any problems.
                Console.WriteLine(exc.Message);
            }
        }
    }

    public interface ISQLite
    {
        SQLite.Net.Interop.ISQLitePlatform GetConnection();
        string GetPath();
    }
}
