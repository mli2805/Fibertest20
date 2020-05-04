import { GeoPoint } from "../../underlying/geoPoint";

export class AccidentLineDto {
  caption: string;
  topLeft: string;
  topCenter: string;
  topRight: string;
  bottom0: string;
  bottom1: string;
  bottom2: string;
  bottom3: string;
  bottom4: string;
  pngPath: string; // url

  position: GeoPoint;
}
