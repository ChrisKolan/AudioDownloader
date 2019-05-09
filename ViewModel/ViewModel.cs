using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ViewModel
{
    public class ViewModel : INotifyPropertyChanged
    {
        private string _selectedQuality;
        //private string _standardOutput;

        public ViewModel()
        {
            Model = new Model.Model();
            Quality = new ObservableCollection<string>
            {
                "Audio quality: 0 Best (Largest mp3 file size)",
                "Audio quality: 1 Better",
                "Audio quality: 2 Optimal",
                "Audio quality: 3 Very good",
                "Audio quality: 4 Good (Balanced mp3 file size)",
                "Audio quality: 5 Default",
                "Audio quality: 6 Average",
                "Audio quality: 7 AudioBook",
                "Audio quality: 8 Worse",
                "Audio quality: 9 Worst (Smallest mp3 file size)"
            };
            SelectedQuality = Quality[2];
            DownloadButton = new Helper.ActionCommand(DownloadButtonCommand);
        }

        public Model.Model Model { get; set; }
        public Helper.ActionCommand DownloadButton { get; set; }
        public string DownloadLink { get; set; }
        public ObservableCollection<string> Quality { get; set; }
        public string SelectedQuality
        {
            get { return _selectedQuality; }
            set
            {
                _selectedQuality = value;
                OnPropertyChanged(nameof(SelectedQuality));
            }
        }

        //public string StandardOutput
        //{
        //    get { return _standardOutput; }
        //    set
        //    {
        //        _standardOutput = Model.CurrentStandardOutputLine;
        //        OnPropertyChanged(nameof(StandardOutput));
        //    }
        //}

        private void DownloadButtonCommand()
        {
            if (string.IsNullOrWhiteSpace(DownloadLink))
            {
                Model.StandardOutput = "Empty link";
                return;
            }
           
            Model.DownloadButtonClick(DownloadLink, SelectedQuality);
        }

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
