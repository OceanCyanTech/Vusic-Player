using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.UserSettings;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.UserViews.Controls.OceanDialogControls
{
    public sealed partial class MassEditor : UserControl
    {
        public MassEditor()
        {
            InitializeComponent();
        }
        public async void LoadItems(ObservableCollection<SongModel> items)
        {
            this.Loaded += MassEditor_Loaded; ItemsToEdit.Clear();

       
            ItemsToEdit = items;
            ifbInformation.IsOpen = false;
            //int count = lstViewDisplay.SelectedItems.Count;
            //txtItemCount.Text = $"{count} {(count == 1 ? "item" : "items")} selected";
            //lstViewDisplay.ItemsSource = ItemsToEdit;
            //lstViewDisplay.SelectAll();
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var playlitss = currentSettings.SavedPlaylists;
            lstViewPlaylists.Items.Clear();
            foreach (var item in playlitss)
            {
                lstViewPlaylists.Items.Add(item.PlaylistName);
            }
        }
        ObservableCollection<SongModel> ItemsToEdit = new();

        private async void MassEditor_Loaded(object sender, RoutedEventArgs e)
        {
            //Debug.WriteLine("Loaded");
            //int count = lstViewDisplay.SelectedItems.Count;
            //txtItemCount.Text = $"{count} {(count == 1 ? "item" : "items")} selected";
            //lstViewDisplay.ItemsSource = ItemsToEdit;
            //lstViewDisplay.SelectAll();
            //var currentSettings = await SettingsLoader.LoadSettingsAsync();
            //var playlitss = currentSettings.SavedPlaylists;
            //lstViewPlaylists.Items.Clear();
            //foreach (var item in playlitss)
            //{
            //    lstViewPlaylists.Items.Add(item.PlaylistName);
            //}
            //lstViewDisplay.ListViewSelectionChange -= LstViewDisplay_ListViewSelectionChange;
            //lstViewDisplay.ListViewSelectionChange += LstViewDisplay_ListViewSelectionChange;
        }

        private void LstViewDisplay_ListViewSelectionChange(object? sender, SelectionChangedEventArgs e)
        {
            //int count = lstViewDisplay.SelectedItems.Count;
            //txtItemCount.Text = $"{count} {(count == 1 ? "item" : "items")} selected";
        }
        private void ShowInfoBar(string Title, string Information, InfoBarSeverity severity)
        {
            ifbInformation.Title = Title;
            ifbInformation.Message = Information;
            ifbInformation.IsOpen = true;
            ifbInformation.Severity = severity;
        }
        private void btnSetAlbum_Click(object sender, RoutedEventArgs e)
        { }
        //    var listofnonexistant = new List<string>();
        //    var selected = lstViewDisplay.SelectedItems.Cast<SongModel>().ToList();
        //    foreach (var item in selected)
        //    {
        //        if (item != null && item.FilePath is string str)
        //        {
        //            if (File.Exists(str))
        //            {
        //                if (AudioMetadata.ChangeAlbumName(str, txtAlbum.Text))
        //                {
        //                    ShowInfoBar("Success", "Album Name for the selected items has been changed!", InfoBarSeverity.Success);

        //                    lstViewDisplay.UpdateAlbumNameForSelected(txtAlbum.Text);
        //                }
        //                else
        //                {
        //                    ShowInfoBar("Error", "An unexpected error occured in changing album name. Check Log Page for more details.", InfoBarSeverity.Error);
        //                }
        //            }
        //            else
        //            {
        //                listofnonexistant.Add(str);
        //            }

        //        }
        //    }


        //    if (listofnonexistant.Count > 0)
        //    {
        //        ShowInfoBar(
        //            "Error",
        //            $"The following files do not exist: {string.Join(", ", listofnonexistant.Select(System.IO.Path.GetFileName))}",
        //            InfoBarSeverity.Error
        //        );
        //    }
        //}

        private void btnSetArtist_Click(object sender, RoutedEventArgs e)
        {
            //; var listofnonexistant = new List<string>();
            //var selected = lstViewDisplay.SelectedItems.Cast<SongModel>().ToList();
            //foreach (var item in selected)
            //{
            //    if (item != null && item.FilePath is string str)
            //    {
            //        if (File.Exists(str))
            //        {
            //            //if (AudioMetadata.ChangeArtistName(str, txtArtist.Text))
            //            //{
            //            //    ShowInfoBar("Success", "Artist property for the selected items has been changed!", InfoBarSeverity.Success);

            //            //    lstViewDisplay.UpdateArtistNameForSelected(txtArtist.Text);
            //            //}
            //            //else
            //            //{
            //            //    ShowInfoBar("Error", "An unexpected error occured in changing artist property. Check Log Page for more details.", InfoBarSeverity.Error);
            //            //}
            //        }
            //        else
            //        {
            //            listofnonexistant.Add(str);
            //        }

            //    }
        
            

            //if (listofnonexistant.Count > 0)
            //{
            //    ShowInfoBar(
            //        "Error",
            //        $"The following files do not exist: {string.Join(", ", listofnonexistant.Select(System.IO.Path.GetFileName))}",
            //        InfoBarSeverity.Error
            //    );
            //}
        }

        private async void btnAddtoPlaylist_Click(object sender, RoutedEventArgs e)
        {
            //    var selected = lstViewDisplay.SelectedItems.Cast<SongModel>().ToList();
            //    var selectedplaylists = lstViewPlaylists.SelectedItems.ToList();
            //    var currentSettings = await SettingsLoader.LoadSettingsAsync();
            //    var playlists = currentSettings.SavedPlaylists;
            //    foreach (var item in selected)
            //    {
            //        if (item != null && item.FilePath is string str)
            //        {
            //            if (File.Exists(str))
            //            {
            //                foreach (var playlist in selectedplaylists)
            //                {
            //                    var exist = playlists.FirstOrDefault(p => p.PlaylistName == playlist.ToString());
            //                    if (exist != null)
            //                    {
            //                        var songspaths = exist.SongsPaths;
            //                        var defaultitem = songspaths.FirstOrDefault(k => k == item.FilePath);
            //                        if (defaultitem == null)
            //                        {
            //                            exist.SongsPaths.Add(item.FilePath);
            //                            exist.PlaylistCount = $"{exist.SongsPaths.Count} {(exist.SongsPaths.Count == 1 ? "item" : "items")}";
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    await SettingsLoader.SaveSettingsAsync(currentSettings);
            //    ShowInfoBar("Success", "The selected items have been added to the playlists selected!", InfoBarSeverity.Success);

            //}
        }
    }
}
