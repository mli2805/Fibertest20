import { RtuMaker } from "../../enums/rtuMaker";

export class AssignBaseParamsDto {
  rtuTitle: string;
  rtuMaker: RtuMaker;
  otdrId: string;
  preciseId: string;
  fastId: string;
  additionalId: string;
}
