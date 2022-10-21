import { RtuMaker } from "../../enums/rtuMaker";
import { OtauPortDto } from "../../underlying/otauPortDto";

export class AttachTraceDto {
  ConnectionId: string;
  RtuMaker: RtuMaker;
  TraceId: string;
  OtauPortDto: OtauPortDto;
  MainOtauPortDto: OtauPortDto;
}
