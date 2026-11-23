// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.8.0 (Build 161126) . All Rights Reserved.
// Licensed under the GNU License. See LICENSE in the project root.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vusic_Player.Configuration.ClassModels;

namespace Vusic_Player.Configuration.Playback
{
    public class QueueObjects : INotifyPropertyChanged
    {

        public static QueueObjects QueueInstance = new QueueObjects();
        #region Back Properties
        private ObservableCollection<SongModel> _queuefull = new ObservableCollection<SongModel>();
        private ObservableCollection<SongModel> _queuetemporary = new ObservableCollection<SongModel>();
        private bool _isloop = false;
        private bool _isQueuePageShuffleEnabled = false;
        private ObservableCollection<SongModel> _queuenext = new ObservableCollection<SongModel>();
        private int _queueindex = 0;
        #endregion
        #region Main Properties
        public int QueueIndex
        {
            get => _queueindex;
            set
            {
                if (_queueindex != value)
                {
                    _queueindex = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsLoop
        {
            get => _isloop;
            set
            {
                if(_isloop != value)
                {
                    _isloop = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsQueuePageShuffleEnabled
        {
            get => _isQueuePageShuffleEnabled;
            set
            {
                if (_isQueuePageShuffleEnabled != value)
                {
                    _isQueuePageShuffleEnabled = value;
                    OnPropertyChanged();
                }
            }
        }
        public ObservableCollection<SongModel> QueueFull
        {
            get => _queuefull;
            set
            {
                if (_queuefull != value)
                {
                    _queuefull = value;
                    OnPropertyChanged();
                }
            }
        }
        public ObservableCollection<SongModel> QueueTemporary
        {
            get => _queuetemporary;
            set
            {
                if (_queuetemporary != value)
                {
                    _queuetemporary = value;
                    OnPropertyChanged();
                }
            }
        }
        public ObservableCollection<SongModel> QueueNext
        {
            get => _queuenext;
            set
            {
                if (_queuenext != value)
                {
                    _queuenext = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion
        #region INotifyImplementation
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }
        #endregion
    }
}
