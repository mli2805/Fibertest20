import { BaseRefType } from "../enums/baseRefType";
import { ReturnCode } from "../enums/returnCode";

export class StateAccidentDto {
    id: number;
    isMeasurementProblem: boolean;
    returnCode: ReturnCode;

    eventRegistrationTimestamp: Date;
    rtuTitle: string;
    traceTitle: string;
    baseRefType: BaseRefType;
  
    comment: string;
}

export class StateAccidentRequestDto {
    fullCount: number;
    accidentPortion: StateAccidentDto[]; 
}