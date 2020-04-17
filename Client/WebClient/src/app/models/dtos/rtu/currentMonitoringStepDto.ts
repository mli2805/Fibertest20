import { BaseRefType } from "../../enums/baseRefType";
import { PortWithTraceDto } from "../../underlying/portWithTraceDto";

export class CurrentMonitoringStepDto {
  rtuId: string;
  step: MonitoringCurrentStep;
  portWithTraceDto: PortWithTraceDto;
  baseRefType: BaseRefType;
}

export enum MonitoringCurrentStep {
  Idle,
  Toggle,
  Measure,
  Interrupted,
  Analysis,
}
