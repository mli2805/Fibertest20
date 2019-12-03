import { BrowserModule } from "@angular/platform-browser";
import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { HttpClientModule } from "@angular/common/http";

import { TraceMonitoringModeUriPipe } from "./pipes/trace-monitoring-mode-uri.pipe";
import { MonitoringModePipe } from "./pipes/monitoring-mode.pipe";
import { MonitoringModeToUriPipe } from "./pipes/monitoring-mode-uri.pipe";
import { RtuPartStateUriPipe } from "./pipes/rtu-part-state-uri.pipe";
import { FiberStatePipe } from "./pipes/fiber-state.pipe";
import { FiberStateUriPipe } from "./pipes/fiber-state-uri.pipe";
import { BooleanUriPipe } from "./pipes/boolean-uri.pipe";
import { EventStatusPipe } from "./pipes/event-status.pipe";
import {
  BaseRefTypePipe,
  BaseRefTypeFemalePipe
} from "./pipes/base-ref-type.pipe";

import {
  MatSidenavModule,
  MatButtonModule,
  MatToolbarModule,
  MatTableModule,
  MatSortModule,
  MatInputModule,
  MatCheckboxModule,
  MatGridListModule,
  MatCardModule,
  MatExpansionModule,
  MatIconModule,
  MatMenuModule,
  MatListModule,
  MatPaginatorModule,
  MatProgressSpinnerModule,
  MatTabsModule,
  MatSlideToggleModule
} from "@angular/material";

import { TranslateService, TranslateModule } from "@ngx-translate/core";
import { TranslateLoader } from "./Utils/translate-loader";
import { languages } from "src/lang/strings";

import { AppRoutingModule } from "./app-routing.module";
import { AppComponent } from "./app.component";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { FtMainNavComponent } from "./components/ft-main-nav/ft-main-nav.component";
import { LayoutModule } from "@angular/cdk/layout";

import { FtRtuTreeComponent } from "./components/ft-rtu-tree/ft-rtu-tree.component";
import { FtRtuChildrenComponent } from "./components/ft-rtu-tree/ft-rtu-children/ft-rtu-children.component";
import { FtFreePortComponent } from "./components/ft-rtu-tree/ft-rtu-children/ft-free-port/ft-free-port.component";
import { FtAttachedLineComponent } from "./components/ft-rtu-tree/ft-rtu-children/ft-attached-line/ft-attached-line.component";
import { FtDetachedLineComponent } from "./components/ft-rtu-tree/ft-rtu-children/ft-detached-line/ft-detached-line.component";
import { FtOtauComponent } from "./components/ft-rtu-tree/ft-rtu-children/ft-otau/ft-otau.component";
import { FtAboutComponent } from "./components/ft-about/ft-about.component";
import { FtLoginComponent } from "./components/ft-login/ft-login.component";
import { PageNotFoundComponent } from "./components/page-not-found/page-not-found.component";
import { FtOptEventsComponent } from "./components/ft-opt-events/ft-opt-events.component";
import { FtNetworkEventsComponent } from "./components/ft-network-events/ft-network-events.component";
import { AuthGuard } from "./utils/auth-guard";

@NgModule({
  declarations: [
    AppComponent,
    FtMainNavComponent,
    FtRtuTreeComponent,
    FtRtuChildrenComponent,
    FtFreePortComponent,
    FtAttachedLineComponent,
    FtDetachedLineComponent,
    FtOtauComponent,
    FtAboutComponent,
    FtLoginComponent,
    PageNotFoundComponent,

    MonitoringModePipe,
    MonitoringModeToUriPipe,
    RtuPartStateUriPipe,
    FiberStatePipe,
    FiberStateUriPipe,
    EventStatusPipe,
    BaseRefTypePipe,
    BaseRefTypeFemalePipe,
    TraceMonitoringModeUriPipe,
    BooleanUriPipe,
    FtOptEventsComponent,
    FtNetworkEventsComponent
  ],
  imports: [
    TranslateModule.forRoot(),
    CommonModule,
    BrowserModule,
    FormsModule,
    HttpClientModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    LayoutModule,
    MatSidenavModule,
    MatButtonModule,
    MatToolbarModule,
    MatTableModule,
    MatSortModule,
    MatInputModule,
    MatCheckboxModule,
    MatGridListModule,
    MatCardModule,
    MatExpansionModule,
    MatIconModule,
    MatMenuModule,
    MatListModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatTabsModule,
    MatSlideToggleModule
  ],
  providers: [AuthGuard],
  bootstrap: [AppComponent]
})
export class AppModule {
  private translateLoader: TranslateLoader;

  constructor(translateService: TranslateService) {
    this.translateLoader = new TranslateLoader(translateService);
    this.translateLoader.init(languages);
    translateService.setDefaultLang("en");
    translateService.use("ru");
  }
}
