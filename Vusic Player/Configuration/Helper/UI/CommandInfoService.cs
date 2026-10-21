// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.0.0 (Build 211026) . All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.Helper.UI
{
    public class CommandInfoService : INotifyPropertyChanged
    {
        public string? SeekText { get; set; }

        public static event Action<string, bool>? OnSeekRequest;
        public static void ShowSeek(double seconds)
        {
            string text = seconds > 0 ? $"+{seconds}" : $"{seconds}";
            bool isForward = seconds > 0;

            // Fire the event
            OnSeekRequest?.Invoke(text, isForward);
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
