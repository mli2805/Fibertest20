import { ReturnCode } from "../../enums/returnCode";

export class RftsEventsDto {
  returnCode: ReturnCode;
  errorMessage: string;

  isNoFiber: boolean;
  levelArray: RftsLevelDto[] = [];
  summary: RftsEventsSummaryDto;

  toString() {
    return `hello!`;
  }
}

export class RftsLevelDto {
  title: string;
  isFailed: boolean;
  firstProblemLocation: string;
  eventArray: RftsEventDto[] = [];
  totalFiberLoss: TotalFiberLossDto;
}

export class RftsEventDto {
  ordinal: number;
  isNew: boolean;
  isFailed: boolean;

  landmarkTitle: string;
  landmarkType: string;
  state: string;
  damageType: string;
  distanceKm: string;
  enabled: string;
  eventType: string;

  reflectanceCoeff: string;
  attenuationInClosure: string;
  attenuationCoeff: string;

  reflectanceCoeffThreshold: MonitoringThreshold;
  attenuationInClosureThreshold: MonitoringThreshold;
  attenuationCoeffThreshold: MonitoringThreshold;

  reflectanceCoeffDeviation: string;
  attenuationInClosureDeviation: string;
  attenuationCoeffDeviation: string;
}

export class MonitoringThreshold {
  value: number;
  isAbsolute: boolean;
}

export class TotalFiberLossDto {
  value: number;
  threshold: MonitoringThreshold;
  deviation: number;
  isPassed: boolean;
}

export class RftsEventsSummaryDto {
  traceState: string;
  orl: number;
  levelStates: LevelState[] = [];
}

export class LevelState {
  levelTitle: string;
  state: string;
}
