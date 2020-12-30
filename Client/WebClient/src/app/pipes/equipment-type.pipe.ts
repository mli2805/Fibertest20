import { Pipe, PipeTransform } from "@angular/core";
import { TranslateService } from "@ngx-translate/core";
import { EquipmentType } from "../models/enums/equipmentType";

@Pipe({
  name: "equipmentTypeToLocalizedStringPipe",
})
export class EquipmentTypePipe implements PipeTransform {
  constructor(private ts: TranslateService) {}

  transform(value: EquipmentType): string {
    switch (value) {
      case EquipmentType.AdjustmentPoint:
        return this.ts.instant("SID_Adjustment_point");
      case EquipmentType.EmptyNode:
        // return this.ts.instant("SID_Node_without_equipment");
        return this.ts.instant("SID_Node");
      case EquipmentType.CableReserve:
        return this.ts.instant("SID_CableReserve");
      case EquipmentType.Other:
        return this.ts.instant("SID_Other");
      case EquipmentType.Closure:
        return this.ts.instant("SID_Closure");
      case EquipmentType.Cross:
        return this.ts.instant("SID_Cross");
      case EquipmentType.Well:
        return this.ts.instant("SID_Well");
      case EquipmentType.Terminal:
        return this.ts.instant("SID_Terminal");

      case EquipmentType.Rtu:
        return this.ts.instant("SID_Rtu");
    }
  }
}
