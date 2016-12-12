﻿using System;
using System.Diagnostics;
using System.IO;
using NLog;
using pdfforge.PDFCreator.Conversion.Jobs.JobInfo;
using pdfforge.PDFCreator.Conversion.Jobs.Jobs;
using pdfforge.PDFCreator.Core.SettingsManagement;
using pdfforge.PDFCreator.Utilities.IO;

namespace pdfforge.PDFCreator.Core.DirectConversion
{
    public abstract class DirectConversionBase : IDirectConversion
    {
        private readonly ISettingsProvider _settingsProvider;
        private readonly IJobInfoManager _jobInfoManager;
        internal static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected DirectConversionBase(ISettingsProvider settingsProvider, IJobInfoManager jobInfoManager)
        {
            _settingsProvider = settingsProvider;
            _jobInfoManager = jobInfoManager;
        }

        /// <summary>
        ///     Create unique job folder in spool folder and copy ps file to it.
        ///     Create inf file from ps file.
        /// </summary>
        /// <returns>inf file in spool folder</returns>
        public string TransformToInfFile(string file, string spoolFolder, string printerName = "")
        {
            if (string.IsNullOrEmpty(file))
            {
                Logger.Error("Launched job without file.");
                return "";
            }

            if (!File.Exists(file))
            {
                Logger.Error("The file \"" + file + "\" does not exist.");
                return "";
            }

            if (!IsValid(file))
                return "";

            string jobFolder;
            try
            {
                jobFolder = CreateJobFolderInSpool(file, spoolFolder);
            }
            catch (Exception ex)
            {
                Logger.Error("Error while creating spool directory for ps-job:\r\n" + ex.Message);
                return "";
            }

            if (string.IsNullOrWhiteSpace(printerName))
                printerName = GetPrimaryPrinter();

            try
            {
                var psFileInJobFolder = CopyFileToJobFolder(jobFolder, file);
                return CreateInfFile(file, jobFolder, psFileInJobFolder, printerName);
            }
            catch (Exception ex)
            {
                Logger.Error("Error while coping ps-file in spool folder:\r\n" + ex.Message);
                Directory.Delete(jobFolder, true); //Delete created folder and files
                return "";
            }
        }

        private string GetPrimaryPrinter()
        {
            return _settingsProvider.Settings.ApplicationSettings.PrimaryPrinter;
        }

        private static string CreateJobFolderInSpool(string file, string spoolFolder)
        {
            var psFilename = Path.GetFileName(file); 
            var jobFolder = Path.Combine(spoolFolder, psFilename);
            jobFolder = new UniqueDirectory(jobFolder).MakeUniqueDirectory();
            Directory.CreateDirectory(jobFolder);
            Logger.Trace("Created spool directory for ps-file job: " + jobFolder);

            return jobFolder;
        }

        private static string CopyFileToJobFolder(string jobFolder, string psFile)
        {
            var psFilename = Path.GetFileName(psFile);
            var psFileInJobFolder = Path.Combine(jobFolder, psFilename);
            File.Copy(psFile, psFileInJobFolder);
            Logger.Debug("Copied ps-file in spool folder: " + psFileInJobFolder);

            return psFileInJobFolder;
        }

        private string CreateInfFile(string psFile, string jobFolder, string psFileInJobFolder, string printerName)
        {
            var psFilename = Path.GetFileName(psFile);
            var infFile = Path.Combine(jobFolder, psFilename + ".inf");

            var jobInfo = new JobInfo();

            var sourceFileInfo = new SourceFileInfo();
            sourceFileInfo.Filename = psFileInJobFolder;
            sourceFileInfo.Author = Environment.UserName;
            sourceFileInfo.ClientComputer = Environment.MachineName.Replace("\\", "");
            sourceFileInfo.Copies = 1;
            sourceFileInfo.DocumentTitle = psFile;
            sourceFileInfo.JobCounter = 0;
            sourceFileInfo.JobId = 0;
            sourceFileInfo.PrinterName = printerName;
            sourceFileInfo.SessionId = Process.GetCurrentProcess().SessionId;
            sourceFileInfo.TotalPages = GetNumberOfPages(psFile);
            sourceFileInfo.Type = JobType.PsJob;
            sourceFileInfo.WinStation = Environment.GetEnvironmentVariable("SESSIONNAME");
            jobInfo.SourceFiles.Add(sourceFileInfo);

            _jobInfoManager.SaveToInfFile(jobInfo, infFile);
            Logger.Debug("Created inf-file for ps-file: " + infFile);

            return infFile;
        }

        internal abstract int GetNumberOfPages(string file);
        internal abstract bool IsValid(string file);
    }
}