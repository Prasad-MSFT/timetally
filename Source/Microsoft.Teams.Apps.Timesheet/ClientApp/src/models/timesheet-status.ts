// <copyright file="timesheet-status.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

// The enumeration that holds the status of timesheet.
export enum TimesheetStatus {
    // Indicates that the timesheet for calendar date not yet filled.
    None,

    // Indicates that the timesheet saved by user.
    Saved,

    // Indicates that the timesheet submitted by user.
    Submitted,

    // Indicates that the timesheet approved by manager.
    Approved,

    // Indicates that the timesheet rejected by manager.
    Rejected
}