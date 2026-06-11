using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.ClassModels
{
    public class FolderModel
    {
        public string FolderName { get; set; } = "Unknown Folder";
        public string FolderPath { get; set; } = "";
        [JsonIgnore]
        public BitmapImage Thumbnail { get; set; } = new();
    }
}
