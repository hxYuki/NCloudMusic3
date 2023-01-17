using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace NCloudMusic3.Models
{
    public enum MusicQuality
    {
        Standard, Higher, ExHigh, Lossless, HiRes
    }
    public class Config
    {
        
        public MusicQuality PlayingQuality { get; set; }

        public MusicQuality DownloadQuality { get; set; }

        public List<string> LocalMusicFolders { get; set; }

        public static Config Default => new Config()
        {
            PlayingQuality = MusicQuality.Standard,
            DownloadQuality = MusicQuality.ExHigh,
            LocalMusicFolders = new ()
        };
    }
}
