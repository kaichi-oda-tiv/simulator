/**
 * Copyright (c) 2019 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

import React, { useContext, useState } from 'react'
import appCss from '../../App/App.module.less';
import css from './SimulationManager.module.less';
import { SimulationContext } from "../../App/SimulationContext";
import DatePicker from 'react-datepicker';
import "react-datepicker/dist/react-datepicker.css";

function FormWeather() {
    const [simulation, setSimulation] = useContext(SimulationContext);
    const { weather, apiOnly, testCaseMode } = simulation;
    let { rain, wetness, fog, cloudiness } = weather || {};
    const [formWarning, setFormWarning] = useState('');

    function validNumberInput(val, min, max, ) {
        return !isNaN(val) && val >= min && val <= max;
    }

    function changeTimeOfDay(datetime) {
        const timestamp = datetime.valueOf() - datetime.getTimezoneOffset() * 60 * 1000;
        const adjusted = new Date(timestamp);
        setSimulation({ ...simulation, timeOfDay: adjusted.toISOString() });
    };

    function adjustTime(datetime) {
        let datetimeDate;

        if (!datetime) {
            let today = new Date();
            /* Setting the default time to noon. */
            today.setHours(12, 0, 0);

            datetimeDate = today;

            changeTimeOfDay(datetimeDate);
        } else {
            datetimeDate = new Date(datetime);
            /*
                We need to add the timezone offfset here since when the incoming value is converted
                into a Date object will result in the value being converted to the current timezone.
            */
            const timestamp = datetimeDate.valueOf() + datetimeDate.getTimezoneOffset() * 60 * 1000;
            datetimeDate = new Date(timestamp);
        }
        return datetimeDate;
    }

    function changeCloudiness(ev) {
        const value = ev.target.value;
        if (!validNumberInput(value, 0, 1)) {
            setFormWarning(`Please put number between ${0} and ${1} for cloudiness.`);
        } else {
            setFormWarning('');
        }
        setSimulation({ ...simulation, weather: { ...weather, cloudiness: parseFloat(value) } });
    }

    function changeRain(ev) {
        const value = ev.target.value;
        if (!validNumberInput(value, 0, 1)) {
            setFormWarning(`Please put number between ${0} and ${1} for rain.`);
        } else {
            setFormWarning('');
        }
        setSimulation({ ...simulation, weather: { ...weather, rain: parseFloat(value) } });
    }

    function changeWetness(ev) {
        const value = ev.target.value;
        if (!validNumberInput(value, 0, 1)) {
            setFormWarning(`Please put number between ${0} and ${1} for wetness.`);
        } else {
            setFormWarning('');
        }
        setSimulation({ ...simulation, weather: { ...weather, wetness: parseFloat(value) } });
    }

    function changeFog(ev) {
        const value = ev.target.value;
        if (!validNumberInput(value, 0, 1)) {
            setFormWarning(`Please put number between ${0} and ${1} for fog.`);
        } else {
            setFormWarning('');
        }
        setSimulation({ ...simulation, weather: { ...weather, fog: parseFloat(value) } });
    }

    return (
        <div className={appCss.formCard}>
            <h4 className={appCss.inputLabel}>
                Day &amp; Time of day
            </h4>
            <p className={appCss.inputDescription}>
                Set the date and time of day during simulation.
            </p>
            <div>
                <DatePicker
                    selected={adjustTime(simulation.timeOfDay)}
                    onChange={changeTimeOfDay}
                    showTimeSelect
                    timeIntervals={30}
                    dateFormat="MM/dd/yyyy HH:mm a"
                    timeCaption="Time"
                    disabled={apiOnly || testCaseMode}
                />
            </div>
            <br />
            <div className={css.weatherInput}>
                <h4 className={appCss.inputLabel}>
                    Rain
                </h4>
                <p className={appCss.inputDescription}>
                    Raining introduces particle droplet effects falling from the sky and camera post post-processing effects.
                </p>
                <input
                    type="number"
                    name="rain"
                    defaultValue={rain || 0}
                    onChange={changeRain}
                    min="0"
                    max="1"
                    step="0.01"
                    placeholder="rain"
                    disabled={apiOnly || testCaseMode}
                />
            </div>
            <div className={css.weatherInput}>
                <h4 className={appCss.inputLabel}>
                    Wetness
                </h4>
                <p className={appCss.inputDescription}>
                    Wetness covers the road and sidewalks with water.
                </p>
                <input
                    type="number"
                    name="wetness"
                    defaultValue={wetness || 0}
                    onChange={changeWetness}
                    min="0"
                    max="1"
                    step="0.01"
                    placeholder="wetness"
                    disabled={apiOnly || testCaseMode}
                />
            </div>
            <br />
            <br />
            <div className={css.weatherInput}>
                <h4 className={appCss.inputLabel}>
                    Fog
                </h4>
                <p className={appCss.inputDescription}>
                    Defines amount of fog and other particles in the air.
                </p>
                <input
                    type="number"
                    name="fog"
                    defaultValue={fog || 0}
                    onChange={changeFog}
                    min="0"
                    max="1"
                    step="0.01"
                    placeholder="fog"
                    disabled={apiOnly || testCaseMode}
                />
            </div>
            <div className={css.weatherInput}>
                <h4 className={appCss.inputLabel}>
                    Cloudiness
                </h4>
                <p className={appCss.inputDescription}>
                    Defines amount of clouds during simulation.
                </p>
                <input
                    type="number"
                    name="cloudiness"
                    defaultValue={cloudiness || 0}
                    onChange={changeCloudiness}
                    min="0"
                    max="1"
                    step="0.01"
                    placeholder="cloudiness"
                    disabled={apiOnly || testCaseMode}
                />
            </div>
            <span className={appCss.formWarning}>{formWarning}</span>
        </div>)
}

export default FormWeather;