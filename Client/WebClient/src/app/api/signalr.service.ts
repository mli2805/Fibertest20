// https://medium.com/@berkokur/a-real-life-scenario-implementation-with-angular-8-and-asp-net-core-signalr-3ad7e5a46fca

import { Injectable, EventEmitter } from "@angular/core";
import * as signalR from "@aspnet/signalr";
import { Utils } from "../Utils/utils";
import { RtuInitializedWebDto } from "../models/dtos/rtu/rtuInitializedWebDto";

@Injectable({
  providedIn: "root"
})
export class SignalrService {
  private hubConnection: signalR.HubConnection;
  public rtuInitializedEmitter = new EventEmitter<RtuInitializedWebDto>();

  // will be built after loggin in, when jsonWebToken provided
  public buildConnection(token: string) {
    const url = Utils.GetWebApiUrl() + "/signalHub";
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

  public initializeRtu(id: string) {
    this.hubConnection.invoke("InitializeRtu", id);
  }

  private registerSignalEvents() {
    // this.hubConnection.on("RtuInitialized", this.onRtuInitialized.bind(this));
    this.hubConnection.on("RtuInitialized", x => this.onRtuInitialized(x));
  }

  private onRtuInitialized(data: RtuInitializedWebDto) {
    this.rtuInitializedEmitter.emit(data);
  }
}
