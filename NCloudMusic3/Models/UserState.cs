using NCloudMusic3.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NCloudMusic3.Models
{
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if(this.PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class User
    {
        public ulong UserId { get; set; }
        public string Nickname { get; set; } = "";
        public string AvatarUrl { get; set; } = "";
        public string BackgroudUrl { get; set; } = "";
        public string Signature { get; set; } = "";
        public DateTime CreateTime { get; set; }
        public bool IsLoginUser { get; set; } = false;
    }
}

namespace NCloudMusic3.ViewModels
{
    public class UserProfile : ViewModel<User>
    {
        private ulong likeListId;

        public bool IsLoginUser
        {
            get => Model.IsLoginUser; set
            {
                Model.IsLoginUser = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsNotLoginUser));
            }
        }
        public bool IsNotLoginUser
        {
            get => !Model.IsLoginUser; set
            {
                Model.IsLoginUser = !value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsLoginUser));
            }
        }

        public ulong UserId
        {
            get => Model.UserId; set
            {
                Model.UserId = value; RaisePropertyChanged();
            }
        }
        public string Nickname
        {
            get => Model.Nickname; set
            {
                Model.Nickname = value; RaisePropertyChanged();
            }
        }
        public string AvatarUrl
        {
            get => Model.AvatarUrl; set
            {
                Model.AvatarUrl = value; RaisePropertyChanged();
            }
        }
        public string BackgroudUrl
        {
            get => Model.BackgroudUrl; set
            {
                Model.BackgroudUrl = value;
                RaisePropertyChanged();
            }
        }
        public string Signature
        {
            get => Model.Signature; set
            {
                Model.Signature = value;
                RaisePropertyChanged();
            }
        }
        public DateTime CreateTime
        {
            get => Model.CreateTime; set
            {
                Model.CreateTime = value;
                RaisePropertyChanged();
            }
        }

        public ulong LikeListId
        {
            get => likeListId; set
            {
                likeListId = value;
                RaisePropertyChanged();
            }
        }
        //public ObservableCollection<ulong> LikedMusicList { get; set; }
    }
}