using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Lumi.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _isModified;
        private int _suppressIsModifiedCounter;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public bool IsModified
        {
            get => _isModified;
            protected set
            {
                if (_isModified != value)
                {
                    _isModified = value;
                    OnPropertyChanged();
                }
            }
        }

        public void AcceptChanges()
        {
            IsModified = false;
        }

        protected IDisposable SuppressIsModified()
        {
            _suppressIsModifiedCounter++;
            return new Scope(() => _suppressIsModifiedCounter--);
        }

        protected bool SetProperty<T>(
            ref T field,
            T value,
            Action? afterChange = null,
            [CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(name);
            afterChange?.Invoke();

            if (_suppressIsModifiedCounter == 0)
                IsModified = true;

            return true;
        }

        private sealed class Scope : IDisposable
        {
            private readonly Action _onDispose;
            private bool _disposed;

            public Scope(Action onDispose) => _onDispose = onDispose;

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _onDispose();
            }
        }
    }
}
