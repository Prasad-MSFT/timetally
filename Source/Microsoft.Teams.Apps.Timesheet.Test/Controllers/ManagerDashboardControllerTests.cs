// <copyright file="ManagerDashboardControllerTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Tests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.Timesheet.Controllers;
    using Microsoft.Teams.Apps.Timesheet.Helpers;
    using Microsoft.Teams.Apps.Timesheet.Tests.Fakes;
    using Microsoft.Teams.Apps.Timesheet.Tests.TestData;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Models = Microsoft.Teams.Apps.Timesheet.Models;

    /// <summary>
    /// Manager dashboard controller tests contains all the test cases for the CRUD operations related to dashboard.
    /// </summary>
    [TestClass]
    public class ManagerDashboardControllerTests
    {
        /// <summary>
        /// Holds the instance telemetry client.
        /// </summary>
        private TelemetryClient telemetryClient;

        /// <summary>
        /// Holds the instance of manager dashboard controller.
        /// </summary>
        private ManagerDashboardController managerDashboardController;

        /// <summary>
        /// The mocked instance of manager dashboard helper.
        /// </summary>
        private Mock<IManagerDashboardHelper> managerDashboardHelper;

        /// <summary>
        /// Mocked instance of logger.
        /// </summary>
        private Mock<ILogger<ManagerDashboardController>> logger;

        /// <summary>
        ///  Initialize all test variables.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.logger = new Mock<ILogger<ManagerDashboardController>>();
            this.telemetryClient = new TelemetryClient(new TelemetryConfiguration());
            this.managerDashboardHelper = new Mock<IManagerDashboardHelper>();
            this.managerDashboardController = new ManagerDashboardController(this.logger.Object, this.managerDashboardHelper.Object, this.telemetryClient);
            var httpContext = FakeHttpContext.MakeFakeContext();
            this.managerDashboardController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext,
            };
        }

        /// <summary>
        /// Test whether OK status is return when requests found for logged-in manager's reportee while fetching dashboard requests.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task GetDashboardRequestsAsync_WhenRequestFound_ShoudlReturnOKStatus()
        {
            // Arrange
            this.managerDashboardHelper
                .Setup(helper => helper.GetDashboardRequestsAsync(It.IsAny<Guid>(), It.IsAny<Models.TimesheetStatus>()))
                .Returns(Task.FromResult(TestData.DashboardRequestDTOs.AsEnumerable()));

            // ACT
            var result = (ObjectResult)await this.managerDashboardController.GetDashboardRequestsAsync();

            // ASSERT
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
        }

        /// <summary>
        /// Test whether not found status is return when requests found for logged-in manager's reportee while fetching dashboard requests.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task GetDashboardRequestsAsync_WhenRequestNotFound_ShoudlReturnNotFoundStatus()
        {
            // Arrange
            IEnumerable<Models.DashboardRequestDTO> nullRequests = null;
            this.managerDashboardHelper
                .Setup(helper => helper.GetDashboardRequestsAsync(It.IsAny<Guid>(), It.IsAny<Models.TimesheetStatus>()))
                .Returns(Task.FromResult(nullRequests));

            // ACT
            var result = (ObjectResult)await this.managerDashboardController.GetDashboardRequestsAsync();

            // ASSERT
            Assert.AreEqual(StatusCodes.Status404NotFound, result.StatusCode);
        }
    }
}