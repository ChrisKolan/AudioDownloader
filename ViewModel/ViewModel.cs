namespace ViewModel
{
    public class ViewModel
    {
        public ViewModel()
        {
            Model = new Model.Model();
            DownloadButton = new Helper.ActionCommand(DownloadButtonCommand);
        }

        public Model.Model Model { get; set; }
        public Helper.ActionCommand DownloadButton { get; set; }

        private void DownloadButtonCommand()
        {
            Model.DownloadButtonClick();
        }
    }
}
