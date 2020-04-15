export class LeafOfAcceptableMeasParams {
  public Resolutions: string[];
  public PulseDurations: string[];
  public PeriodsToAverage: string[];
  public MeasCountsToAverage: string[];
}

export class BranchOfAcceptableMeasParams {
  public Distances: object;
  public BackscatteredCoefficient: number;
  public RefractiveIndex: number;
}

export class TreeOfAcceptableVeasParams {
  public Units: object;
}
