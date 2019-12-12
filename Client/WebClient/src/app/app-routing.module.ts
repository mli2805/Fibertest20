import { NgModule } from "@angular/core";
import { Routes, RouterModule } from "@angular/router";
import { FtAboutComponent } from "./components/ft-about/ft-about.component";
import { FtRtuTreeComponent } from "./components/ft-rtu-tree/ft-rtu-tree.component";
import { PageNotFoundComponent } from "./components/page-not-found/page-not-found.component";
import { FtLoginComponent } from "./components/ft-login/ft-login.component";
import { FtOptEventsComponent } from "./components/ft-opt-events/ft-opt-events.component";
import { AuthGuard } from "./utils/auth-guard";
import { FtTraceStatisticsComponent } from "./components/details/ft-trace-statistics/ft-trace-statistics.component";
import { FtRtuStateComponent } from "./components/details/ft-rtu-state/ft-rtu-state.component";
import { FtTraceInformationComponent } from "./components/details/ft-trace-information/ft-trace-information.component";
import { FtNetworkEventsComponent } from "./components/ft-network-events/ft-network-events.component";

const routes: Routes = [
  { path: "login", component: FtLoginComponent },
  { path: "about", component: FtAboutComponent },
  { path: "rtu-tree", component: FtRtuTreeComponent, canActivate: [AuthGuard] },
  {
    path: "optical-events",
    component: FtOptEventsComponent,
    canActivate: [AuthGuard]
  },
  { path: "network-events", component: FtNetworkEventsComponent },

  { path: "rtu-state/:id", component: FtRtuStateComponent },
  { path: "trace-information/:id", component: FtTraceInformationComponent },
  { path: "trace-statistics/:id", component: FtTraceStatisticsComponent },

  { path: "logout", component: FtLoginComponent },
  { path: "", redirectTo: "/login", pathMatch: "full" },
  { path: "**", component: PageNotFoundComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
