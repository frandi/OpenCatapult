﻿using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Azure.Management.AppService.Fluent;
using Moq;
using Newtonsoft.Json;
using Polyrific.Catapult.Plugins.Core.Configs;
using Xunit;

namespace Polyrific.Catapult.Plugins.AzureAppService.UnitTests
{
    public class HostingProviderTests
    {
        private readonly Mock<IAzureUtils> _azureUtils;
        private readonly Mock<IDeployUtils> _deployUtils;

        public HostingProviderTests()
        {
            _azureUtils = new Mock<IAzureUtils>();
            _deployUtils = new Mock<IDeployUtils>();
        }

        [Theory]
        [InlineData("")]
        [InlineData("dev")]
        public async void Deploy_Success(string slotName)
        {
            var webSite = new Mock<IWebApp>();
            webSite.SetupGet(x => x.DefaultHostName).Returns("https://test.azurewebsites.net");

            var slot = new Mock<IDeploymentSlot>();
            slot.SetupGet(x => x.DefaultHostName).Returns("https://test.azurewebsites.net");

            _azureUtils.Setup(x => x.GetOrCreateWebsite(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(webSite.Object);
            _azureUtils.Setup(x => x.GetSlot(It.IsAny<IWebApp>(), slotName)).Returns(slot.Object);
            _azureUtils.Setup(x => x.GetPublishingProfile(It.IsAny<IWebAppBase>())).Returns(new Mock<IPublishingProfile>().Object);
            _deployUtils.Setup(x => x.ExecuteDeployWebsiteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            var artifact = Path.Combine(AppContext.BaseDirectory, "working", "20180817.1");

            var taskConfig = new DeployTaskConfig
            {
                ArtifactLocation = artifact,
                WorkingLocation = artifact,
            };
            var additionalConfigs = new Dictionary<string, string>
            {
                { "ApplicationId", "123" },
                { "ApplicationKey", "xxx" },
                { "TenantId", "123" },
                { "SubscriptionId", "subsid" },
                { "ResourceGroupName", "resourcegroup" },
                { "AppServiceName", "myproject" },
                { "DeploymentSlot", slotName }
            };

            var provider = new Program(new string[] { GetArgString("main", "TestProject", taskConfig, additionalConfigs) }, _azureUtils.Object, _deployUtils.Object);

            var result = await provider.Deploy();

            Assert.Equal("https://test.azurewebsites.net", result.hostLocation);
            Assert.Equal("", result.errorMessage);
        }

        private string GetArgString(string process, string projectName, DeployTaskConfig taskConfig, Dictionary<string, string> additionalConfigs)
        {
            var dict = new Dictionary<string, object>
            {
                {"process", process},
                {"project", projectName},
                {"config", taskConfig},
                {"additional", additionalConfigs}
            };

            return JsonConvert.SerializeObject(dict);
        }
    }
}
