export enum InteractionsCommandType {
  Nothing,

  RtuInformation = 11,
  RtuOnMap,
  RtuNetworkSettings,
  RtuState,
  RtuLandmarks,
  RtuMonitoringSettings,
  RtuToAutoMode,
  RtuToManualMode,

  TraceInformation = 21,
  TraceOnMap,
  TraceAssingBaseRefs,
  TraceState,
  TraceStatistics,
  TraceLandmarks,
  TraceDetach,
  TraceOutOfTurnMeasurement,
  TraceMeauserementClient,

  PortAttachTrace = 31,
  PortAttachOtau,
  PortMeasurementClient
}
