/**
 * Copyright (c) 2019 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

import React, { useState, useEffect, useContext } from "react";
import SingleSelect from "../Select/SingleSelect";
import Alert from "../Alert/Alert";
import Checkbox from "../Checkbox/Checkbox";
import appCss from "../../App/App.module.less";
import { getList } from "../../APIs.js";
import { IoIosClose } from "react-icons/io";
import { SimulationContext } from "../../App/SimulationContext";
import axios from "axios";

function FormGeneral() {
  const [clusterList, setClusterList] = useState();
  const [alert, setAlert] = useState({ status: false });
  const [simulation, setSimulation] = useContext(SimulationContext);

  const {
    name,
    cluster,
    apiOnly,
    testCaseMode,
    headless,
    interactive,
  } = simulation;

  const modeList = [
    { name: 'Random', value: 'random' },
    { name: 'API Only', value: 'apiOnly' },
    { name: 'Test Case', value: 'testCaseMode' }
  ];

  function changeName(ev) {
    setSimulation({ ...simulation, name: ev.target.value });
  }

  const getDefaultValueForMode = () => {
    if (apiOnly && !testCaseMode) {
      return modeList[1].value;
    } else if (apiOnly && testCaseMode) {
      return modeList[2].value;
    } else {
      return modeList[0].value;
    }
  };

  function changeMode(ev) {
    const val = ev.currentTarget.value;

    if (val === 'apiOnly') {
      setSimulation({
        ...simulation,
        apiOnly: true,
        testCaseMode: false
      });
    } else if (val === 'testCaseMode') {
      setSimulation({
        ...simulation,
        apiOnly: true,
        testCaseMode: true
      });
    } else {
      setSimulation({
        ...simulation,
        apiOnly: false,
        testCaseMode: false
      });
    }
  }

  function changeHeadless() {
    setSimulation((prev) => ({ ...simulation, headless: !prev.headless }));
  }

  function changeCluster(ev) {
    setSimulation({ ...simulation, cluster: parseInt(ev.target.value) });
  };

  let source = axios.CancelToken.source();
  let unmounted;
  useEffect(() => {

    if (simulation.testCaseMode !== true) {
      setSimulation({ ...simulation, testCaseMode: false });
    }

    unmounted = false;
    const fetchData = async () => {
      setAlert({ status: false });
      const result = await getList("clusters", source.token);

      if (unmounted) {
        return;
      }

      if (result.status === 200) {
        setClusterList(result.data);
        if (!result.data.filter((c) => c.id === cluster).length) {
          // Reset cluster to default
          setSimulation({ ...simulation, cluster: 0 });
        }

        if (!simulation.timeOfDay) {
          let today = new Date();
          /* Setting the default time to noon. */
          today.setHours(12, 0, 0);
          const timestamp = today.valueOf() - today.getTimezoneOffset() * 60 * 1000;
          const adjusted = new Date(timestamp);
          setSimulation({ ...simulation, timeOfDay: adjusted.toISOString() });
        }

      } else {
        let alertMsg;
        if (result.name === "Error") {
          alertMsg = result.message;
        } else {
          alertMsg = `${result.statusText}: ${result.data.error}`;
        }
        setAlert({ status: true, type: "error", message: alertMsg });
      }
    };

    fetchData();

    return () => {
      unmounted = true;
      source.cancel("Cancelling in cleanup.");
    };
  }, []);

  function alertHide() {
    setAlert({ status: false });
  }

  return (
    <div className={appCss.formCard}>
      {alert.status && (
        <Alert type={alert.alertType} msg={alert.alertMsg}>
          <IoIosClose onClick={alertHide} />
        </Alert>
      )}
      <h4 className={appCss.inputLabel}>Test Case Name</h4>
      <input
        required
        name="name"
        type="text"
        defaultValue={name}
        placeholder="name"
        onChange={changeName}
        style={{ marginBottom: '0.350rem' }}
      />
      <br />
      <br />
      <h4 className={appCss.inputLabel}>Select Cluster</h4>
      <p className={appCss.inputDescription}>
        Select cluster to run distributed test case on several machines.
      </p>
      <SingleSelect
        data-for="cluster"
        placeholder="select a cluster"
        defaultValue={cluster}
        onChange={changeCluster}
        options={clusterList}
        label="name"
        value="id"
        style={{ marginBottom: '0.350rem' }}
      />
      <br />
      <br />
      <h4 className={appCss.inputLabel}>Mode</h4>
      <p className={appCss.inputDescription}>
        In 'API Only' and 'Test Case' mode Map, Ego Vehicle and other parameters
        are defined using Python API, and thus the various settings are disabled here.
      </p>
      <SingleSelect
        data-for="mode"
        placeholder="select a mode"
        defaultValue={getDefaultValueForMode()}
        onChange={changeMode}
        options={modeList}
        label="name"
        value="value"
        style={{ marginBottom: '0.350rem' }}
      />
      <br />
      <br />
      <h4 className={appCss.inputLabel}>No-Render Mode</h4>
      <p className={appCss.inputDescription}>
        In No-Render Mode (option disabled for "Random->Map&amp;Vehicle->Interactive Mode(checked)"), the main view is not rendered.
        That is, you will not be able to view the test case in the Simulator window. Use this mode to
        optimize test case performance when interaction is not needed.
      </p>
      <Checkbox
        checked={headless}
        label="Run test case in No-Render Mode"
        name={"headless"}
        disabled={!apiOnly && !testCaseMode && interactive}
        onChange={changeHeadless}
        style={{ marginBottom: '0.350rem' }}
      />
    </div >
  );
}

export default FormGeneral;
