import { RtuMaker } from "../../enums/rtuMaker";
import { OtauPortDto } from "../../underlying/otauPortDto";

export class AttachTraceDto {
  RtuMaker: RtuMaker;
  TraceId: string;
  OtauPortDto: OtauPortDto;
}
