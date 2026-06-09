using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.ClassModels
{
    public class FavouritesRecommend
    {
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        [JsonIgnore]
        public BitmapImage? Thumbnail { get; set; }
    }
}
