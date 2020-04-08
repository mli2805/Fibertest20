import { Injectable } from "@angular/core";
import { RtuDto } from "../models/dtos/rtuTree/rtuDto";

@Injectable()
export class FtDetachedTracesProvider {
  public data: RtuDto;
  public constructor() {}
}
