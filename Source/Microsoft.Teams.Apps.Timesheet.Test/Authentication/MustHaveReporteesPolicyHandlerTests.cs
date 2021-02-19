// <copyright file="MustHaveReporteesPolicyHandlerTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Test.Authentication
{
    using System.Linq;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using Microsoft.Graph;
    using Microsoft.Teams.Apps.Timesheet.Authentication;
    using Microsoft.Teams.Apps.Timesheet.Models;
    using Microsoft.Teams.Apps.Timesheet.Services.MicrosoftGraph;
    using Microsoft.Teams.Apps.Timesheet.Tests.Fakes;
    using Microsoft.Teams.Apps.Timesheet.Tests.TestData;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// The class lists unit tests cases related to MustHaveReporteesPolicyHandler authorization.
    /// </summary>
    [TestClass]
    public class MustHaveReporteesPolicyHandlerTests
    {
        /// <summary>
        /// Holds the instance of <see cref="MustBeManagerPolicyHandler"/>.
        /// </summary>
        private MustBeManagerPolicyHandler mustHaveReporteesPolicyHandler;

        /// <summary>
        /// The mocked instance of user service.
        /// </summary>
        private Mock<IUsersService> userService;

        /// <summary>
        /// The instance of memory cache.
        /// </summary>
        private IMemoryCache memoryCache;

        /// <summary>
        /// The mocked instance of bot settings.
        /// </summary>
        private Mock<IOptions<BotSettings>> botOptions;

        /// <summary>
        /// Initializes all test variables.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.userService = new Mock<IUsersService>();
            this.memoryCache = new FakeMemoryCache();
            this.botOptions = new Mock<IOptions<BotSettings>>();
            this.mustHaveReporteesPolicyHandler = new MustBeManagerPolicyHandler(this.memoryCache, this.userService.Object, this.botOptions.Object);
        }

        /// <summary>
        /// Tests whether <see cref="MustBeManagerPolicyHandler"/> policy succeed.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task ValidateHandleAsync_HasReportees_Succeed()
        {
            this.userService
                .Setup(x => x.GetReporteesAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(TestData.Reportees.AsEnumerable()));

            this.botOptions.Setup(x => x.Value).Returns(new BotSettings { ManagerReporteesCacheDurationInHours = 1 });

            var authorizationContext = FakeHttpContext.GetFakeAuthorizationHandlerContextForMustHaveReporteesPolicy();
            await this.mustHaveReporteesPolicyHandler.HandleAsync(authorizationContext);

            Assert.IsTrue(authorizationContext.HasSucceeded);
        }

        /// <summary>
        /// Tests whether <see cref="MustBeManagerPolicyHandler"/> policy failed.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task ValidateHandleAsync_NoReportees_Failed()
        {
            this.userService
                .Setup(x => x.GetReporteesAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(Enumerable.Empty<User>()));

            this.botOptions.Setup(x => x.Value).Returns(new BotSettings { ManagerReporteesCacheDurationInHours = 1 });

            var authorizationContext = FakeHttpContext.GetFakeAuthorizationHandlerContextForMustHaveReporteesPolicy();
            await this.mustHaveReporteesPolicyHandler.HandleAsync(authorizationContext);

            Assert.IsFalse(authorizationContext.HasSucceeded);
        }
    }
}
