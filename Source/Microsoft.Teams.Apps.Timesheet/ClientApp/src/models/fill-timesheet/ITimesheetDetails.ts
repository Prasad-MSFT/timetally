// <copyright file="ITimesheetDetails.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import { Guid } from "guid-typescript";

export default interface ITimesheetDetails {
    taskId: Guid;
    taskTitle: string;
    hours: number;
    status: number;
    managerComments: string;
}