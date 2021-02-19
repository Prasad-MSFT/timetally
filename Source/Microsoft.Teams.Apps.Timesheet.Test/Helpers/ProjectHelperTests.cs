// <copyright file="ProjectHelperTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Tests.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.Timesheet.Extensions;
    using Microsoft.Teams.Apps.Timesheet.Helpers;
    using Microsoft.Teams.Apps.Timesheet.ModelMappers;
    using Microsoft.Teams.Apps.Timesheet.ModelMappers.Task;
    using Microsoft.Teams.Apps.Timesheet.Models;
    using Microsoft.Teams.Apps.Timesheet.Repositories;
    using Microsoft.Teams.Apps.Timesheet.Services.MicrosoftGraph;
    using Microsoft.Teams.Apps.Timesheet.Tests.Fakes;
    using Microsoft.Teams.Apps.Timesheet.Tests.TestData;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Project helper tests contains all the test cases for methods managing projects.
    /// </summary>
    [TestClass]
    public class ProjectHelperTests
    {
        /// <summary>
        /// Instance of project helper.
        /// </summary>
        private ProjectHelper projectHelper;

        /// <summary>
        /// The mocked instance of timesheet repository.
        /// </summary>
        private Mock<ITimesheetRepository> timesheetRepository;

        /// <summary>
        /// The mocked instance of project repository.
        /// </summary>
        private Mock<IProjectRepository> projectRepository;

        /// <summary>
        /// The mocked instance of task repository.
        /// </summary>
        private Mock<ITaskRepository> taskRepository;

        /// <summary>
        /// The mocked instance of member repository.
        /// </summary>
        private Mock<IMemberRepository> memberRepository;

        /// <summary>
        /// The mocked instance of repository accessors to access repositories.
        /// </summary>
        private Mock<IRepositoryAccessors> repositoryAccessors;

        /// <summary>
        /// The mocked instance of timesheet database context.
        /// </summary>
        private Mock<TimesheetContext> timesheetContext;

        /// <summary>
        /// The mocked instance of user service.
        /// </summary>
        private Mock<IUsersService> userService;

        /// <summary>
        ///  Initialize all test variables.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.timesheetContext = new Mock<TimesheetContext>();
            this.timesheetRepository = new Mock<ITimesheetRepository>();
            this.projectRepository = new Mock<IProjectRepository>();
            this.taskRepository = new Mock<ITaskRepository>();
            this.memberRepository = new Mock<IMemberRepository>();
            this.repositoryAccessors = new Mock<IRepositoryAccessors>();
            this.userService = new Mock<IUsersService>();
            this.projectHelper = new ProjectHelper(this.timesheetContext.Object, this.repositoryAccessors.Object, new ProjectMapper(), new MemberMapper(), new TaskMapper());
        }

        /// <summary>
        /// Tests whether we can get project utilization data with valid parameter.
        /// </summary>
        [TestMethod]
        public void GetProjectById_WithValidParams_ShouldReturnValidData()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.ProjectRepository).Returns(() => this.projectRepository.Object);
            this.projectRepository
                .Setup(projectRepo => projectRepo.GetProjectById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(TestData.Project);

            var managerId = Guid.NewGuid();

            // ACT
            var project = this.projectHelper.GetProjectById(TestData.Project.Id, managerId);

            // ASSERT
            Assert.AreEqual(TestData.ExpectedProjectDTO.Id, project.Id);
            this.projectRepository.Verify(projectRepo => projectRepo.GetProjectById(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests whether null is return when project not found while fetching project details.
        /// </summary>
        [TestMethod]
        public void GetProjectById_WhenProjectNotFound_ShouldReturnNull()
        {
            // ARRANGE
            Project nullProject = null;
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.ProjectRepository).Returns(() => this.projectRepository.Object);
            this.projectRepository
                .Setup(projectRepo => projectRepo.GetProjectById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(nullProject);

            var managerId = Guid.NewGuid();
            var projectId = Guid.NewGuid();

            // ACT
            var project = this.projectHelper.GetProjectById(projectId, managerId);

            // ASSERT
            Assert.AreEqual(null, project);
            this.projectRepository.Verify(projectRepo => projectRepo.GetProjectById(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests whether we can create project with valid model and valid project.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task CreateProject_WithValidModel_ShouldReturnValidProject()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.ProjectRepository).Returns(() => this.projectRepository.Object);
            this.repositoryAccessors.Setup(accessor => accessor.Context).Returns(FakeTimesheetContext.GetFakeTimesheetContext());

            this.projectRepository
                .Setup(projectRepo => projectRepo.CreateProject(It.IsAny<Project>()))
                .Returns(TestData.Project);
            this.userService
                .Setup(service => service.GetReporteesAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(TestData.Reportees.AsEnumerable()));
            this.timesheetContext
                .Setup(context => context.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(1));

            var managerId = Guid.NewGuid();

            // ACT
            var result = await this.projectHelper.CreateProjectAsync(TestData.ProjectDTO, managerId);

            // ASSERT
            Assert.AreEqual(TestData.Project.CreatedBy, result.CreatedBy);
            this.projectRepository.Verify(projectRepo => projectRepo.CreateProject(It.IsAny<Project>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests whether null exception is thrown when project details are null while creating project.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task CreateProject_WhenProjectDetailsAreNull_ShouldThrowNullException()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.ProjectRepository).Returns(() => this.projectRepository.Object);

            ProjectDTO projectDetails = null;
            var managerId = Guid.NewGuid();

            try
            {
                // ACT
                var project = await this.projectHelper.CreateProjectAsync(projectDetails, managerId);
            }
            catch (ArgumentNullException exception)
            {
                // ASSERT
                Assert.AreEqual(nameof(projectDetails), exception.ParamName);
                this.projectRepository.Verify(projectRepo => projectRepo.CreateProject(It.IsAny<Project>()), Times.Never());
            }
        }

        /// <summary>
        /// Tests whether true is return when project is updated.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task UpdateProject_WithValidModel_ShouldReturnTrue()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.ProjectRepository).Returns(() => this.projectRepository.Object);
            this.repositoryAccessors.Setup(accessor => accessor.Context).Returns(FakeTimesheetContext.GetFakeTimesheetContext());
            this.projectRepository
                .Setup(projectRepo => projectRepo.GetProjectById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(TestData.Project);
            this.projectRepository
                .Setup(projectRepo => projectRepo.Update(It.IsAny<Project>()))
                .Returns(TestData.Project);
            this.timesheetContext
                .Setup(context => context.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(1));

            var managerId = TestData.Project.CreatedBy;

            // ACT
            var result = await this.projectHelper.UpdateProjectAsync(TestData.ProjectUpdateDTO, managerId);

            // ASSERT
            Assert.IsTrue(result);
            this.projectRepository.Verify(projectRepo => projectRepo.GetProjectById(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.AtLeastOnce());
            this.projectRepository.Verify(projectRepo => projectRepo.Update(It.IsAny<Project>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests whether argument exception is thrown when project details are null while updating project.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task UpdateProject_WhenProjectDetailsAreNull_ShouldThrowArgumentException()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.ProjectRepository).Returns(() => this.projectRepository.Object);

            ProjectUpdateDTO nullProjectUpdateDTO = null;
            var managerId = Guid.NewGuid();

            try
            {
                // ACT
                var isUpdated = await this.projectHelper.UpdateProjectAsync(nullProjectUpdateDTO, managerId);
            }
            catch (ArgumentException exception)
            {
                // ASSERT
                Assert.AreEqual("The project details must be provided.", exception.Message);
                this.projectRepository.Verify(projectRepo => projectRepo.GetProjectById(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never());
                this.projectRepository.Verify(projectRepo => projectRepo.Update(It.IsAny<Project>()), Times.Never());
            }
        }

        /// <summary>
        /// Tests whether false is return when failure at database while updating project.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task UpdateProject_WhenFailureAtDatabase_ShouldReturnFalse()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.ProjectRepository).Returns(() => this.projectRepository.Object);
            this.repositoryAccessors.Setup(accessor => accessor.Context).Returns(FakeTimesheetContext.GetFakeTimesheetContext());
            this.projectRepository
                .Setup(projectRepo => projectRepo.GetProjectById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(TestData.Project);
            this.repositoryAccessors
                .Setup(repositoryAccessor => repositoryAccessor.SaveChangesAsync())
                .Returns(Task.FromResult(0));

            var managerId = Guid.NewGuid();

            // ACT
            var result = await this.projectHelper.UpdateProjectAsync(TestData.ProjectUpdateDTO, managerId);

            // ASSERT
            Assert.IsFalse(result);
            this.projectRepository.Verify(projectRepo => projectRepo.GetProjectById(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.AtLeastOnce());
            this.projectRepository.Verify(projectRepo => projectRepo.Update(It.IsAny<Project>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests whether we can get project utilization data with valid parameter.
        /// </summary>
        [TestMethod]
        public void GetProjectUtilization_WithValidParams_ShouldReturnValidData()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.TimesheetRepository).Returns(() => this.timesheetRepository.Object);
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.ProjectRepository).Returns(() => this.projectRepository.Object);
            this.timesheetRepository
                .Setup(timesheetRepo => timesheetRepo.GetTimesheetRequestsByProjectId(It.IsAny<Guid>(), It.IsAny<TimesheetStatus>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(TestData.ApprovedTimesheets);
            this.projectRepository
                .Setup(projectRepo => projectRepo.GetProjectById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(TestData.Project);

            var managerId = Guid.NewGuid().ToString();
            var projectId = Guid.NewGuid();
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5);
            var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, startDate.Day + 1);

            // ACT
            var projectUtilization = this.projectHelper.GetProjectUtilization(projectId, managerId, startDate, endDate);

            // ASSERT
            Assert.AreEqual(TestData.ExpectedProjectUtilization.Id, projectUtilization.Id);
            this.timesheetRepository.Verify(timesheetRepo => timesheetRepo.GetTimesheetRequestsByProjectId(It.IsAny<Guid>(), It.IsAny<TimesheetStatus>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.AtLeastOnce());
            this.projectRepository.Verify(projectRepo => projectRepo.GetProjectById(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests  whether null is return when project is not created by logged-in user while fetching project utilization data.
        /// </summary>
        [TestMethod]
        public void GetProjectUtilization_WhenProjectNotFound_ShouldReturnNull()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.ProjectRepository).Returns(() => this.projectRepository.Object);

            Project project = null;
            this.projectRepository
                .Setup(projectRepo => projectRepo.GetProjectById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(project);

            var managerId = Guid.NewGuid().ToString();
            var projectId = Guid.NewGuid();
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5);
            var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, startDate.Day + 1);

            // ACT
            var projectUtilization = this.projectHelper.GetProjectUtilization(projectId, managerId, startDate, endDate);

            // ASSERT
            Assert.AreEqual(null, projectUtilization);
            this.projectRepository.Verify(projectRepo => projectRepo.GetProjectById(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once());
            this.timesheetRepository.Verify(timesheetRepo => timesheetRepo.GetTimesheetRequestsByProjectId(It.IsAny<Guid>(), It.IsAny<TimesheetStatus>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never());
        }

        /// <summary>
        /// Tests whether true is return on successful creation of tasks.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task CreateTasks_WithCorrectModel_ShouldReturnTrue()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.TaskRepository).Returns(() => this.taskRepository.Object);
            this.taskRepository.Setup(taskRepo => taskRepo.CreateTasksAsync(It.IsAny<IEnumerable<TaskEntity>>())).Returns(Task.FromResult(true));
            this.repositoryAccessors.Setup(accessor => accessor.Context).Returns(FakeTimesheetContext.GetFakeTimesheetContext());
            this.repositoryAccessors
                .Setup(repositoryAccessor => repositoryAccessor.SaveChangesAsync())
                .Returns(Task.FromResult(TestData.TaskDTOs.Count));

            var projectId = Guid.NewGuid();

            // ACT
            var isAdded = await this.projectHelper.AddProjectTasksAsync(projectId, TestData.TaskDTOs);

            // ASSERT
            Assert.IsTrue(isAdded);
            this.taskRepository.Verify(taskRepo => taskRepo.CreateTasksAsync(It.IsAny<IEnumerable<TaskEntity>>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests whether false is return when failure at database while creating tasks.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task CreateTasks_WhenFailureAtDatabase_ShouldReturnFalse()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.TaskRepository).Returns(() => this.taskRepository.Object);
            this.taskRepository.Setup(taskRepo => taskRepo.CreateTasksAsync(It.IsAny<IEnumerable<TaskEntity>>())).Returns(Task.FromResult(true));
            this.repositoryAccessors.Setup(accessor => accessor.Context).Returns(FakeTimesheetContext.GetFakeTimesheetContext());
            this.repositoryAccessors
                .Setup(repositoryAccessor => repositoryAccessor.SaveChangesAsync())
                .Returns(Task.FromResult(0));

            // ACT
            var isAdded = await this.projectHelper.AddProjectTasksAsync(Guid.NewGuid(), TestData.TaskDTOs);

            // ASSERT
            Assert.IsFalse(isAdded);
            this.taskRepository.Verify(taskRepo => taskRepo.CreateTasksAsync(It.IsAny<IEnumerable<TaskEntity>>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests whether true is return on successful deletion of tasks.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task DeleteTasks_WithCorrectModel_ShouldReturnTrue()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.TaskRepository).Returns(() => this.taskRepository.Object);
            this.repositoryAccessors.Setup(accessor => accessor.Context).Returns(FakeTimesheetContext.GetFakeTimesheetContext());
            this.taskRepository
                .Setup(taskRepo => taskRepo.FindAsync(It.IsAny<Expression<Func<TaskEntity, bool>>>()))
                .Returns(Task.FromResult(TestData.Tasks as IEnumerable<TaskEntity>));
            this.taskRepository
                .Setup(taskRepo => taskRepo.UpdateTasks(It.IsAny<List<TaskEntity>>()));
            this.repositoryAccessors
                .Setup(repositoryAccessor => repositoryAccessor.SaveChangesAsync())
                .Returns(Task.FromResult(1));

            var projectId = Guid.NewGuid();

            // ACT
            var operationResult = await this.projectHelper.DeleteProjectTasksAsync(TestData.Tasks);

            // ASSERT
            Assert.IsTrue(operationResult);
            this.taskRepository.Verify(taskRepo => taskRepo.UpdateTasks(It.IsAny<List<TaskEntity>>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests whether false is return when failure at database while deleting tasks.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task DeleteTasks_WhenFailureAtDatabase_ShouldReturnFalse()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.TaskRepository).Returns(() => this.taskRepository.Object);
            this.repositoryAccessors.Setup(accessor => accessor.Context).Returns(FakeTimesheetContext.GetFakeTimesheetContext());
            this.taskRepository
                .Setup(taskRepo => taskRepo.FindAsync(It.IsAny<Expression<Func<TaskEntity, bool>>>()))
                .Returns(Task.FromResult(TestData.Tasks as IEnumerable<TaskEntity>));
            this.taskRepository
                .Setup(taskRepo => taskRepo.UpdateTasks(It.IsAny<List<TaskEntity>>()));
            this.repositoryAccessors
                .Setup(repositoryAccessor => repositoryAccessor.SaveChangesAsync())
                .Returns(Task.FromResult(0));

            var projectId = Guid.NewGuid();

            // ACT
            var operationResult = await this.projectHelper.DeleteProjectTasksAsync(TestData.Tasks);

            // ASSERT
            Assert.IsFalse(operationResult);
            this.taskRepository.Verify(taskRepo => taskRepo.UpdateTasks(It.IsAny<List<TaskEntity>>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests whether we can get project tasks overview data with valid parameter.
        /// </summary>
        [TestMethod]
        public void GetProjectTasksOverview_WithValidParams_ShouldReturnValidData()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.TimesheetRepository).Returns(() => this.timesheetRepository.Object);
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.TaskRepository).Returns(() => this.taskRepository.Object);
            this.timesheetRepository
                .Setup(timesheetRepo => timesheetRepo.GetTimesheetRequestsByProjectId(It.IsAny<Guid>(), It.IsAny<TimesheetStatus>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(TestData.ApprovedTimesheets);
            this.taskRepository
                .Setup(taskRepo => taskRepo.GetTasksByProjectId(It.IsAny<Guid>()))
                .Returns(TestData.Tasks);

            var projectId = Guid.NewGuid();
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5);
            var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, startDate.Day + 1);

            // ACT
            var projectTasksOverview = this.projectHelper.GetProjectTasksOverview(projectId, startDate, endDate);

            // ASSERT
            Assert.AreEqual(TestData.ExpectedProjectTasksOverview.Count(), projectTasksOverview.Count());
            this.timesheetRepository.Verify(timesheetRepo => timesheetRepo.GetTimesheetRequestsByProjectId(It.IsAny<Guid>(), It.IsAny<TimesheetStatus>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.AtLeastOnce());
            this.taskRepository.Verify(taskRepo => taskRepo.GetTasksByProjectId(It.IsAny<Guid>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests whether empty list is return when tasks is not found while fetching project task overview.
        /// </summary>
        [TestMethod]
        public void GetProjectTasksOverview_WhenTasksNotFound_ShouldReturnEmptyList()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.TimesheetRepository).Returns(() => this.timesheetRepository.Object);
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.TaskRepository).Returns(() => this.taskRepository.Object);
            this.timesheetRepository
                .Setup(timesheetRepo => timesheetRepo.GetTimesheetRequestsByProjectId(It.IsAny<Guid>(), It.IsAny<TimesheetStatus>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(TestData.ApprovedTimesheets);
            this.taskRepository
                .Setup(taskRepo => taskRepo.GetTasksByProjectId(It.IsAny<Guid>()))
                .Returns(new List<TaskEntity>());

            var projectId = Guid.NewGuid();
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5);
            var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, startDate.Day + 1);

            // ACT
            var projectTasksOverview = this.projectHelper.GetProjectTasksOverview(projectId, startDate, endDate);

            // ASSERT
            Assert.IsTrue(projectTasksOverview.IsNullOrEmpty());
            this.taskRepository.Verify(taskRepo => taskRepo.GetTasksByProjectId(It.IsAny<Guid>()), Times.AtLeastOnce());
            this.timesheetRepository.Verify(timesheetRepo => timesheetRepo.GetTimesheetRequestsByProjectId(It.IsAny<Guid>(), It.IsAny<TimesheetStatus>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never());
        }

        /// <summary>
        /// Tests whether true is return on successfully adding project members.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task AddProjectMembers_WithCorrectModel_ShouldReturnTrue()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.MemberRepository).Returns(() => this.memberRepository.Object);
            this.repositoryAccessors.Setup(accessor => accessor.Context).Returns(FakeTimesheetContext.GetFakeTimesheetContext());
            this.memberRepository
                .Setup(memberRepo => memberRepo.GetAllMembers(It.IsAny<Guid>()))
                .Returns(TestData.Members);
            this.memberRepository
                .Setup(memberRepo => memberRepo.AddUsersAsync(It.IsAny<List<Member>>()))
                .Returns(Task.FromResult(true));
            this.repositoryAccessors
                .Setup(repositoryAccessor => repositoryAccessor.SaveChangesAsync())
                .Returns(Task.FromResult(1));

            // ACT
            var resultResponse = await this.projectHelper.AddProjectMembersAsync(Guid.NewGuid(), TestData.MembersDTO);

            // ASSERT
            Assert.IsTrue(resultResponse);
            this.memberRepository.Verify(memberRepo => memberRepo.AddUsersAsync(It.IsAny<IEnumerable<Member>>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests whether false is return when failure at database while adding project members.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task AddProjectMembers_WhenFailureAtDatabase_ShouldReturnFalse()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.MemberRepository).Returns(() => this.memberRepository.Object);
            this.repositoryAccessors.Setup(accessor => accessor.Context).Returns(FakeTimesheetContext.GetFakeTimesheetContext());
            this.memberRepository
                .Setup(memberRepo => memberRepo.GetAllMembers(It.IsAny<Guid>()))
                .Returns(TestData.Members);
            this.memberRepository
                .Setup(memberRepo => memberRepo.AddUsersAsync(It.IsAny<List<Member>>()))
                .Returns(Task.FromResult(true));
            this.repositoryAccessors
                .Setup(repositoryAccessor => repositoryAccessor.SaveChangesAsync())
                .Returns(Task.FromResult(0));

            // ACT
            var resultResponse = await this.projectHelper.AddProjectMembersAsync(Guid.NewGuid(), TestData.MembersDTO);

            // ASSERT
            Assert.IsFalse(resultResponse);
            this.memberRepository.Verify(memberRepo => memberRepo.AddUsersAsync(It.IsAny<IEnumerable<Member>>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests whether we can add project members with correct model who were removed earlier and get result true.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task AddExistingUsers_WithCorrectModel_ShouldReturnTrue()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.MemberRepository).Returns(() => this.memberRepository.Object);
            this.repositoryAccessors.Setup(accessor => accessor.Context).Returns(FakeTimesheetContext.GetFakeTimesheetContext());
            this.memberRepository
                .Setup(memberRepo => memberRepo.GetAllMembers(It.IsAny<Guid>()))
                .Returns(TestData.Members);
            this.memberRepository
                .Setup(memberRepo => memberRepo.UpdateMembers(It.IsAny<List<Member>>()));
            this.repositoryAccessors
                .Setup(repositoryAccessor => repositoryAccessor.SaveChangesAsync())
                .Returns(Task.FromResult(TestData.Members.Count));

            // ACT
            var resultResponse = await this.projectHelper.AddProjectMembersAsync(Guid.NewGuid(), TestData.ExistingMembers);

            // ASSERT
            Assert.IsTrue(resultResponse);
            this.memberRepository.Verify(memberRepo => memberRepo.UpdateMembers(It.IsAny<List<Member>>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests whether on successfully deleting members return true.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task DeleteMembersFromProject_WithCorrectModel_ShouldReturnTrue()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.MemberRepository).Returns(() => this.memberRepository.Object);
            this.repositoryAccessors.Setup(accessor => accessor.Context).Returns(FakeTimesheetContext.GetFakeTimesheetContext());

            this.memberRepository
                .Setup(memberRepo => memberRepo.FindAsync(It.IsAny<Expression<Func<Member, bool>>>()))
                .Returns(Task.FromResult(TestData.Members.AsEnumerable()));
            this.memberRepository
                .Setup(memberRepo => memberRepo.UpdateMembers(It.IsAny<List<Member>>()));
            this.repositoryAccessors
                .Setup(repositoryAccessor => repositoryAccessor.SaveChangesAsync())
                .Returns(Task.FromResult(TestData.Members.Count()));

            var projectId = Guid.NewGuid();

            // ACT
            var operationResult = await this.projectHelper.DeleteProjectMembersAsync(TestData.Members);

            // ASSERT
            Assert.IsTrue(operationResult);
            this.memberRepository.Verify(memberRepo => memberRepo.UpdateMembers(It.IsAny<List<Member>>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests whether false is return if there is failure at database while deleting project members.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task DeleteMembersFromProject_WhenFailureAtDatabase_ShouldReturnFalse()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.MemberRepository).Returns(() => this.memberRepository.Object);
            this.repositoryAccessors.Setup(accessor => accessor.Context).Returns(FakeTimesheetContext.GetFakeTimesheetContext());

            this.memberRepository
                .Setup(memberRepo => memberRepo.FindAsync(It.IsAny<Expression<Func<Member, bool>>>()))
                .Returns(Task.FromResult(TestData.Members.AsEnumerable()));
            this.memberRepository
                .Setup(memberRepo => memberRepo.UpdateMembers(It.IsAny<List<Member>>()));
            this.repositoryAccessors
                .Setup(repositoryAccessor => repositoryAccessor.SaveChangesAsync())
                .Returns(Task.FromResult(0));

            var projectId = Guid.NewGuid();

            // ACT
            var operationResult = await this.projectHelper.DeleteProjectMembersAsync(TestData.Members);

            // ASSERT
            Assert.IsFalse(operationResult);
            this.memberRepository.Verify(memberRepo => memberRepo.UpdateMembers(It.IsAny<List<Member>>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests whether we can get project members overview with valid parameters.
        /// </summary>
        [TestMethod]
        public void GetProjectMembersOverview_WithValidParams_ShouldReturnOKStatusWithValidData()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.TimesheetRepository).Returns(() => this.timesheetRepository.Object);
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.MemberRepository).Returns(() => this.memberRepository.Object);

            this.timesheetRepository
                .Setup(timesheetRepo => timesheetRepo.GetTimesheetRequestsByProjectId(It.IsAny<Guid>(), It.IsAny<TimesheetStatus>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(TestData.ApprovedTimesheets);
            this.memberRepository
                .Setup(memberRepo => memberRepo.GetMembers(It.IsAny<Guid>()))
                .Returns(TestData.Members);

            var projectId = Guid.NewGuid();
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5);
            var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, startDate.Day + 1);

            // ACT
            var projectMembersOverview = this.projectHelper.GetProjectMembersOverview(projectId, startDate, endDate);

            // ASSERT
            Assert.IsFalse(projectMembersOverview.IsNullOrEmpty());
            Assert.AreEqual(TestData.ExpectedProjectMembersOverview.Count(), projectMembersOverview.Count());
            this.timesheetRepository.Verify(timesheetRepo => timesheetRepo.GetTimesheetRequestsByProjectId(It.IsAny<Guid>(), It.IsAny<TimesheetStatus>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.AtLeastOnce());
            this.memberRepository.Verify(memberRepo => memberRepo.GetMembers(It.IsAny<Guid>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Tests whether empty list is return when members not found while fetching project members overview.
        /// </summary>
        [TestMethod]
        public void GetProjectMembersOverview_WhenMembersNotFound_ShouldReturnEmptyList()
        {
            // ARRANGE
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.TimesheetRepository).Returns(() => this.timesheetRepository.Object);
            this.repositoryAccessors.Setup(repositoryAccessor => repositoryAccessor.MemberRepository).Returns(() => this.memberRepository.Object);

            this.timesheetRepository
                .Setup(timesheetRepo => timesheetRepo.GetTimesheetRequestsByProjectId(It.IsAny<Guid>(), It.IsAny<TimesheetStatus>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(TestData.ApprovedTimesheets);
            this.memberRepository
                .Setup(memberRepo => memberRepo.GetMembers(It.IsAny<Guid>()))
                .Returns(new List<Member>());

            var projectId = Guid.NewGuid();
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5);
            var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, startDate.Day + 1);

            // ACT
            var projectMembersOverview = this.projectHelper.GetProjectMembersOverview(projectId, startDate, endDate);

            // ASSERT
            Assert.IsTrue(projectMembersOverview.IsNullOrEmpty());
            this.memberRepository.Verify(memberRepo => memberRepo.GetMembers(It.IsAny<Guid>()), Times.AtLeastOnce());
            this.timesheetRepository.Verify(timesheetRepo => timesheetRepo.GetTimesheetRequestsByProjectId(It.IsAny<Guid>(), It.IsAny<TimesheetStatus>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never());
        }
    }
}