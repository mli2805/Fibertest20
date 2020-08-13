export class RftsEventsDto {
  levelArray: RftsLevelDto[];
  totalFiberLoss: TotalFiberLossDto;
  footer: RftsEventsFooterDto;
}

export class RftsLevelDto {
  title: string;
}

export class TotalFiberLossDto {
  value: string;
  threshold: string;
  deviation: string;
  state: string;
}

export class RftsEventsFooterDto {
  state: string;
  orl: string;
  levelStates: LevelState[];
}

export class LevelState {
  levelTitle: string;
  state: string;
}
