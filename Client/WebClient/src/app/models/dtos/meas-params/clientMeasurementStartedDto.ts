import { ReturnCode } from "../../enums/returnCode";
import { OtauPortDto } from "../../underlying/otauPortDto";

export class ClientMeasurementStartedDto{
    returnCode: ReturnCode;
    errorMessage: string;
    clientMeasurementId: string;
    traceId: string;
    otauPortDto: OtauPortDto;
}