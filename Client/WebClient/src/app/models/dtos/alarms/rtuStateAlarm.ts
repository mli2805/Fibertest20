import { ReturnCode } from "../../enums/returnCode";
import { StateAccidentDto } from "../stateAccidentDto";
import { AlarmIndicator, IAlarm } from "./alarm";

export class RtuStateAlarm implements IAlarm {
    id: number;
    returnCode: ReturnCode;
    isMeasurementProblem: boolean;
    rtuId: string;
    traceId: string;
    hasBeenSeen: boolean;
}

export class RtuStateAlarmIndicator extends AlarmIndicator {
    public RtuStateAccidentReceived(signal: StateAccidentDto): string {
        const alarmsJson = sessionStorage.getItem(this.inStorageName);
        this.list = JSON.parse(alarmsJson) as RtuStateAlarm[];

        const alarm = signal.isMeasurementProblem 
            ? this.list.find((a) => a.isMeasurementProblem && a.traceId === signal.traceId)
            : this.list.find((a) => !a.isMeasurementProblem && a.rtuId === signal.rtuId);

        const goodCode = signal.isMeasurementProblem ? ReturnCode.MeasurementEndedNormally : ReturnCode.RtuManagerServiceWorking;

        if (alarm === undefined && signal.returnCode !== goodCode){
            this.list.push(this.createNewAlarm(signal));
        } else {
            if (signal.returnCode === goodCode){
                const index = this.list.indexOf(alarm);
                this.list.splice(index, 1);
            } else {
                alarm.id = signal.id;
                alarm.hasBeenSeen = false;
            }
        }

        sessionStorage.setItem(this.inStorageName, JSON.stringify(this.list));
        return this.GetIndicator();
    }

    createNewAlarm(signal: StateAccidentDto){
        console.log(`createNewAlarm (rtu state) from signal ${signal}`)
        const newAlarm = new RtuStateAlarm();
        newAlarm.id = signal.id;
        newAlarm.isMeasurementProblem = signal.isMeasurementProblem;
        newAlarm.traceId = signal.traceId;
        newAlarm.hasBeenSeen = false;
        return newAlarm;
    }
}