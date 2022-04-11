import { OtauPortDto } from "../../underlying/otauPortDto";

export class DoClientMeasurementDto {
  connectionId: string;
  clientIp: string;
  rtuId: string;

  selectedMeasParams: MeasParam[];
  veexMeasOtdrParameters : VeexMeasOtdrParameters;

  otdrId: string;
  otauPortDtoList: OtauPortDto[];
  mainOtauPortDto: OtauPortDto;
  otauIp: string;
  otauTcpPort: number;
}

export class VeexMeasOtdrParameters{
  measurementType: string;
  opticalLineProperties: OpticalLineProperties;
  lasers: Laser[];
  distanceRange: string;
  pulseDuration: string;
  resolution: string;
  fastMeasurement: boolean;
  averagingTime: string;
  highFrequencyResolution: boolean;
}

export class OpticalLineProperties{
  kind: string;
  lasersProperties: LasersProperty[];

  constructor(kind: string, lasersProperties: LasersProperty[]){
    this.kind = kind;
    this.lasersProperties = lasersProperties;
  }
}

export class LasersProperty{
  laserUnit: string;
  backscatterCoefficient: number;
  refractiveIndex: number;

  constructor(laserUnit: string, backscatterCoefficient: number, refractiveIndex: number){
    this.laserUnit = laserUnit;
    this.backscatterCoefficient = backscatterCoefficient;
    this.refractiveIndex = refractiveIndex;
  }
}

export class Laser{
  laserUnit: string;

  constructor(laserUnit: string){
    this.laserUnit = laserUnit;
  }
}

export class MeasParam {
  param: number;
  value: number;

  constructor(param: number, value: number) {
    this.param = param;
    this.value = value;
  }
}
