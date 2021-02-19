// <copyright file="MustBeValidReporteePolicyHandlerTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Test.Authentication
{
    using System;
    using System.Linq;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using Microsoft.Graph;
    using Microsoft.Teams.Apps.Timesheet.Authentication;
    using Microsoft.Teams.Apps.Timesheet.Helpers;
    using Microsoft.Teams.Apps.Timesheet.Models;
    using Microsoft.Teams.Apps.Timesheet.Repositories;
    using Microsoft.Teams.Apps.Timesheet.Tests.Fakes;
    using Microsoft.Teams.Apps.Timesheet.Tests.TestData;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// The class lists unit tests cases related to MustBeValidReporteePolicyHandler authorization.
    /// </summary>
    [TestClass]
    public class MustBeValidReporteePolicyHandlerTests
    {
        /// <summary>
        /// Holds the instance of <see cref="MustBeValidReporteePolicyHandler"/>.
        /// </summary>
        private MustBeValidReporteePolicyHandler mustBeValidReporteePolicyHandler;

        /// <summary>
        /// The mocked instance of repository accessors to access repositories.
        /// </summary>
        private Mock<IRepositoryAccessors> repositoryAccessors;

        /// <summary>
        /// The mocked instance of bot settings.
        /// </summary>
        private Mock<IOptions<BotSettings>> botOptions;

        /// <summary>
        /// The mocked instance of user helper.
        /// </summary>
        private Mock<IUserHelper> userHelper;

        /// <summary>
        /// The mocked instance of HTTP accessors.
        /// </summary>
        private IHttpContextAccessor httpAccessors;

        /// <summary>
        /// Initializes all test variables.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.repositoryAccessors = new Mock<IRepositoryAccessors>();
            this.botOptions = new Mock<IOptions<BotSettings>>();
            this.userHelper = new Mock<IUserHelper>();
            this.httpAccessors = FakeHttpContext.GetFakeHttpAccessorsForMustBeValidReporteePolicy();
            this.mustBeValidReporteePolicyHandler = new MustBeValidReporteePolicyHandler(this.userHelper.Object, this.httpAccessors);
        }

        /// <summary>
        /// Tests whether <see cref="MustBeValidReporteePolicyHandler"/> policy has succeeded.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task ValidateHandleAsync_ValidReportee_Succeed()
        {
            var memberRepository = new Mock<IMemberRepository>();
            this.userHelper
                .Setup(x => x.GetAllReporteesAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(TestData.Users.AsEnumerable()));

            this.repositoryAccessors.Setup(x => x.MemberRepository).Returns(memberRepository.Object);
            this.botOptions.Setup(x => x.Value).Returns(new BotSettings { ManagerReporteesCacheDurationInHours = 1 });

            var fakeAuthorizationContext = FakeHttpContext.GetFakeAuthorizationHandlerContextForMustBeValidReporteePolicy();
            await this.mustBeValidReporteePolicyHandler.HandleAsync(fakeAuthorizationContext);

            Assert.IsTrue(fakeAuthorizationContext.HasSucceeded);
        }

        /// <summary>
        /// Tests whether <see cref="MustBeValidReporteePolicyHandler"/> policy has succeeded.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task ValidateHandleAsync_InvalidReportee_Failed()
        {
            var memberRepository = new Mock<IMemberRepository>();
            this.userHelper
                .Setup(x => x.GetAllReporteesAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(Enumerable.Empty<User>()));

            this.repositoryAccessors.Setup(x => x.MemberRepository).Returns(memberRepository.Object);
            this.botOptions.Setup(x => x.Value).Returns(new BotSettings { ManagerReporteesCacheDurationInHours = 1 });

            var fakeAuthorizationContext = FakeHttpContext.GetFakeAuthorizationHandlerContextForMustBeValidReporteePolicy();
            await this.mustBeValidReporteePolicyHandler.HandleAsync(fakeAuthorizationContext);

            Assert.IsFalse(fakeAuthorizationContext.HasSucceeded);
        }
    }
}
