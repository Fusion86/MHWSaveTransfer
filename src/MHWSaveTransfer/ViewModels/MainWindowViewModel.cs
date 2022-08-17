using Cirilla.Core.Models;
using GongSolutions.Wpf.DragDrop;
using MHWSaveTransfer.Dialogs;
using MHWSaveTransfer.Helpers;
using MHWSaveTransfer.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace MHWSaveTransfer.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged, IDropTarget
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<SaveSlotViewModel> MySaveSlots { get; } = new ObservableCollection<SaveSlotViewModel>();
        public ObservableCollection<SaveSlotViewModel> OtherSaveSlots { get; } = new ObservableCollection<SaveSlotViewModel>();

        public string SteamId { get; set; }
        public string VersionString => "Version: " + Assembly.GetExecutingAssembly().GetName().Version;

        public RelayCommand OpenSaveDataCommand { get; }
        public RelayCommand SaveSaveDataCommand { get; }
        public RelayCommand ImportSaveDataCommand { get; }
        public RelayCommand ClearWorkspaceCommand { get; }
        public RelayCommand ChangeSteamIdCommand { get; }

        private string? saveDataDirectory;
        private SaveData? saveData { get; set; }
        private List<SteamAccount>? steamUsersWithMhw;

        private readonly SteamWebApi steamWebApi = new SteamWebApi(SuperSecret.STEAM_WEB_API_KEY);
        private const string FILE_DIALOG_SAVEDATA_FILTER = "SAVEDATA1000|SAVEDATA1000|All files (*.*)|*.*";

        public MainWindowViewModel()
        {
            OpenSaveDataCommand = new RelayCommand(OpenSaveData);
            SaveSaveDataCommand = new RelayCommand(SaveSaveData, CanSaveSaveData);
            ImportSaveDataCommand = new RelayCommand(ImportSaveData, CanImportSaveData);
            ClearWorkspaceCommand = new RelayCommand(ClearWorkspace);
            ChangeSteamIdCommand = new RelayCommand(ChangeSteamId, CanChangeSteamId);

            UpdateSteamIdDisplay();
            _ = Task.Run(() => steamUsersWithMhw = SteamUtility.GetSteamUsersWithMhw());
        }

        private void UpdateSteamIdDisplay()
        {
            if (saveData == null)
            {
                SteamId = "(none)";
            }
            else
            {
                SteamId = saveData.SteamId.ToString();
                _ = Task.Run(async () =>
                {
                    var personaName = await steamWebApi.GetPersonaName(saveData.SteamId.ToString());
                    SteamId = $"{saveData.SteamId} ({personaName})";
                });
            }
        }

        private void SetInitialDirectoryToSaveLocation(FileDialog fd)
        {
            SteamAccount? steamAccount = null;

            if (steamUsersWithMhw != null)
            {
                if (steamUsersWithMhw.Count > 1)
                {
                    // TODO: Ask user to select a steam account.
                    steamAccount = steamUsersWithMhw[0];
                }
                else if (steamUsersWithMhw.Count == 1)
                {
                    steamAccount = steamUsersWithMhw[0];
                }

                if (steamAccount != null)
                {
                    string? saveDir = SteamUtility.GetMhwSaveDir(steamUsersWithMhw[0]);
                    if (saveDir != null)
                        fd.InitialDirectory = saveDir;
                }
            }
        }

        #region Commands

        private void OpenSaveData()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = FILE_DIALOG_SAVEDATA_FILTER;

            SetInitialDirectoryToSaveLocation(ofd);

            if (ofd.ShowDialog() == true)
            {
                MySaveSlots.Clear();
                saveData = null;

                try
                {
                    saveData = new SaveData(ofd.FileName);

                    // Remember where the save file is located, so that we can open the "Save" dialog in the same directory.
                    saveDataDirectory = Path.GetDirectoryName(ofd.FileName);

                    UpdateSteamIdDisplay();

                    foreach (var slot in saveData.SaveSlots)
                        MySaveSlots.Add(new SaveSlotViewModel(slot));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Couldn't open " + Path.GetFileName(ofd.FileName), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanSaveSaveData() => saveData != null;
        private void SaveSaveData()
        {
            if (saveData == null) return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = "SAVEDATA1000";
            sfd.Filter = FILE_DIALOG_SAVEDATA_FILTER;

            if (saveDataDirectory != null)
                sfd.InitialDirectory = saveDataDirectory;

            if (sfd.ShowDialog() == true)
            {
                for (int i = 0; i < 3; i++)
                    saveData.SaveSlots[i] = MySaveSlots[i].SaveSlot;

                saveData.Save(sfd.FileName);
            }
        }

        private bool CanImportSaveData() => true;
        private void ImportSaveData()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = FILE_DIALOG_SAVEDATA_FILTER;

            if (ofd.ShowDialog() == true)
            {
                foreach (string fileName in ofd.FileNames)
                {
                    try
                    {
                        SaveData saveData = new SaveData(fileName);
                        foreach (var saveSlot in saveData.SaveSlots)
                        {
                            // Only add if not already added
                            SaveSlotViewModel newVm = new SaveSlotViewModel(saveSlot);
                            if (OtherSaveSlots.FirstOrDefault(x => x.SoftCompare(newVm)) == null)
                                OtherSaveSlots.Add(newVm);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Couldn't open " + Path.GetFileName(fileName), MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ClearWorkspace()
        {
            MySaveSlots.Clear();
            OtherSaveSlots.Clear();
            saveData = null;
            UpdateSteamIdDisplay();
        }

        private bool CanChangeSteamId() => saveData != null;
        private void ChangeSteamId()
        {
            if (saveData == null) return;

            ChangeSteamIdDialog dialog = new ChangeSteamIdDialog(saveData.SteamId.ToString());
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Try to set SteamId in savedata
                    saveData.SteamId = long.Parse(dialog.SteamId);
                    UpdateSteamIdDisplay();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Couldn't change SteamID", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        #region Drag/drop

        void IDropTarget.DragOver(IDropInfo e)
        {
            //SaveSlotViewModel sourceItem = (SaveSlotViewModel)e.Data;
            SaveSlotViewModel targetItem = (SaveSlotViewModel)e.TargetItem;

            if (e.DragInfo.SourceCollection == MySaveSlots)
            {
                if (e.TargetCollection == MySaveSlots)
                {
                    e.DropTargetAdorner = DropTargetAdorners.Highlight;
                    e.Effects = DragDropEffects.Move;
                }
                else if (e.TargetCollection == OtherSaveSlots)
                {
                    e.DropTargetAdorner = DropTargetAdorners.Highlight;
                    e.Effects = DragDropEffects.Copy;
                }
            }
            else if (e.DragInfo.SourceCollection == OtherSaveSlots)
            {
                if (e.TargetCollection == MySaveSlots && targetItem != null)
                {
                    e.DropTargetAdorner = DropTargetAdorners.Highlight;
                    e.Effects = DragDropEffects.Copy;
                }
            }
        }

        void IDropTarget.Drop(IDropInfo e)
        {
            SaveSlotViewModel sourceItem = (SaveSlotViewModel)e.Data;
            SaveSlotViewModel targetItem = (SaveSlotViewModel)e.TargetItem;

            if (e.DragInfo.SourceCollection == MySaveSlots)
            {
                if (e.TargetCollection == MySaveSlots)
                {
                    // When you move an item after the last item UnfilteredInsertIndex == MySaveSlots.Count
                    // which would throw and OutOfRange exception so in those cases we simply
                    // set insertIndex -= 1 which gives the same desired result (move item after)
                    int insertIndex = e.UnfilteredInsertIndex;

                    if (e.InsertPosition.HasFlag(RelativeInsertPosition.AfterTargetItem))
                        insertIndex--;

                    if (insertIndex > MySaveSlots.Count - 1)
                        insertIndex = MySaveSlots.Count - 1;

                    var tmp = MySaveSlots[insertIndex];
                    MySaveSlots[insertIndex] = MySaveSlots[e.DragInfo.SourceIndex];
                    MySaveSlots[e.DragInfo.SourceIndex] = tmp;
                }
                else if (e.TargetCollection == OtherSaveSlots)
                {
                    // Add (a copy) if not exists
                    if (OtherSaveSlots.FirstOrDefault(x => x.SoftCompare(sourceItem)) == null)
                        OtherSaveSlots.Add((SaveSlotViewModel)sourceItem.Clone());
                }
            }
            else if (e.DragInfo.SourceCollection == OtherSaveSlots)
            {
                if (e.TargetCollection == MySaveSlots && targetItem != null)
                {
                    int insertIndex = e.UnfilteredInsertIndex;
                    if (e.InsertPosition.HasFlag(RelativeInsertPosition.BeforeTargetItem))
                        insertIndex++;

                    // Replace
                    MySaveSlots.Insert(insertIndex, sourceItem);
                    MySaveSlots.Remove(targetItem);
                }
            }
        }

        #endregion
    }
}
