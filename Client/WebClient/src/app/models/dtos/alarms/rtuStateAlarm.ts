import { StateAccidentDto } from "../stateAccidentDto";
import { AlarmIndicator, IAlarm } from "./alarm";

export class RtuStateAlarm implements IAlarm {
    id: number;
    isMeasurementProblem: boolean;
    rtuId: string;
    traceId: string;
    hasBeenSeen: boolean;
}

export class RtuStateAlarmIndicator extends AlarmIndicator {
    public RtuStateAccidentReceived(signal: StateAccidentDto): string {
        const alarmsJson = sessionStorage.getItem(this.inStorageName);

        //TODO
        
        return this.GetIndicator();
    }
}