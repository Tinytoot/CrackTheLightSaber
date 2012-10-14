using System;
using System.Windows.Input;
using Microsoft.Phone.Tasks;

namespace CrackTheLightSaber.Commands
{
    public class RateThisAppCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            MarketplaceReviewTask reviewTask = new MarketplaceReviewTask();
            reviewTask.Show();
        }
    }
}
