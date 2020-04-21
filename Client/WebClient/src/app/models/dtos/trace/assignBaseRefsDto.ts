import { RtuMaker } from "../../enums/rtuMaker";
import { OtauPortDto } from "../../underlying/otauPortDto";
import { BaseRefDto } from "../../underlying/baseRefDto";

export class AssignBaseReftsDto {
  username: string;
  clientIp: string;
  rtuId: string;
  rtuMaker: RtuMaker;
  otdrId: string; //  in VeEX RTU main OTDR has its own ID
  traceId: string;
  otauPortDto: OtauPortDto; // could be null if trace isn't attached to port yet
  baseRefs: BaseRefDto[];
  deleteOldSorFields: number[];
}
