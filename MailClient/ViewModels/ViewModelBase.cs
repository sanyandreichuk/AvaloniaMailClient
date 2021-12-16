using ReactiveUI;

namespace MailClient.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set => this.RaiseAndSetIfChanged(ref isBusy, value);
        }
    }
}
