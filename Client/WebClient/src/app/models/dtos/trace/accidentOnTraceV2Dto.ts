import { FiberState } from "../../enums/fiberState";
import { OpticalAccidentType } from "../../enums/opticalAccidentType";
import { GeoPoint } from "../../underlying/geoPoint";
import { AccidentNeighbourDto } from "./accidentNeighbourDto";

export class AccidentOnTraceV2Dto {
  brokenRftsEventNumber: number;
  accidentSeriosness: FiberState;
  opticalTypeOfAccidnet: OpticalAccidentType;

  isAccidentInOldEvent: boolean;
  isAccidentInLastEvent: boolean;
  accidentCoors: GeoPoint;

  accidentLandmarkIndex: number;
  accidentToRtuOpticalDistanceKm: number;
  accidentTitle: string;
  accidentToRtuPhysicalDistanceKm: number;

  accidentToLeftOpticalDistanceKm: number;
  accidentToLeftPhysicalDistanceKm: number;
  accidentToRightOpticalDistanceKm: number;
  accidentToRightPhysicalDistanceKm: number;

  Left: AccidentNeighbourDto;
  Right: AccidentNeighbourDto;
}
