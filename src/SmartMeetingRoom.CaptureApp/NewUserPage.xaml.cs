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
using Microsoft.Practices.Unity;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SmartMeetingRoom.CaptureApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NewUserPage : Page
    {
        private Common common;
        private MediaCapture webcam;
        public NewUserPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Triggered every time the page is navigated to.
        /// </summary>
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

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            await common.StopCameraPreview(webcam);
            UnityContainer container = await Common.InitializeDiContainer();
            await common.CapturePhoto(webcam, container.Resolve<IImageFilter>(), UserNameBox.Text, UserDeptBox.Text);// common.Capture(webcam, container.Resolve<IImageFilter>(), UserNameBox.Text, UserDeptBox.Text);
            Frame.Navigate(typeof(MainPage));
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }
    }
}
