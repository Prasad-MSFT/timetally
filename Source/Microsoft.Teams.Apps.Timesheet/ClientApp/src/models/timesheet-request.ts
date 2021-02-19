// <copyright file="timesheet-request.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import { TimesheetStatus } from "./timesheet-status"

export default interface ITimesheetRequest {
    date: Date,
    hours: number,
    status: TimesheetStatus
}