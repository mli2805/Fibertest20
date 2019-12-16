export enum FiberState {
  Nothing = -1,
  //
  NotInTrace = 0x0,
  NotJoined = 0x1,
  //
  Unknown = 0x2, // there were no measurements for this trace yet
  NotInZone = 0x3,
  //
  Ok = 0x4,
  Suspicion = 0x5,
  Minor = 0x6,
  Major = 0x7,
  Critical = 0x8,
  User = 0x9,
  FiberBreak = 0xa,
  NoFiber = 0xb,
  //
  HighLighted = 0xe,
  DistanceMeasurement = 0xf
}
