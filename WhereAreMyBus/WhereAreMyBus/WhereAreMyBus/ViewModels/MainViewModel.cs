namespace WhereAreMyBus.ViewModels
{

    public class MainViewModel
    {
        public LocationViewModel Location { get; set; }

        public MainViewModel()
        {
            Location = new LocationViewModel();
        }
    }
}
