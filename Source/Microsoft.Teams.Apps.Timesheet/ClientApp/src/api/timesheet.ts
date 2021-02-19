// <copyright file="timesheet.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import axios from "./axios-decorator";
import { IRequestApproval } from "../models/request-approval";
import { TimesheetStatus } from "../models/timesheet-status";

/**
 * Approve timesheets.
 * @param requestApproval The request approval of which timesheet status to approve.
 */
export const approveTimesheetsAsync = async (
    requestApproval: IRequestApproval[],
    handleTokenAccessFailure: (error: string) => void) => {
    let url = `/api/timesheets/approve`;

    return axios.post(url, handleTokenAccessFailure, requestApproval);
}

/**
 * Reject timesheets.
 * @param requestApproval The request approval of which timesheet status to reject.
 */
export const rejectTimesheetsAsync = async (
    requestApproval: IRequestApproval[],
    handleTokenAccessFailure: (error: string) => void) => {
    let url = `/api/timesheets/reject`;

    return axios.post(url, handleTokenAccessFailure, requestApproval);
}

/**
 * Gets user timesheet requests.
 * @param reporteeId The reportee Id of which requests to get.
 */
export const getUserTimesheetsAsync = async (
    reporteeId: string,
    handleTokenAccessFailure: (error: string) => void) => {
    let url = `/api/users/${reporteeId}/timesheets/${TimesheetStatus.Submitted}`;
    return await axios.get(url, handleTokenAccessFailure);
}