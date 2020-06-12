using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shell;

namespace Model
{
    public class ReadLineExtractorModel
    {
        bool measureDownloadTime;
        string downloadedFileSize = string.Empty;
        bool isIndeterminate = false;
        TaskbarItemProgressState taskbarItemProgressStateModel = TaskbarItemProgressState.Normal;
        int progressBarPercent = 0;
        double taskBarProgressValue = 0.0;
        bool measureProcessingTime = false;
        string standardOutputOut = string.Empty;

        public (bool measureDownloadTime, string downloadedFileSize, bool isIndeterminate, TaskbarItemProgressState taskbarItemProgressStateModel, int progressBarPercent, double taskBarProgressValue, bool measureProcessingTime, string standardOutputOut) Extract(string standardOutput, string finishedMessage)
        {
            int positionFrom;
            int positionTo;
           
            if (standardOutput.Contains(Localization.Properties.Resources.ThreadPoolWorkerDownload) && standardOutput.Contains(Localization.Properties.Resources.ThreadPoolWorkerEta))
            {
                measureDownloadTime = true;
                positionFrom = standardOutput.IndexOf(Localization.Properties.Resources.ThreadPoolWorkerOf, StringComparison.InvariantCultureIgnoreCase) + Localization.Properties.Resources.ThreadPoolWorkerOf.Length;
                positionTo = standardOutput.LastIndexOf(Localization.Properties.Resources.ThreadPoolWorkerAt, StringComparison.InvariantCultureIgnoreCase);

                if ((positionTo - positionFrom) > 0)
                    downloadedFileSize = standardOutput.Substring(positionFrom, positionTo - positionFrom);

                positionFrom = standardOutput.IndexOf("] ", StringComparison.InvariantCultureIgnoreCase) + "] ".Length;
                positionTo = standardOutput.LastIndexOf("%", StringComparison.InvariantCultureIgnoreCase);

                if ((positionTo - positionFrom) > 0)
                {
                    var percent = standardOutput.Substring(positionFrom, positionTo - positionFrom);
                    if (double.TryParse(percent.Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var downloadedPercent))
                    {
                        isIndeterminate = false;
                        taskbarItemProgressStateModel = TaskbarItemProgressState.Normal;
                        progressBarPercent = Convert.ToInt32(Math.Round(downloadedPercent));
                        taskBarProgressValue = HelpersModel.GetTaskBarProgressValue(100, progressBarPercent);
                    }
                    else
                    {
                        isIndeterminate = true;
                        taskbarItemProgressStateModel = TaskbarItemProgressState.Indeterminate;
                    }
                }
                standardOutputOut = standardOutput;
            }
            if (standardOutput.Contains("has already been"))
            {
                standardOutputOut = standardOutput;
                downloadedFileSize = Localization.Properties.Resources.FileHasAlreadyBeenDownloaded;
                measureDownloadTime = false;
            }
            if (standardOutput.Contains("[ffmpeg]"))
            {
                standardOutputOut = finishedMessage;
                measureProcessingTime = true;
                measureDownloadTime = false;
            }

            return (measureDownloadTime, downloadedFileSize, isIndeterminate, taskbarItemProgressStateModel, progressBarPercent, taskBarProgressValue, measureProcessingTime, standardOutputOut);
        }
    }
}
