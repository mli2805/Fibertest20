import { EquipmentType } from "../enums/equipmentType";
import { GeoPoint } from "../underlying/geoPoint";

export class LandmarkDto {
  public ordinal: number;
  public nodeTitle: string;
  public eqType: EquipmentType;
  public equipmentTitle: string;
  public distanceKm: number;
  public eventOrdinal: number; // -1 if there is no related event
  public coors: GeoPoint;
}
