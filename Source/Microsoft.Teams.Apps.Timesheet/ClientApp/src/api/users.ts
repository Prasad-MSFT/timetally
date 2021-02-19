// <copyright file="users.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import axios from "./axios-decorator";
import { AxiosRequestConfig } from "axios";

/**
 * Search reportees
 * @param searchString Search text.
 */
export const getReportees = async (searchString: string, handleTokenAccessFailure: (error: string) => void) => {
    let url = '/api/me/reportees';
    let config: AxiosRequestConfig = axios.getAPIRequestConfigParams({ search: searchString });

    return await axios.get(url, handleTokenAccessFailure, config);
};

/**
 * Gets the user profiles
 * @param userIds The user IDs of which profiles to get
 */
export const getUserProfiles = async (userIds: Array<string>, handleTokenAccessFailure: (error: string) => void): Promise<any> => {
    let url = '/api/users';
    return await axios.post(url, handleTokenAccessFailure, userIds);
};