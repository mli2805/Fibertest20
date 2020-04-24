import { RtuMaker } from "../../enums/rtuMaker";
import { OtauPortDto } from "../../underlying/otauPortDto";
import { BaseRefFile } from "../../underlying/baseRefFile";

export class AssignBaseRefDtoWithFiles {
  rtuId: string;
  rtuMaker: RtuMaker;
  otdrId: string; //  in VeEX RTU main OTDR has its own ID
  traceId: string;
  otauPortDto: OtauPortDto; // could be null if trace isn't attached to port yet
  baseRefs: BaseRefFile[];
}
