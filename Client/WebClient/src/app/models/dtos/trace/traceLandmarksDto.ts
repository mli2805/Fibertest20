import { LandmarkDto } from "../landmarkDto";
import { TraceHeaderDto } from "./traceHeaderDto";

export class TraceLandmarksDto {
  public header: TraceHeaderDto = new TraceHeaderDto();
  public landmarks: LandmarkDto[];
}
