/**
 * Copyright (c) 2019 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

import React, { useState, useEffect } from 'react';
import { getList } from '../../APIs'
import axios from 'axios';
import DataGrid from 'react-data-grid';
import Intl from 'intl';
import 'intl/locale-data/jsonp/en-US';

/*
import DatePicker from 'react-datepicker';
import setMinutes from "date-fns/setMinutes";
import setHours from "date-fns/setHours";
*/

import "react-datepicker/dist/react-datepicker.css";
import 'react-data-grid/dist/react-data-grid.css';
import './testresults.css';

import { Column, Cell } from '@enact/ui/Layout';
import Alert from '../Alert/Alert';
import { IoIosClose } from "react-icons/io";
import appCss from '../../App/App.module.less';

function TestResultsManager(props) {
    const [testResults, setTestResults] = useState([]);
    const [alert, setAlert] = useState({ status: false });
    const [isLoading, setIsLoading] = useState(true);

    const [selectedSimulation, setSelectedSimulation] = useState(null);
    const [searchString, setSearchString] = useState('');

    function alertHide() {
        setAlert({ status: false });
    }

    const onViewResultLinkClick = (testResultId) => {
        props.onViewResult(testResultId, { sim: selectedSimulation, search: searchString });
    }

    const CellDateRenderer = (value) => {
        if (value && value.row && value.row.created) {

            let dateTime = getReadableDateTime(new Date(value.row.created));

            return (
                <div className="runDateFullContainer">
                    <div className="runDateRelative">{dateTime.relativeTime}</div>
                    <div className="runDateContainer" key={value.row.id}>
                        <div className="date">{dateTime.date}</div>
                        <div className="time">{dateTime.time}</div>
                    </div>
                </div>
            );
        }
        else {
            return (<div />);
        }
    };

    const CellReportRenderer = (value) => {
        if (value) {

            const status = value.row.status;
            const success = value.row.success;

            let classNameSuffix = '';
            let message = '';
            if (status === 'completed') {
                classNameSuffix = (success === true ? 'success' : 'failed');
                message = (success === true ? 'Success' : 'Failed');
            } else if (status === 'inprogress') {
                classNameSuffix = 'inprogress';
                message = 'In Progress';
            } else {
                classNameSuffix = 'error';
                message = String(status).toUpperCase();
            }

            return (
                <div className="resultsStatusButtonContainer">
                    <div className={'resultStatusButton ' + classNameSuffix}>
                        <div>{message}</div>
                    </div>
                </div>
            );
        }
        else {
            return (<div />);
        }
    };

    const CellLinkRenderer = (value) => {
        if (value) {
            let id = (value && value.row && value.row.id) ? value.row.id : '';
            return (
                <div className="resultsLinkButtonContainer">
                    {value.row.status === 'completed' &&
                        <div className="reportLinkButton" key={id + '_report'} onClick={() => onViewResultLinkClick(id)}>
                            <div>View Report</div>
                        </div>
                    }
                    {/* <div className="videoLinkButton" key={id + '_video'} onClick={() => onViewResultVideoClick(id)}>
                        <div>View Video</div>
                    </div> */}
                </div>);
        }
        else {
            return (<div />);
        }
    };

    const columns = [
        { key: 'created', name: 'Created', width: 200, formatter: CellDateRenderer },
        { key: 'name', name: 'Test Case Name' },
        { key: 'status', name: 'Result', width: 100, formatter: CellReportRenderer, selectable: false },
        { key: 'id', name: 'Report', width: 100, formatter: CellLinkRenderer, selectable: false }
    ];

    /*
    const randomDate = function (start, end) {
        return (new Date(start.getTime() + Math.random() * (end.getTime() - start.getTime())));
    }
    */

    const minute = 60,
        hour = minute * 60,
        day = hour * 24,
        week = day * 7,
        month = (week * 4) + (2 * day),
        year = day * 365;

    const getReadableDateTime = function (date) {

        //Converting the incoming UTC time to local time.
        date = new Date(date + ' UTC');

        let delta = Math.round(((new Date()) - date) / 1000);

        let relativeTime;
        if (delta < 30) {
            relativeTime = 'Just now';
        } else if (delta < minute) {
            relativeTime = delta + ' seconds ago';
        } else if (delta < 2 * minute) {
            relativeTime = '1 minute ago';
        } else if (delta < hour) {
            relativeTime = Math.floor(delta / minute) + ' minutes ago';
        } else if (delta < 2 * hour) {
            relativeTime = '1 hour ago';
        } else if (delta < day) {
            relativeTime = Math.floor(delta / hour) + ' hours ago';
        } else if (delta < 2 * day) {
            relativeTime = '1 day ago';
        } else if (delta < week) {
            relativeTime = Math.floor(delta / day) + ' days ago';
        } else if (delta < 2 * week) {
            relativeTime = '1 week ago';
        } else if (delta < month) {
            relativeTime = Math.floor(delta / week) + ' weeks ago';
        } else if (delta < 2 * month) {
            relativeTime = '1 month ago';
        } else if (delta < year) {
            relativeTime = Math.floor(delta / month) + ' months ago';
        } else if (delta < 2 * year) {
            relativeTime = ' 1 year ago';
        } else {
            relativeTime = Math.floor(delta / year) + ' years ago';
        }

        const monthFormatter = new Intl.DateTimeFormat("en-US", { month: "short" });
        const dayFormatter = new Intl.DateTimeFormat("en-US", { day: "2-digit" });
        const timeFormatter = new Intl.DateTimeFormat("en-US", { hour: "2-digit", minute: "2-digit", second: "2-digit" });

        return {
            relativeTime: relativeTime,
            date: monthFormatter.format(date)
                + '-' + dayFormatter.format(date)
                + '-' + date.getFullYear(),
            time: timeFormatter.format(date)
        };
    }

    function getTestResultsAPIParamsString(params) {

        let testResultsAPIParams = '?';

        if (params.searchParams) {
            if (params.searchParams.sim) {
                setSelectedSimulation(params.searchParams.sim);
                testResultsAPIParams += 'simId=' + params.searchParams.sim.id + '&';
            }
            if (params.searchParams.search) {
                setSearchString(params.searchParams.search);
                testResultsAPIParams += 'search=' + params.searchParams.search;
            }
        } else if (params.selectedSimulation) {
            setSelectedSimulation(params.selectedSimulation);
            testResultsAPIParams += 'simId=' + params.selectedSimulation.id;
        }

        return testResultsAPIParams;
    }

    const fetchData = async (testResultsAPIParams) => {

        setIsLoading(true);

        const result = await getList('testresults',
            source.token,
            testResultsAPIParams
        );

        if (result.status === 200) {
            setTestResults(result.data);

            setIsLoading(false);
        } else {
            let alertMsg;
            if (result.name === "Error") {
                alertMsg = result.message;
            } else {
                alertMsg = `${result.statusText}: ${result.data ? result.data.error : 'Unknown error.'}`;
            }
            setAlert({ status: true, type: 'error', message: alertMsg });
        }
    };

    function setServerSearchForTestResults(ev) {
        setSearchString(ev.currentTarget.value);
    }

    function getServerSearchForTestResults(ev) {
        if (ev.keyCode === 13) {
            console.log('SEARCHING');
            const testResultsAPIParams = getTestResultsAPIParamsString({ searchParams: { sim: selectedSimulation, search: searchString } });
            console.log(testResultsAPIParams);
            fetchData(testResultsAPIParams);
        }
    }

    let source = axios.CancelToken.source();
    useEffect(() => {
        let testResultsAPIParams = null;
        if (props.selectedSimulation || props.searchParams) {
            testResultsAPIParams = getTestResultsAPIParamsString(props);
            props.onTestResultsManagerLoaded();
        }

        fetchData(testResultsAPIParams);

        return () => {
            source.cancel('Cancelling in cleanup.');
            setSelectedSimulation(null);
        };
    }, []);

    return (
        <Column className={appCss.pageContainer}>
            {
                alert.status &&
                <Alert type={alert.alertType} msg={alert.alertMsg}>
                    <IoIosClose onClick={alertHide} />
                </Alert>
            }
            <Cell shrink>
                <div className="testResultsTitleContainer">
                    Test Results
                </div>
            </Cell>
            <Cell shrink>
                <div className="controlPanelRow">
                    <div className="resultsSearchContainer">
                        <div className="searchInputLabel">Search:</div>
                        <input
                            value={searchString}
                            style={{ width: 'calc(100% - 120px)' }}
                            placeholder="Search by test case name..."
                            onChange={setServerSearchForTestResults}
                            onKeyDown={getServerSearchForTestResults}
                        />
                    </div>
                    {/*
                    <div className="resultsDateRangeContainer">
                        <div className="dateContainer">
                            <div className="searchInputLabel">Filter: </div>
                            <DatePicker
                                id="startEventDate"
                                showTimeSelect
                                dateFormat="MMMM/dd/yyyy h:mm aa"
                                placeholderText='select start date'
                                popperPlacement="bottom-end"
                                onChange={this.handleStartDateSelection}
                            />
                        </div>
                        <div className="dateContainer">
                            <DatePicker
                                id="endEventDate"
                                showTimeSelect
                                dateFormat="MMMM/dd/yyyy h:mm aa"
                                placeholderText='select end date'
                                popperPlacement="bottom-end"
                                injectTimes={[
                                            setHours(setMinutes(new Date(), 59), 23)
                                        ]}
                                onChange={this.handleEndDateSelection}
                            />
                        </div>
                    </div>
                    */}
                </div>
            </Cell>
            <Cell>
                {!isLoading &&
                    <div className="datagridContainer">
                        <div className="totalResultsCountContainer">
                            Total Results:
                            <div className="totalResultsCount">{testResults.length}</div>
                            {selectedSimulation &&
                                <div className="simulationName">
                                    Showing test results for: <span className="simulationNameValue">{selectedSimulation.name}</span>
                                </div>
                            }
                        </div>
                        <DataGrid
                            columns={columns}
                            rows={testResults}
                            rowHeight={50}
                        />
                    </div>
                }
                {
                    isLoading &&
                    <div style={{ marginLeft: '20px' }}>Loading...</div>
                }
            </Cell>
        </Column >
    );
}

export default TestResultsManager;