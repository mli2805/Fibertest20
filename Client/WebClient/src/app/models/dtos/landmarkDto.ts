import { EquipmentType } from "../enums/equipmentType";
import { GeoPoint } from "../underlying/geoPoint";

export class LandmarkDto {
  public ordinal: number;
  public nodeTitle: string;
  public equipmentType: EquipmentType;
  public equipmentTitle: string;
  public cableReserves: string;
  public gpsDistance: string;
  public gpsSection: string;
  public isUserInput: boolean;
  public opticalDistance: string;
  public opticalSection: string;
  public eventOrdinal: number; // -1 if there is no related event
  public coors: GeoPoint;
}
