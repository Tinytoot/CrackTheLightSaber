using System;
using System.Windows.Input;
using Microsoft.Phone.Tasks;
using System.Reflection;

namespace CrackTheLightSaber.Commands
{
    public class SendAnEmailCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            var assembly = Assembly.GetExecutingAssembly().FullName;
            string version = assembly.Split('=')[1].Split(',')[0];

            EmailComposeTask emailTask = new EmailComposeTask();
            emailTask.To = "orangecrushie@gmail.com";
            emailTask.Subject = "Support Request from Crack The Light Saber " + version;
            emailTask.Show();
        }
    }
}
