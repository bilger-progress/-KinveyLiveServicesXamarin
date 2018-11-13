using System;
using Xamarin.Forms;
using Kinvey;
using Plugin.Connectivity;
using System.Linq;

namespace KinveyLiveServicesXamarin
{
    public partial class MainPage : ContentPage
    {
        private string app_key = "";
        private string app_secret = "";
        private string username = "";
        private string password = "";

        private bool liveServicesProcessing = false;

        public MainPage()
        {
            InitializeComponent();

            try
            {
                Client.Builder cb = new Client.Builder(this.app_key, this.app_secret).SetFilePath(DependencyService.Get<ISQLite>().GetPath());
                cb.Build();
            }
            catch (KinveyException knvExc)
            {
                // Handle any Kinvey exception.
                Console.WriteLine("Kinvey Exception: " + knvExc.Message);
            }
            catch (Exception exc)
            {
                // Handle any General exception.
                Console.WriteLine("General Exception: " + exc.Message);
            }

            // Listen for connectivity changes.
            CrossConnectivity.Current.ConnectivityChanged += (sender, args) =>
            {
                // Make sure to log messages.
                Console.WriteLine("Connectivity Changed. IsConnected: " + CrossConnectivity.Current.IsConnected);
            };

            // Listen for connectivity type changes.
            CrossConnectivity.Current.ConnectivityTypeChanged += (sender, args) =>
            {
                // Make sure to log messages.
                Console.WriteLine("Connectivity Type Changed. Types: " + args.ConnectionTypes.FirstOrDefault());

                // If there's internet connection, reconnect to Kinvey Live Services.
                if (CrossConnectivity.Current.IsConnected)
                {
                    this.ProceedKinveyLiveServices();
                }
            };

            // Kick-off the process.
            this.ProceedKinveyLiveServices();
        }

        /// <summary>
        /// 
        /// Handles login if there is not a user already.
        /// Disconnects any existing connections to KLS.
        /// Registers for KLS and subscribes for a collection.
        /// 
        /// </summary>
        private async void ProceedKinveyLiveServices()
        {
            // If processing is on, then do not bother.
            if (this.liveServicesProcessing) 
            {
                return;
            }

            // Indicate that processing is on.
            this.liveServicesProcessing = true;

            // Login if user is not present.
            if (!Client.SharedClient.IsUserLoggedIn())
            {
                try
                {
                    await User.LoginAsync(this.username, this.password);
                }
                catch (KinveyException knvExc)
                {
                    // Handle any Kinvey exception.
                    Console.WriteLine("Kinvey Exception: " + knvExc.Message);
                }
                catch (Exception exc)
                {
                    // Handle any General exception.
                    Console.WriteLine("General Exception: " + exc.Message);
                }
            }

            // If user is present, then go ahead and register KLS.
            if (Client.SharedClient.IsUserLoggedIn())
            {
                try
                {
                    // First make sure to close all open connections.
                    await Client.SharedClient.ActiveUser.UnregisterRealtimeAsync();
                    // Then register fresh.
                    await Client.SharedClient.ActiveUser.RegisterRealtimeAsync();
                    DataStore<Book> Books = DataStore<Book>.Collection("Books");
                    // Subscribe to a collection.
                    await Books.Subscribe(new KinveyDataStoreDelegate<Book>
                    {
                        OnNext = (result) => {
                            // Handle new real-time messages.
                            Console.WriteLine("KLS Book title: " + result.Title);
                        },
                        OnStatus = (status) => {
                            // Handle subscription status changes.
                            Console.WriteLine("KLS Subscription Status Change: " + status.Message);
                        },
                        OnError = (error) => {
                            // Handle errors.
                            Console.WriteLine("KLS Error: " + error.Message);
                        }
                    });
                }
                catch (KinveyException knvExc)
                {
                    // Handle any Kinvey exception.
                    Console.WriteLine("Kinvey Exception: " + knvExc.Message);
                }
                catch (Exception exc)
                {
                    // Handle any General exception.
                    Console.WriteLine("General Exception: " + exc.Message);
                }
            }

            // At the end indicate that processing has finished.
            this.liveServicesProcessing = false;
        }
    }

    public interface ISQLite
    {
        SQLite.Net.Interop.ISQLitePlatform GetConnection();
        string GetPath();
    }
}
