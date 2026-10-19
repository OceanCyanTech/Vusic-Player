// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.0.0 (Build 211026) . All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root.

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Vusic_Player.Configuration.Helper.UI.Customization
{
    public class IconHolder : INotifyPropertyChanged
    {
        private string _theme = "Ocean";
        private string _backgroundblur = "ms-appx:///Assets/Themes/Ocean/backgroundblur.png";
        private string _sourceerror = "ms-appx:///Assets/Themes/Ocean/sourceerror.png";

        private string _skipback = "ms-appx:///Assets/Themes/Ocean/skip10back.png";
        private string _next = $"ms-appx:///Assets/Themes/Ocean/next.png";
        private string _refresh = $"ms-appx:///Assets/Themes/Ocean/refresh.png";
        private string _error = $"ms-appx:///Assets/Themes/Ocean/error.png";
        private string _success = $"ms-appx:///Assets/Themes/Ocean/success.png";
        private string _info = $"ms-appx:///Assets/Themes/Ocean/infoicon.png";
        private string _skipahead = $"ms-appx:///Assets/Themes/Ocean/skip10ahead.png";
        private string _fontfamily = $"Bahnschrift";
        private string _previous = "ms-appx:///Assets/Themes/Ocean/previous.png";
        private string _speed = "ms-appx:///Assets/Themes/Ocean/speedicon.png";
        private string _miniplayer = "ms-appx:///Assets/Themes/Ocean/miniplayer.png";
        private string _play = "ms-appx:///Assets/Themes/Ocean/play.png";
        private string _options = "ms-appx:///Assets/Themes/Ocean/options.png";
        private string _expandedicon = "ms-appx:///Assets/Themes/Ocean/expandedicon.png";
        private string _toolsicon = "ms-appx:///Assets/Themes/Ocean/toolsicon.png";
        private string _findlyricsonline = "ms-appx:///Assets/Themes/Ocean/findlyricsonline.png";
        private string _favicon = "ms-appx:///Assets/Themes/Ocean/favicon.png";
        private string _fileinfo = "ms-appx:///Assets/Themes/Ocean/fileinfoicon.png";
        private string _openfile = "ms-appx:///Assets/Themes/Ocean/openfileicon.png";
        private string _musicsuggestions = "ms-appx:///Assets/Themes/Ocean/musicsuggestions.png";
        private string _addicon = "ms-appx:///Assets/Themes/Ocean/addicon.png";
        private string _playlisticon = "ms-appx:///Assets/Themes/Ocean/playlisticon.png";
        private string _musicnoteicon = "ms-appx:///Assets/Themes/Ocean/musicnoteicon.png";
        private string _defaultshow = "ms-appx:///Assets/Themes/Ocean/defaultshow.png";

        private string _openfolder = "ms-appx:///Assets/Themes/Ocean/openfolder.png";
        private string _importicon = "ms-appx:///Assets/Themes/Ocean/importicon.png";
        private string _removeicon = "ms-appx:///Assets/Themes/Ocean/removeicon.png";
        private string _foldericon = "ms-appx:///Assets/Themes/Ocean/foldericon.png";
        private string _sampleimage = "ms-appx:///Assets/Themes/Ocean/sampleimage.png";
        private string _tickicon = "ms-appx:///Assets/Themes/Ocean/tickicon.png";
        private string _defaultvideo = "ms-appx:///Assets/Themes/Ocean/defaultvideo.png";
        private string _renameicon = "ms-appx:///Assets/Themes/Ocean/renameicon.png";
        private string _shuffleicon = "ms-appx:///Assets/Themes/Ocean/shuffleicon.png";
        private string _addimageicon = "ms-appx:///Assets/Themes/Ocean/addimgicon.png";
        private string _refreshimageicon = "ms-appx:///Assets/Themes/Ocean/refreshimg.png";
        private string _defaultalbum = "ms-appx:///Assets/Themes/Ocean/defaultalbum.png";
        private string _removeimage = "ms-appx:///Assets/Themes/Ocean/removeimg.png";
        private string _defaultartist = "ms-appx:///Assets/Themes/Ocean/defaultartist.png";
        private string _editicon = "ms-appx:///Assets/Themes/Ocean/editicon.png";
        private string _artistpictureonline = "ms-appx:///Assets/Themes/Ocean/artistpictureonline.png";
        private string _artistpicturelocal = "ms-appx:///Assets/Themes/Ocean/artistpicturelocal.png";
        private string _selectall = "ms-appx:///Assets/Themes/Ocean/selectall.png";
        private string _addfolder = "ms-appx:///Assets/Themes/Ocean/addfolder.png";
        private string _history = "ms-appx:///Assets/Themes/Ocean/history.png";
        private string _emptystuffleaves = "ms-appx:///Assets/Themes/Ocean/emptystuffleaves.png";
        private string _addtoqueue = "ms-appx:///Assets/Themes/Ocean/addtoqueue.png";
        private string _missingicon = "ms-appx:///Assets/Themes/Ocean/missingicon.png";
        private string _openfolderlocation = "ms-appx:///Assets/Themes/Ocean/openfolderlocation.png";
        private string _copyicon = "ms-appx:///Assets/Themes/Ocean/copyicon.png";
        private string _sorticon = "ms-appx:///Assets/Themes/Ocean/sorticon.png";
        private string _addtoplaylist = "ms-appx:///Assets/Themes/Ocean/addtoplaylist.png";
        private string _deleteicon = "ms-appx:///Assets/Themes/Ocean/deleteicon.png";
        private string _editalbummass = "ms-appx:///Assets/Themes/Ocean/editalbummass.png";
        private string _editartistmass = "ms-appx:///Assets/Themes/Ocean/editartistmass.png";
        private string _fileicon = "ms-appx:///Assets/Themes/Ocean/fileicon.png";
        private string _searchicon = "ms-appx:///Assets/Themes/Ocean/searchicon.png";
        private string _genreicon = "ms-appx:///Assets/Themes/Ocean/genreicon.png";



        public static IconHolder Instance { get; } = new IconHolder();
        public string SampleImage
        {
            get => _sampleimage;
            set
            {
                if (_sampleimage != value)
                {
                    _sampleimage = value;
                    OnPropertyChanged();
                }
            }
        }
        public string AddToQueue
        {
            get => _addtoqueue;
            set
            {
                if (_addtoqueue != value)
                {
                    _addtoqueue = value;
                    OnPropertyChanged();
                }
            }
        }
        public string MissingIcon
        {
            get => _missingicon;
            set
            {
                if (_missingicon != value)
                {
                    _missingicon = value;
                    OnPropertyChanged();
                }
            }
        }
        public string OpenFolderLocation
        {
            get => _openfolderlocation;
            set
            {
                if (_openfolderlocation != value)
                {
                    _openfolderlocation = value;
                    OnPropertyChanged();
                }
            }
        }
        public string CopyIcon
        {
            get => _copyicon;
            set
            {
                if (_copyicon != value)
                {
                    _copyicon = value;
                    OnPropertyChanged();
                }
            }
        }
        public string SortIcon
        {
            get => _sorticon;
            set
            {
                if (_sorticon != value)
                {
                    _sorticon = value;
                    OnPropertyChanged();
                }
            }
        }
        public string AddToPlaylist
        {
            get => _addtoplaylist;
            set
            {
                if (_addtoplaylist != value)
                {
                    _addtoplaylist = value;
                    OnPropertyChanged();
                }
            }
        }
        public string DeleteIcon
        {
            get => _deleteicon;
            set
            {
                if (_deleteicon != value)
                {
                    _deleteicon = value;
                    OnPropertyChanged();
                }
            }
        }
        public string EditAlbumMass
        {
            get => _editalbummass;
            set
            {
                if (_editalbummass != value)
                {
                    _editalbummass = value;
                    OnPropertyChanged();
                }
            }
        }
        public string EditArtistMass
        {
            get => _editartistmass;
            set
            {
                if (_editartistmass != value)
                {
                    _editartistmass = value;
                    OnPropertyChanged();
                }
            }
        }
        public string FileIcon
        {
            get => _fileicon;
            set
            {
                if (_fileicon != value)
                {
                    _fileicon = value;
                    OnPropertyChanged();
                }
            }
        }
        public string SearchIcon
        {
            get => _searchicon;
            set
            {
                if (_searchicon != value)
                {
                    _searchicon = value;
                    OnPropertyChanged();
                }
            }
        }
        public string GenreIcon
        {
            get => _genreicon;
            set
            {
                if (_genreicon != value)
                {
                    _genreicon = value;
                    OnPropertyChanged();
                }
            }
        }
        public string OpenFile
        {
            get => _openfile;
            set
            {
                if (_openfile != value)
                {
                    _openfile = value;
                    OnPropertyChanged();
                }
            }
        }

        public string TickIcon
        {
            get => _tickicon;
            set
            {
                if (_tickicon != value)
                {
                    _tickicon = value;
                    OnPropertyChanged();
                }
            }
        }
        public string RenameIcon
        {
            get => _renameicon;
            set
            {
                if (_renameicon != value)
                {
                    _renameicon = value;
                    OnPropertyChanged();
                }
            }
        }
        public string ShuffleIcon
        {
            get => _shuffleicon;
            set
            {
                if (_shuffleicon != value)
                {
                    _shuffleicon = value;
                    OnPropertyChanged();
                }
            }
        }
        public string AddImageIcon
        {
            get => _addimageicon;
            set
            {
                if (_addimageicon != value)
                {
                    _addimageicon = value;
                    OnPropertyChanged();
                }
            }
        }
        public string RefreshImageIcon
        {
            get => _refreshimageicon;
            set
            {
                if (_refreshimageicon != value)
                {
                    _refreshimageicon = value;
                    OnPropertyChanged();
                }
            }
        }
        public string DefaultAlbum
        {
            get => _defaultalbum;
            set
            {
                if (_defaultalbum != value)
                {
                    _defaultalbum = value;
                    OnPropertyChanged();
                }
            }
        }
        public string RemoveImage
        {
            get => _removeimage;
            set
            {
                if (_removeimage != value)
                {
                    _removeimage = value;
                    OnPropertyChanged();
                }
            }
        }
        public string DefaultArtist
        {
            get => _defaultartist;
            set
            {
                if (_defaultartist != value)
                {
                    _defaultartist = value;
                    OnPropertyChanged();
                }
            }
        }
        public string EditIcon
        {
            get => _editicon;
            set
            {
                if (_editicon != value)
                {
                    _editicon = value;
                    OnPropertyChanged();
                }
            }
        }
        public string ArtistPictureOnline
        {
            get => _artistpictureonline;
            set
            {
                if (_artistpictureonline != value)
                {
                    _artistpictureonline = value;
                    OnPropertyChanged();
                }
            }
        }
        public string ArtistPictureLocal
        {
            get => _artistpicturelocal;
            set
            {
                if (_artistpicturelocal != value)
                {
                    _artistpicturelocal = value;
                    OnPropertyChanged();
                }
            }
        }
        public string SelectAll
        {
            get => _selectall;
            set
            {
                if (_selectall != value)
                {
                    _selectall = value;
                    OnPropertyChanged();
                }
            }
        }
        public string AddFolder
        {
            get => _addfolder;
            set
            {
                if (_addfolder != value)
                {
                    _addfolder = value;
                    OnPropertyChanged();
                }
            }
        }
        public string History
        {
            get => _history;
            set
            {
                if (_history != value)
                {
                    _history = value;
                    OnPropertyChanged();
                }
            }
        }
        public string EmptyStuffLeaves
        {
            get => _emptystuffleaves;
            set
            {
                if (_emptystuffleaves != value)
                {
                    _emptystuffleaves = value;
                    OnPropertyChanged();
                }
            }
        }
        public string MusicSuggestions
        {
            get => _musicsuggestions;
            set
            {
                if (_musicsuggestions != value)
                {
                    _musicsuggestions = value;
                    OnPropertyChanged();
                }
            }
        }
        public string AddIcon
        {
            get => _addicon;
            set
            {
                if (_addicon != value)
                {
                    _addicon = value;
                    OnPropertyChanged();
                }
            }
        }
        public string PlaylistIcon
        {
            get => _playlisticon;
            set
            {
                if (_playlisticon != value)
                {
                    _playlisticon = value;
                    OnPropertyChanged();
                }
            }
        }
        public string MusicNoteIcon
        {
            get => _musicnoteicon;
            set
            {
                if (_musicnoteicon != value)
                {
                    _musicnoteicon = value;
                    OnPropertyChanged();
                }
            }
        }
        public string DefaultShow
        {
            get => _defaultshow;
            set
            {
                if (_defaultshow != value)
                {
                    _defaultshow = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SourceError
        {
            get => _sourceerror;
            set
            {
                if (_sourceerror != value)
                {
                    _sourceerror = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Play
        {
            get => _play;
            set
            {
                if (_play != value)
                {
                    _play = value;
                    OnPropertyChanged();
                }
            }
        }
        public string DefaultVideo
        {
            get => _defaultvideo;
            set
            {
                if (_defaultvideo != value)
                {
                    _defaultvideo = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Theme
        {
            get => _theme;
            set => SetProperty(ref _theme, value);
        }
        public string Refresh
        {
            get => _refresh;
            set => SetProperty(ref _refresh, value);
        }
        public string Error
        {
            get => _error;
            set => SetProperty(ref _error, value);
        }
        public string BackgroundBlur
        {
            get => _backgroundblur;
            set
            {
                if (_backgroundblur != value)
                {
                    _backgroundblur = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Openfolder
        {
            get => _openfolder;
            set
            {
                if (_openfolder != value)
                {
                    _openfolder = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Importicon
        {
            get => _importicon;
            set
            {
                if (_importicon != value)
                {
                    _importicon = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Removeicon
        {
            get => _removeicon;
            set
            {
                if (_removeicon != value)
                {
                    _removeicon = value;
                    OnPropertyChanged();
                }
            }
        }
        public string FolderIcon
        {
            get => _foldericon;
            set
            {
                if (_foldericon != value)
                {
                    _foldericon = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Next
        {
            get => _next;
            set => SetProperty(ref _next, value);
        }

        public string Success
        {
            get => _success;
            set => SetProperty(ref _success, value);
        }

        public string InfoIcon
        {
            get => _info;
            set => SetProperty(ref _info, value);
        }
        public string SkipBack
        {
            get => _skipback;
            set => SetProperty(ref _skipback, value);
        }
        public string SkipAhead
        {
            get => _skipahead;
            set => SetProperty(ref _skipahead, value);
        }

        public string FontFamily
        {
            get => _fontfamily;
            set => SetProperty(ref _fontfamily, value);
        }
        public string Previous
        {
            get => _previous;
            set
            {
                if (_previous != value)
                {
                    _previous = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Speed
        {
            get => _speed;
            set
            {
                if (_speed != value)
                {
                    _speed = value;
                    OnPropertyChanged();
                }
            }
        }
        public string MiniPlayer
        {
            get => _miniplayer;
            set
            {
                if (_miniplayer != value)
                {
                    _miniplayer = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Options
        {
            get => _options;
            set
            {
                if (_options != value)
                {
                    _options = value;
                    OnPropertyChanged();
                }
            }
        }
        public string ExpandedIcon
        {
            get => _expandedicon;
            set
            {
                if (_expandedicon != value)
                {
                    _expandedicon = value;
                    OnPropertyChanged();
                }
            }
        }
        public string ToolsIcon
        {
            get => _toolsicon;
            set
            {
                if (_toolsicon != value)
                {
                    _toolsicon = value;
                    OnPropertyChanged();
                }
            }
        }
        public string FindLyricsOnline
        {
            get => _findlyricsonline;
            set
            {
                if (_findlyricsonline != value)
                {
                    _findlyricsonline = value;
                    OnPropertyChanged();
                }
            }
        }
        public string FavIcon
        {
            get => _favicon;
            set
            {
                if (_favicon != value)
                {
                    _favicon = value;
                    OnPropertyChanged();
                }
            }
        }
        public string FileInfo
        {
            get => _fileinfo;
            set
            {
                if (_fileinfo != value)
                {
                    _fileinfo = value;
                    OnPropertyChanged();
                }
            }
        }


        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler? PropertyChanged;


        /// Compares current value with new value. If different, updates and raises notification.

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            var dispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread()
                             ?? App.MainWindowInstance?.DispatcherQueue;

            if (dispatcher != null && !dispatcher.HasThreadAccess)
            {
                dispatcher.TryEnqueue(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)));
            }
            else
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
