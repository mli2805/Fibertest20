import { BrowserModule } from "@angular/platform-browser";
import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { HttpClientModule } from "@angular/common/http";
import { VxSorViewerModule } from "@veex/sor";

import { TraceMonitoringModeUriPipe } from "./pipes/trace-monitoring-mode-uri.pipe";
import { MonitoringModePipe } from "./pipes/monitoring-mode.pipe";
import { MonitoringModeToUriPipe } from "./pipes/monitoring-mode-uri.pipe";
import { RtuPartStatePipe } from "./pipes/rtu-part-state.pipe";
import { RtuPartStateUriPipe } from "./pipes/rtu-part-state-uri.pipe";
import { FiberStatePipe } from "./pipes/fiber-state.pipe";
import { FiberStateUriPipe } from "./pipes/fiber-state-uri.pipe";
import { BooleanUriPipe } from "./pipes/boolean-uri.pipe";
import { EventStatusPipe } from "./pipes/event-status.pipe";
import { FrequencyPipe } from "./pipes/frequency.pipe";
import { ReturnCodePipe } from "./pipes/return-code.pipe";
import { ChannelEventPipe } from "./pipes/channel-event.pipe";
import {
  BaseRefPipe,
  BaseRefTypePipe,
  BaseRefTypeFemalePipe,
} from "./pipes/base-ref-type.pipe";

import { TranslateService, TranslateModule } from "@ngx-translate/core";
import { TranslateLoader } from "./Utils/translate-loader";
import { languages } from "src/lang/strings";

import { AppRoutingModule } from "./app-routing.module";
import { AppComponent } from "./app.component";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { LayoutModule } from "@angular/cdk/layout";
import { FlexLayoutModule } from "@angular/flex-layout";

