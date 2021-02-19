// <copyright file="TaskHelperTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Test.Helpers
{
    using System;
    using System.Linq;
    using System.Net;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.Timesheet.Helpers.Task;
    using Microsoft.Teams.Apps.Timesheet.Models;
    using Microsoft.Teams.Apps.Timesheet.Repositories;
    using Microsoft.Teams.Apps.Timesheet.Tests.TestData;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// This class lists unit test cases related to tasks.
    /// </summary>
    [TestClass]
    public class TaskHelperTests
    {
        /// <summary>
        /// The mocked instance of repository accessors to access repositories.
        /// </summary>
        private Mock<IRepositoryAccessors> repositoryAccessors;

        /// <summary>
        /// Mocked instance of logger.
        /// </summary>
        private Mock<ILogger<TaskHelper>> logger;

        /// <summary>
        /// The mocked instance of project repository.
        /// </summary>
        private Mock<IProjectRepository> projectRepository;

        private Mock<IMemberRepository> memberRepository;

        private Mock<ITaskRepository> taskRepository;

        /// <summary>
        ///  Initialize all test variables.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.logger = new Mock<ILogger<TaskHelper>>();
            this.projectRepository = new Mock<IProjectRepository>();
            this.memberRepository = new Mock<IMemberRepository>();
            this.taskRepository = new Mock<ITaskRepository>();
            this.repositoryAccessors = new Mock<IRepositoryAccessors>();
        }

        /// <summary>
        /// Tests whether task is added with correct model.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task AddTask_WithValidModel_ReturnsStatusOk()
        {
            this.repositoryAccessors.Setup(ra => ra.ProjectRepository).Returns(() => this.projectRepository.Object);
            this.repositoryAccessors.Setup(ra => ra.MemberRepository).Returns(() => this.memberRepository.Object);
            this.repositoryAccessors.Setup(ra => ra.TaskRepository).Returns(() => this.taskRepository.Object);

            this.repositoryAccessors.
                Setup(repositoryAccessor => repositoryAccessor.SaveChangesAsync()).
                Returns(Task.FromResult(1));

            var project = TestData.Projects.First();

            this.projectRepository.
                Setup(projectRepository => projectRepository.GetAsync(It.IsAny<Guid>())).
                Returns(Task.FromResult(project));

            this.memberRepository.
                Setup(memberRepository => memberRepository.GetMembers(It.IsAny<Guid>())).
                Returns(TestData.Members);

            var taskDetails = TestData.Task;
            taskDetails.StartDate = project.StartDate;
            taskDetails.EndDate = project.EndDate;

            this.taskRepository.
                Setup(taskRepository => taskRepository.Add(It.IsAny<TaskEntity>())).
                Returns(taskDetails);

            var taskHelper = new TaskHelper(this.repositoryAccessors.Object, this.logger.Object);
            var userObjectId = Guid.Parse("82ab7412-f6c1-491d-be16-f797e6903667");

            var addResult = await taskHelper.AddMemberTaskAsync(TestData.Task, Guid.Parse("1eec371f-edbe-4ad1-be1d-d4cd3515541e"), userObjectId);

            Assert.AreEqual(HttpStatusCode.OK, addResult.StatusCode);
        }

        /// <summary>
        /// Tests whether it returns HTTP status code BadRequest if project is not valid.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task AddTask_WithInvalidProject_ReturnsStatusBadRequest()
        {
            this.repositoryAccessors.Setup(ra => ra.ProjectRepository).Returns(() => this.projectRepository.Object);
            this.repositoryAccessors.Setup(ra => ra.MemberRepository).Returns(() => this.memberRepository.Object);

            Project project = null;

            this.projectRepository.
                Setup(projectRepository => projectRepository.GetAsync(It.IsAny<Guid>())).
                Returns(Task.FromResult(project));

            this.memberRepository.
                Setup(memberRepository => memberRepository.GetMembers(It.IsAny<Guid>())).
                Returns(TestData.Members);

            var taskHelper = new TaskHelper(this.repositoryAccessors.Object, this.logger.Object);

            var addResult = await taskHelper.AddMemberTaskAsync(TestData.Task, Guid.Parse("1eec371f-edbe-4ad1-be1d-d4cd3515541e"), Guid.Parse("e9be1d47-2707-4dfc-b2a9-e62648c3a04e"));

            Assert.AreEqual(HttpStatusCode.BadRequest, addResult.StatusCode);
        }

        /// <summary>
        /// Tests whether it returns HTTP status code Unauthorized if user is not member of project.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task AddTask_WithInvalidProjectMember_ReturnsStatusUnauthorized()
        {
            this.repositoryAccessors.Setup(ra => ra.ProjectRepository).Returns(() => this.projectRepository.Object);
            this.repositoryAccessors.Setup(ra => ra.MemberRepository).Returns(() => this.memberRepository.Object);

            this.projectRepository.
                Setup(projectRepository => projectRepository.GetAsync(It.IsAny<Guid>())).
                Returns(Task.FromResult(TestData.Projects.First()));

            this.memberRepository.
                Setup(memberRepository => memberRepository.GetMembers(It.IsAny<Guid>())).
                Returns(TestData.InvalidMembers);

            var taskHelper = new TaskHelper(this.repositoryAccessors.Object, this.logger.Object);

            var addResult = await taskHelper.AddMemberTaskAsync(TestData.Task, Guid.Parse("1eec371f-edbe-4ad1-be1d-d4cd3515541e"), Guid.Parse("e9be1d47-2707-4dfc-b2a9-e62648c3a04e"));

            Assert.AreEqual(HttpStatusCode.Unauthorized, addResult.StatusCode);
        }

        /// <summary>
        /// Tests whether it returns HTTP status code NoContent if valid model is passed.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task DeleteTask_WithValidModel_ReturnsStatusNoContent()
        {
            this.repositoryAccessors.Setup(ra => ra.TaskRepository).Returns(() => this.taskRepository.Object);

            this.repositoryAccessors.
                Setup(repositoryAccessor => repositoryAccessor.SaveChangesAsync()).
                Returns(Task.FromResult(1));

            this.taskRepository.
                Setup(taskRepository => taskRepository.Update(It.IsAny<TaskEntity>())).
                Returns(TestData.Task);

            this.taskRepository.
                Setup(taskRepository => taskRepository.GetTask(It.IsAny<Guid>())).
                Returns(TestData.Task);

            var taskHelper = new TaskHelper(this.repositoryAccessors.Object, this.logger.Object);

            var addResult = await taskHelper.DeleteMemberTaskAsync(TestData.Task.Id, Guid.Parse("e9be1d47-2707-4dfc-b2a9-e62648c3a04e"), Guid.Parse("1eec371f-edbe-4ad1-be1d-d4cd3515541e"));

            Assert.AreEqual(HttpStatusCode.NoContent, addResult.StatusCode);
        }

        /// <summary>
        /// Tests whether it returns HTTP status code NotFound if task is not found for deletion.
        /// </summary>
        /// <returns>A task that represents the work queued to execute.</returns>
        [TestMethod]
        public async Task DeleteTask_WithInvalidModel_ReturnsStatusNotFound()
        {
            this.repositoryAccessors.Setup(ra => ra.TaskRepository).Returns(() => this.taskRepository.Object);

            this.repositoryAccessors.
                Setup(repositoryAccessor => repositoryAccessor.SaveChangesAsync()).
                Returns(Task.FromResult(1));

            this.taskRepository.
                Setup(taskRepository => taskRepository.Update(It.IsAny<TaskEntity>())).
                Returns(TestData.Task);

            TaskEntity task = null;

            this.taskRepository.
                Setup(taskRepository => taskRepository.GetTask(It.IsAny<Guid>())).
                Returns(task);

            var taskHelper = new TaskHelper(this.repositoryAccessors.Object, this.logger.Object);

            var addResult = await taskHelper.DeleteMemberTaskAsync(TestData.Task.Id, Guid.Parse("e9be1d47-2707-4dfc-b2a9-e62648c3a04e"), Guid.Parse("1eec371f-edbe-4ad1-be1d-d4cd3515541e"));

            Assert.AreEqual(HttpStatusCode.NotFound, addResult.StatusCode);
        }
    }
}