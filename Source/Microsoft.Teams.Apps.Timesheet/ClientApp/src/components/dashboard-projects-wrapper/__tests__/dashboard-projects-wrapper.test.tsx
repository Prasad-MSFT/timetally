// <copyright file="create-event-wrapper.test.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import DashboardProjectWrapper from "../dashboard-projects-wrapper";
import { Provider } from "@fluentui/react-northstar";
import { render, unmountComponentAtNode } from "react-dom";
import { act } from "react-dom/test-utils";
import pretty from "pretty";
import { IDashboardProject } from "../../../models/dashboard/dashboard-project";
import { Guid } from "guid-typescript";

jest.mock("react-i18next", () => ({
    useTranslation: () => ({
        t: (key: any) => key,
        i18n: { changeLanguage: jest.fn() },
    }),

    withTranslation: () => (Component: any) => {
        Component.defaultProps = {
            ...Component.defaultProps,
            t: (key: any) => key,
        };
        return Component;
    },
}));
jest.mock("@microsoft/teams-js", () => ({
    initialize: () => {
        return true;
    },
    getContext: (callback: any) =>
        callback(
            Promise.resolve({ teamId: "ewe", entityId: "sdsd", locale: "en-US" })
        ),
}));

let container: any = null;
let projects: Array<IDashboardProject> = [
    { id: Guid.create(), title: "Project 1", totalHours: 22, utilizedHours: 10 },
    { id: Guid.create(), title: "Project 2", totalHours: 100, utilizedHours: 0 },
];
beforeEach(() => {
    // setup a DOM element as a render target
    container = document.createElement("div");
    // container *must* be attached to document so events work correctly.
    document.body.appendChild(container);
});

afterEach(() => {
    // cleanup on exiting
    unmountComponentAtNode(container);
    container.remove();
    container = null;
});

describe("DashboardProjectWrapper", () => {
    it("renders snapshots", () => {
        act(() => {
            render(
                <Provider>
                    <DashboardProjectWrapper isMobileView={false} projects={projects} onProjectCardClick={() => { }} />
                </Provider>,
                container
            );
        });

        expect(pretty(container.innerHTML)).toMatchSnapshot();
    });
});