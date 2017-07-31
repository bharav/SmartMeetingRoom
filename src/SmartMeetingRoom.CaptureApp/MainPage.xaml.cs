using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using Windows.Media.Capture;
using Windows.Devices.Gpio;
using Windows.UI.Core;
using Microsoft.Practices.Unity;
using System.Threading.Tasks;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SmartMeetingRoom.CaptureApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Common common;
        private MediaCapture webcam;

        // GPIO Related Variables:
        private GpioHelper gpioHelper;
        private bool gpioAvailable;
        private bool motionJustSensed = false;

        public MainPage()
        {
            this.InitializeComponent();
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                //Sets passed through WecamHelper from MainPage as local webcam object
                //common = e.Parameter as Common;
                common = new Common();

            }
            catch (Exception exception)
            {
                Debug.WriteLine("Error when navigating to NewUserPage: " + exception.Message);

                // Navigate back to main page
                Frame.Navigate(typeof(MainPage));
            }
        }

        private async void WebcamFeed_Loaded(object sender, RoutedEventArgs e)
        {
            webcam = await Common.InitializeCameraAsync();
            WebcamFeed.Source = webcam;

            // Check to make sure MediaCapture isn't null before attempting to start preview. Will be null if no camera is attached.
            if (WebcamFeed.Source != null)
            {
                await common.StartCameraPreview(webcam);
            }

        }

        /// <summary>
        /// Called once, when the app is first opened. Initializes device GPIO.
        /// </summary>
        public void InitializeGpio()
        {
            try
            {
                // Attempts to initialize application GPIO. 
                gpioHelper = new GpioHelper();
                gpioAvailable = gpioHelper.Initialize();
            }
            catch
            {
                // This can fail if application is run on a device, such as a laptop, that does not have a GPIO controller.
                gpioAvailable = false;
                Debug.WriteLine("GPIO controller not available.");
            }

            // If initialization was successful, attach motion sensor event handler
            if (gpioAvailable)
            {
                gpioHelper.GetPirSensor().ValueChanged += PirSensorChanged;
            }
        }
        /// <summary>
        /// Triggered when motion sensor changes - someone either enters room or someone exits room
        /// </summary>
        private async void PirSensorChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (!motionJustSensed)
            {
                // Checks to see if event was triggered from an entry (rising edge) or exit (falling edge)
                if (args.Edge == GpioPinEdge.RisingEdge)
                {
                    //motion sensor was just triggered
                    motionJustSensed = true;

                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        await SomeoneEntered();
                    });

                }
            }
        }

        private async void NewUserButton_Click(object sender, RoutedEventArgs e)
        {
            await common.StopCameraPreview(webcam);
            Frame.Navigate(typeof(NewUserPage));
        }
        /// <summary>
        /// Called when someone enters room or vitual motion button is pressed.
        /// Captures photo of current webcam view and sends it to Oxford for facial recognition processing.
        /// </summary>
        private async Task SomeoneEntered()
        {

            UnityContainer container = await Common.InitializeDiContainer();
            await common.Capture(webcam, container.Resolve<IImageFilter>(), "", "");
        }

        private void ShutdownButton_Click(object sender, RoutedEventArgs e)
        {
            // Exit app
            Application.Current.Exit();
        }


        private async void StartMeeting_Click(object sender, RoutedEventArgs e)
        {
            await SomeoneEntered();

        }
    }
}
