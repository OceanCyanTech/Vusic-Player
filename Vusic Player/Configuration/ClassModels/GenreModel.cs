using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.ClassModels
{
    public class GenreModel
    {
        public string GenreName { get; set; } = "";
        public string GenreCover { get; set; } = "";
        public string GenreCount { get; set; } = "";
        public string? GenreTag { get; set; }
    }
}
