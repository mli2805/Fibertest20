// https://medium.com/@berkokur/a-real-life-scenario-implementation-with-angular-8-and-asp-net-core-signalr-3ad7e5a46fca

import { Injectable, EventEmitter } from "@angular/core";
import * as signalR from "@aspnet/signalr";
import { Utils } from "../Utils/utils";
import { RtuInitializedWebDto } from "../models/dtos/rtu/rtuInitializedWebDto";
import { ReturnCode } from "../models/enums/returnCode";
import { CurrentMonitoringStepDto } from "../models/dtos/rtu/currentMonitoringStepDto";
import { ClientMeasurementDoneDto } from "../models/dtos/port/clientMeasurementDoneDto";
import { formatDate } from "@angular/common";
import { MonitoringStoppedDto } from "../models/dtos/rtu/monitoringStoppedDto";
import { TraceStateDto } from "../models/dtos/trace/traceStateDto";
import { MonitoringStartdedDto } from "../models/dtos/rtu/monitoringStartedDto";
import { NetworkEventDto } from "../models/dtos/networkEventDto";

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
  public monitoringStoppedEmitter = new EventEmitter<MonitoringStoppedDto>();
  public monitoringStartedEmitter = new EventEmitter<MonitoringStartdedDto>();
  public measurementAddedEmitter = new EventEmitter<TraceStateDto>();
  public networkEventAddedEmitter = new EventEmitter<NetworkEventDto>();

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

  private onNotifyMonitoringStep(signal: string) {
    const a = signal.length - 1;
    const timestamp = `, "Timestamp":"${formatDate(
      Date.now(),
      "HH:mm:ss:SSS",
      "en-US"
    )}"`;
    const withTimestamp = [
      signal.substring(0, a),
      timestamp,
      signal.substring(a),
    ].join("");

    const ob = JSON.parse(withTimestamp);
    const obCamel = Utils.toCamel(ob);
    this.monitoringStepNotifier.emit(obCamel);
  }

  private registerSignalEvents() {
    // this.hubConnection.on("RtuInitialized", this.onRtuInitialized.bind(this));
    this.hubConnection.on("RtuInitialized", (data: RtuInitializedWebDto) =>
      this.rtuInitializedEmitter.emit(data)
    );

    this.hubConnection.on("NotifyMonitoringStep", (signal: string) => {
      this.onNotifyMonitoringStep(signal);
    });

    this.hubConnection.on("ClientMeasurementDone", (signal: string) => {
      const dto = JSON.parse(signal);
      this.clientMeasurementEmitter.emit(dto);
    });

    this.hubConnection.on("MonitoringStopped", (signal: string) => {
      const dto = JSON.parse(signal);
      this.monitoringStoppedEmitter.emit(dto);
    });

    this.hubConnection.on("MonitoringStarted", (signal: string) => {
      const dto = JSON.parse(signal);
      this.monitoringStartedEmitter.emit(dto);
    });

    this.hubConnection.on("AddMeasurement", (signal: string) => {
      const dto = JSON.parse(signal);
      this.measurementAddedEmitter.emit(dto);
    });

    this.hubConnection.on("AddNetworkEvent", (signal: string) => {
      const dto = JSON.parse(signal);
      this.networkEventAddedEmitter.emit(dto);
    });
  }
}
