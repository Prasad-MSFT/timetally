// <copyright file="add-task.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Flex, Text, Input, Button, Loader } from "@fluentui/react-northstar";
import { AddIcon, CloseIcon } from '@fluentui/react-icons-northstar';
import { Icon } from '@fluentui/react/lib/Icon';
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import DatePickerWrapper from "../../common/date-picker/date-picker";
import ITask from "../../../models/task";
import { cloneDeep } from "lodash";
import { Guid } from "guid-typescript";
import moment from "moment";

import "./add-task.scss";

interface IAddTaskProps extends WithTranslation {
    tasks?: ITask[];
    isMobileView: boolean;
    projectStartDate: Date;
    projectEndDate: Date;
    isAddTaskOnDoneClick: boolean;
    onTasksUpdated?: (tasks: ITask[]) => void;
    onDoneClick: (filteredTasks: ITask[]) => void;
    isLoading?: boolean;
}

interface IAddTaskState {
    tasks: ITask[],
    mobileInput: string,
    key: number
}

/**Render component to add task in project*/
class AddTask extends React.Component<IAddTaskProps, IAddTaskState> {
    readonly localize: TFunction;

    /** Constructor which initializes state */
    constructor(props: IAddTaskProps) {
        super(props);
        this.localize = this.props.t;

        this.state = {
            tasks: this.props.tasks ? cloneDeep(this.props.tasks) : [],
            mobileInput: "",
            key: 0
        };
    }

    // Handler which will be invoked when user clicked "+ Add row" button.
    onTaskRowAdded = async () => {
        let tasks = this.state.tasks ? cloneDeep(this.state.tasks) : [];
        tasks.push({
            id: Guid.createEmpty().toString(),
            title: "",
            startDate: moment(this.props.projectStartDate).startOf('day').toDate(),
            endDate: moment(this.props.projectEndDate).startOf('day').toDate(),
            projectId: Guid.createEmpty().toString()
        });

        if (!this.props.isAddTaskOnDoneClick && this.props.onTasksUpdated) {
            this.props.onTasksUpdated(tasks);
        }

        this.setState({ tasks });
    }

    /**
     * Event handler called when a task title get changed.
     * @param taskAtIndex The index in array where task details available.
     * @param value The update task title.
     */
    onTaskInputChange = async (taskAtIndex: number, value: any) => {
        let tasks: ITask[] = cloneDeep(this.state.tasks);

        if (taskAtIndex > -1 && taskAtIndex < tasks.length) {
            let taskToUpdate = tasks[taskAtIndex];

            if (taskToUpdate) {
                taskToUpdate.title = value;

                if (!this.props.isAddTaskOnDoneClick && this.props.onTasksUpdated) {
                    this.props.onTasksUpdated(tasks);
                }

                this.setState({ tasks });
            }
        }
    }

    /**
     * Handle when user clicks on done button.
     */
    handleDoneButtonClick = () => {
        let tasks = this.state.tasks;
        let filteredTasks: ITask[] = [];

        tasks.map((task: ITask) => {
            if (task.title?.trim().length > 0)
            {
                filteredTasks.push(task);
            }
        })
        this.props.onDoneClick(filteredTasks);
    }

    /**
     * Invoked from mobile when user enter tasks.
     * @param value Input tasks
     */
    onMobileInputChange = (value: string) => {
        let inputTasks = value.split(",");
        let tasks: any = []
        inputTasks.map((inputTask: any) => {
            tasks.push(inputTask.trim())
        });
        this.setState((prevState: IAddTaskState) => ({
            tasks,
            mobileInput: value,
            key: prevState.key + 1
        }))
    }

    /**
     * Event handler invoked on selecting start date of a task.
     * @param taskAtIndex The index in array of which task details needs to be updated.
     * @param date The selected date.
     */
    onStartDateChange = (taskAtIndex: number, date: Date) => {
        let tasks: ITask[] = cloneDeep(this.state.tasks);

        if (taskAtIndex > -1 && taskAtIndex < tasks.length) {
            let taskToUpdate = tasks[taskAtIndex];

            if (taskToUpdate) {
                taskToUpdate.startDate = moment(date).startOf('day').toDate();

                if (!this.props.isAddTaskOnDoneClick && this.props.onTasksUpdated) {
                    this.props.onTasksUpdated(tasks);
                }

                this.setState({ tasks });
            }
        }
    }

    /**
     * Event handler invoked on selecting end date of a task.
     * @param taskAtIndex The index in array of which task details needs to be updated.
     * @param date The selected date.
     */
    onEndDateChange = (taskAtIndex: number, date: Date) => {
        let tasks: ITask[] = cloneDeep(this.state.tasks);

        if (taskAtIndex > -1 && taskAtIndex < tasks.length) {
            let taskToUpdate = tasks[taskAtIndex];

            if (taskToUpdate) {
                taskToUpdate.endDate = moment(date).startOf('day').toDate();

                if (!this.props.isAddTaskOnDoneClick && this.props.onTasksUpdated) {
                    this.props.onTasksUpdated(tasks);
                }

                this.setState({ tasks });
            }
        }
    }

    /**
     * Event handler called when deleting a task.
     * @param taskAtIndex The index in array of which task to be deleted.
     */
    onDeleteTask = (taskAtIndex: number) => {
        let tasks: ITask[] = cloneDeep(this.state.tasks);

        if (taskAtIndex > -1 && taskAtIndex < tasks.length) {
            tasks.splice(taskAtIndex, 1);

            if (!this.props.isAddTaskOnDoneClick && this.props.onTasksUpdated) {
                this.props.onTasksUpdated(tasks);
            }

            this.setState({ tasks });
        }
    }

