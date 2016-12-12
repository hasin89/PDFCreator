﻿using NUnit.Framework;
using pdfforge.DataStorage;
using pdfforge.PDFCreator.Core.SettingsManagement;

namespace pdfforge.PDFCreator.UnitTest.Core.SettingsManagement
{
    [TestFixture]
    internal class SettingsUpgrader3To4Test
    {
        [Test]
        public void DataWithVersion3_UpgradeRequiredToVersion4_ReturnsTrue()
        {
            var data = Data.CreateDataStorage();
            data.SetValue(SettingsUpgrader.VersionSettingPath, "3");

            var upgrader = new SettingsUpgrader(data);

            Assert.IsTrue(upgrader.RequiresUpgrade(4));
        }

        [Test]
        public void DataWithVersion3_UpgradeToVersion4_SetsVersionTo4()
        {
            var data = Data.CreateDataStorage();
            data.SetValue(SettingsUpgrader.VersionSettingPath, "3");
            var upgrader = new SettingsUpgrader(data);

            upgrader.Upgrade(4);

            Assert.AreEqual("4", data.GetValue(SettingsUpgrader.VersionSettingPath));
        }

        [Test]
        public void Test_RenameTiffColorBlackWhiteAsBlackWhiteG4Fax()
        {
            var data = Data.CreateDataStorage();
            data.SetValue(SettingsUpgrader.VersionSettingPath, "3");
            data.SetValue(@"ConversionProfiles\numClasses", "1");
            data.SetValue(@"ConversionProfiles\0\TiffSettings\Color", "BlackWhite");

            var upgrader = new SettingsUpgrader(data);

            upgrader.Upgrade(4);

            Assert.AreEqual("BlackWhiteG4Fax", data.GetValue(@"ConversionProfiles\0\TiffSettings\Color"));
        }
    }
}
