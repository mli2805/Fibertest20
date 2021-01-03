export class GeoPoint {
  latitude: number;
  longitude: number;

  public toDetailedString(mode: number): string {
    switch (mode) {
      case 0:
        return `${this.latitude.toFixed(6)}\xB0  ${this.longitude.toFixed(
          6
        )}\xB0`;
      case 1: {
        const dLat = Math.trunc(this.latitude);
        const mLat = (this.latitude - dLat) * 60;

        const dLng = Math.trunc(this.longitude);
        const mLng = (this.longitude - dLng) * 60;
        return `${dLat}\xB0${mLat.toFixed(6)}  ${dLng}\xB0${mLng.toFixed(6)}`;
      }
      case 2: {
        const dLat = Math.trunc(this.latitude);
        const mLat = (this.latitude - dLat) * 60;
        const miLat = Math.trunc(mLat);
        const sLat = (mLat - miLat) * 60;

        const dLng = Math.trunc(this.longitude);
        const mLng = (this.longitude - dLng) * 60;
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
