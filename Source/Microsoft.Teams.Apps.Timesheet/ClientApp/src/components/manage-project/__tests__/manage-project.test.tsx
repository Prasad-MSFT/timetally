// <copyright file="create-event-wrapper.test.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import ManageProject from "../manage-project";
import { Provider } from "@fluentui/react-northstar";
import { render, unmountComponentAtNode } from "react-dom";
import { act } from "react-dom/test-utils";
import pretty from "pretty";

jest.mock("../../../api/project");

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
beforeEach(async () => {
    // setup a DOM element as a render target
    container = document.createElement("div");
    // container *must* be attached to document so events work correctly.
    document.body.appendChild(container);

    await act(async () => {
        render(
            <Provider>
                <ManageProject />
            </Provider>,
            container
        );
    });
});

afterEach(() => {
    // cleanup on exiting
    unmountComponentAtNode(container);
    container.remove();
    container = null;
});

describe("ManageProjectWrapper", () => {
    it("renders snapshots", async () => {
        expect(pretty(container.innerHTML)).toMatchSnapshot();
    });

    it("removes member", async () => {
        const firstMember = document.querySelector("[data-tid=remove-member-0]");

        await act(async () => {
            firstMember?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        const memberTable = document.querySelector("[data-tid=member-table]");
        expect(memberTable?.childElementCount).toBe(2);
    });

    it("removes task", async () => {
        const firstTask = document.querySelector("[data-tid=remove-task-0]");

        await act(async () => {
            firstTask?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });

        const taskTable = document.querySelector("[data-tid=task-table]");
        expect(taskTable).toBe(null);
    });

    it("adds new task", async () => {
        const addNewTaskButton = document.querySelector(
            "[data-tid=addTaskButton]"
        );

        await act(async () => {
            addNewTaskButton?.dispatchEvent(
                new MouseEvent("click", { bubbles: true })
            );
        });

        const taskInput = document.querySelector("[data-tid=task-title-0]");

        act(() => {
            let nativeInputValueSetter = Object.getOwnPropertyDescriptor(
                window.HTMLInputElement.prototype,
                "New task"
            )?.set;
            nativeInputValueSetter?.call(taskInput?.firstChild, "Random");
            let ev = new Event("input", { bubbles: true });
            taskInput?.firstChild?.dispatchEvent(ev);
        });

        const doneTask = document.querySelector("[data-tid=submitTasks]");
        await act(async () => {
            doneTask?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
        });
    });
});
