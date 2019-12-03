import { Pipe, PipeTransform } from "@angular/core";
import { TraceDto } from "../models/dtos/traceDto";
import { MonitoringMode } from "../models/enums/monitoringMode";

@Pipe({
  name: "traceMonitoringModeUri"
})
export class TraceMonitoringModeUriPipe implements PipeTransform {
  transform(value: TraceDto): string {
    if (!value.hasEnoughBaseRefsToPerformMonitoring) {
      return "./assets/images/EmptySquare.png";
    } else if (!value.isIncludedInMonitoringCycle) {
      return "./assets/images/GreyHalfSquare.png";
    } else if (value.rtuMonitoringMode !== MonitoringMode.On) {
      return "./assets/images/GreySquare.png";
    } else {
      return "./assets/images/BlueSquare.png";
    }

    return "./assets/images/EmptySquare.png";
  }
}