    /**
     * Render task input row
     */
    renderTaskInputRow  = () => {
        let counter = 0;
        let rows = (<Flex gap="gap.small" vAlign="center" column>
            {this.state.tasks.map((task: ITask, index: number) => {
                let row = <Flex key={`project-task-${index}`} gap="gap.medium" vAlign="center">
                    <Text size="small" content={`${++counter}.`} design={{ marginTop: index === 0 ? "2.5rem" : "0" }} />
                    {
                        index === 0 ?
                            <Flex column gap="gap.small" fill>
                                <Text size="small" content={this.localize("taskNameLabel")} />
                                <Input
                                    className="input"
                                    type="text"
                                    placeholder={this.localize('taskNameInputPlaceholder')}
                                    onChange={(event: any) => this.onTaskInputChange(index, event.target.value)}
                                    value={task.title}
                                    title={task.title}
                                    fluid
                                />
                            </Flex> :
                            <Input
                                className="input"
                                type="text"
                                placeholder={this.localize('taskNameInputPlaceholder')}
                                onChange={(event: any) => this.onTaskInputChange(index, event.target.value)}
                                value={task.title}
                                title={task.title}
                                fluid
                            />
                    }
                    {
                        index === 0 ?
                            <Flex column gap="gap.small">
                                <Text size="small" content={this.localize("addTaskStartDate")} />
                                <DatePickerWrapper
                                    className="add-task-datepicker"
                                    theme=""
                                    selectedDate={task.startDate}
                                    minDate={this.props.projectStartDate}
                                    onDateSelect={(date: Date) => this.onStartDateChange(index, date)}
                                    disableSelectionForPastDate={false}
                                />
                            </Flex> :
                            <DatePickerWrapper
                                className="add-task-datepicker"
                                theme=""
                                selectedDate={task.startDate}
                                minDate={this.props.projectStartDate}
                                onDateSelect={(date: Date) => this.onStartDateChange(index, date)}
                                disableSelectionForPastDate={false}
                            />
                    }
                    {
                        index === 0 ?
                            <Flex column gap="gap.small">
                                <Text size="small" content={this.localize("addTaskEndDate")} />
                                <DatePickerWrapper
                                    className="add-task-datepicker"
                                    theme=""
                                    selectedDate={task.endDate}
                                    minDate={this.props.projectStartDate}
                                    onDateSelect={(date: Date) => this.onEndDateChange(index, date)}
                                    disableSelectionForPastDate={false}
                                />
                            </Flex> :
                            <DatePickerWrapper
                                className="add-task-datepicker"
                                theme=""
                                selectedDate={task.endDate}
                                minDate={this.props.projectStartDate}
                                onDateSelect={(date: Date) => this.onEndDateChange(index, date)}
                                disableSelectionForPastDate={false}
                            />
                    }
                    <CloseIcon
                        className="cursor-pointer"
                        design={{ marginTop: index === 0 ? "2.5rem" : "0" }}
                        onClick={() => { this.onDeleteTask(index) }} />
                </Flex>
                return row;
            })}
            <Button className="add-row-button" icon={<AddIcon outline />} content={this.localize("addRowButtonLabel")} onClick={this.onTaskRowAdded}/>
        </Flex>);
        return rows;
    }

    /**Render view for mobile */
    renderMobileInput = () => {
        return (
            <div>
                <Flex vAlign="center">
                    <Flex.Item push>
                        <Button primary text content={<Text className="add-button" content={this.localize("addButtonLabel")} weight="semibold" />}  onClick={this.handleDoneButtonClick} />
                    </Flex.Item>
                </Flex>
                <Flex gap="gap.medium" vAlign="center" className="mobile-input-container" >
                    <Icon iconName="Org" className="add-task-icon"/>
                    <Flex column className="mobile-input">
                        <Text className="input-title input-element" content={this.localize("addTasksLabel") } />
                        <Input 
                            className="input-element"
                            type="text"
                            placeholder={this.localize('taskNameInputMobilePlaceholder')}
                            onChange={(event: any) => this.onMobileInputChange(event.target.value)}
                            value={this.state.mobileInput}
                            title={this.state.mobileInput}
                            fluid
                            inverted
                        />
                    </Flex>
                </Flex>
            </div>
        );
    }

    /** Renders the component*/
    render() {
        if (this.props.isLoading && this.props.isLoading) {
            return <Loader />;
        }

        if (!this.props.isMobileView){
            return (
                <div className="add-task-container"> 
                    <Flex column fill >
                        <Text content={this.localize("addTaskTaskModuleHeader")} weight="semibold" /><br />
                        <div className={this.props.isMobileView ? "input-rows-mobile" : "input-rows-desktop"}>
                            {!this.props.isMobileView && this.renderTaskInputRow()}
                        </div>
                    </Flex>
                    { this.props.isAddTaskOnDoneClick ?
                        <div className="footer">
                            <Flex>
                                <Flex.Item push>
                                    <Button primary className="action-button" content={this.localize("doneButtonLabel")} onClick={this.handleDoneButtonClick} />
                                </Flex.Item>
                            </Flex>
                        </div> : null }
                </div>);
        }
        else {
            return (
                <div className="add-task-container"> 
                    {this.renderMobileInput()}
                </div>);
        }
        
    }
}

export default withTranslation()(AddTask);