﻿using System.ComponentModel;

namespace MTGAHelper.Tool.OutputLogMessageViewer.ViewModels
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedEvent(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ObservableProperty<T> : ObservableObject
    {
        private T value;
        public T Value
        {
            get { return value; }
            set { this.value = value; RaisePropertyChangedEvent(nameof(Value)); }
        }

        public ObservableProperty(T value)
        {
            Value = value;
        }
    }
}
