import { Injectable } from "@angular/core";
import { RtuDto } from "../models/dtos/rtuTree/rtuDto";
import { OtauPortDto } from "../models/underlying/otauPortDto";

@Injectable()
export class FtDetachedTracesProvider {
  public selectedRtu: RtuDto;
  public selectedPort: OtauPortDto;
  public data: object;
  public constructor() {}
}
