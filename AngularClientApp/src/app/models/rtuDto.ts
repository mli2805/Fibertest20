import { MonitoringMode } from './monitoringMode';

export interface RtuDto {
  rtuId: string;
  monitoringMode: MonitoringMode;
  title: string;
  version: string;
  version2: string;
}
