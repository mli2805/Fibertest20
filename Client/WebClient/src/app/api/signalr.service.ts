// https://medium.com/@berkokur/a-real-life-scenario-implementation-with-angular-8-and-asp-net-core-signalr-3ad7e5a46fca

import { Injectable } from "@angular/core";
import * as signalR from "@aspnet/signalr";

@Injectable({
  providedIn: "root"
})
export class SignalrService {
  private hubConnection: signalR.HubConnection;

  constructor() {
    this.buildConnection();
  }

  public buildConnection() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:2334/signalHub")
      .build();
  }

  public startConnection() {
    this.hubConnection
      .start()
      .then(() => {
        console.log("Connection started...");
      })
      .catch(err => {
        console.log("Error while starting connection: " + err);
        setTimeout(function() {
          this.startConnection();
        }, 3000);
      });
  }

  public sendSomething() {
    this.hubConnection.invoke("Send", "message", "username");
  }
}
