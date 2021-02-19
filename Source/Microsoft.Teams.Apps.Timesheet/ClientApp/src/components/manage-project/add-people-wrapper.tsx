// <copyright file="add-people.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Flex, Text, Button, Loader } from "@fluentui/react-northstar";
import { IUserDropdownItem } from "../common/people-picker/people-picker";
import AddMembersByBillingType from "../common/add-members/add-members-by-billing-type";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import { Guid } from "guid-typescript";
import IProjectMember from "../../models/project-member";

import "./manage-project.scss";


interface IAddPeopleState {
    billableUsers: IUserDropdownItem[];
    nonBillableUsers: IUserDropdownItem[];
}

interface IAddPeopleWrapperProps extends WithTranslation {
    onDoneClick: (selectedUsers: IProjectMember[]) => void;
    existingUsers: IUserDropdownItem[];
    projectId: string;
    isMobileView: boolean;
    isLoading: boolean;
}

// Wrapper to hold billable and non billable users.
class AddPeopleWrapper extends React.Component<IAddPeopleWrapperProps, IAddPeopleState> {
    readonly localize: TFunction;
    teamId: string;

    /** 
     * Constructor which initializes state.
     */
    constructor(props: IAddPeopleWrapperProps) {
        super(props);
        this.localize = this.props.t;
        this.teamId = "";
        this.state = {
            billableUsers: [],
            nonBillableUsers: []
        };
    }

    /**
     * Convert IUserDropdownItem to ITimesheetRequest.
     * @param selectedUser Selected user details.
     * @param status User status.
     */
    getUserRequest = (selectedUser: IUserDropdownItem, status: boolean) => {
        let userRequest: IProjectMember = {
            userId: selectedUser.id,
            isBillable: status,
            projectId: this.props.projectId
        };
        return userRequest;
    }

    /**
     * Invoked when user click on Done/Add and add people, user has selected.
     */
    onDoneClick = async () => {
        let timesheetRequests: IProjectMember[] = [];
        if (this.state.billableUsers.length > 0) {
            this.state.billableUsers.map((selectedUser: any) => {
                let member = this.getUserRequest(selectedUser, true);
                timesheetRequests.push(member);
            });
        }

        if (this.state.nonBillableUsers.length > 0) {
            this.state.nonBillableUsers.map((selectedUser: any) => {
                let member = this.getUserRequest(selectedUser, false);
                timesheetRequests.push(member);
            });
        }
        this.props.onDoneClick(timesheetRequests);
    }

    /**
    * Handler which will be invoked when new user is selected as billable.
    * @param selectedItem selected value of an user.
    */
    onBillableUserChanged= async (selectedItem: IUserDropdownItem[]) => {
        this.setState({ billableUsers: selectedItem });
    }

    /**
    * Handler which will be invoked when new user is selected as non-billable.
    * @param selectedItem selected value of an user.
    */
    onNonBillableUserChanged = async (selectedItem: IUserDropdownItem[]) => {
        this.setState({ nonBillableUsers: selectedItem });
    }

    /**
     * Render view for mobile.
     */
    renderMobileView = () => {
        if (this.props.isLoading) {
            return <Loader />;
        }

        return (
            <div className="mobile-add-people">
                <Flex vAlign="center">
                    <Flex.Item push>
                        <Button primary text content={<Text className="add-button" content={this.localize("addButtonLabel")} weight="semibold" />} onClick={this.onDoneClick} />
                    </Flex.Item>
                </Flex>
                <AddMembersByBillingType
                    isMobileView={this.props.isMobileView}
                    onBillableUserChanged={this.onBillableUserChanged}
                    onNonBillableUserChanged={this.onNonBillableUserChanged}
                    existingUsers={this.props.existingUsers}
                />
            </div>);
    }

    /**
     * Render view for web.
     */
    renderWebView() {
        if (this.props.isLoading) {
            return <Loader />
        }

        return (
            <div className="web-add-people">
                <AddMembersByBillingType
                    isMobileView={this.props.isMobileView}
                    onBillableUserChanged={this.onBillableUserChanged}
                    onNonBillableUserChanged={this.onNonBillableUserChanged}
                    existingUsers={this.props.existingUsers}
                />
                <div className="footer">
                    <Flex>
                        <Flex.Item push>
                            <Button primary className="action-button" content={this.localize("doneButtonLabel")} onClick={this.onDoneClick} />
                        </Flex.Item>
                    </Flex>
                </div>
            </div>);
    }

    /**
     * Renders the component.
     */
    render() {
        if (this.props.isMobileView) {
            return this.renderMobileView();
        }
        else {
            return this.renderWebView();
        }

    }
}

export default withTranslation()(AddPeopleWrapper);