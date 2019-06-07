import React from "react";

export const SimulationContext = React.createContext({
    simulationEvents: null,
    mapDownloadEvents: null
});

export const SimulationProvider = SimulationContext.Provider;
export const SimulationConsumer = SimulationContext.Consumer;