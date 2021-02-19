// <copyright file="TimesheetControllerTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Tests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.Timesheet.Controllers;
    using Microsoft.Teams.Apps.Timesheet.Helpers;
    using Microsoft.Teams.Apps.Timesheet.ModelMappers;
    using Microsoft.Teams.Apps.Timesheet.Models;
    using Microsoft.Teams.Apps.Timesheet.Repositories;
    using Microsoft.Teams.Apps.Timesheet.Tests.Fakes;
    using Microsoft.Teams.Apps.Timesheet.Tests.TestData;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Models = Microsoft.Teams.Apps.Timesheet.Models;
    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// Timesheet controller tests contains all the test cases for the timesheet CRUD operations.
    /// </summary>
    [TestClass]
    public class TimesheetControllerTests
    {
        /// <summary>
        /// Holds the instance telemetryClient.
        /// </summary>
        private TelemetryClient telemetryClient;

        /// <summary>
        /// Holds the instance of timesheet controller.
        /// </summary>
        private TimesheetController timesheetController;

        /// <summary>
        /// The mocked instance of timesheet helper.
        /// </summary>
        private Mock<ITimesheetHelper> timesheetHelper;

        /// <summary>
        /// The mocked instance of user helper.
        /// </summary>
        private Mock<IUserHelper> userHelper;

        /// <summary>
        /// The mocked instance of manager dashboard helper.
        /// </summary>
        private Mock<IManagerDashboardHelper> managerDashboardHelper;

        /// <summary>
        /// The mocked instance of timesheet repository.
        /// </summary>
        private Mock<ITimesheetRepository> timesheetRepository;

        /// <summary>
        /// The mocked instance of project repository.
        /// </summary>
        private Mock<IProjectRepository> projectRepository;

        /// <summary>
        /// The mocked instance of repository accessors to access repositories.
        /// </summary>
        private Mock<IRepositoryAccessors> repositoryAccessors;

        /// <summary>
        /// The mocked instance of timesheet database context.
        /// </summary>
        private Mock<TimesheetContext> timesheetContext;

        /// <summary>
        /// Mocked instance of timesheet controller logger.
        /// </summary>
        private Mock<ILogger<TimesheetController>> timesheetControllerLogger;

        /// <summary>
        /// Mocked instance of timesheet helper logger.
        /// </summary>
        private Mock<ILogger<TimesheetHelper>> timesheetHelperLogger;

        /// <summary>
        /// The mocked instance of bot settings.
        /// </summary>
        private IOptions<BotSettings> mockBotSettings;

        /// <summary>
        ///  Initialize all test variables.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.timesheetControllerLogger = new Mock<ILogger<TimesheetController>>();
            this.timesheetHelperLogger = new Mock<ILogger<TimesheetHelper>>();
            this.telemetryClient = new TelemetryClient(new TelemetryConfiguration());
            this.timesheetContext = new Mock<TimesheetContext>();
            this.timesheetRepository = new Mock<ITimesheetRepository>();
            this.projectRepository = new Mock<IProjectRepository>();
            this.repositoryAccessors = new Mock<IRepositoryAccessors>();
            this.mockBotSettings = Options.Create(new BotSettings()
            {
                MicrosoftAppId = string.Empty,
                MicrosoftAppPassword = string.Empty,
                AppBaseUri = string.Empty,
                CardCacheDurationInHour = 12,
                TimesheetFreezeDayOfMonth = 12,
                WeeklyEffortsLimit = 44,
            });
            this.timesheetHelper = new Mock<ITimesheetHelper>();
            this.userHelper = new Mock<IUserHelper>();
            this.managerDashboardHelper = new Mock<IManagerDashboardHelper>();
            this.timesheetController = new TimesheetController(this.timesheetControllerLogger.Object, this.telemetryClient, this.timesheetHelper.Object);
            var httpContext = MakeFakeContext();
            this.timesheetController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        /// <summary>
        /// Test whether OK status is return while saving timesheets added with status "Saved".
        /// </summary>
        [TestMethod]
        public async Task SaveTimesheets_WithCorrectModel_ShouldReturnOKStatus()
        {
            // ARRANGE
            this.timesheetHelper
                .Setup(helper => helper.SaveTimesheetsAsync(It.IsAny<IEnumerable<UserTimesheet>>(), It.IsAny<DateTime>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(TestData.OKResultResponse));

            var userTimesheetToAdd = TestData.UserTimesheets;
            var addTimeCount = userTimesheetToAdd.Count;

            // ACT
            var response = (ObjectResult)await this.timesheetController.SaveTimesheetsAsync(DateTime.UtcNow.Date, userTimesheetToAdd);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status200OK, response.StatusCode);
        }

        /// <summary>
        /// Test whether OK status is return while submitting timesheet requests with status "Submitted".
        /// </summary>
        [TestMethod]
        public async Task SubmitedTimesheets_WithCorrectModel_ShouldReturnOKStatus()
        {
            // ARRANGE
            this.timesheetHelper
                .Setup(helper => helper.SubmitTimesheetsAsync(It.IsAny<DateTime>(), It.IsAny<IEnumerable<UserTimesheet>>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(TestData.OKResultResponse));

            var userTimesheetToUpdate = TestData.UserTimesheets;

            // ACT
            var response = (ObjectResult)await this.timesheetController.SubmitTimesheetsAsync(DateTime.UtcNow.Date, userTimesheetToUpdate);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status200OK, response.StatusCode);
        }

        /// <summary>
        /// Test whether new timesheet request get duplicated and return OK status.
        /// </summary>
        [TestMethod]
        public async Task DuplicateTimesheets_WithCorrectModel_ShouldReturnOKStatus()
        {
            // ARRANGE
            this.timesheetHelper
                .Setup(helper => helper.DuplicateEffortsAsync(It.IsAny<DateTime>(), It.IsAny<IEnumerable<DateTime>>(), It.IsAny<DateTime>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(TestData.OKResultResponse));

            var duplicateEfforts = new DuplicateEffortsDTO
            {
                SourceDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5),
                TargetDates = new List<DateTime>
                {
                    new DateTime(DateTime.Now.Year, DateTime.Now.Month, 6),
                },
            };

            // ACT
            var response = (ObjectResult)await this.timesheetController.DuplicateEffortsAsync(DateTime.UtcNow.Date, duplicateEfforts);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status200OK, response.StatusCode);

        }

        /// <summary>
        /// Test whether bad request status is return with null model while rejecting timesheets.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task RejectTimesheets_WithNullModel_ShoudlReturnBadRequestStatus()
        {
            // ACT
            var result = (ObjectResult)await this.timesheetController.RejectTimesheetsAsync(null);

            // ASSERT
            Assert.AreEqual(result.StatusCode, StatusCodes.Status400BadRequest);
        }

        /// <summary>
        /// Test whether not found status is return with invalid timesheets while rejecting timesheets.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task RejectTimesheets_WithInvalidTimesheets_ShoudlReturnNotFoundStatus()
        {
            // ARRANGE
            IEnumerable<TimesheetEntity> timesheets = null;

            this.timesheetHelper
                .Setup(helper => helper.GetSubmittedTimesheetsByIds(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()))
                .Returns(timesheets);

            // ACT
            var result = (ObjectResult)await this.timesheetController.RejectTimesheetsAsync(TestData.RequestApprovalDTOs);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status404NotFound, result.StatusCode);
        }

        /// <summary>
        /// Test whether no content status is return with valid timesheets on successfully rejecting timesheets.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task RejectTimesheets_WithValidTimesheets_ShoudlReturnNoContentStatus()
        {
            // ARRANGE
            this.timesheetHelper
                .Setup(helper => helper.GetSubmittedTimesheetsByIds(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()))
                .Returns(TestData.SubmittedTimesheets);
            this.timesheetHelper
                .Setup(helper => helper.ApproveOrRejectTimesheetRequestsAsync(It.IsAny<IEnumerable<TimesheetEntity>>(), It.IsAny<IEnumerable<RequestApprovalDTO>>(), It.IsAny<TimesheetStatus>()))
                .Returns(Task.FromResult(true));

            // ACT
            var result = (StatusCodeResult)await this.timesheetController.RejectTimesheetsAsync(TestData.RequestApprovalDTOs);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status204NoContent, result.StatusCode);
        }

        /// <summary>
        /// Test whether bad request status is return with null model while approving timesheets.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task ApproveTimesheets_WithNullModel_ShoudlReturnBadRequestStatus()
        {
            // ACT
            var result = (ObjectResult)await this.timesheetController.ApproveTimesheetsAsync(null);

            // ASSERT
            Assert.AreEqual(result.StatusCode, StatusCodes.Status400BadRequest);
        }

        /// <summary>
        /// Test whether not found status is return with invalid timesheets while approving timesheets.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task Approve_WithInvalidTimesheets_ShoudlReturnNotFoundStatus()
        {
            // ARRANGE
            IEnumerable<TimesheetEntity> timesheets = null;

            this.timesheetHelper
                .Setup(helper => helper.GetSubmittedTimesheetsByIds(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()))
                .Returns(timesheets);

            // ACT
            var result = (ObjectResult)await this.timesheetController.ApproveTimesheetsAsync(TestData.RequestApprovalDTOs);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status404NotFound, result.StatusCode);
        }

        /// <summary>
        /// Test whether no content status is return with valid timesheets on successfully approving timesheets.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task ApproveTimesheets_WithValidTimesheets_ShoudlReturnNoContentStatus()
        {
            // ARRANGE
            this.timesheetHelper
                .Setup(helper => helper.GetSubmittedTimesheetsByIds(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()))
                .Returns(TestData.SubmittedTimesheets);
            this.timesheetHelper
                .Setup(helper => helper.ApproveOrRejectTimesheetRequestsAsync(It.IsAny<IEnumerable<TimesheetEntity>>(), It.IsAny<IEnumerable<RequestApprovalDTO>>(), It.IsAny<TimesheetStatus>()))
                .Returns(Task.FromResult(true));

            // ACT
            var result = (StatusCodeResult)await this.timesheetController.ApproveTimesheetsAsync(TestData.RequestApprovalDTOs);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status204NoContent, result.StatusCode);
        }

        /// <summary>
        /// Make fake HTTP context for unit testing.
        /// </summary>
        /// <returns>Returns fake HTTP context.</returns>
        private static HttpContext MakeFakeContext()
        {
            var userAadObjectId = Guid.NewGuid().ToString();
            var context = new Mock<HttpContext>();
            var request = new Mock<HttpContext>();
            var response = new Mock<HttpContext>();
            var user = new Mock<ClaimsPrincipal>();
            var identity = new Mock<IIdentity>();
            var claim = new Claim[]
            {
                new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", userAadObjectId),
            };

            context.Setup(ctx => ctx.User).Returns(user.Object);
            user.Setup(ctx => ctx.Identity).Returns(identity.Object);
            user.Setup(ctx => ctx.Claims).Returns(claim);
            identity.Setup(id => id.IsAuthenticated).Returns(true);
            identity.Setup(id => id.Name).Returns("test");
            return context.Object;
        }
    }
}