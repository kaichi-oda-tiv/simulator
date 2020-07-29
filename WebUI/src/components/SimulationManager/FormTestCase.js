/**
 * Copyright (c) 2019 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

import React, { useContext, useEffect, useState } from 'react'
import appCss from '../../App/App.module.less';
import Checkbox from "../Checkbox/Checkbox";
import SingleSelect from '../Select/SingleSelect';
import { SimulationContext } from "../../App/SimulationContext";

function FormTestCase() {
    const [simulation, setSimulation] = useContext(SimulationContext);

    const {
        testCaseMode,
        generateResults,
        testCaseReportName,
        runtimeTemplateType,
        testCaseFile,
        testCaseBridge
    } = simulation;

    const [testCaseFileList, setTestCaseFileList] = useState([]);

    const testCaseRuntimeTypesList = [
        { name: 'Python API', value: 'pythonAPI' },
        { name: 'Scenic', value: 'scenic' }
    ];

    const pythonScriptsList = [
        { name: 'Cut In', value: 'Python/SampleTestCases/cut-in.py' },
        { name: 'Pedestrian Crossing', value: 'Python/SampleTestCases/ped-crossing.py' },
        { name: 'Red Light Runner', value: 'Python/SampleTestCases/red-light-runner.py' },
        { name: 'Sudden Braking', value: 'Python/SampleTestCases/sudden-braking.py' }
    ];

    const scenicScriptsList = [
        { name: 'Cut In SF', value: 'Scenic/cut-in/scenic-cut-in-sf.sc' },
        { name: 'Cut In', value: 'Scenic/cut-in/scenic-cut-in.sc' },
        { name: 'Random', value: 'Scenic/random-placement/scenic-example.sc' },
        { name: 'Scenario 2', value: 'Scenic/scenario_2_1/scenario_2_1_Apollo.sc' },
        { name: 'Borregas', value: 'Scenic/borregas-intersection/scenic-borregas.sc' }
    ];

    function changeGenerateResults() {
        setSimulation((prev) => ({
            ...simulation,
            generateResults: !prev.generateResults
        }))
    }

    function changeTestCaseReportName(ev) {
        const newTestCaseReportName = ev.currentTarget.value;
        setSimulation({
            ...simulation,
            testCaseReportName: newTestCaseReportName
        });
    }

    const setRuntimeTemplateType = (rtt) => {
        setSimulation({
            ...simulation,
            runtimeTemplateType: rtt
        });
        if (rtt === 'pythonAPI') {
            setTestCaseFileList(pythonScriptsList);
        } else if (rtt === 'scenic') {
            setTestCaseFileList(scenicScriptsList);
        }
    };

    function changeRuntimeTemplateType(ev) {
        const newRuntimeTemplateType = ev.currentTarget.value;
        setRuntimeTemplateType(newRuntimeTemplateType);
    }

    function changeTestCaseFile(ev) {
        const newTestCaseFile = ev.currentTarget.value;
        setSimulation({
            ...simulation,
            testCaseFile: newTestCaseFile
        });
    }

    function changeTestCaseBridge(ev) {
        const testCaseBridgeIP = ev.currentTarget.value;
        setSimulation({
            ...simulation,
            testCaseBridge: testCaseBridgeIP
        });
    }

    useEffect(() => {

        if (generateResults === undefined) {
            //Calling the set function twice since this is like double bang (!!) of the 'undefined' value.
            changeGenerateResults();
            changeGenerateResults();
        }

        //We are not going to set a default value here because this will make the user select a value without his knowledge (since this is in a different tab).
        /*
        if (!runtimeTemplateType) {
            setRuntimeTemplateType('pythonAPI');
        } else {
        */
        setRuntimeTemplateType(runtimeTemplateType);
        /* } */
    }, []);

    return (
        <div className={appCss.formCard}>
            <h4 className={appCss.inputLabel}>Generate Analytics</h4>
            <p className={appCss.inputDescription}>
                Every individual run of the Test Case will
                generate individual test results.
            </p>
            <Checkbox
                checked={generateResults}
                label="Generate results for each run"
                name={"generateResults"}
                onChange={changeGenerateResults}
            />
            {generateResults &&
                <div>
                    <input
                        type="text"
                        name="testCaseReportName"
                        required
                        defaultValue={testCaseReportName}
                        onChange={changeTestCaseReportName}
                        placeholder="enter test case report name"
                    />
                </div>
            }
            <br />
            <br />
            <h4 className={appCss.inputLabel}>Test Case Runtime</h4>
            <p className={appCss.inputDescription}>
                The runtime template that is used to run the test case.
            </p>
            <SingleSelect
                data-for="runtimeTemplateType"
                placeholder="select a runtime type"
                defaultValue={runtimeTemplateType ? runtimeTemplateType : 'DEFAULT'}
                onChange={changeRuntimeTemplateType}
                options={testCaseRuntimeTypesList}
                label="name"
                value="value"
                disabled={!testCaseMode}
            />
            {!!runtimeTemplateType &&
                <SingleSelect
                    data-for="testCaseFile"
                    placeholder="select a test case file"
                    defaultValue={testCaseFile ? testCaseFile : 'DEFAULT'}
                    onChange={changeTestCaseFile}
                    options={testCaseFileList}
                    label="name"
                    value="value"
                    disabled={!testCaseMode}
                />
            }
            <br />
            <br />
            <h4 className={appCss.inputLabel}>Bridge Connection String</h4>
            <div>
                <input
                    type="text"
                    name="bridge"
                    defaultValue={testCaseBridge}
                    onChange={changeTestCaseBridge}
                    placeholder="enter bridge ip"
                    disabled={!testCaseMode}
                />
            </div>
        </div>)
}

export default FormTestCase;