
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace poligon_pezeglądarka_grafiki.ViewModel;

public class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    protected virtual bool SetProperty<T>(ref T storage,
                                      T value,
                                      [CallerMemberName] string propertyName = "")
    {
        if (object.Equals(storage, value)) return false;

        storage = value;
        this.OnPropertyChanged(propertyName);

        return true;
    }
}
