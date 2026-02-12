using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Lumi.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        protected bool SetProperty<T>(
            ref T field,
            T value,
            Action? afterChange = null,
            [CallerMemberName] string? name = null)
        {
            // EqualityComparer vermeidet Boxing bei ValueTypes und nutzt korrektes Equals-Verhalten
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(name);
            afterChange?.Invoke();

            return true;
        }
    }
}
