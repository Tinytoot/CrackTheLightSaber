using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Phone.Controls;
using Microsoft.Devices.Sensors;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;

namespace CrackTheWhip
{
    public partial class MainPage : PhoneApplicationPage
    {
        Accelerometer accelrometer;

        //represents the time of the first significant movement 
        DateTimeOffset movementMoment = new DateTimeOffset();  

        double firstShakeStep = 0; //represents the value of the first significant movement 
        
        // Constructor
        public MainPage()
        {
            InitializeComponent();

			//Shows the rate reminder message, according to the settings of the RateReminder.
            (App.Current as App).rateReminder.Notify();
        }

		/// <summary>
        /// Navigates to about page.
        /// </summary>
        private void GoToAbout(object sender, GestureEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/About.xaml", UriKind.RelativeOrAbsolute));
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
     
            // If the Motion object is null, initialize it and add a CurrentValueChanged
            // event handler.
            if (accelrometer == null)
            {
                accelrometer = new Accelerometer();
                accelrometer.ReadingChanged += new EventHandler<AccelerometerReadingEventArgs>(accelrometer_ReadingChanged);
            }

            // Try to start the Motion API.
            try
            {
                accelrometer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("unable to start the Motion API.");
            }
        }

        void accelrometer_ReadingChanged(object sender, AccelerometerReadingEventArgs e)
        {
            //if the movement takes less than 500 milliseconds   
            if (e.Timestamp.Subtract(movementMoment).Duration().TotalMilliseconds <= 1000)   
            {    
                if ((e.Z <= -1 || e.Z >= 1) && (firstShakeStep <= Math.Abs(e.Z)))       
                    firstShakeStep = e.Z;            
                if (firstShakeStep != 0)    
                {
                    if (firstShakeStep < 1)
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() => LightSaberOff());
                    }
                    else
                    {
                       Deployment.Current.Dispatcher.BeginInvoke(() => LightSaberOn());
                    }

                    firstShakeStep = 0;
                }   
            }   
            
            movementMoment = e.Timestamp; 
        }

        private void LightSaberOn()
        {
            this.LightSaber.Visibility = Visibility.Visible;
            PlaySound(@"Sounds\coolsaber.wav");
        }

        private void LightSaberOff()
        {
            this.LightSaber.Visibility = Visibility.Collapsed;
            PlaySound(@"Sounds\saberoff.wav");
        }

        private void PlaySound(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                using (var stream = TitleContainer.OpenStream(path))
                {
                    if (stream != null)
                    {
                        var effect = SoundEffect.FromStream(stream);
                        FrameworkDispatcher.Update();
                        effect.Play();
                    }
                }
            }
        }

    }
}
