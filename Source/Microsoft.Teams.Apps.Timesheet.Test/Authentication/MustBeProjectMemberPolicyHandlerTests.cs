// <copyright file="MustBeProjectMemberPolicyHandlerTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Test.Authentication
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
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
    /// The class lists unit tests cases related to MustBeProjectMemberPolicyHandler authorization.
    /// </summary>
    [TestClass]
    public class MustBeProjectMemberPolicyHandlerTests
    {
        /// <summary>
        /// Holds the instance of <see cref="MustBeProjectMemberPolicyHandler"/>.
        /// </summary>
        private MustBeProjectMemberPolicyHandler mustBeProjectMemberPolicyHandler;

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
        /// Initializes all test variables.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.repositoryAccessors = new Mock<IRepositoryAccessors>();
            this.memoryCache = new FakeMemoryCache();
            this.botOptions = new Mock<IOptions<BotSettings>>();
            this.mustBeProjectMemberPolicyHandler = new MustBeProjectMemberPolicyHandler(memoryCache, repositoryAccessors.Object, botOptions.Object);
        }

        /// <summary>
        /// Tests whether <see cref="MustBeProjectMemberPolicyHandler"/> policy has succeeded.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task ValidateHandleAsync_UserIsPartOfProjects_Succeed()
        {
            var memberRepository = new Mock<IMemberRepository>();
            memberRepository
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Member, bool>>>()))
                .Returns(Task.FromResult(TestData.Members.AsEnumerable()));

            this.repositoryAccessors.Setup(x => x.MemberRepository).Returns(memberRepository.Object);
            this.botOptions.Setup(x => x.Value).Returns(new BotSettings { UserPartOfProjectsCacheDurationInHour = 1 });

            var fakeAuthorizationContext = FakeHttpContext.GetFakeAuthorizationHandlerContext();
            await this.mustBeProjectMemberPolicyHandler.HandleAsync(fakeAuthorizationContext);

            Assert.IsTrue(fakeAuthorizationContext.HasSucceeded);
        }

        /// <summary>
        /// Tests whether <see cref="MustBeProjectMemberPolicyHandler"/> policy has failed.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task ValidateHandleAsync_UserIsNotPartOfProjects_Failed()
        {
            var memberRepository = new Mock<IMemberRepository>();
            memberRepository
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<Member, bool>>>()))
                .Returns(Task.FromResult(Enumerable.Empty<Member>()));

            this.repositoryAccessors.Setup(x => x.MemberRepository).Returns(memberRepository.Object);
            this.botOptions.Setup(x => x.Value).Returns(new BotSettings { UserPartOfProjectsCacheDurationInHour = 1 });

            var fakeAuthorizationContext = FakeHttpContext.GetFakeAuthorizationHandlerContext();
            await this.mustBeProjectMemberPolicyHandler.HandleAsync(fakeAuthorizationContext);

            Assert.IsFalse(fakeAuthorizationContext.HasSucceeded);
        }
    }
}
