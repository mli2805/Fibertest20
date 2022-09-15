import { OtauPortDto } from "../../underlying/otauPortDto";

export class DoClientMeasurementDto {
  connectionId: string;
  clientIp: string;
  rtuId: string;

  selectedMeasParams: MeasParam[];
  veexMeasOtdrParameters : VeexMeasOtdrParameters;
  analysisParameters : AnalysisParameters

  otauPortDto: OtauPortDto[];
  otdrId: string;

  isForAutoBase : boolean;
  IsAutoLmax : boolean;
  KeepOtdrConnection : boolean
}

export class AnalysisParameters{
  macrobendThreshold : number;
  findOnlyFirstAndLastEvents : boolean;
  setUpIitEvents : boolean;
  lasersParameters : LasersParameter[];
}

export class LasersParameter{
  eventLossThreshold : number;
  eventReflectanceThreshold : number;
  endOfFiberThreshold : number;
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
  position: number;

  constructor(param: number, position: number) {
    this.param = param;
    this.position = position;
  }
}
