// <copyright file="UserControllerTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Tests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Graph;
    using Microsoft.Teams.Apps.Timesheet.Controllers;
    using Microsoft.Teams.Apps.Timesheet.Helpers;
    using Microsoft.Teams.Apps.Timesheet.Services.MicrosoftGraph;
    using Microsoft.Teams.Apps.Timesheet.Tests.Fakes;
    using Microsoft.Teams.Apps.Timesheet.Tests.TestData;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Models = Microsoft.Teams.Apps.Timesheet.Models;

    /// <summary>
    /// User controller tests contains all the test cases for the graph operations.
    /// </summary>
    [TestClass]
    public class UserControllerTests
    {
        /// <summary>
        /// Holds the instance telemetryClient.
        /// </summary>
        private TelemetryClient telemetryClient;

        /// <summary>
        /// Holds the instance of user controller.
        /// </summary>
        private UserController userController;

        /// <summary>
        /// Mocked the instance of user graph service.
        /// </summary>
        private Mock<IUsersService> userGraphService;

        /// <summary>
        /// Mocked instance of logger.
        /// </summary>
        private Mock<ILogger<UserController>> logger;

        /// <summary>
        /// The mocked instance of timesheet helper.
        /// </summary>
        private Mock<ITimesheetHelper> timesheetHelper;

        /// <summary>
        ///  Initialize all test variables.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.logger = new Mock<ILogger<UserController>>();
            this.telemetryClient = new TelemetryClient(new TelemetryConfiguration());
            this.userGraphService = new Mock<IUsersService>();
            this.timesheetHelper = new Mock<ITimesheetHelper>();
            this.userController = new UserController(this.logger.Object, this.userGraphService.Object, this.telemetryClient, this.timesheetHelper.Object);
            var httpContext = FakeHttpContext.MakeFakeContext();
            this.userController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext,
            };
        }

        /// <summary>
        /// Test whether we can get reportee with random string.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task CanGetReporteeAsync()
        {
            // ARRANGE
            this.userGraphService
                .Setup(graphService => graphService.GetReporteesAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new List<User>() as IEnumerable<User>));

            // ACT
            var result = (ObjectResult)await this.userController.GetReporteesAsync("random");

            // ASSERT
            Assert.AreEqual(result.StatusCode, StatusCodes.Status200OK);
        }

        /// <summary>
        /// Test whether we can get manager.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task CanGetManagerAsync()
        {
            // ARRANGE
            this.userGraphService
                .Setup(graphService => graphService.GetManagerAsync())
                .Returns(Task.FromResult(new DirectoryObject()));

            // ACT
            var result = (ObjectResult)await this.userController.GetManagerAsync();

            // ASSERT
            Assert.AreEqual(result.StatusCode, StatusCodes.Status200OK);
        }

        /// <summary>
        /// Test whether unauthorized status is return when user not report to logged in manager while fetching timesheets.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task GetTimesheetRequestsByStatus_WhenUserNotReportToLoggedInManager_ShouldReturnUnauthorizedStatus()
        {
            // ARRANGE
            this.userGraphService
                .Setup(service => service.GetReporteesAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(TestData.Users.AsEnumerable()));

            var reporteeId = Guid.NewGuid();

            // ACT
            var result = (ObjectResult)await this.userController.GetTimesheetRequestsByStatusAsync(reporteeId, (int)Models.TimesheetStatus.Submitted);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status401Unauthorized, result.StatusCode);
        }

        /// <summary>
        /// Test whether bad request status is return with invalid timesheet status while fetching timesheets.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task GetTimesheetRequestsByStatus_WithInvalidTimesheetStatus_ShouldReturnBadRequestStatus()
        {
            // ARRANGE
            var reporteeId = Guid.NewGuid();
            var invalidTimesheetStatus = 8;

            // ACT
            var result = (ObjectResult)await this.userController.GetTimesheetRequestsByStatusAsync(reporteeId, invalidTimesheetStatus);

            // ASSERT
            Assert.AreEqual(result.StatusCode, StatusCodes.Status400BadRequest);
        }

        /// <summary>
        /// Test whether OK status is return with valid parameters while fetching timesheets.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task GetTimesheetRequestsByStatus_WithValidParams_ShouldReturnOKStatus()
        {
            // ARRANGE
            this.userGraphService
                .Setup(service => service.GetReporteesAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(TestData.Users.AsEnumerable()));
            this.timesheetHelper
                .Setup(helper => helper.GetTimesheetRequestsByStatusAsync(It.IsAny<string>(), It.IsAny<Models.TimesheetStatus>()))
                .Returns(TestData.ExpectedSubmittedRequestDTO.AsEnumerable());
            var reporteeId = Guid.Parse("99051013-15d3-4831-a301-ded45bf3d12a");

            // ACT
            var result = (ObjectResult)await this.userController.GetTimesheetRequestsByStatusAsync(reporteeId, (int)Models.TimesheetStatus.Approved);

            // ASSERT
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
        }
    }
}