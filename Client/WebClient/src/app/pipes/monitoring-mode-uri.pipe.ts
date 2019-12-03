import { Pipe, PipeTransform } from "@angular/core";
import { MonitoringMode } from "../models/enums/monitoringMode";

@Pipe({
  name: "monitoringModeToUriPipe"
})
export class MonitoringModeToUriPipe implements PipeTransform {
  constructor() {}

  transform(value: MonitoringMode): string {
    switch (value) {
      case MonitoringMode.Off:
        return "./assets/images/GreySquare.png";
      case MonitoringMode.On:
        return "./assets/images/BlueSquare.png";
      case MonitoringMode.Unknown:
        return "./assets/images/EmptySquare.png";
    }
  }
}
