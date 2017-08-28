namespace WhereAreMyBus.Views
{
    using Models;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using ViewModels;
    using WhereAreMyBus.Services;
    using Xamarin.Forms;
    using Xamarin.Forms.Maps;
    using Plugin.Toasts;

    public partial class LocationPage : ContentPage
    {
        #region Attributes
        GeolocatorService geolocatorService;
        ApiService apiService;
        DialogService dialogService;
        int c = 1;
        #endregion

        #region Properties
        public static CancellationTokenSource CancellationToken { get; set; }

        #endregion

        #region Constructors
        public LocationPage()
        {
            InitializeComponent();
            geolocatorService = new GeolocatorService();
            apiService = new ApiService();
            dialogService = new DialogService();

            MoveToCurrentLocation();

        }

        #endregion

        #region Methods
        async void MoveToCurrentLocation()
        {
            await geolocatorService.GetLocation();
            if (geolocatorService.Latitude != 0 && geolocatorService.Longitude != 0)
            {
                var position = new Position(geolocatorService.Latitude, geolocatorService.Longitude);
                MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromMiles(.3)));
            }
            Task.Run(async () => backgroundThread());
        }

        async Task backgroundThread()
        {
            LocationPage.CancellationToken = new CancellationTokenSource();
            while (!LocationPage.CancellationToken.IsCancellationRequested)
            {

                LocationPage.CancellationToken.Token.ThrowIfCancellationRequested();
                await Task.Delay(2000, LocationPage.CancellationToken.Token).ContinueWith(async (arg) => {
                    if (!LocationPage.CancellationToken.Token.IsCancellationRequested)
                    {
                        LocationPage.CancellationToken.Token.ThrowIfCancellationRequested();
                        //await trackLocation();
                        await ShowPoins();
                    }
                });
                
            }
        }

        async Task ShowPoins()
        {
            var locationsViewModel = LocationViewModel.GetInstance();
            await locationsViewModel.LoadPins();
            if (locationsViewModel.Pins.Count > 0)
            {
                Device.BeginInvokeOnMainThread(() => MyMap.Pins.Clear());
                foreach (var pin in locationsViewModel.Pins)
                {
                    if (pin != null)
                    {
                        Device.BeginInvokeOnMainThread(() => MyMap.Pins.Add(pin));
                    }
                }
            }
        }

        async Task trackLocation() {
            var checkConnetion = await apiService.CheckConnection();
            if (!checkConnetion.IsSuccess)
            {
                await dialogService.ShowMessage("Error", checkConnetion.Message);
                return;
            }
            await geolocatorService.GetLocation();
            if (geolocatorService.Latitude != 0 && geolocatorService.Longitude != 0)
            {
                var location = new Location
                {
                    Vehiculo = "101",
                    Latitud = (float)geolocatorService.Latitude,
                    Longitud = (float)geolocatorService.Longitude,
                    Ruta = "Medellín - Donmatías (7:15 am)",
                    Placa = "ABC-123"
                };
                //var urlAPI = Application.Current.Resources["URLAPI"].ToString();
                var urlAPI = "https://wherearemybus-1503502678166.firebaseio.com";
                var response = await apiService.Put<Location>(
                    urlAPI,
                    "/locations",
                    "/-KsH7X4jjNVU-coTtuV4.json", location);

                if (!response.IsSuccess)
                {
                    await dialogService.ShowMessage("Error", "Problem ocurred retrieving the locations, try latter.");
                    return;
                }
            }
            
        }
        #endregion
    }
}
