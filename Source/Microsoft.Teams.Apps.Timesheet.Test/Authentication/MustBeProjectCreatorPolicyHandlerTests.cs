// <copyright file="MustBeProjectCreatorPolicyHandlerTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Test.Authentication
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.Timesheet.Authentication;
    using Microsoft.Teams.Apps.Timesheet.Models;
    using Microsoft.Teams.Apps.Timesheet.Repositories;
    using Microsoft.Teams.Apps.Timesheet.Tests.Fakes;
    using Microsoft.Teams.Apps.Timesheet.Tests.TestData;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// The class lists unit tests cases related to MustBeProjectCreatorPolicyHandler authorization.
    /// </summary>
    [TestClass]
    public class MustBeProjectCreatorPolicyHandlerTests
    {
        /// <summary>
        /// Holds the instance of <see cref="MustBeProjectCreatorPolicyHandler"/>.
        /// </summary>
        private MustBeProjectCreatorPolicyHandler mustBeProjectCreatorPolicyHandler;

        /// <summary>
        /// The mocked instance of repository accessors to access repositories.
        /// </summary>
        private Mock<IRepositoryAccessors> repositoryAccessors;

        /// <summary>
        /// The instance of memory cache.
        /// </summary>
        private IMemoryCache memoryCache;

        /// <summary>
        /// The mocked instance of bot settings.
        /// </summary>
        private Mock<IOptions<BotSettings>> botOptions;

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
            this.memoryCache = new FakeMemoryCache();
            this.botOptions = new Mock<IOptions<BotSettings>>();
            this.httpAccessors = FakeHttpContext.GetFakeHttpAccessorsForMustBeProjectCreatorPolicy();
            this.mustBeProjectCreatorPolicyHandler = new MustBeProjectCreatorPolicyHandler(this.memoryCache, this.repositoryAccessors.Object, this.botOptions.Object, this.httpAccessors);
        }

        /// <summary>
        /// Tests whether <see cref="MustBeProjectCreatorPolicyHandler"/> policy succeed.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task ValidateHandleAsync_UserIsProjectCreator_Succeed()
        {
            var projectRepository = new Mock<IProjectRepository>();
            projectRepository
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Project, bool>>>()))
                .Returns(Task.FromResult(TestData.Projects.AsEnumerable()));

            this.repositoryAccessors.Setup(x => x.ProjectRepository).Returns(projectRepository.Object);
            this.botOptions.Setup(x => x.Value).Returns(new BotSettings { ManagerProjectValidationCacheDurationInHours = 1 });

            var authorizationContext = FakeHttpContext.GetFakeAuthorizationHandlerContextForMustBeProjectCreatorPolicy();
            await this.mustBeProjectCreatorPolicyHandler.HandleAsync(authorizationContext);

            Assert.IsTrue(authorizationContext.HasSucceeded);
        }

        /// <summary>
        /// Tests whether <see cref="MustBeProjectCreatorPolicyHandler"/> policy failed.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task ValidateHandleAsync_UserIsNotProjectCreator_Failed()
        {
            var projectRepository = new Mock<IProjectRepository>();
            projectRepository
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Project, bool>>>()))
                .Returns(Task.FromResult(Enumerable.Empty<Project>()));

            this.repositoryAccessors.Setup(x => x.ProjectRepository).Returns(projectRepository.Object);
            this.botOptions.Setup(x => x.Value).Returns(new BotSettings { ManagerProjectValidationCacheDurationInHours = 1 });

            var authorizationContext = FakeHttpContext.GetFakeAuthorizationHandlerContextForMustBeProjectCreatorPolicy();
            await this.mustBeProjectCreatorPolicyHandler.HandleAsync(authorizationContext);

            Assert.IsFalse(authorizationContext.HasSucceeded);
        }
    }
}
