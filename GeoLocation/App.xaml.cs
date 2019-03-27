using System;
using GeoLocation.Managers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace GeoLocation
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new Pages.MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            //AppManager.TurnOnLocationPolling();
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
            //AppManager.TurnOffLocationPolling();
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
            //AppManager.TurnOnLocationPolling();
        }

    }
}
