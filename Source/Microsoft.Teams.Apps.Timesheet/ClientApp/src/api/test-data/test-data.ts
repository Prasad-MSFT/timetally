// <copyright file="test-data.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import moment from "moment";
import { Guid } from "guid-typescript";
import IUserTimesheet from "../../models/fill-timesheet/user-timesheet";
import { TimesheetStatus } from "../../models/timesheet-status";
import IResource from "../../models/Resource";

const startOfCurrentWeek: Date = moment().startOf('week').startOf('day').toDate();
const projectEndDate: Date = moment(startOfCurrentWeek).add(10, 'day').startOf('day').toDate();

export default class TestData {
    public static getUserTimesheets: IUserTimesheet[] = [
        {
            timesheetDate: startOfCurrentWeek,
            projectDetails: [
                {
                    id: Guid.create().toString(),
                    title: "Timesheet App Template",
                    isProjectViewExpanded: false,
                    endDate: projectEndDate,
                    isAddNewTaskActivated: false,
                    isAddNewTaskInProgress: false,
                    startDate: startOfCurrentWeek,
                    timesheetDetails: [
                        {
                            taskId: Guid.create().toString(),
                            taskTitle: "Analysis",
                            hours: 8,
                            managerComments: "",
                            status: TimesheetStatus.None,
                            endDate: projectEndDate,
                            isAddedByMember: false,
                            isDeleteTaskInProgress: false,
                            startDate: startOfCurrentWeek
                        },
                        {
                            taskId: Guid.create().toString(),
                            taskTitle: "Development",
                            hours: 8,
                            managerComments: "",
                            status: TimesheetStatus.None,
                            endDate: projectEndDate,
                            isAddedByMember: false,
                            isDeleteTaskInProgress: false,
                            startDate: startOfCurrentWeek
                        }
                    ]
                }
            ]
        },
        {
            timesheetDate: moment(startOfCurrentWeek).add(1, 'day').toDate(),
            projectDetails: [
                {
                    id: Guid.create().toString(),
                    title: "Microsoft Teams",
                    isProjectViewExpanded: false,
                    endDate: projectEndDate,
                    isAddNewTaskActivated: false,
                    isAddNewTaskInProgress: false,
                    startDate: startOfCurrentWeek,
                    timesheetDetails: [
                        {
                            taskId: Guid.create().toString(),
                            taskTitle: "Analysis",
                            hours: 4,
                            managerComments: "",
                            status: TimesheetStatus.Saved,
                            endDate: projectEndDate,
                            isAddedByMember: false,
                            isDeleteTaskInProgress: false,
                            startDate: startOfCurrentWeek
                        },
                    ]
                }
            ]
        },
        {
            timesheetDate: moment(startOfCurrentWeek).add(2, 'day').toDate(),
            projectDetails: [
                {
                    id: Guid.create().toString(),
                    title: "Microsoft Teams",
                    isProjectViewExpanded: false,
                    endDate: projectEndDate,
                    isAddNewTaskActivated: false,
                    isAddNewTaskInProgress: false,
                    startDate: startOfCurrentWeek,
                    timesheetDetails: [
                        {
                            taskId: Guid.create().toString(),
                            taskTitle: "Analysis",
                            hours: 8,
                            managerComments: "",
                            status: TimesheetStatus.Submitted,
                            endDate: projectEndDate,
                            isAddedByMember: false,
                            isDeleteTaskInProgress: false,
                            startDate: startOfCurrentWeek
                        },
                    ]
                }
            ]
        },
        {
            timesheetDate: moment(startOfCurrentWeek).add(3, 'day').toDate(),
            projectDetails: [
                {
                    id: Guid.create().toString(),
                    title: "Microsoft Teams",
                    isProjectViewExpanded: false,
                    endDate: projectEndDate,
                    isAddNewTaskActivated: false,
                    isAddNewTaskInProgress: false,
                    startDate: startOfCurrentWeek,
                    timesheetDetails: [
                        {
                            taskId: Guid.create().toString(),
                            taskTitle: "Analysis",
                            hours: 8,
                            managerComments: "",
                            status: TimesheetStatus.Rejected,
                            endDate: projectEndDate,
                            isAddedByMember: false,
                            isDeleteTaskInProgress: false,
                            startDate: startOfCurrentWeek
                        },
                    ]
                }
            ]
        },
        {
            timesheetDate: moment(startOfCurrentWeek).add(4, 'day').toDate(),
            projectDetails: [
                {
                    id: Guid.create().toString(),
                    title: "Microsoft Teams",
                    isProjectViewExpanded: false,
                    endDate: projectEndDate,
                    isAddNewTaskActivated: false,
                    isAddNewTaskInProgress: false,
                    startDate: startOfCurrentWeek,
                    timesheetDetails: [
                        {
                            taskId: Guid.create().toString(),
                            taskTitle: "Analysis",
                            hours: 1,
                            managerComments: "",
                            status: TimesheetStatus.Approved,
                            endDate: projectEndDate,
                            isAddedByMember: false,
                            isDeleteTaskInProgress: false,
                            startDate: startOfCurrentWeek
                        },
                    ]
                }
            ]
        }
    ];

    public static getResources: IResource = {
        weeklyEffortsLimit: 40,
        timesheetFreezeDayOfMonth: 10,
        dailyEffortsLimit: 8
    }
}