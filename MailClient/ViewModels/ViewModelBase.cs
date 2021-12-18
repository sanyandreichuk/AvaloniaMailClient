using ReactiveUI;

namespace MailClient.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        protected ObservableAsPropertyHelper<bool>? isBusy;
        public bool IsBusy => isBusy?.Value ?? false;
    }
}
