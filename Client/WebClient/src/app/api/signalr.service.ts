// https://medium.com/@berkokur/a-real-life-scenario-implementation-with-angular-8-and-asp-net-core-signalr-3ad7e5a46fca

import { Injectable, EventEmitter } from "@angular/core";
import * as signalR from "@aspnet/signalr";
import { Utils } from "../Utils/utils";
import { RtuInitializedWebDto } from "../models/dtos/rtu/rtuInitializedWebDto";
import { ReturnCode } from "../models/enums/returnCode";
import { CurrentMonitoringStepDto } from "../models/dtos/rtu/currentMonitoringStepDto";
import { ClientMeasurementDoneDto } from "../models/dtos/port/clientMeasurementDoneDto";

@Injectable({
  providedIn: "root",
})
export class SignalrService {
  private hubConnection: signalR.HubConnection;
  public rtuInitializedEmitter = new EventEmitter<RtuInitializedWebDto>();
  public monitoringStepNotifier = new EventEmitter<CurrentMonitoringStepDto>();
  public clientMeasurementEmitter = new EventEmitter<
    ClientMeasurementDoneDto
  >();

  // will be built after loggin in, when jsonWebToken provided
  public buildConnection(token: string) {
    const url = Utils.GetWebApiUrl() + "/webApiSignalRHub";
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(url, { accessTokenFactory: () => token })
      .build();
  }

  public async startConnection() {
    try {
      await this.hubConnection.start();
      console.log("SignalR connection started...");
      this.registerSignalEvents();
    } catch (err) {
      console.log("Error while starting connection: " + err);
      setTimeout(() => this.startConnection(), 3000);
    }
  }

  public async initializeRtu(id: string) {
    try {
      console.log(this.hubConnection);
      if (this.hubConnection === undefined) {
        const res = JSON.parse(sessionStorage.getItem("currentUser"));
        this.buildConnection(res.jsonWebToken);
      }
      if (this.hubConnection.state !== signalR.HubConnectionState.Connected) {
        await this.startConnection();
        console.log("restart: ", this.hubConnection);
      }
      await this.hubConnection.invoke("InitializeRtu", id);
    } catch (err) {
      console.log(err);
      const data = new RtuInitializedWebDto();
      data.returnCode = ReturnCode.RtuInitializationError;
      this.rtuInitializedEmitter.emit(data);
    }
  }

  private registerSignalEvents() {
    // this.hubConnection.on("RtuInitialized", this.onRtuInitialized.bind(this));
    this.hubConnection.on("RtuInitialized", (data: RtuInitializedWebDto) =>
      this.rtuInitializedEmitter.emit(data)
    );
    this.hubConnection.on("MonitoringStepNotified", (ntf: string) => {
      const a = ntf.length - 1;
      const timestamp = `, "Timestamp":"${Date()}"`;
      const withTimestamp = [
        ntf.substring(0, a),
        timestamp,
        ntf.substring(a),
      ].join("");

      const ob = JSON.parse(withTimestamp);
      const obCamel = Utils.toCamel(ob);
      this.monitoringStepNotifier.emit(obCamel);
    });
    this.hubConnection.on("ClientMeasurementDone", (signal: string) => {
      const dto = JSON.parse(signal);
      this.clientMeasurementEmitter.emit(dto);
    });
  }
}
