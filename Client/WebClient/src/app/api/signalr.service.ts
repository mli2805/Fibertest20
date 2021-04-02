// https://medium.com/@berkokur/a-real-life-scenario-implementation-with-angular-8-and-asp-net-core-signalr-3ad7e5a46fca

import { Injectable, EventEmitter } from "@angular/core";
import * as signalR from "@aspnet/signalr";
import { Utils } from "../Utils/utils";
import { RtuInitializedWebDto } from "../models/dtos/rtu/rtuInitializedWebDto";
import { CurrentMonitoringStepDto } from "../models/dtos/rtu/currentMonitoringStepDto";
import { ClientMeasurementDoneDto } from "../models/dtos/port/clientMeasurementDoneDto";
import { formatDate } from "@angular/common";
import { MonitoringStoppedDto } from "../models/dtos/rtu/monitoringStoppedDto";
import { TraceStateDto } from "../models/dtos/trace/traceStateDto";
import { MonitoringStartdedDto } from "../models/dtos/rtu/monitoringStartedDto";
import { NetworkEventDto } from "../models/dtos/networkEventDto";
import { BopEventDto } from "../models/dtos/bopEventDto";
import { UpdateMeasurementDto } from "../models/dtos/trace/updateMeasurementDto";
import { ServerAsksClientToExitDto } from "../models/dtos/serverAsksClientToExitDto";

@Injectable({
  providedIn: "root",
})
export class SignalrService {
  private hubConnection: signalR.HubConnection;
  public rtuInitializedEmitter = new EventEmitter<RtuInitializedWebDto>();
  public fetchTreeEmitter = new EventEmitter();
  public monitoringStepNotifier = new EventEmitter<CurrentMonitoringStepDto>();
  public clientMeasEmitter = new EventEmitter<ClientMeasurementDoneDto>();
  public monitoringStoppedEmitter = new EventEmitter<MonitoringStoppedDto>();
  public monitoringStartedEmitter = new EventEmitter<MonitoringStartdedDto>();
  public measurementAddedEmitter = new EventEmitter<TraceStateDto>();
  public networkEventAddedEmitter = new EventEmitter<NetworkEventDto>();
  public bopEventAddedEmitter = new EventEmitter<BopEventDto>();
  public measurementUpdatedEmitter = new EventEmitter<UpdateMeasurementDto>();
  public serverAsksExitEmitter = new EventEmitter<ServerAsksClientToExitDto>();

  public buildConnection(token: string) {
    const url = Utils.GetWebApiUrl() + "/webApiSignalRHub";
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(url, { accessTokenFactory: () => token })
      // token is not obligatory, connection would be set even without token
      // but without JWT you can only subscribe on notifications from signalR
      // and cannot invoke signalR methods if they have attribute [Autorize]
      .build();
  }

  public async startConnection(): Promise<string> {
    try {
      await this.hubConnection.start();
      const connectionId = await this.hubConnection.invoke("getConnectionId");
      this.registerSignalEvents();
      return connectionId;
    } catch (err) {
      console.log("Error while starting connection: " + err);
      setTimeout(() => this.startConnection(), 3000);
    }
  }

  public async reStartConnection() {
    console.log("restart signalR connection");
    if (sessionStorage.getItem("currentUser") === null) {
      return;
    }
    console.log(this.hubConnection);
    if (this.hubConnection === undefined) {
      const res = JSON.parse(sessionStorage.getItem("currentUser"));
      this.buildConnection(res.jsonWebToken);
    }
    if (this.hubConnection.state !== signalR.HubConnectionState.Connected) {
      await this.startConnection();
      const connectionId = await this.hubConnection.invoke("getConnectionId");
      console.log("signalR connection restarted, ID: ", connectionId);
    }
  }

  public stopConnection() {
    if (this.hubConnection !== undefined) {
      this.hubConnection.stop();
    }
  }

  // public async initializeRtu(id: string) {
  //   try {
  //     await this.reStartConnection();
  //     await this.hubConnection.invoke("InitializeRtu", id);
  //   } catch (err) {
  //     console.log(err);
  //     const data = new RtuInitializedWebDto();
  //     data.returnCode = ReturnCode.RtuInitializationError;
  //     this.rtuInitializedEmitter.emit(data);
  //   }
  // }

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

    this.hubConnection.on("FetchTree", () => {
      this.fetchTreeEmitter.emit();
    });

    this.hubConnection.on("NotifyMonitoringStep", (signal: string) => {
      this.onNotifyMonitoringStep(signal);
    });

    this.hubConnection.on("ClientMeasurementDone", (signal: string) => {
      const dto = JSON.parse(signal) as ClientMeasurementDoneDto;
      this.clientMeasEmitter.emit(dto);
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
      const dto = JSON.parse(signal) as TraceStateDto;
      console.log(
        `measurement ${dto.sorFileId} reg.time ${Utils.ToLongRussian(
          new Date(dto.registrationTimestamp)
        )} for trace ${dto.traceId.substr(1, 6)} came with state ${
          dto.traceState
        }`
      );
      this.measurementAddedEmitter.emit(dto);
    });

    this.hubConnection.on("AddNetworkEvent", (signal: string) => {
      const dto = JSON.parse(signal);
      this.networkEventAddedEmitter.emit(dto);
    });

    this.hubConnection.on("AddBopEvent", (signal: string) => {
      const dto = JSON.parse(signal);
      this.bopEventAddedEmitter.emit(dto);
    });

    this.hubConnection.on("UpdateMeasurement", (signal: string) => {
      const dto = JSON.parse(signal);
      this.measurementUpdatedEmitter.emit(dto);
    });

    this.hubConnection.on("ServerAsksClientToExit", (signal: string) => {
      const dto = JSON.parse(signal);
      this.serverAsksExitEmitter.emit(dto);
    });
  }
}
