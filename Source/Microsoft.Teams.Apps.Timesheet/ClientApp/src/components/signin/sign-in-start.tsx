// <copyright file="sign-in-start.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import React, { useEffect } from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import { RouteComponentProps } from "react-router-dom";
import { getAuthenticationConsentMetadata } from '../../api/authentication-metadata-api';

/** Initiates sign in request with authentication metadata */
const SignInSimpleStart: React.FunctionComponent<RouteComponentProps> = props => {
    const history = props.history;
    let location = {...window.location};

    useEffect(() => {
        microsoftTeams.initialize();
        microsoftTeams.getContext((context: microsoftTeams.Context) => {
            const windowLocationOriginDomain = location.origin.replace("https://", "");
            const login_hint = context.upn ?? "";

            getAuthenticationConsentMetadata(windowLocationOriginDomain, login_hint).then((result: any) => {
                history.push(result.data);
            });
        });
    }, [props.history]);

    return (
        <></>
    );
};

export default SignInSimpleStart;