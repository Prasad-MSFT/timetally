// <copyright file="IRequestApproval.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import { Guid } from "guid-typescript";

export interface IRequestApproval {
    userId: Guid;
    timesheetDate: Array<Date>,
    status: number;
    managerComments: string;
}