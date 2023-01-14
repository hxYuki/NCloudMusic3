using NCloudMusic3.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NCloudMusic3.ViewModels
{
    public class ViewModel<T> : NotifyPropertyChanged where T : new()
    {
        protected T Model { get; set; } = new();

        public void SetModel(T model)
        {
            Model = model;
            foreach (var p in this.GetType().GetProperties())
            {
                RaisePropertyChanged(p.Name);
            }
        }
    }

}
