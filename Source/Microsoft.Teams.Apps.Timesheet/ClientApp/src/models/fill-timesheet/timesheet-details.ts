// <copyright file="timesheet-details.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import { TimesheetStatus } from "../timesheet-status";

export default interface ITimesheetDetails {
    taskId: string,
    taskTitle: string,
    hours: number,
    status: TimesheetStatus,
    managerComments: string,
    isAddedByMember: boolean,
    isDeleteTaskInProgress: boolean,
    startDate: Date,
    endDate: Date
}