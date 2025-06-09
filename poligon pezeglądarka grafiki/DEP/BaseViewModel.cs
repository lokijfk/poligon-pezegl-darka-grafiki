using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace poligon_pezeglądarka_grafiki.DEP;

public class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    protected virtual bool SetProperty<T>(ref T storage,
                                      T value,
                                      [CallerMemberName] string propertyName = "")
    {
        if (Equals(storage, value)) return false;

        storage = value;
        OnPropertyChanged(propertyName);

        return true;
    }
}
