import { Pipe, PipeTransform } from "@angular/core";
import { GeoPoint } from "../models/underlying/geoPoint";

@Pipe({
  name: "gpsCoordinatesPipe",
})
export class GpsCoordinatesPipe implements PipeTransform {
  constructor() {}
  transform(value: GeoPoint, mode: number) {
    switch (mode) {
      case 0:
        return `${value.latitude.toFixed(6)}\xB0  ${value.longitude.toFixed(
          6
        )}\xB0`;
      case 1: {
        const dLat = Math.trunc(value.latitude);
        const mLat = (value.latitude - dLat) * 60;

        const dLng = Math.trunc(value.longitude);
        const mLng = (value.longitude - dLng) * 60;
        return `${dLat}\xB0${mLat.toFixed(6)}  ${dLng}\xB0${mLng.toFixed(6)}`;
      }
      case 2: {
        const dLat = Math.trunc(value.latitude);
        const mLat = (value.latitude - dLat) * 60;
        const miLat = Math.trunc(mLat);
        const sLat = (mLat - miLat) * 60;

        const dLng = Math.trunc(value.longitude);
        const mLng = (value.longitude - dLng) * 60;
        const miLng = Math.trunc(mLng);
        const sLng = (mLng - miLng) * 60;
        return `${dLat}\xB0${mLat}\xB1${sLat.toFixed(
          6
        )} ${dLng}\xB0${mLng}\xB2${sLng.toFixed(6)}`;
      }
      default:
        return "";
    }
  }
}
