using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCloudMusic3.Models
{
    public class Music
    {
        public ulong Id {  get; set; }
        public string Title {  get; set; }
        public string[] Translation {  get; set; }
        public Artist[] Artists { get; set; }
        public string[] Alias { get; set; }
        public Album Album { get; set; }
        public string CoverUrl {  get; set; }
        public CloudType CloudInfo {  get; set; }
        public string Path {  get; set; }
        public string LocalPath {  get; set; }

        public enum CloudType
        {
            CloudOnly = 1, InfoOnly=2, Normal = 0
        }
    }
    public class Album
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public string PictureUrl { get; set; }
    }
    public class Artist
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public Artist[] Alias { get; set; }
    }
}
