// https://medium.com/@berkokur/a-real-life-scenario-implementation-with-angular-8-and-asp-net-core-signalr-3ad7e5a46fca

import { Injectable } from "@angular/core";
import * as signalR from "@aspnet/signalr";
import { EventEmitter } from "protractor";
import { SignalDto } from "../models/dtos/signalDto";

@Injectable({
  providedIn: "root"
})
export class SignalrService {
  private hubConnection: signalR.HubConnection;
  signalReceived = new EventEmitter();

  constructor() {
    this.buildConnection();
  }

  public buildConnection() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl("https://localhost:44327")
      .build();
  }

  public startConnection() {
    this.hubConnection
      .start()
      .then(() => {
        console.log("Connection started...");
        this.registerSignalEvents();
      })
      .catch(err => {
        console.log("Error while starting connection: " + err);
        setTimeout(function() {
          this.startConnection();
        }, 3000);
      });
  }

  public registerSignalEvents() {
    this.hubConnection.on("SignalMessageReceived", data => {
      this.signalReceived.emit(data);
    });
  }

  public sendSomething() {
    this.hubConnection
      .invoke("methodOnServer", 5, "blah-blah")
      .then(() => this.registerSignalEvents());
  }
}
