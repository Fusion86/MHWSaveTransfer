using Cirilla.Core.Models;
using GongSolutions.Wpf.DragDrop;
using MHWSaveTransfer.Dialogs;
using MHWSaveTransfer.Helpers;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace MHWSaveTransfer.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged, IDropTarget
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<SaveSlotViewModel> MySaveSlots { get; } = new ObservableCollection<SaveSlotViewModel>();
        public ObservableCollection<SaveSlotViewModel> OtherSaveSlots { get; } = new ObservableCollection<SaveSlotViewModel>();

        public string SteamId => _saveData?.SteamId.ToString() ?? "(none)";
        public string VersionString => "Version: " + Assembly.GetExecutingAssembly().GetName().Version;

        public RelayCommand OpenSaveDataCommand { get; }
        public RelayCommand SaveSaveDataCommand { get; }
        public RelayCommand ImportSaveDataCommand { get; }
        public RelayCommand ClearWorkspaceCommand { get; }
        public RelayCommand ChangeSteamIdCommand { get; }

        private SaveData _saveData { get; set; }

        public MainWindowViewModel()
        {
            OpenSaveDataCommand = new RelayCommand(OpenSaveData);
            SaveSaveDataCommand = new RelayCommand(SaveSaveData, CanSaveSaveData);
            ImportSaveDataCommand = new RelayCommand(ImportSaveData, CanImportSaveData);
            ClearWorkspaceCommand = new RelayCommand(ClearWorkspace);
            ChangeSteamIdCommand = new RelayCommand(ChangeSteamId, CanChangeSteamId);
        }

        #region Commands

        private void OpenSaveData()
        {
            MySaveSlots.Clear();
            _saveData = null;

            OpenFileDialog ofd = new OpenFileDialog();

            string savePath = Utility.GetMhwSaveDir();
            if (savePath != null)
                ofd.InitialDirectory = savePath;

            if (ofd.ShowDialog() == true)
            {
                try
                {
                    _saveData = new SaveData(ofd.FileName);

                    foreach (var slot in _saveData.SaveSlots)
                        MySaveSlots.Add(new SaveSlotViewModel(slot));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Couldn't open " + Path.GetFileName(ofd.FileName), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanSaveSaveData() => _saveData != null;
        private void SaveSaveData()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = "SAVEDATA1000";
            if (sfd.ShowDialog() == true)
            {
                for (int i = 0; i < 3; i++)
                    _saveData.SaveSlots[i] = MySaveSlots[i].SaveSlot;

                _saveData.Save(sfd.FileName);
            }
        }

        private bool CanImportSaveData() => true;
        private async void ImportSaveData()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
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
            _saveData = null;
        }

        private bool CanChangeSteamId() => _saveData != null;
        private void ChangeSteamId()
        {
            EnterTextDialog dialog = new EnterTextDialog("Enter SteamID", SteamId);
            dialog.Owner = Application.Current.MainWindow;
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Try to set SteamId in savedata
                    _saveData.SteamId = long.Parse(dialog.Text);

                    // Update UI
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(SteamId)));
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
            SaveSlotViewModel sourceItem = e.Data as SaveSlotViewModel;
            SaveSlotViewModel targetItem = e.TargetItem as SaveSlotViewModel;

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
            SaveSlotViewModel sourceItem = e.Data as SaveSlotViewModel;
            SaveSlotViewModel targetItem = e.TargetItem as SaveSlotViewModel;

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
