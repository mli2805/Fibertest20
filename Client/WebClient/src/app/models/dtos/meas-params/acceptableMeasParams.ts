export class LeafOfAcceptableMeasParams {
  public Resolutions: string[];
  public PulseDurations: string[];
  public PeriodsToAverage: string[];
  public MeasCountsToAverage: string[];
}

export class BranchOfAcceptableMeasParams {
  public Distances: Map<number, LeafOfAcceptableMeasParams>;
  public BackscatteredCoefficient: number;
  public RefractiveIndex: number;
}

export class TreeOfAcceptableVeasParams {
  public Units: Map<string, BranchOfAcceptableMeasParams>;
}
