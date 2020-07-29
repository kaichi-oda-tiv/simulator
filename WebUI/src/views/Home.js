/**
 * Copyright (c) 2019 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

import React from 'react';
import { Row, Cell } from '@enact/ui/Layout';
import Nav from '../components/Nav/Nav.js';
import MapManager from '../components/MapManager/MapManager.js';
import VehicleManager from '../components/VehicleManager/VehicleManager.js';
import ClusterManager from '../components/ClusterManager/ClusterManager.js';
import SimulationManager from '../components/SimulationManager/SimulationManager.js';
import TestResultsManager from '../components/TestResults/TestResultsManager.js';
import TestResultView from '../components/TestResults/TestResultView.js';
import { HashRouter as Router, Route, Switch } from 'react-router-dom';
import { FaCar, FaMap, FaNetworkWired, FaRunning, FaTable } from 'react-icons/fa';
import css from './Home.module.less';
import EventSource from 'eventsource';
import { SimulationProvider } from "../App/SimulationContext.js";

const items = [
	{ name: 'Maps', icon: FaMap },
	{ name: 'Vehicles', icon: FaCar },
	{ name: 'Clusters', icon: FaNetworkWired },
	{ name: 'Test Cases', icon: FaRunning },
	{ name: 'Test Results', icon: FaTable }
];

class Home extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			events: null,
			selected: null,
			subRoute: null,
			redirect: null,
			refSearchParams: null,
			selectedSimulationForTestResults: null
		}
		this.eventSource = new EventSource('/events');
	}

	componentDidMount() {
		this.eventSource.addEventListener('simulation', (e) => this.handleSimEvents(e));
		this.eventSource.addEventListener('MapDownload', (e) => this.handleMapEvents(e));
		this.eventSource.addEventListener('VehicleDownload', (e) => this.handleVehEvents(e));
		this.eventSource.addEventListener('MapDownloadComplete', (e) => this.handleMapEvents(e));
		this.eventSource.addEventListener('VehicleDownloadComplete', (e) => this.handleVehEvents(e));
	}

	handleSimEvents = (e) => this.setState({ simulationEvents: e });
	handleMapEvents = (e) => this.setState({ mapDownloadEvents: e });
	handleVehEvents = (e) => this.setState({ vehicleDownloadEvents: e });

	onSelect = (location, history) => (selected) => {
		const to = '/' + (selected.replace(/ /g, ''));
		if (location.pathname !== to) {
			history.push(to);
		}
		this.setState({ selected });
	}

	onViewAnalytics = (simulation) => {
		this.setState({
			selectedSimulationForTestResults: simulation,
			redirect: 'Test Results'
		});
	}

	onTestResultsManagerLoaded = () => {
		/*
			Cleaning this up for use cases where the TestResultsManager (Test Results List)
			is opened by clicking on the left menu (thus, resetting search and selected simulation).
		*/
		this.setState({
			selectedSimulationForTestResults: null,
			refSearchParams: null
		});
	}

	onViewResult = (testResultId, refSearchParams) => {
		this.setState({
			redirect: 'Test Results',
			subRoute: testResultId,
			refSearchParams: refSearchParams
		});
	}

	onBackButton = (refSearchParams) => {
		/*
			Setting this up for use cases where the TestResultsManager (Test Results List)
			is opened by clicking on the back button inside TestResultView (Test Report).
		*/
		this.setState({
			refSearchParams: refSearchParams,
			redirect: 'Test Results'
		});
	}

	MapManager = () => <MapManager />;
	VehicleManager = () => <VehicleManager />;
	ClusterManager = () => <ClusterManager />;

	SimulationManager = () => <SimulationManager
		onViewAnalytics={this.onViewAnalytics}
	/>;
	TestResultsManager = () => <TestResultsManager
		selectedSimulation={this.state.selectedSimulationForTestResults}
		searchParams={this.state.refSearchParams}
		onTestResultsManagerLoaded={this.onTestResultsManagerLoaded}
		onViewResult={this.onViewResult}
	/>;
	TestResultView = () => <TestResultView
		onBackButton={this.onBackButton}
		refSearchParams={this.state.refSearchParams}
	/>;

	routeRender = ({ location, history }) => {

		let selected = '';
		if (!this.state.redirect) {

			let currentRoute = location.pathname + location.search + location.hash;
			if (currentRoute) {
				currentRoute = currentRoute.toLowerCase();

				let curRoute = currentRoute.replace(/\//g, '');
				for (let i = 0; i < items.length; i++) {
					let pageName = items[i].name;
					if (curRoute === pageName.replace(/ /g, '').toLowerCase()) {
						selected = pageName;
						break;
					}
				}
			}

			if (selected === '') {
				if (currentRoute.indexOf('/testresults/') === 0) {
					selected = 'Test Results';
				} else {
					history.replace('Maps');
					return <></>;
				}
			}
		} else {
			selected = this.state.redirect;

			if (location.pathname !== 'TestResults'
				&& selected === 'Test Results'
				&& !this.state.subRoute) {
				history.replace('/TestResults');
			}

			if (this.state.subRoute) {
				history.replace('/TestResults/' + this.state.subRoute);
			}
			this.setState({ redirect: null, subRoute: null });
		}

		return <Row style={{ height: '100%' }}>
			<Cell size={200}>
				<Nav
					position='side'
					items={items}
					onSelect={this.onSelect(location, history)}
					selected={selected}
				/>
			</Cell>
			<Cell>
				<main>
					<Switch>
						<Route exact path='/' component={this.MapManager} />
						<Route path='/maps' component={this.MapManager} />
						<Route path='/vehicles' component={this.VehicleManager} />
						<Route path='/clusters' component={this.ClusterManager} />
						<Route path='/testcases' component={this.SimulationManager} />
						<Route exact path='/testresults/:testResultId' component={this.TestResultView} />
						<Route path='/testresults' component={this.TestResultsManager} />
						<Route component={this.MapManager} onSelect={this.onDefaultRoute} />
					</Switch>
				</main>
			</Cell>
		</Row>
	}

	render({ ...rest }) {
		const { simulationEvents, mapDownloadEvents, vehicleDownloadEvents } = this.state;
		return <SimulationProvider value={{ simulationEvents, mapDownloadEvents, vehicleDownloadEvents }}>
			<Router {...rest} className={css}>
				<Route render={this.routeRender} />
			</Router>
		</SimulationProvider>
	};
};

export default Home;
