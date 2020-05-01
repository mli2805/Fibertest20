import { GeoPoint } from "../../underlying/geoPoint";

export class AccidentNeighbourDto {
  landmarkIndex: number;
  title: string;
  coors: GeoPoint;
  toRtuOpticalDistanceKm: number;
  toRtuPhysicalDistanceKm: number;
}