import { FtMainNavComponent } from "./components/ft-main-nav/ft-main-nav.component";
import { FtRtuTreeComponent } from "./components/ft-rtu-tree/ft-rtu-tree.component";
import { FtRtuChildrenComponent } from "./components/ft-rtu-tree/ft-rtu-line/ft-rtu-children/ft-rtu-children.component";
import { FtFreePortComponent } from "./components/ft-rtu-tree/ft-rtu-line/ft-rtu-children/ft-free-port/ft-free-port.component";
import { FtAttachedLineComponent } from "./components/ft-rtu-tree/ft-rtu-line/ft-rtu-children/ft-attached-line/ft-attached-line.component";
import { FtDetachedLineComponent } from "./components/ft-rtu-tree/ft-rtu-line/ft-rtu-children/ft-detached-line/ft-detached-line.component";
import { FtOtauComponent } from "./components/ft-rtu-tree/ft-rtu-line/ft-rtu-children/ft-otau/ft-otau.component";
import { FtAboutComponent } from "./components/ft-about/ft-about.component";
import { FtLoginComponent } from "./components/ft-login/ft-login.component";
import { PageNotFoundComponent } from "./components/page-not-found/page-not-found.component";
import { FtOptEventsComponent } from "./components/ft-opt-events/ft-opt-events.component";
import { FtNetworkEventsComponent } from "./components/ft-network-events/ft-network-events.component";
import { AuthGuard } from "./utils/auth-guard";
import { FtTraceStatisticsComponent } from "./components/details/trace/ft-trace-statistics/ft-trace-statistics.component";
import { FtTraceInformationComponent } from "./components/details/trace/ft-trace-information/ft-trace-information.component";
import { FtRtuStateComponent } from "./components/details/rtu/ft-rtu-state/ft-rtu-state.component";
import { FtRtuInformationComponent } from "./components/details/rtu/ft-rtu-information/ft-rtu-information.component";
import { FtRtuNetworkSettingsComponent } from "./components/details/rtu/ft-rtu-network-settings/ft-rtu-network-settings.component";
import { FtRtuMonitoringSettingsComponent } from "./components/details/rtu/ft-rtu-monitoring-settings/ft-rtu-monitoring-settings.component";
import { FtRtuMonitoringPortsComponent } from "./components/details/rtu/ft-rtu-monitoring-ports/ft-rtu-monitoring-ports.component";
import { LoginGuard } from "./utils/login-guard";
import { FtRtuLineComponent } from "./components/ft-rtu-tree/ft-rtu-line/ft-rtu-line.component";
import { FtPortAttachTraceComponent } from "./components/details/port/ft-port-attach-trace/ft-port-attach-trace.component";
import { FtPortAttachOtauComponent } from "./components/details/port/ft-port-attach-otau/ft-port-attach-otau.component";
import { FtPortMeasurementClientComponent } from "./components/details/port/ft-port-measurement-client/ft-port-measurement-client.component";
import { FtRtuTreeEventService } from "./components/ft-rtu-tree/ft-rtu-tree-event-service";
import { FtTraceStateComponent } from "./components/details/trace/ft-trace-state/ft-trace-state.component";
import { FtAssignBaseComponent } from "./components/details/trace/ft-assign-base/ft-assign-base.component";
import { FtBopEventsComponent } from "./components/ft-bop-events/ft-bop-events.component";
import { SorViewerComponent } from "./components/sor-viewer/sor-viewer.component";
import { FtOutOfTurnMeasurementComponent } from "./components/details/trace/ft-out-of-turn-measurement/ft-out-of-turn-measurement.component";
import { FtRftsEventsComponent } from "./components/details/trace/ft-rfts-events/ft-rfts-events.component";
import { FtRftsEventsLevelComponent } from "./components/details/trace/ft-rfts-events/ft-rfts-events-level/ft-rfts-events-level.component";
import { FtSimpleDialogComponent } from "./components/ft-simple-dialog/ft-simple-dialog.component";
import { MaterialModule } from "./material.module";
import { FtIitHeaderComponent } from "./components/ft-iit-header/ft-iit-header.component";
import { FtTraceLandmarksComponent } from "./components/details/trace/ft-trace-landmarks/ft-trace-landmarks.component";
import { EquipmentTypePipe } from "./pipes/equipment-type.pipe";
import { FtBaseRefsComponent } from "./components/details/trace/ft-trace-statistics/ft-base-refs/ft-base-refs/ft-base-refs.component";

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
    RtuPartStatePipe,
    RtuPartStateUriPipe,
    FiberStatePipe,
    FiberStateUriPipe,
    FrequencyPipe,
    EventStatusPipe,
    ChannelEventPipe,
    BaseRefPipe,
    BaseRefTypePipe,
    BaseRefTypeFemalePipe,
    TraceMonitoringModeUriPipe,
    BooleanUriPipe,
    ReturnCodePipe,
    EquipmentTypePipe,

    FtIitHeaderComponent,
    FtSimpleDialogComponent,
    FtOptEventsComponent,
    FtNetworkEventsComponent,
    FtTraceStatisticsComponent,
    FtBaseRefsComponent,
    FtTraceInformationComponent,
    FtRtuStateComponent,
    FtOtauComponent,
    FtRtuInformationComponent,
    FtRtuNetworkSettingsComponent,
    FtRtuMonitoringSettingsComponent,
    FtRtuMonitoringPortsComponent,
    FtRtuLineComponent,
    FtPortAttachTraceComponent,
    FtPortAttachOtauComponent,
    FtPortMeasurementClientComponent,
    FtTraceStateComponent,
    FtAssignBaseComponent,
    FtTraceLandmarksComponent,
    FtBopEventsComponent,
    FtOutOfTurnMeasurementComponent,
    FtRftsEventsComponent,
    FtRftsEventsLevelComponent,
    SorViewerComponent,
  ],
  imports: [
    VxSorViewerModule,
    TranslateModule.forRoot(),
    CommonModule,
    BrowserModule,
    FormsModule,
    HttpClientModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    LayoutModule,
    FlexLayoutModule,

    MaterialModule,
  ],
  entryComponents: [FtSimpleDialogComponent],
  providers: [
    AuthGuard,
    LoginGuard,
    FrequencyPipe,
    FiberStatePipe,
    ReturnCodePipe,
    EventStatusPipe,
    FtRtuTreeEventService,
  ],
  bootstrap: [AppComponent],
})
export class AppModule {
  private translateLoader: TranslateLoader;

  constructor(translateService: TranslateService) {
    this.translateLoader = new TranslateLoader(translateService);
    this.translateLoader.init(languages);
    translateService.setDefaultLang("en");
    translateService.use(this.getLang().substr(0, 2));
  }

  private getLang() {
    if (navigator.languages !== undefined) {
      console.log(navigator.languages);
      return navigator.languages[0];
    } else {
      console.log(navigator.language);
      return navigator.language;
    }
  }
}
