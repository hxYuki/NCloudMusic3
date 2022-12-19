using NCloudMusic3.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCloudMusic3.Models
{
    public class MusicList : NotifyPropertyChanged
    {
        private ulong id;
        private string name;
        private string coverImgUrl;
        private int trackCount;
        private User creator;
        private string description;
        private bool isFromSubscribe;
        private DateTime createTime;

        public ulong Id
        {
            get => id; set
            {
                id = value; RaisePropertyChanged();
            }
        }
        public string Name
        {
            get => name; set
            {
                name = value; RaisePropertyChanged();
            }
        }
        public string CoverImgUrl
        {
            get => coverImgUrl; set
            {
                coverImgUrl = value; RaisePropertyChanged();
            }
        }
        public int TrackCount
        {
            get => trackCount; set
            {
                trackCount = value; RaisePropertyChanged();
            }
        }
        public User Creator
        {
            get => creator; set
            {
                creator = value; RaisePropertyChanged();
            }
        }
        public string Description
        {
            get => description; set
            {
                description = value; RaisePropertyChanged();
            }
        }
        public bool IsFromSubscribe
        {
            get => isFromSubscribe; set
            {
                isFromSubscribe = value; RaisePropertyChanged();
            }
        }
        public DateTime CreateTime
        {
            get => createTime; set
            {
                createTime = value; RaisePropertyChanged();
            }
        }
    }
}
    //public class MusicListVM : ViewModel<MusicList>
    //{
    //    public ulong Id
    //    {
    //        get => Model.Id; set
    //        {
    //            Model.Id = value;
    //            RaisePropertyChanged();
    //        }
    //    }
    //    public string Name
    //    {
    //        get => Model.Name; set
    //        {
    //            Model.Name = value; RaisePropertyChanged();
    //        }
    //    }
    //    public string CoverImgUrl
    //    {
    //        get => Model.CoverImgUrl; set
    //        {
    //            Model.CoverImgUrl = value; RaisePropertyChanged();
    //        }
    //    }
    //    public int TrackCount
    //    {
    //        get => Model.TrackCount; set
    //        {
    //            Model.TrackCount = value; RaisePropertyChanged();
    //        }
    //    }
    //    public User Creator
    //    {
    //        get => Model.Creator; set
    //        {
    //            Model.Creator = value; RaisePropertyChanged();
    //        }
    //    }
    //    public string Description
    //    {
    //        get => Model.Description; set
    //        {
    //            Model.Description = value; RaisePropertyChanged();
    //        }
    //    }
    //    public bool IsFromSubscribe
    //    {
    //        get => Model.IsFromSubscribe; set
    //        {
    //            Model.IsFromSubscribe = value; RaisePropertyChanged();
    //        }
    //    }
    //    public DateTime CreateTime
    //    {
    //        get => Model.CreateTime; set
    //        {
    //            Model.CreateTime = value; RaisePropertyChanged();
    //        }
    //    }
    //}
