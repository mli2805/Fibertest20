import { OtauDto } from './otauDto';
import { FiberState } from '../enums/fiberState';

export interface TraceDto {
  traceId: string;
  rtuId: string;
  title: string;

  otauPort: OtauDto;
  isAttached: boolean;
  port: number;

  state: FiberState;

  hasEnoughBaseRefsToPerformMonitoring: boolean;
  isIncludedInMonitoringCycle: boolean;
}
