import { OneApiService } from "../api/one.service";
import { ReturnCode } from "../models/enums/returnCode";
import { RequestAnswer } from "../models/underlying/requestAnswer";
import { Utils } from "./utils";

export class HeartbeatSender {
  static badHeartbeatCount = 0;


  /*
  0 - has not logget yet
  -1 - problem, but give it a chance
  -9 - exit program
  +1 - ok
  */
  static async Send(oneApiService: OneApiService): Promise<number> {
    // console.log(`Heartbeat timer tick at ${Utils.stime()}`);
    try {
      const user = sessionStorage.getItem("currentUser");
      if (user === null) {
        console.log("user has not logged yet");
        return 0;
      } else {
        const currentUser = JSON.parse(sessionStorage.currentUser);

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
          if (this.badHeartbeatCount >= 3) {
            return -9;
          }
          console.log(
            `badHeartbeatCount ${this.badHeartbeatCount}, there is a problem, but give it a chance`
          );
          return -1;
        } else if (res.returnCode !== ReturnCode.Ok) {
          console.log(`Heartbeat: ${res.errorMessage} at ${Utils.stime()}`);
          return -9;
        } else {
          // console.log(`Heartbeat result is OK`);
          this.badHeartbeatCount = 0;
          return 1;
        }
      }
    } catch (error) {
      console.log(`can't send heartbeat: ${error.message}`);
      this.badHeartbeatCount++;
      if (this.badHeartbeatCount >= 3) {
        return -9;
      }
    }
  }
}
