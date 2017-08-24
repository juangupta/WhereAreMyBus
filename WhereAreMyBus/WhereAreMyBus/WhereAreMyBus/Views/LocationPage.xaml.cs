namespace WhereAreMyBus.Views
{
    using Models;
    using System.Threading;
    using System.Threading.Tasks;
    using ViewModels;
    using WhereAreMyBus.Services;
    using Xamarin.Forms;
    using Xamarin.Forms.Maps;

    public partial class LocationPage : ContentPage
    {
        #region Attributes
        GeolocatorService geolocatorService;
        ApiService apiService;
        DialogService dialogService;
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

            await ShowPoins();
            Task.Run(async () => trackLocation());
        }

        async Task ShowPoins()
        {
            var locationsViewModel = LocationViewModel.GetInstance();
            await locationsViewModel.LoadPins();
            foreach (var pin in locationsViewModel.Pins)
            {
                MyMap.Pins.Add(pin);
            }
        }

        async Task trackLocation()
        {
            LocationPage.CancellationToken = new CancellationTokenSource();
            while (!LocationPage.CancellationToken.IsCancellationRequested)
            {

                LocationPage.CancellationToken.Token.ThrowIfCancellationRequested();
                await Task.Delay(1000, LocationPage.CancellationToken.Token).ContinueWith(async (arg) => {
                    if (!LocationPage.CancellationToken.Token.IsCancellationRequested)
                    {
                        LocationPage.CancellationToken.Token.ThrowIfCancellationRequested();
                        var checkConnetion = await apiService.CheckConnection();
                        if (!checkConnetion.IsSuccess)
                        {
                            await dialogService.ShowMessage("Error", checkConnetion.Message);
                            return;
                        }

                        var location = new Location
                        {
                            Conductor = "Roberto",
                            Latitud = (float)geolocatorService.Latitude,
                            Longitud = (float)geolocatorService.Longitude,
                            Ruta = "Medellín - Santa Rosa",
                            Vehiculo = "DEF123"
                        };
                        //var urlAPI = Application.Current.Resources["URLAPI"].ToString();
                        var urlAPI = "https://wherearemybus-1503502678166.firebaseio.com";
                        var response = await apiService.Put<Location>(
                            urlAPI,
                            "/locations",
                            "-KsH7X4jjNVU-coTtuV4.json", location);

                        if (!response.IsSuccess)
                        {
                            await dialogService.ShowMessage("Error", "Problem ocurred retrieving the locations, try latter.");
                            return;
                        }

                    }
                });
                
            }
        }
        #endregion
    }
}
