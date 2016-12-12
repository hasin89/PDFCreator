﻿using System;
using System.Collections.Generic;
using pdfforge.LicenseValidator;
using pdfforge.PDFCreator.Conversion.Jobs.Jobs;
using pdfforge.PDFCreator.Core.Controller;
using pdfforge.PDFCreator.Core.GpoAdapter;
using pdfforge.PDFCreator.Core.Services.Licensing;
using pdfforge.PDFCreator.Core.SettingsManagement;
using pdfforge.PDFCreator.Core.Startup.StartConditions;
using pdfforge.PDFCreator.Editions.EditionBase;
using pdfforge.PDFCreator.UI.ViewModels.Assistants.Update;
using pdfforge.PDFCreator.UI.ViewModels.WindowViewModels;
using SimpleInjector;

namespace pdfforge.PDFCreator.Editions.PDFCreatorBusiness
{
    public class PDFCreatorBusinessBootstrapper : Bootstrapper
    {
        protected override string EditionName => "PDFCreator Business";
        protected override bool HideLicensing => false;
        protected override bool ShowWelcomeWindow => false;
        protected override ButtonDisplayOptions ButtonDisplayOptions => new ButtonDisplayOptions(true, true);

        protected override void RegisterUpdateAssistant(Container container)
        {
            container.RegisterSingleton<IUpdateAssistant, UpdateAssistant>();
            container.RegisterSingleton<IUpdateLauncher, AutoUpdateLauncher>();
            container.RegisterSingleton(() => new UpdateInformationProvider(Urls.PdfCreatorBusinessUpdateInfoUrl, "PDFCreatorBusiness"));
        }

        protected override void RegisterActivationHelper(Container container)
        {
            var product = Product.PdfCreatorBusiness;
            container.RegisterSingleton<ILicenseServerHelper>(() => new LicenseServerHelper(product));
            container.RegisterSingleton<IActivationHelper>(() => new ActivationHelper(product, container.GetInstance<ILicenseServerHelper>()));
        }

        protected override void RegisterUserTokenExtractor(Container container)
        {
            container.Register<IPsParserFactory, PsParserFactory>();
            container.Register<IUserTokenExtractor, UserTokenExtractor>();
        }

        protected override IList<Type> GetStartupConditions(IList<Type> defaultConditions)
        {
            defaultConditions.Add(typeof(TerminalServerNotAllowedCondition));
            defaultConditions.Add(typeof(LicenseCondition));

            return defaultConditions;
        }

        protected override SettingsProvider CreateSettingsProvider()
        {
            return new GpoAwareSettingsProvider();
        }
    }
}
