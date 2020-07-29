/**
 * Copyright (c) 2019 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

import React, { useState, useEffect } from 'react';
import { getItem, deleteItem } from '../../APIs'
import axios from 'axios';

import { Tab, Tabs, TabList, TabPanel } from 'react-tabs';
import 'react-tabs/style/react-tabs.css';

import FormModal from '../Modal/FormModal';

import { FaRegTrashAlt, FaRegCaretSquareDown, FaRegCaretSquareUp } from 'react-icons/fa';
import { FiArrowLeftCircle } from 'react-icons/fi';
import { GoSettings } from 'react-icons/go';

import { Column, Cell } from '@enact/ui/Layout';
import Alert from '../Alert/Alert';
import { IoIosClose } from "react-icons/io";
import { withRouter } from 'react-router-dom';

function TestResultView(props) {
    const [testResult, setTestResult] = useState();
    const [testIterationReport, setTestIterationReport] = useState();
    const [alert, setAlert] = useState({ status: false });
    const [isLoading, setIsLoading] = useState(true);
    const [showConfigParams, setShowConfigParams] = useState(false);

    const [iterationResults, setIterationResults] = useState([]);
    const [selectedIteration, setSelectedIteration] = useState([]);

    const [bridgeTypes, setBridgeTypes] = useState('');
    const [bridgeConnections, setBridgeConnections] = useState('');
    const [totalEgoCollisions, setTotalEgoCollisions] = useState([]);
    const [failureCause, setFailureCause] = useState('');
    const [resultStats, setResultStats] = useState([]);
    const [videoCaptures, setVideoCaptures] = useState([]);

    const [openConfirmationDialog, setOpenConfirmationDialog] = useState(false);
    const [showConfirmation, setShowConfirmation] = useState(false);
    const [showVideo, setShowVideo] = useState(false);
    const [videoSource, setVideoSource] = useState('');

    let tempCollisionCount = 0;
    let tempOtherCount = 0;

    function alertHide() {
        setAlert({ status: false });
    }

    let onBackButton = props.onBackButton;
    let refSearchParams = props.refSearchParams;

    function goBackToResultsListingPage() {
        if (onBackButton) {
            onBackButton(refSearchParams);
        }
    }

    function onDeleteReport() {
        setOpenConfirmationDialog(true);
    }

    async function deleteReport() {
        const result = await deleteItem('testresults', props.match.params.testResultId, source.token);
        if (result.status === 200) {
            setShowConfirmation(true);
        }
    }

    function onCloseConfirmation() {
        setOpenConfirmationDialog(false);
    }

    function onDeleteReportConfirmation(type) {
        if (type === 'save') {
            deleteReport();
        } else if (type === 'cancel') {
            onCloseConfirmation()
        }
    }

    function openVideo(index) {
        setShowVideo(true);
        setVideoSource(videoCaptures[index]);
    }

    function closeVideo() {
        setShowVideo(false);
        setVideoSource('');
    }

    /*
    const CellMultiRenderer = (valObject) => {
        let valCells = [];
        let valRows = [];
        let rowCount = 0;
        for (let prop in valObject) {
            valCells.push(<td className="sensorParamsPropCell" key={rowCount}>
                {prop}: {valObject[prop]}
            </td>);

            rowCount++;
            if (rowCount > 2) {
                valRows.push(<tr className="sensorParamsPropRow" key={valRows.length}>{valCells}</tr>);
                valCells = [];
                rowCount = 0;
            }
        }
        if (rowCount > 0)
            valRows.push(<tr className="sensorParamsPropRow" key={valRows.length}>{valCells}</tr>);

        return (<table className="sensorParamsContainer"><tbody>{valRows}</tbody></table>);
    }

    const CellTransformRenderer = value => {
        if (value && value.row && value.row.transform) {

            return CellMultiRenderer(value.row.transform);

        } else {

            return (<div></div>);

        }
    }

    const CellParamsRenderer = (value) => {
        if (value && value.row && value.row.params) {

            return CellMultiRenderer(value.row.params);

        }
        else {

            return (<div></div>);

        }
    };

    const getSensorData = (sensorJSONString) => {
        let sensorData = sensorJSONString.replace('\\n', '').replace('\\', '');
        sensorData = JSON.parse(sensorData);
        console.log(sensorData);
        return sensorData;
    }

    const sensorConfigColumns = [
        { key: 'name', name: 'Sensor', width: 150 },
        { key: 'params', name: 'Params', formatter: CellParamsRenderer },
        { key: 'transform', name: 'Transform', formatter: CellTransformRenderer, width: 250 }
    ];
    */

    const getFormattedTime = (datetimeString) => {
        const dateTime = datetimeString.split('T');
        const time = dateTime[1].split('.')[0];
        return time + ' ' + dateTime[0];
    }

    const getDuration = (durationInSeconds) => {

        let time = new Date(Number(durationInSeconds) * 1000);
        let hours = time.getUTCHours();
        let minutes = time.getUTCMinutes();
        let seconds = time.getSeconds();

        return (
            hours.toString().padStart(2, '0')
            + ':' +
            minutes.toString().padStart(2, '0')
            + ':' +
            seconds.toString().padStart(2, '0')
        );
    }

    const getStats = (stats) => {
        const keys = Object.keys(stats);
        const vals = Object.values(stats);
        const rows = {};

        let statTitle = '';
        let statUnit = '';
        let statUnitSuper = '';

        for (let i = 0; i <= keys.length; i++) {
            const statKey = keys[i];
            let statVal = Number(vals[i]).toFixed(2);
            switch (statKey) {
                case 'Distance':
                    {
                        statTitle = 'Distance travelled:';
                        statUnit = 'km';
                        statUnitSuper = '';
                        break;
                    }
                case 'SpeedAvg':
                    {
                        statTitle = 'Average speed:';
                        statUnit = 'km/hr';
                        statUnitSuper = '';
                        break;
                    }
                case 'SpeedMin':
                    {
                        statTitle = 'Min speed:';
                        statUnit = 'km/hr';
                        statUnitSuper = '';
                        break;
                    }
                case 'SpeedMax':
                    {
                        statTitle = 'Max speed:';
                        statUnit = 'km/hr';
                        statUnitSuper = '';
                        break;
                    }
                case 'AccelLongMax':
                    {
                        statTitle = 'Max longitudinal acceleration:';
                        statUnit = 'm/s';
                        statUnitSuper = '2'
                        break;
                    }
                case 'AccelLatMax':
                    {
                        statTitle = 'Max lateral acceleration:';
                        statUnit = 'm/s';
                        statUnitSuper = '2'
                        break;
                    }
                case 'JerkLongMax':
                    {
                        statTitle = 'Max longitudinal jerk:';
                        statUnit = 'm/s';
                        statUnitSuper = '3'
                        break;
                    }
                case 'JerkLatMax':
                    {
                        statTitle = 'Max lateral jerk:';
                        statUnit = 'm/s';
                        statUnitSuper = '3'
                        break;
                    }
                default:
                    {
                        continue;
                    }
            }
            rows[statTitle] =
                <tr key={statKey}>
                    <td style={{ width: '220px' }}>{statTitle}</td>
                    <td>{statVal}</td>
                    <td>{statUnit}{<sup>{statUnitSuper}</sup>}</td>
                </tr>;
        }

        return rows;
    };

    const getEvent = (event) => {
        const keys = Object.keys(event);
        const vals = Object.values(event);
        const rows = [];

        let eventTitle = '';
        let eventValue = '';
        let eventUnit = '';

        for (let i = 0; i <= keys.length; i++) {
            const eventKey = keys[i];
            const eventVal = vals[i];
            switch (eventKey) {
                case 'Time':
                    {
                        eventTitle = 'Time of collision (from start):';
                        eventValue = getDuration(eventVal);
                        eventUnit = '';
                        break;
                    }
                case 'Location':
                    {
                        eventTitle = 'Map location:';
                        eventValue = '( ' + Number(eventVal.x).toFixed(4) + ', ' + Number(eventVal.y).toFixed(4) + ', ' + Number(eventVal.z).toFixed(4) + ' )';
                        eventUnit = '';
                        break;
                    }
                case 'EgoVelocity':
                    {
                        eventTitle = 'Speed of ego at collision:';
                        eventValue = (Math.sqrt(Math.pow(eventVal.x, 2) + Math.pow(eventVal.y, 2) + Math.pow(eventVal.z, 2))).toFixed(2);
                        eventUnit = 'km/hr';
                        break;
                    }
                case 'OtherType':
                    {
                        eventTitle = 'Type of collision:';
                        eventValue = (eventVal === 'NPC' ? 'Vehicle' : eventVal);
                        eventUnit = '';
                        break;
                    }
                case 'OtherVelocity':
                    {
                        eventTitle = 'Speed of object/agent at collision:';
                        eventValue = (Math.sqrt(Math.pow(eventVal.x, 2) + Math.pow(eventVal.y, 2) + Math.pow(eventVal.z, 2))).toFixed(2);
                        eventUnit = 'km/hr';
                        break;
                    }
                default:
                    {
                        continue;
                    }
            }
            rows.push(
                <tr key={eventTitle}>
                    <td style={{ width: '240px' }}>{eventTitle}</td>
                    <td style={{ width: '250px' }}>{eventValue}</td>
                    <td>{eventUnit}</td>
                </tr >
            );
        }
        return rows;
    };

    const analyseAndSetTestResults = (iterationReportData) => {
        if (iterationReportData
            && iterationReportData.Results
            && iterationReportData.Results.Agents) {

            //Need to use arrays here instead of a map since the Agent Names are not unique.
            const bridgeTypes = [];
            const resBridgeConnections = [];
            const collisions = [];
            const stats = [];
            const videos = [];

            const configAgents = iterationReportData.Agents;
            configAgents.forEach((agent) => {

                let bridgeType = '';
                if (agent.Bridge && agent.Bridge.Name) {
                    bridgeType = agent.Bridge.Name;
                } else {
                    bridgeType = 'No Bridge';
                }
                bridgeTypes.push(bridgeType);

                let bridgeConnection = '';
                if (agent.Connection) {
                    bridgeConnection = agent.Connection;
                } else {
                    bridgeConnection = 'None';
                }
                resBridgeConnections.push(bridgeConnection);
            });

            let causeOfFailure = '';
            const resultAgents = iterationReportData.Results.Agents;
            resultAgents.forEach((agent) => {

                let collisionCount = 0;
                if (agent.Events) {
                    agent.Events.forEach((event) => {
                        if (event.Type === 'EgoCollision') {
                            causeOfFailure = 'One or more collisions reported.'
                            collisionCount++;
                        }
                        if (!causeOfFailure) {
                            causeOfFailure = 'One or more failure events reported. Please see the "Events" box below.'
                        }
                    });
                }
                if (!causeOfFailure) {
                    causeOfFailure = 'Unknown'
                }
                collisions.push(collisionCount);

                let stat = {};
                if (agent.Sensors) {
                    //Only the first sensor is required to be analyzed;
                    for (let key in agent.Sensors) {
                        stat = getStats(agent.Sensors[key]);
                        break;
                    }
                }
                stats.push(stat);

                if (agent.VideoCapture) {
                    videos.push(agent.VideoCapture);
                } else {
                    videos.push(null);
                }
            });

            setBridgeTypes(bridgeTypes);
            setBridgeConnections(resBridgeConnections);
            setTotalEgoCollisions(collisions);
            setFailureCause(causeOfFailure);
            setResultStats(stats);
            setVideoCaptures(videos);

        } else {

            setBridgeTypes('');
            setBridgeConnections('');
            setTotalEgoCollisions([]);
            setResultStats([]);
            setVideoCaptures([]);
        }

        setTestIterationReport(iterationReportData);
    };

    const selectIteration = (index) => {

        setSelectedIteration(index);

        analyseAndSetTestResults(iterationResults[index]);
    }

    /*
    const randomDate = function (start, end) {
        const dateVal = new Date(start.getTime() + Math.random() * (end.getTime() - start.getTime()));

        const monthFormatter = new Intl.DateTimeFormat("en-US", { month: "short" });
        const dayFormatter = new Intl.DateTimeFormat("en-US", { day: "2-digit" });
        const timeFormatter = new Intl.DateTimeFormat("en-US", { hour: "2-digit", minute: "2-digit", second: "2-digit" });

        return {
            date: monthFormatter.format(dateVal)
                + '-' + dayFormatter.format(dateVal)
                + '-' + dateVal.getFullYear(),
            time: timeFormatter.format(dateVal)
        };
    }
    */

    let source = axios.CancelToken.source();
    let unmounted;
    useEffect(() => {

        unmounted = false;

        if (props.match && props.match.params && props.match.params.testResultId) {

            const fetchData = async () => {

                setIsLoading(true);

                const result = await getItem('testresults', props.match.params.testResultId, source.token);

                if (result.status === 200) {
                    if (!unmounted) {

                        const resultData = result.data;
                        setTestResult(resultData);

                        const iterations = JSON.parse(resultData.result);
                        setIterationResults(iterations);

                        selectIteration(0);
                        //Need to call this function here since for the first time the iterations might not be ready.
                        analyseAndSetTestResults(iterations[0]);

                        setIsLoading(false);
                    };
                } else {
                    let alertMsg;
                    if (!unmounted) {
                        if (result.name === "Error") {
                            alertMsg = result.message;
                        } else {
                            alertMsg = `${result.statusText}: ${result.data.error}`;
                        }
                        setAlert({ status: true, type: 'error', message: alertMsg });
                    }
                }
            };
            fetchData();
        }

        return () => {
            unmounted = true;
            source.cancel('Cancelling in cleanup.');
            setTestIterationReport();
        };
    }, [props.match.params.testResultId]);

    return (
        <Column style={{ background: '#FFFFFF' }}>
            {
                alert.status &&
                <Alert type={alert.alertType} msg={alert.alertMsg}>
                    <IoIosClose onClick={alertHide} />
                </Alert>
            }
            <Cell shrink>
                {testIterationReport && !isLoading &&
                    <>
                        <div className="testReportTitleContainer">
                            <div className="testReportNameContainer">
                                <div><b>{testResult.name}</b></div>
                                <div style={{ margin: '10px 0px 0px 0px' }}>Test Case: {testResult.simulationName}</div>
                            </div>
                            <div title="Go back to Test Results" className="backToPreviousPageButton" onClick={goBackToResultsListingPage}>
                                {FiArrowLeftCircle()}
                            </div>
                            <div title={(showConfigParams ? 'Hide' : 'Show') + ' Environment Configuration'} className="showHideEnvironmentConfig" onClick={(ev) => setShowConfigParams(!showConfigParams)}>
                                {showConfigParams ? FaRegCaretSquareUp() : FaRegCaretSquareDown()} {GoSettings()} <div />
                            </div>
                            <div title="Delete this Test Report" className="deleteReportButton" onClick={onDeleteReport}>
                                {FaRegTrashAlt()}
                            </div>
                        </div>
                        <div className={'testConfigParamsContainer' + ' ' + (showConfigParams ? 'show' : 'hide')}>
                            <div className="testConfigParams">
                                <div>Cluster: <span className="testResultParam">{testIterationReport.ClusterName}</span></div>
                                <div>Mode: <span className="testResultParam">{testIterationReport.ApiOnly ? 'API Only' : (testIterationReport.TestCase ? 'Test Case' : 'Random')}</span></div>
                            </div>
                            <div className="testConfigParams">
                                <div>No-Render: <span className="testResultParam">{String(testIterationReport.Headless)}</span></div>
                                <div>Interactive: <span className="testResultParam">{String(testIterationReport.Interactive)}</span></div>
                            </div>
                            <div className="testConfigParams">
                                <div>Map: <span className="testResultParam">{testIterationReport.MapName}</span></div>
                                <div>Seed: <span className="testResultParam">{testIterationReport.Seed}</span></div>
                            </div>
                            {(testIterationReport.RuntimeTemplateType || testIterationReport.TestCaseFile) &&
                                <div className="testConfigParams">
                                    <div>Template Type: <span className="testResultParam">{testIterationReport.RuntimeTemplateType}</span></div>
                                    <div>Template File: <span className="testResultParam">{testIterationReport.TestCaseFile}</span></div>
                                </div>
                            }
                            <div className="testConfigParams">
                                <div>Random Traffic: <span className="testResultParam">{String(testIterationReport.UseTraffic)}</span></div>
                                <div>Random Pedestrians: <span className="testResultParam">{String(testIterationReport.UsePedestrians)}</span></div>
                            </div>
                            <div className="testConfigParams">
                                <div>Cloudiness: <span className="testResultParam">{testIterationReport.Cloudiness}</span></div>
                                <div>Fog: <span className="testResultParam">{testIterationReport.Fog}</span></div>
                            </div>
                            <div className="testConfigParams">
                                <div>Rain: <span className="testResultParam">{testIterationReport.Rain}</span></div>
                                <div>Wetness: <span className="testResultParam">{testIterationReport.Wetness}</span></div>
                            </div>
                            <div className="testConfigParams">
                                <div>Time Of Day: <span className="testResultParam">{testIterationReport.TimeOfDay ? testIterationReport.TimeOfDay.split('T')[1] : ''}</span></div>
                            </div>
                        </div>
                        <div className="iterationResultsContainer">
                            <div style={{ margin: '5px 10px' }}>Test Iterations:</div>
                            <div className="iterationResultsCubesContainer">
                                {iterationResults.map((iteration, index) => {
                                    return (
                                        <div
                                            key={'cube_' + index}
                                            className={'iterationResultsCube' + '-' + String(iteration.Results.Status).toLowerCase() + (selectedIteration === index ? ' selected' : '')}
                                            onClick={() => selectIteration(index)}
                                        >
                                            {index + 1}
                                        </div>
                                    )
                                })
                                }
                            </div>
                        </div>
                        <div className="testReportStatusAndTimeContainer">
                            <div className="testReportStatusContainer">
                                <div className="testReportStatusValue">

                                    Iteration: <div style={{ marginLeft: '8px' }}>{selectedIteration + 1}</div>

                                    <div className="testReportStatusDivider">
                                        |
                                    </div>
                                    Status:
                                    <div className={'testReportStatus' + '-' + String(testIterationReport.Results.Status).toLowerCase()}>
                                        {(testIterationReport.Results.Status).toUpperCase()}
                                    </div>
                                </div>
                                <div className="testReportStatusCause">
                                    {testIterationReport.Results.Status !== 'Success' &&
                                        <>
                                            <i>Cause:</i> {testIterationReport.Results.Cause ? testIterationReport.Results.Cause : failureCause}
                                        </>
                                    }
                                </div>
                            </div>
                            <div className="testResultTimeInfoContainer">
                                <div>Test Case Start Time: <span style={{ marginLeft: '10px' }} />{getFormattedTime(testIterationReport.Results.StartTime)}</div>
                                <div>Test Case Stop Time: <span style={{ marginLeft: '10px' }} />{getFormattedTime(testIterationReport.Results.StopTime)}</div>
                                <div>Simulation Duration: <span style={{ marginLeft: '10px' }} />{getDuration(testIterationReport.Results.Duration)}</div>
                            </div>
                        </div>
                        <div className="testResultAgentsContainer">
                            <Tabs>
                                <TabList>
                                    {testIterationReport.Results.Agents.map((value, index) => {
                                        return <Tab className="react-tabs__tab agent-tab" selectedClassName="react-tabs__tab--selected agent-tab--selected" key={index}>{value.Name}</Tab>
                                    })}
                                </TabList>
                                {testIterationReport.Results.Agents.map((value, index) => {
                                    return (
                                        <TabPanel key={index}>
                                            <div style={{ display: "flex" }}>
                                                <div className="reportConnectionContainer">
                                                    <div style={{ width: '100%' }}>
                                                        <div style={{ margin: '0px 0px 5px 0px' }}>Bridge type: <span style={{ marginLeft: '10px' }} />{bridgeTypes.length > index ? bridgeTypes[index] : 'None'}</div>
                                                        <div style={{ margin: '0px 0px 5px 0px' }}>Connection: <span style={{ marginLeft: '10px' }} />{bridgeConnections.length > index ? bridgeConnections[index] : 'None'}</div>
                                                    </div>
                                                    <div style={{ width: '100%' }}>
                                                        {videoCaptures[index] &&
                                                            <div className="videoLinkButton" onClick={() => openVideo(index)}>
                                                                <div style={{ margin: 'auto' }}>View Video</div>
                                                            </div>
                                                        }
                                                    </div>
                                                </div>
                                            </div>
                                            <div className="reportResultsContainer">
                                                <div className="reportStatisticsContainer">
                                                    <div>Statistics</div>
                                                    {(resultStats.length > index) &&
                                                        <table>
                                                            <tbody>
                                                                <tr><td /></tr>
                                                                {resultStats[index]['Distance travelled:']}
                                                                <tr><td /></tr>
                                                                <tr><td /></tr>
                                                                {resultStats[index]['Average speed:']}
                                                                {resultStats[index]['Max speed:']}
                                                                {resultStats[index]['Min speed:']}
                                                                <tr><td /></tr>
                                                                <tr><td /></tr>
                                                                {resultStats[index]['Max longitudinal acceleration:']}
                                                                <tr><td /></tr>
                                                                <tr><td /></tr>
                                                                {resultStats[index]['Max lateral acceleration:']}
                                                                <tr><td /></tr>
                                                                <tr><td /></tr>
                                                                {resultStats[index]['Max longitudinal jerk:']}
                                                                {resultStats[index]['Max lateral jerk:']}
                                                            </tbody>
                                                        </table>
                                                    }
                                                </div>
                                                <div className="reportEventsContainer">
                                                    <div>Events</div>
                                                    <div style={{ fontSize: '14px', margin: '20px 10px 10px 10px' }}>Total Collisions: <span style={{ marginLeft: '10px' }} />{index < totalEgoCollisions.length ? totalEgoCollisions[index] : 0}</div>
                                                    {tempCollisionCount = 0, tempOtherCount = 0, value.Events.map((event, i) => {
                                                        event.Type === 'EgoCollision' ? tempCollisionCount++ : tempOtherCount++;
                                                        return (
                                                            <div key={i} className="reportEventContainer">
                                                                <div>
                                                                    {event.Type === 'EgoCollision' && (<>Collision: <span style={{ marginLeft: '5px' }} />{tempCollisionCount}</>)}
                                                                    {event.Type !== 'EgoCollision' && (<>{event.Type}: <span style={{ marginLeft: '5px' }} />{tempOtherCount}</>)}
                                                                </div>
                                                                <table>
                                                                    <tbody>{getEvent(event)}</tbody>
                                                                </table>
                                                            </div>
                                                        )
                                                    }, this)}
                                                </div>
                                            </div>
                                        </TabPanel>
                                    )
                                })}
                            </Tabs>
                        </div>
                    </>
                }
                {
                    isLoading &&
                    <div style={{ marginLeft: '20px' }}>Loading...</div>
                }
            </Cell>
            {openConfirmationDialog &&
                <FormModal
                    title="Delete Test Result"
                    hideCancelButton={showConfirmation}
                    submitButtonLabel={showConfirmation ? 'OK' : 'Confirm deletion'}
                    onModalClose={showConfirmation ? onCloseConfirmation : onDeleteReportConfirmation}
                >
                    {
                        !showConfirmation &&
                        <div style={{ width: '500px', padding: '10px', fontSize: '18px', textAlign: 'center' }}>
                            <span style={{ color: '#ff0000' }}>Are you sure you want to delete this test result?</span>
                            <br /><br />Note: <i>This will delete the test reports for all iterations.</i>
                        </div>
                    }
                    {
                        showConfirmation &&
                        <div style={{ width: '500px', padding: '10px', fontSize: '18px', textAlign: 'center' }}>
                            <span>This test result has been deleted.</span>
                        </div>
                    }
                </FormModal>
            }
            {showVideo &&
                <div className="videoModalWindowContainer">
                    <div className="videoModalWindow" />
                    <div className="videoContainer">
                        <div className='videoCloseButton' onClick={closeVideo}>close</div>
                        <video
                            src={'/videos/' + videoSource}
                            type="video/mp4"
                            controls
                            preload='auto'
                            width="100%"
                            height="100%"
                        />
                    </div>
                </div>
            }
        </Column >
    );
}

export default withRouter(TestResultView);