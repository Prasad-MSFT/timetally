// <copyright file="manager-dashboard.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import axios from "../api/axios-decorator";
import { AxiosRequestConfig } from "axios";

/**
 * Get approved and active project details for dashboard between date range.
 * @param startDate The start date of the date range.
 * @param endDate The end date of the date range.
 */
export const getDashboardProjectsAsync = async (
    startDate: Date,
    endDate: Date,
    handleTokenAccessFailure: (error: string) => void) => {
    let url = `/api/projects/dashboard`;
    let config: AxiosRequestConfig = axios.getAPIRequestConfigParams({
        startDate: startDate,
        endDate: endDate
    });

    return axios.get(url, handleTokenAccessFailure, config);
};

/**
 * Gets dashboard requests
 */
export const getDashboardRequestsAsync = async (
    handleTokenAccessFailure: (error: string) => void) => {
    let url = `/api/ManagerDashboard`;
    return await axios.get(url, handleTokenAccessFailure, undefined, undefined);
};