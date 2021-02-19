// <copyright file="TimesheetHelperTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Test.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.Timesheet.Helpers;
    using Microsoft.Teams.Apps.Timesheet.ModelMappers;
    using Microsoft.Teams.Apps.Timesheet.Models;
    using Microsoft.Teams.Apps.Timesheet.Repositories;
    using Microsoft.Teams.Apps.Timesheet.Tests.Fakes;
    using Microsoft.Teams.Apps.Timesheet.Tests.TestData;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// This class lists unit test cases related to Timesheets.
    /// </summary>
    [TestClass]
    public class TimesheetHelperTests
    {
        /// <summary>
        /// Instance of timesheet helper.
        /// </summary>
        private TimesheetHelper timesheetHelper;

        /// <summary>
        /// The mocked instance of repository accessors to access repositories.
        /// </summary>
        private Mock<IRepositoryAccessors> repositoryAccessors;

        /// <summary>
        /// Mocked instance of logger.
        /// </summary>
        private Mock<ILogger<TimesheetHelper>> logger;

        /// <summary>
        /// The instance of bot settings.
        /// </summary>
        private IOptions<BotSettings> botSettings;

        /// <summary>
        ///  Initialize all test variables.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.logger = new Mock<ILogger<TimesheetHelper>>();
            this.repositoryAccessors = new Mock<IRepositoryAccessors>();

            this.botSettings = Options.Create(new BotSettings()
            {
                MicrosoftAppId = string.Empty,
                MicrosoftAppPassword = string.Empty,
                AppBaseUri = string.Empty,
                CardCacheDurationInHour = 12,
                TimesheetFreezeDayOfMonth = 12,
                WeeklyEffortsLimit = 44,
            });
            this.timesheetHelper = new TimesheetHelper(this.botSettings, this.repositoryAccessors.Object, new TimesheetMapper(), this.logger.Object);
        }

        /// <summary>
        /// Tests whether duplicate efforts operation unsuccessful if frozen dates are provided.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task DuplicateEfforts_ProvideFrozenTargetDates_ReturnsUnsuccessfulOperation()
        {
            var sourceDate = new DateTime(2021, 1, 21);
            var targetDates = new List<DateTime>
            {
                new DateTime(2020, 11, 1),
                new DateTime(2020, 11, 2),
            };

            var result = await this.timesheetHelper.DuplicateEffortsAsync(sourceDate, targetDates, DateTime.UtcNow, Guid.Parse("e9be1d47-2707-4dfc-b2a9-e62648c3a04e"));

            Assert.IsTrue(result.StatusCode != HttpStatusCode.OK);

            // Ensure nothing has changed in database.
            Assert.IsTrue((IEnumerable<TimesheetDTO>)result.Response == Enumerable.Empty<TimesheetDTO>());
        }

        /// <summary>
        /// Tests whether duplicate efforts operation unsuccessful if provided target dates are less than project start date.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task DuplicateEfforts_ProvideTargetDateLessThanProjectStartDate_ReturnsUnsuccessfulOperation()
        {
            var projectRepository = new Mock<IProjectRepository>();

            projectRepository
                .Setup(x => x.GetProjectsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(TestData.Projects.AsEnumerable()));

            var timesheetRepository = new Mock<ITimesheetRepository>();

            timesheetRepository
                .Setup(x => x.GetTimesheetsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(TestData.Timesheets));

            this.repositoryAccessors.Setup(x => x.ProjectRepository).Returns(projectRepository.Object);
            this.repositoryAccessors.Setup(x => x.TimesheetRepository).Returns(timesheetRepository.Object);

            var sourceDate = new DateTime(2021, 1, 2);
            var targetDates = new List<DateTime>
            {
                new DateTime(2021, 1, 1),
            };

            var result = await this.timesheetHelper.DuplicateEffortsAsync(sourceDate, targetDates, DateTime.UtcNow, Guid.Parse("e9be1d47-2707-4dfc-b2a9-e62648c3a04e"));

            timesheetRepository.Verify(x => x.Add(It.IsAny<TimesheetEntity>()), Times.Never());

            // Ensure nothing has changed in database.
            Assert.IsTrue((IEnumerable<TimesheetDTO>)result.Response == Enumerable.Empty<TimesheetDTO>());
        }

        /// <summary>
        /// Tests whether duplicate efforts operation unsuccessful if source date does not have any Projects.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task DuplicateEfforts_SourceDateDoesNotHaveProjects_ReturnsUnsuccessfulOperation()
        {
            var projectRepository = new Mock<IProjectRepository>();

            projectRepository
                .Setup(x => x.GetProjectsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(TestData.Projects.AsEnumerable()));

            var timesheetRepository = new Mock<ITimesheetRepository>();

            timesheetRepository
                .Setup(x => x.GetTimesheetsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(TestData.Timesheets));

            this.repositoryAccessors.Setup(x => x.ProjectRepository).Returns(projectRepository.Object);
            this.repositoryAccessors.Setup(x => x.TimesheetRepository).Returns(timesheetRepository.Object);

            var sourceDate = new DateTime(2020, 12, 1);
            var targetDates = new List<DateTime>
            {
                new DateTime(2021, 1, 22),
                new DateTime(2021, 1, 24),
            };

            var result = await this.timesheetHelper.DuplicateEffortsAsync(sourceDate, targetDates, DateTime.UtcNow, Guid.Parse("e9be1d47-2707-4dfc-b2a9-e62648c3a04e"));

            Assert.IsTrue(result.StatusCode != HttpStatusCode.OK);

            // Ensure nothing has changed in database.
            Assert.IsTrue((IEnumerable<TimesheetDTO>)result.Response == Enumerable.Empty<TimesheetDTO>());
        }

        /// <summary>
        /// Tests whether save Timesheets operation is successful.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task SaveTimesheets_UserTimesheetsProvided_TimesheetsSaveSuccessful()
        {
            var projectRepository = new Mock<IProjectRepository>();
            var timesheetRepository = new Mock<ITimesheetRepository>();

            projectRepository
                .Setup(x => x.GetProjectsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(TestData.Projects.AsEnumerable()));

            timesheetRepository
                .Setup(x => x.GetTimesheetsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(TestData.Timesheets));

            this.repositoryAccessors.Setup(x => x.ProjectRepository).Returns(projectRepository.Object);
            this.repositoryAccessors.Setup(x => x.TimesheetRepository).Returns(timesheetRepository.Object);
            this.repositoryAccessors.Setup(x => x.SaveChangesAsync()).Returns(Task.FromResult(1));
            this.repositoryAccessors.Setup(x => x.Context).Returns(FakeTimesheetContext.GetFakeTimesheetContext());

            var userTimesheetsToSave = new List<UserTimesheet>
            {
                new UserTimesheet
                {
                    TimesheetDate = new DateTime(2021, 01, 24),
                    ProjectDetails = new List<ProjectDetails>
                    {
                        new ProjectDetails
                        {
                            StartDate = new DateTime(2021, 01, 02),
                            EndDate = new DateTime(2021, 02, 10),
                            Id = Guid.Parse("bfb77fc0-12a9-4250-a5fb-e52ddc48ff86"),
                            TimesheetDetails = new List<TimesheetDetails>
                            {
                                new TimesheetDetails
                                {
                                    Hours = 6,
                                    ManagerComments = string.Empty,
                                    TaskId = Guid.Parse("1eec371f-edbe-4ad1-be1d-d4cd3515540e"),
                                    TaskTitle = "Task",
                                },
                            },
                        },
                    },
                },
            };

            var result = await this.timesheetHelper.SaveTimesheetsAsync(userTimesheetsToSave, DateTime.UtcNow, Guid.Parse("e9be1d47-2707-4dfc-b2a9-e62648c3a04e"));

            timesheetRepository.Verify(x => x.Add(It.IsAny<TimesheetEntity>()), Times.AtLeastOnce());
            Assert.IsNotNull(result.Response);

            var targetDatesToSave = userTimesheetsToSave.Select(x => x.TimesheetDate);
            var savedTimesheets = result.Response as IEnumerable<TimesheetDTO>;
            var savedTargetDates = savedTimesheets.Select(x => x.TimesheetDate);

            // Ensure whether all Timesheets saved.
            Assert.IsTrue(savedTargetDates.All(savedTargetDate => targetDatesToSave.Contains(savedTargetDate)));
        }

        /// <summary>
        /// Tests whether submit Timesheets operation is successful.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task SubmitTimesheets_UserTimesheetsProvidedWithUnassignedProject_TimesheetsSubmitUnsuccessful()
        {
            var projectRepository = new Mock<IProjectRepository>();
            var timesheetRepository = new Mock<ITimesheetRepository>();

            projectRepository
                .Setup(x => x.GetProjectsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(TestData.Projects.AsEnumerable()));

            timesheetRepository
                .Setup(x => x.GetTimesheetsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(TestData.Timesheets));

            timesheetRepository
                .Setup(x => x.FindAsync(It.IsAny<Expression<Func<TimesheetEntity, bool>>>()))
                .Returns(Task.FromResult(TestData.Timesheets.AsEnumerable()));

            this.repositoryAccessors.Setup(x => x.ProjectRepository).Returns(projectRepository.Object);
            this.repositoryAccessors.Setup(x => x.TimesheetRepository).Returns(timesheetRepository.Object);
            this.repositoryAccessors.Setup(x => x.Context).Returns(FakeTimesheetContext.GetFakeTimesheetContext());

            var result = await this.timesheetHelper.SubmitTimesheetsAsync(DateTime.UtcNow, TestData.UserTimesheets.AsEnumerable(), Guid.Parse("e9be1d47-2707-4dfc-b2a9-e62648c3a04e"));

            Assert.IsTrue(result.StatusCode == HttpStatusCode.BadRequest);

            // Ensure nothing has changed in database.
            Assert.IsTrue((IEnumerable<TimesheetDTO>)result.Response == Enumerable.Empty<TimesheetDTO>());
        }

        /// <summary>
        /// Tests whether previous month dates received in case if timesheet is not frozen.
        /// </summary>
        [TestMethod]
        public void GetNotYetFrozenTimesheetDates_PreviousMonthDatesProvided_PreviousMonthDatesReceived()
        {
            var previousMonthDates = new List<DateTime>
            {
                new DateTime(2020, 12, 02),
            };

            var notYetFrozenTimesheetDates = this.timesheetHelper.GetNotYetFrozenTimesheetDates(previousMonthDates, new DateTime(2021, 01, 02));

            Assert.IsNotNull(notYetFrozenTimesheetDates);

            // Ensures to receive all previous month dates.
            Assert.IsTrue(notYetFrozenTimesheetDates.All(notYetFrozenTimesheetDate => previousMonthDates.Contains(notYetFrozenTimesheetDate)));
        }

        /// <summary>
        /// Tests whether single day timesheet get returned.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task GetTimesheetsAsync_ActiveProjectsExistsAndAssignedToUser_ReturnsTimesheetOfADay()
        {
            var projectRepository = new Mock<IProjectRepository>();

            projectRepository
                .Setup(x => x.GetProjectsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(TestData.Projects.AsEnumerable()));

            var timesheetRepository = new Mock<ITimesheetRepository>();

            timesheetRepository
                .Setup(x => x.GetTimesheetsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(TestData.Timesheets));

            this.repositoryAccessors.Setup(x => x.ProjectRepository).Returns(projectRepository.Object);
            this.repositoryAccessors.Setup(x => x.TimesheetRepository).Returns(timesheetRepository.Object);

            var result = await this.timesheetHelper.GetTimesheetsAsync(new DateTime(2021, 01, 02), new DateTime(2021, 01, 02), Guid.Parse("e9be1d47-2707-4dfc-b2a9-e62648c3a04e"));

            Assert.IsNotNull(result);

            // The count ensures that the single day timesheet get received.
            Assert.IsTrue(result.Count() == 1);
        }

        /// <summary>
        /// Tests whether user Timesheets are not available.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task GetTimesheetsAsync_NoActiveProjectAssignedToUser_ReturnsZeroUserTimesheets()
        {
            var projectRepository = new Mock<IProjectRepository>();

            projectRepository
                .Setup(x => x.GetProjectsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(TestData.Projects.AsEnumerable()));

            var timesheetRepository = new Mock<ITimesheetRepository>();

            timesheetRepository
                .Setup(x => x.GetTimesheetsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(TestData.Timesheets));

            this.repositoryAccessors.Setup(x => x.ProjectRepository).Returns(projectRepository.Object);
            this.repositoryAccessors.Setup(x => x.TimesheetRepository).Returns(timesheetRepository.Object);

            var result = await this.timesheetHelper.GetTimesheetsAsync(new DateTime(2019, 01, 01), new DateTime(2019, 01, 01), Guid.Parse("e9be1d47-2707-4dfc-b2a9-e62648c3a04e"));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count() == 0);
        }
    }
}
