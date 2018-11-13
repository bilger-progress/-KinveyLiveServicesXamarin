using System;
using Xamarin.Forms;
using Kinvey;
using Plugin.Connectivity;
using System.Linq;

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
            // Ping Kinvey Backend.
            this.KinveyPing();
            // Continue setting-up Kinvey Live Services.
            this.ProceedKinveyLiveServices();

            // Listen for connectivity changes.
            CrossConnectivity.Current.ConnectivityChanged += (sender, args) =>
            {
                // Make sure to log messages.
                Console.WriteLine("Connectivity Changed. IsConnected: " + CrossConnectivity.Current.IsConnected);
            };

            CrossConnectivity.Current.ConnectivityTypeChanged += (sender, args) =>
            {
                Console.WriteLine("Connectivity  Type Changed. Types: " + args.ConnectionTypes.FirstOrDefault());

                // If there's connection, reconnect to Kinvey Live Services.
                if (CrossConnectivity.Current.IsConnected)
                {
                    this.ProceedKinveyLiveServices();
                }
            };
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

        /// <summary>
        /// 
        /// Login if not already.
        /// Make sure to destroy any existing connection to Kinvey Live Services.
        /// Register for Kinvey Live Services.
        /// Subscribe for "Books" collection.
        /// Output any messages on the console.
        /// 
        /// </summary>
        private async void ProceedKinveyLiveServices()
        {
            if (!Client.SharedClient.IsUserLoggedIn())
            {
                // Sample user.
                await User.LoginAsync("", "");
            }

            // Close all already existing KLS registrations first.
            try
            {
                await kinveyClient.ActiveUser.UnregisterRealtimeAsync();

            }
            catch (KinveyException exc)
            {
                // Handle unregistration errors.
                Console.WriteLine(exc.Message);
            }

            // Register for Kinvey Live Services.
            try
            {
                await kinveyClient.ActiveUser.RegisterRealtimeAsync();
            }
            catch (KinveyException exc)
            {
                // Handle registration errors.
                Console.WriteLine(exc.Message);
            }

            // Will test with book entities.
            DataStore<Book> Books = DataStore<Book>.Collection("Books");
            await Books.Subscribe(new KinveyDataStoreDelegate<Book>
            {
                OnNext = (result) => {
                    // Handle new real-time messages.
                    Console.WriteLine("Book title: " + result.Title);
                },
                OnStatus = (status) => {
                    // Handle subscription status changes.
                    Console.WriteLine("Subscription Status Change: " + status.Message);
                },
                OnError = (error) => {
                    // Handle errors.
                    Console.WriteLine("Error: " + error.Message);
                }
            });
        }
    }

    public interface ISQLite
    {
        SQLite.Net.Interop.ISQLitePlatform GetConnection();
        string GetPath();
    }
}
