import { ReturnCode } from "../../enums/returnCode";

export class ClientMeasurementVeexResultDto {
    clientIp: string;
    connectionId: string;
    returnCode: ReturnCode;
    veexMeasurementStatus: string;
}