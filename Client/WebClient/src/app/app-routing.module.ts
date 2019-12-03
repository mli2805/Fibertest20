import { NgModule } from "@angular/core";
import { Routes, RouterModule } from "@angular/router";
import { FtAboutComponent } from "./components/ft-about/ft-about.component";
import { FtRtuTreeComponent } from "./components/ft-rtu-tree/ft-rtu-tree.component";
import { PageNotFoundComponent } from "./components/page-not-found/page-not-found.component";
import { FtLoginComponent } from "./components/ft-login/ft-login.component";
import { FtOptEventsComponent } from "./components/ft-opt-events/ft-opt-events.component";
import { AuthGuard } from "./utils/auth-guard";

const routes: Routes = [
  { path: "login", component: FtLoginComponent },
  { path: "about", component: FtAboutComponent },
  { path: "rtu-tree", component: FtRtuTreeComponent, canActivate: [AuthGuard] },
  {
    path: "optical-events",
    component: FtOptEventsComponent,
    canActivate: [AuthGuard]
  },
  { path: "network-events", component: FtOptEventsComponent },
  { path: "logout", component: FtLoginComponent },
  { path: "", redirectTo: "/login", pathMatch: "full" },
  { path: "**", component: PageNotFoundComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
