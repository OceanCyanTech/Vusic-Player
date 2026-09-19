using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.ClassModels
{
    public class FilesToModifyAdded : INotifyPropertyChanged
    {
        private string _glyph { get; set; } = "\uEC4F";
        private string _title { get; set; } = "Unknown Title";
        private string _filepath { get; set; } = "";
        private string _outputpath { get; set; } = "";
        private string _errortooltip { get; set; } = "";
        private string _directorypath { get; set; } = "Click to select individual export directory";
        private double _progress = 0;
        private string _imageState = "";
        private Visibility _visibilityofindividualdirectoryselection = Visibility.Collapsed;
        private Visibility _visibilityofcompletedFileLocation = Visibility.Collapsed;
        private bool _visibilityofindividualdirectoryselectionbool = false;
        private bool _dirselectionenabled = true;
        public string Glyph
        {
            get => _glyph;
            set { _glyph = value; OnPropertyChanged(); }
        }
        public Visibility IndividualDirectorySel
        {
            get => _visibilityofindividualdirectoryselection;
            set { _visibilityofindividualdirectoryselection = value; OnPropertyChanged(); }
        }
        public Visibility VisibilityOfCompletedFileLocation
        {
            get => _visibilityofcompletedFileLocation;
            set { _visibilityofcompletedFileLocation = value; OnPropertyChanged(); }
        }
        public bool IndividualDirectorySelBool
        {
            get => _visibilityofindividualdirectoryselectionbool;
            set { _visibilityofindividualdirectoryselectionbool = value; OnPropertyChanged(); }
        }

        public bool DirectorySelectionEnabled
        {
            get => _dirselectionenabled;
            set { _dirselectionenabled = value; OnPropertyChanged(); }
        }


        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }
        public string FilePath
        {
            get => _filepath;
            set { _filepath = value; OnPropertyChanged(); }
        }
        public string OutputPath
        {
            get => _outputpath;
            set { _outputpath = value; OnPropertyChanged(); }
        }
        public string DirectoryPath
        {
            get => _directorypath;
            set { _directorypath = value; OnPropertyChanged(); }
        }
        public string ErrorToolTip
        {
            get => _errortooltip;
            set { _errortooltip = value; OnPropertyChanged(); }
        }
        public string ImageState
        {
            get => _imageState;
            set { _imageState = value; OnPropertyChanged(); }
        }
        public double Progress
        {
            get => _progress;
            set { _progress = value; OnPropertyChanged(); }
        }
      
      
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
