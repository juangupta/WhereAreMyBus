using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WhereAreMyBus.Models;
using WhereAreMyBus.Services;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace WhereAreMyBus.ViewModels
{
    public class LocationViewModel
    {
        #region Attributes
        ApiService apiService;
        DialogService dialogService;
        #endregion

        #region Properties
        public ObservableCollection<Pin> Pins { get; set; }
        #endregion

        #region Constructors
        public LocationViewModel()
        {
            instance = this;

            apiService = new ApiService();
            dialogService = new DialogService();

            Pins = new ObservableCollection<Pin>();
        }
        #endregion

        #region Singleton
        private static LocationViewModel instance;

        public static LocationViewModel GetInstance()
        {
            if (instance == null)
            {
                instance = new LocationViewModel();
            }

            return instance;
        }
        #endregion

        #region Methods
        public async Task LoadPins()
        {
            var checkConnetion = await apiService.CheckConnection();
            if (!checkConnetion.IsSuccess)
            {
                await dialogService.ShowMessage("Error", checkConnetion.Message);
                return;
            }

            //var urlAPI = Application.Current.Resources["URLAPI"].ToString();
            var urlAPI = "https://wherearemybus-1503502678166.firebaseio.com";
            var response = await apiService.GetList<Location>(
                urlAPI,
                "/locations",
                ".json");

            if (!response.IsSuccess)
            {
                await dialogService.ShowMessage("Error", "Problem ocurred retrieving the locations, try latter.");
                return;
            }

            //dynamic data = JObject.Parse((string)response.Result);
            /*var list = ((IDictionary<string, JToken>)data).Select(k =>
    JsonConvert.DeserializeObject<Location>(k.Value.ToString())).ToList();*/
            ReloadPins((List<Location>)response.Result);
        }

        void ReloadPins(List<Location> locations)
        {
            Pins.Clear();
            foreach (var location in locations)
            {
                var position = new Position(location.Latitud, location.Longitud);
                Pins.Add(new Pin
                {
                    Type = PinType.Generic,
                    Position = position,
                    Label = string.Format("Ruta: {0} - Conductor: {1} - Vehículo: {2}",location.Conductor, location.Vehiculo)
                });
            }
        }
        #endregion

    }
}
