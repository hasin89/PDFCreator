﻿using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using pdfforge.PDFCreator.Conversion.Jobs.JobInfo;

namespace pdfforge.PDFCreator.Core.Workflow
{
    /// <summary>
    ///     The JobInfoQueue manages the pending JobInfos that are waiting to be converted
    /// </summary>
    public class JobInfoQueue : IJobInfoQueue
    {
        private readonly HashSet<string> _jobFileSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly IJobInfoManager _jobInfoManager;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public JobInfoQueue(IJobInfoManager jobInfoManager)
        {
            _jobInfoManager = jobInfoManager;
            JobInfos = new List<JobInfo>();
        }

        public IList<JobInfo> JobInfos { get; }

        public event EventHandler<NewJobInfoEventArgs> OnNewJobInfo;

        /// <summary>
        ///     Get the number ob items in the JobInfo Queue
        /// </summary>
        public int Count => JobInfos.Count;

        /// <summary>
        ///     Get the next pending job. If this is null, the queue is empty
        /// </summary>
        public JobInfo NextJob => IsEmpty ? null : JobInfos[0];

        /// <summary>
        ///     Determines if the Queue is emtpy
        /// </summary>
        /// <returns>true, if the Queue is empty</returns>
        public bool IsEmpty => JobInfos.Count == 0;

        /// <summary>
        ///     Appends an item to the end of the JobInfo Queue
        /// </summary>
        /// <param name="jobInfo">The JobInfo to add</param>
        public void Add(JobInfo jobInfo)
        {
            var jobFile = Path.GetFullPath(jobInfo.InfFile);
            _logger.Debug("New JobInfo: " + jobFile);
            _logger.Debug("DocumentTitle: " + jobInfo.SourceFiles[0].DocumentTitle);
            _logger.Debug("ClientComputer: " + jobInfo.SourceFiles[0].ClientComputer);
            _logger.Debug("SessionId: " + jobInfo.SourceFiles[0].SessionId);
            _logger.Debug("PrinterName: " + jobInfo.SourceFiles[0].PrinterName);
            _logger.Debug("JobCounter: " + jobInfo.SourceFiles[0].JobCounter);
            _logger.Debug("JobId: " + jobInfo.SourceFiles[0].JobId);

            if (_jobFileSet.Contains(jobFile))
                return;

            _logger.Debug("Added JobInfo: " + jobFile);
            JobInfos.Add(jobInfo);
            _jobFileSet.Add(jobFile);

            OnNewJobInfo?.Invoke(null, new NewJobInfoEventArgs(jobInfo));
        }

        /// <summary>
        ///     Removes a JobInfo from the Queue
        /// </summary>
        /// <param name="jobInfo">The JobInfo to remove</param>
        /// <returns>true, if successful</returns>
        public bool Remove(JobInfo jobInfo)
        {
            return Remove(jobInfo, false);
        }

        public void Add(IEnumerable<JobInfo> jobInfos)
        {
            foreach (var jobInfo in jobInfos)
            {
                Add(jobInfo);
            }
        }

        /// <summary>
        ///     Removes a JobInfo from the Queue
        /// </summary>
        /// <param name="jobInfo">The JobInfo to remove</param>
        /// <param name="deleteFiles">If true, the inf and source files will be deleted</param>
        /// <returns>true, if successful</returns>
        public bool Remove(JobInfo jobInfo, bool deleteFiles)
        {
            _jobFileSet.Remove(jobInfo.InfFile);

            if (deleteFiles)
                _jobInfoManager.DeleteInfAndSourceFiles(jobInfo);

            return JobInfos.Remove(jobInfo);
        }
    }
}