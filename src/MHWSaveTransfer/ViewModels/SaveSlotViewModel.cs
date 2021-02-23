using Cirilla.Core.Enums;
using Cirilla.Core.Models;
using MHWSaveTransfer.Dialogs;
using System;
using System.ComponentModel;
using System.Windows;

namespace MHWSaveTransfer.ViewModels
{
    public class SaveSlotViewModel : INotifyPropertyChanged, ICloneable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public RelayCommand ChangeHunterNameCommand { get; }
        public RelayCommand ChangePalicoNameCommand { get; }
        public RelayCommand ToggleGenderCommand { get; }

        public SaveSlot SaveSlot;

        public SaveSlotViewModel(SaveSlot saveSlot)
        {
            SaveSlot = saveSlot;

            ChangeHunterNameCommand = new RelayCommand(ChangeHunterName);
            ChangePalicoNameCommand = new RelayCommand(ChangePalicoName);
            ToggleGenderCommand = new RelayCommand(ToggleGender);
        }

        public override bool Equals(object? obj)
        {
            if (obj is SaveSlotViewModel other)
            {
                return SaveSlot.Equals(other.SaveSlot);
            }
            else
            {
                return base.Equals(obj);
            }
        }

        public override int GetHashCode()
        {
            return SaveSlot.GetHashCode();
        }

        public object Clone()
        {
            return new SaveSlotViewModel((SaveSlot)SaveSlot.Clone());
        }

        /// <summary>
        /// Compare based on only HunterName + PalicoName + PlayTime
        /// </summary>
        /// <returns>True when same</returns>
        public bool SoftCompare(SaveSlotViewModel other)
        {
            string str1 = SaveSlot.HunterName + SaveSlot.PalicoName + SaveSlot.PlayTime;
            string str2 = other.SaveSlot.HunterName + other.SaveSlot.PalicoName + other.SaveSlot.PlayTime;
            return str1 == str2;
        }

        public string HunterName { get => SaveSlot.HunterName; set => SaveSlot.HunterName = value; }
        public int HunterRank { get => SaveSlot.HunterRank; set => SaveSlot.HunterRank = value; }
        public int Zenny { get => SaveSlot.Zenny; set => SaveSlot.Zenny = value; }
        public int ResearchPoints { get => SaveSlot.ResearchPoints; set => SaveSlot.ResearchPoints = value; }
        public int HunterXp { get => SaveSlot.HunterXp; set => SaveSlot.HunterXp = value; }
        public string Gender => Enum.GetName(typeof(Gender), SaveSlot.CharacterAppearance.Gender);
        public string PalicoName { get => SaveSlot.PalicoName; set => SaveSlot.PalicoName = value; }

        public string PlayTime
        {
            get
            {
                TimeSpan time = TimeSpan.FromSeconds(SaveSlot.PlayTime);
                return Math.Floor(time.TotalHours) + time.ToString(@"\:mm\:ss");
            }
        }

        #region Commands

        private void ChangeHunterName()
        {
            EnterTextDialog dialog = new EnterTextDialog("Enter hunter name", HunterName);
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    HunterName = dialog.Text;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Couldn't change hunter name", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ChangePalicoName()
        {
            EnterTextDialog dialog = new EnterTextDialog("Enter palico name", PalicoName);
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    PalicoName = dialog.Text;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Couldn't change palico name", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ToggleGender()
        {
            SaveSlot.CharacterAppearance.Gender = SaveSlot.CharacterAppearance.Gender == Cirilla.Core.Enums.Gender.Female ? Cirilla.Core.Enums.Gender.Male : Cirilla.Core.Enums.Gender.Female;
            PropertyChanged(this, new PropertyChangedEventArgs(nameof(Gender)));
        }

        #endregion
    }
}
