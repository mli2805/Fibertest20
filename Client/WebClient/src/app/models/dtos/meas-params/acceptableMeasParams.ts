export class LeafOfAcceptableMeasParams {
  public resolutions: string[];
  public pulseDurations: string[];
  public periodsToAverage: string[];
  public measCountsToAverage: string[];
}

export class BranchOfAcceptableMeasParams {
  public distances: Map<string, LeafOfAcceptableMeasParams>;
  public backscatteredCoefficient: number;
  public refractiveIndex: number;
}

export class TreeOfAcceptableVeasParams {
  public units: Map<string, BranchOfAcceptableMeasParams>;
}
