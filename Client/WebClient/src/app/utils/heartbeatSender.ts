import { OneApiService } from "../api/one.service";
import { ReturnCode } from "../models/enums/returnCode";
import { RequestAnswer } from "../models/underlying/requestAnswer";
import { Utils } from "./utils";

export class HeartbeatSender {
  static badHeartbeatCount: number;

  static async Send(oneApiService: OneApiService): Promise<boolean> {
    // console.log(`Heartbeat timer tick at ${Utils.stime()}`);
    try {
      const user = sessionStorage.getItem("currentUser");
      if (user === null) {
        console.log("user has not logged yet");
        return true;
      } else {
        const currentUser = JSON.parse(sessionStorage.currentUser);
        const settings = JSON.parse(sessionStorage.settings);

        const res = (await oneApiService
          .getRequest(`authentication/heartbeat/${currentUser.connectionId}`)
          .toPromise()) as RequestAnswer;

        if (res.returnCode >= 2000 && res.returnCode < 3000) {
          console.log(
            `Heartbeat network connection failed at ${Utils.stime()}. Return code is ${
              res.returnCode
            }`
          );
          this.badHeartbeatCount++;
          if (this.badHeartbeatCount > 3) {
            return false;
          }
          return true; // problem, but give it a chance
        } else if (res.returnCode !== ReturnCode.Ok) {
          console.log(`Heartbeat: ${res.errorMessage} at ${Utils.stime()}`);
          return false;
        } else {
          console.log(`Heartbeat result is OK`);
          this.badHeartbeatCount = 0;
          return true;
        }
      }
    } catch (error) {
      console.log(`can't send heartbeat: ${error.message}`);
      this.badHeartbeatCount++;
      if (this.badHeartbeatCount > 3) {
        return false;
      }
    }
  }
}
