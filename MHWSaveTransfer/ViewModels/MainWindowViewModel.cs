using Cirilla.Core.Models;
using GongSolutions.Wpf.DragDrop;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace MHWSaveTransfer.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged, IDropTarget
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<SaveSlotViewModel> MySaveSlots { get; } = new ObservableCollection<SaveSlotViewModel>();
        public ObservableCollection<SaveSlotViewModel> OtherSaveSlots { get; } = new ObservableCollection<SaveSlotViewModel>();

        public string SteamId => _saveData?.Header.SteamId.ToString() ?? "(none)";

        public RelayCommand OpenSaveDataCommand { get; }
        public RelayCommand SaveSaveDataCommand { get; }
        public RelayCommand ImportSaveDataCommand { get; }
        public RelayCommand ClearWorkspaceCommand { get; }

        private SaveData _saveData { get; set; }

        public MainWindowViewModel()
        {
            OpenSaveDataCommand = new RelayCommand(OpenSaveData);
            SaveSaveDataCommand = new RelayCommand(SaveSaveData, CanSaveSaveData);
            ImportSaveDataCommand = new RelayCommand(ImportSaveData, CanImportSaveData);
            ClearWorkspaceCommand = new RelayCommand(ClearWorkspace);
        }

        #region Commands

        private void OpenSaveData()
        {
            MySaveSlots.Clear();
            _saveData = null;

            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    _saveData = new SaveData(ofd.FileName);
                    _saveData.SaveSlots.ForEach(x => MySaveSlots.Add(new SaveSlotViewModel(x)));
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
                        saveData.SaveSlots.ForEach(x =>
                        {
                            // Only add if not already added, based on SaveSlotViewModel.Equals()
                            SaveSlotViewModel newVm = new SaveSlotViewModel(x);
                            if (OtherSaveSlots.Contains(newVm) == false)
                                OtherSaveSlots.Add(newVm);
                        });
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
                    // Change order
                }
                else if (e.TargetCollection == OtherSaveSlots)
                {
                    // Add (a copy) if not exists
                    if (OtherSaveSlots.Contains(sourceItem) == false)
                        OtherSaveSlots.Add((SaveSlotViewModel)sourceItem.Clone());
                }
            }
            else if (e.DragInfo.SourceCollection == OtherSaveSlots)
            {
                if (e.TargetCollection == MySaveSlots && targetItem != null)
                {
                    // Replace
                    MySaveSlots.Insert(e.UnfilteredInsertIndex, sourceItem);
                    MySaveSlots.Remove(targetItem);
                }
            }
        }

        #endregion
    }
}
