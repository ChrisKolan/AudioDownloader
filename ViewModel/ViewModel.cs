using ViewModel.Helper;

namespace ViewModel
{
    public class ViewModel
    {
        public ViewModel()
        {
            Model = new Model.Model();
            DownloadButton = new ActionCommand(DownloadButtonCommand);
            HelpButton = new ActionCommand(HelpButtonCommand);
        }

        public Model.Model Model { get; set; }
        public ActionCommand DownloadButton { get; set; }
        public ActionCommand HelpButton { get;  set; }

        private void DownloadButtonCommand()
        {
            Model.DownloadButtonClick();
        }
        private void HelpButtonCommand()
        {
            Model.HelpButtonClick();
        }
    }
}
