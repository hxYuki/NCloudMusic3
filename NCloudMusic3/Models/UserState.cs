using NCloudMusic3.Helpers;
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class User : CacheHolder<ulong, User>
    {
        //static Dictionary<ulong, User> g = new();
        public ulong UserId { get=>key; set=>key=value; }
        public string Nickname { get; set; } = "";
        public string AvatarUrl { get; set; } = "";
        public string BackgroudUrl { get; set; } = "";
        public string Signature { get; set; } = "";
        public DateTime CreateTime { get; set; }
        public bool IsLoginUser { get; set; } = false;


        //public static User Create(ulong uid)
        //{
        //    if (!g.ContainsKey(uid))
        //    {
        //        g[uid] = new User() { UserId = uid };
        //    }
        //    return g[uid];

        //    //return new User() { Id = id, Name = name, PictureUrl = pictureUrl };
        //}
        //public static User UpdateOrCreate(ulong id, Action<User> updater)
        //{
        //    if (!g.ContainsKey(id))
        //    {
        //        var t = new User() { UserId = id };
        //        g[id] = t;
        //    }
        //    updater(g[id]);
        //    return g[id];
        //}
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