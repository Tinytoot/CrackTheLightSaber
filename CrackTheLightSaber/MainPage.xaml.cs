using System;
using System.Windows.Input;
using Microsoft.Devices;
using Microsoft.Phone.Applications.Common;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System.Windows.Media;
using System.Windows.Controls;


namespace CrackTheLightSaber
{
    public partial class MainPage : PhoneApplicationPage
    {

        // Constructor
        public MainPage()
        {
            InitializeComponent();

			//Shows the rate reminder message, according to the settings of the RateReminder.
            (App.Current as App).rateReminder.Notify();
            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
            AccelerometerHelper.Instance.ReadingChanged += new EventHandler<AccelerometerHelperReadingEventArgs>(OnAccelerometerHelperReadingChanged);
            AccelerometerHelper.Instance.Active = true;

            saberState = SaberState.Off;
        }

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            CheckState();
        }


        private void OnAccelerometerHelperReadingChanged(object sender, AccelerometerHelperReadingEventArgs e)
        {
            if (e.OptimalyFilteredAcceleration.X >= 0.6 && e.OptimalyFilteredAcceleration.X <= 1)
            {  
        
                if (saberState == SaberState.On)
                {
                    LightSaberSwing();

                }
                else if( saberState == SaberState.Off)
                {
                    saberState = SaberState.Starting;
                    LightSaberSwitch();
                }

            }
        }

		/// <summary>
        /// Navigates to about page.
        /// </summary>
        private void GoToAbout(object sender, GestureEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/About.xaml", UriKind.RelativeOrAbsolute));
        }



        void LightSaberSwitch()
        {
            VibrateController.Default.Start(TimeSpan.FromMilliseconds(50)); // short
            string soundToPlay = string.Empty;
            if (saberState == SaberState.Starting)
                soundToPlay = @"Sounds\coolsaber.wav";
            else
                soundToPlay = @"Sounds\saberoff.wav";

            Dispatcher.BeginInvoke(() => { PlaySound(soundToPlay); });
        }

        private void LightSaberSwing()
        {
            PlaySound(@"Sounds\Swing.wav");
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

        private void LightSaber_Tap(object sender, GestureEventArgs e)
        {
            if (saberState == SaberState.Off)
                saberState = SaberState.Starting;
            else if (saberState == SaberState.On)
                saberState = SaberState.Stopping;
            else
                return;

            LightSaberSwitch();
            
        }
       

        #region GoToSleepSaberKeet
        enum SaberState
        {
            On,
            Off,
            Starting,
            Stopping
        }

        SaberState saberState = new SaberState();

        void CheckState()
        {
            switch (saberState)
            {
                case SaberState.On:
                    break;
                case SaberState.Off:
                    break;
                case SaberState.Starting:
                    ShowHideSaber();
                    break;
                case SaberState.Stopping:
                    ShowHideSaber();
                    break;
                default:
                    break;
            }

        }


        void ShowHideSaber()
        {
            int moveValue = 30;
            if (saberState == SaberState.Starting)
                moveValue = -30;

            int lightSaberCoverTop = (int)Canvas.GetTop(lightSaberCover) + moveValue;
            Canvas.SetTop(lightSaberCover, lightSaberCoverTop);



            if (lightSaberCoverTop > 10)
            {
                saberState = SaberState.Off;
                Canvas.SetTop(lightSaberCover, 10);
            }
            if (lightSaberCoverTop < -460)
                saberState = SaberState.On;
        }
        #endregion

    }
}
