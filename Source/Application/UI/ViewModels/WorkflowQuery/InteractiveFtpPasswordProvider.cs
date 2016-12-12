﻿using System.Text;
using NLog;
using pdfforge.DynamicTranslator;
using pdfforge.Obsidian;
using pdfforge.PDFCreator.Conversion.Actions.Queries;
using pdfforge.PDFCreator.Conversion.Jobs;
using pdfforge.PDFCreator.Conversion.Jobs.Jobs;
using pdfforge.PDFCreator.Conversion.Jobs.Query;
using pdfforge.PDFCreator.Core.Workflow.Exceptions;
using pdfforge.PDFCreator.UI.Interactions;
using pdfforge.PDFCreator.UI.Interactions.Enums;

namespace pdfforge.PDFCreator.UI.ViewModels.WorkflowQuery
{
    public class InteractiveFtpPasswordProvider : IFtpPasswordProvider
    {
        private readonly IInteractionInvoker _interactionInvoker;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly ITranslator _translator;

        public InteractiveFtpPasswordProvider(ITranslator translator, IInteractionInvoker interactionInvoker)
        {
            _translator = translator;
            _interactionInvoker = interactionInvoker;
        }

        public bool SetPassword(Job job)
        {
            job.Passwords.FtpPassword = job.Profile.Ftp.Password;

            if (!string.IsNullOrEmpty(job.Passwords.FtpPassword))
                return true;

            var result = QueryFtpPassword(job);

            if (result.Success)
            {
                job.Passwords.FtpPassword = result.Data;
                return true;
            }

            job.Profile.Ftp.Enabled = false;
            return false;
        }

        public ActionResult RetypePassword(Job job)
        {
            _logger.Debug("Retype FTP password started");

            var interaction = CreateAndInvokeInteraction(job, true);

            if (interaction.Result != PasswordResult.StorePassword)
                return new ActionResult(ErrorCode.Ftp_UserCancelled);  

            job.Passwords.FtpPassword = interaction.Password;
            return new ActionResult();
        }

        private QueryResult<string> QueryFtpPassword(Job job)
        {
            var result = new QueryResult<string>();

            var interaction = CreateAndInvokeInteraction(job, false);

            if (interaction.Result == PasswordResult.Skip)
            {
                _logger.Info("User skipped ftp password. Ftp upload disabled.");
                return result;
            }

            if (interaction.Result == PasswordResult.StorePassword)
            {
                result.Success = true;
                result.Data = interaction.Password;
                return result;
            }

            throw new AbortWorkflowException("Cancelled the FTP password dialog.");
        }

        private PasswordInteraction CreateAndInvokeInteraction(Job job, bool retype)
        {
            var sb = new StringBuilder();
            if (retype)
                sb.AppendLine(_translator.GetTranslation("InteractiveWorkflow", "RetypeSmtpPwMessage"));
            
            var title = _translator.GetTranslation("FtpActionSettings", "PasswordTitle");
            var description = _translator.GetTranslation("FtpActionSettings", "PasswordDescription");

            var button = retype ? PasswordMiddleButton.None : PasswordMiddleButton.Skip;
            var interaction = new PasswordInteraction(button, title, description, false)
            {
                Password = job.Passwords.FtpPassword,
                IntroText = sb.ToString()
            };

            _interactionInvoker.Invoke(interaction);

            return interaction;
        }
    }
}
