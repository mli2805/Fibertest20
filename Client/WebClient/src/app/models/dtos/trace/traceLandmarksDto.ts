import { LandmarkDto } from "../landmarkDto";
import { TraceHeaderDto } from "./traceHeaderDto";

export class TraceLandmarksDto {
  public header: TraceHeaderDto;
  public landmarks: LandmarkDto[];
}
