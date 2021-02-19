// <copyright file="ManagerDashboardHelperTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Tests.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Graph;
    using Microsoft.Teams.Apps.Timesheet.Helpers;
    using Microsoft.Teams.Apps.Timesheet.ModelMappers;
    using Microsoft.Teams.Apps.Timesheet.Models;
    using Microsoft.Teams.Apps.Timesheet.Repositories;
    using Microsoft.Teams.Apps.Timesheet.Services.MicrosoftGraph;
    using Microsoft.Teams.Apps.Timesheet.Tests.TestData;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Models = Microsoft.Teams.Apps.Timesheet.Models;

    /// <summary>
    /// Manager dashboard helper tests contains all the test cases for helping methods.
    /// </summary>
    [TestClass]
    public class ManagerDashboardHelperTests
    {
        /// <summary>
        /// Holds the instance of manager dashboard helper.
        /// </summary>
        private ManagerDashboardHelper managerDashboardHelper;

        /// <summary>
        /// The mocked instance of timesheet repository.
        /// </summary>
        private Mock<ITimesheetRepository> timesheetRepository;

        /// <summary>
        /// The mocked instance of repository accessors to access repositories.
        /// </summary>
        private Mock<IRepositoryAccessors> repositoryAccessors;

        /// <summary>
        /// The mocked instance of timesheet database context.
        /// </summary>
        private Mock<TimesheetContext> timesheetContext;

        /// <summary>
        /// Mocked instance of graph service.
        /// </summary>
        private Mock<IUsersService> userGraphService;

        /// <summary>
        ///  Initialize all test variables.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.timesheetContext = new Mock<TimesheetContext>();
            this.timesheetRepository = new Mock<ITimesheetRepository>();
            this.repositoryAccessors = new Mock<IRepositoryAccessors>();
            this.userGraphService = new Mock<IUsersService>();
            this.managerDashboardHelper = new ManagerDashboardHelper(this.repositoryAccessors.Object, this.userGraphService.Object, new ManagerDashboardMapper());
        }

        /// <summary>
        /// Test whether we can get dashboard requests with valid data.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task GetDashboardRequests_WithValidParams_ShouldReturnValidData()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.TimesheetRepository).Returns(() => this.timesheetRepository.Object);
            this.timesheetRepository
                 .Setup(timesheetRepo => timesheetRepo.GetTimesheetRequestsByManager(It.IsAny<Guid>(), It.IsAny<TimesheetStatus>()))
                 .Returns(TestData.SavedTimesheets
                    .AsEnumerable()
                    .GroupBy(x => x.UserId)
                    .ToDictionary(x => x.Key, x => x.ToList()));
            this.userGraphService
                .Setup(graphService => graphService.GetUsersAsync(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(new List<User>
                {
                    new User
                    {
                        Id = "3fd7af65-67df-43cb-baa0-30917e133d94",
                        DisplayName = "Random",
                    },
                }.AsEnumerable()));

            var managerId = Guid.NewGuid();

            // ACT
            var dashboardRequestDTO = (await this.managerDashboardHelper.GetDashboardRequestsAsync(managerId, TimesheetStatus.Submitted)).ToList();

            // ASSERT
            Assert.AreEqual(1, dashboardRequestDTO.Count);
            this.timesheetRepository.Verify(timesheetRepo => timesheetRepo.GetTimesheetRequestsByManager(It.IsAny<Guid>(), It.IsAny<TimesheetStatus>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Test whether empty list is return when timesheet not found while fetching dashboard request.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task GetDashboardRequests_WhenTimesheetsNotFound_ShouldReturnEmptyList()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.TimesheetRepository).Returns(() => this.timesheetRepository.Object);

            this.timesheetRepository
                    .Setup(timesheetRepo => timesheetRepo.GetTimesheetRequestsByManager(It.IsAny<Guid>(), It.IsAny<TimesheetStatus>()))
                    .Returns(new List<TimesheetEntity>()
                    .AsEnumerable()
                    .GroupBy(x => x.UserId)
                    .ToDictionary(x => x.Key, x => x.ToList()));
            this.userGraphService
                .Setup(graphService => graphService.GetUsersAsync(It.IsAny<IEnumerable<string>>()))
                .Returns(Task.FromResult(new List<User>
                {
                    new User
                    {
                        Id = "2fd7af65-67df-43cb-baa0-30917e133d94",
                        DisplayName = "Random",
                    },
                }.AsEnumerable()));

            var managerId = Guid.NewGuid();

            // ACT
            var dashboardRequestDTO = await this.managerDashboardHelper.GetDashboardRequestsAsync(managerId, TimesheetStatus.Submitted);

            // ASSERT
            Assert.IsNull(dashboardRequestDTO);
            this.timesheetRepository.Verify(timesheetRepo => timesheetRepo.GetTimesheetRequestsByManager(It.IsAny<Guid>(), It.IsAny<TimesheetStatus>()), Times.AtLeastOnce());
        }
    }
}