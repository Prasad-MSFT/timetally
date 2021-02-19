// <copyright file="ManagerDashboardMapperTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Tests.Mappers
{
    using System.Linq;
    using Microsoft.Teams.Apps.Timesheet.Extensions;
    using Microsoft.Teams.Apps.Timesheet.ModelMappers;
    using Microsoft.Teams.Apps.Timesheet.Tests.TestData;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Manager dashboard mapper tests contains test cases for getting distinct dates.
    /// </summary>
    [TestClass]
    public class ManagerDashboardMapperTests
    {
        /// <summary>
        /// Holds the instance of manager dashboard mapper.
        /// </summary>
        private ManagerDashboardMapper managerDashboardMapper;

        /// <summary>
        ///  Initialize all test variables.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.managerDashboardMapper = new ManagerDashboardMapper();
        }

        /// <summary>
        /// Test whether valid data is return with valid parameters while getting distinct dates.
        /// </summary>
        [TestMethod]
        public void GetDistinctDates_WithValidParams_ShouldReturnValidDates()
        {
            var a = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            var dateRange = this.managerDashboardMapper.GetDistinctDates(TestData.SavedTimesheets);

            Assert.IsFalse(dateRange.IsNullOrEmpty());
            Assert.AreEqual(TestData.ExpectedDateRange.Count, dateRange.Count);
            Assert.AreEqual(TestData.ExpectedDateRange.First().First().Date, dateRange.First().First().Date);
        }
    }
}