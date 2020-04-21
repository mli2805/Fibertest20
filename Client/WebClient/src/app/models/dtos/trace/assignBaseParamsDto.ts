import { RtuMaker } from "../../enums/rtuMaker";

export class AssignBaseParamsDto {
  rtuTitle: string;
  rtuMaker: RtuMaker;
  hasPrecise: boolean;
  hasFast: boolean;
  hasAdditional: boolean;
}
