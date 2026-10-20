// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.0.0 (Build 211026) . All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.Helper.UI.Customization
{
    public class ThemeManager : INotifyPropertyChanged
    {
        #region Back Properties
        private string _globaltheme = "ocean";
        #endregion
        public static ThemeManager Instance { get; } = new ThemeManager();
        #region Main Properties
        public string GlobalTheme
        {
            get => _globaltheme;
            set
            {
                if (_globaltheme != value)
                {
                    _globaltheme = value;
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
