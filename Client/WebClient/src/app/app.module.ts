import { BrowserModule } from "@angular/platform-browser";
import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { HttpClientModule } from "@angular/common/http";
import { VxSorViewerModule } from '@veex/sor';

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
  BaseRefTypePipe,
  BaseRefTypeFemalePipe,
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
  MatSelectModule,
  MatTabsModule,
  MatSlideToggleModule,
  MatFormFieldModule,
  MatNativeDateModule,
  MatRadioModule,
} from "@angular/material";

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
import { NoRightClickDirective } from "./utils/no-right-click.directive";
import { LoginGuard } from "./utils/login-guard";
import { FtRtuLineComponent } from "./components/ft-rtu-tree/ft-rtu-line/ft-rtu-line.component";
import { FtPortAttachTraceComponent } from "./components/details/port/ft-port-attach-trace/ft-port-attach-trace.component";
import { FtPortAttachOtauComponent } from "./components/details/port/ft-port-attach-otau/ft-port-attach-otau.component";
import { FtPortMeasurementClientComponent } from "./components/details/port/ft-port-measurement-client/ft-port-measurement-client.component";
import { FtRtuTreeEventService } from "./components/ft-rtu-tree/ft-rtu-tree-event-service";
import { FtTraceStateComponent } from "./components/details/trace/ft-trace-state/ft-trace-state.component";
import { FtAssignBaseComponent } from "./components/details/trace/ft-assign-base/ft-assign-base.component";
import { FtBopEventsComponent } from './components/ft-bop-events/ft-bop-events.component';
import { SorViewerComponent } from './components/sor-viewer/sor-viewer.component';

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
    BaseRefTypePipe,
    BaseRefTypeFemalePipe,
    TraceMonitoringModeUriPipe,
    BooleanUriPipe,
    ReturnCodePipe,

    FtOptEventsComponent,
    FtNetworkEventsComponent,
    FtTraceStatisticsComponent,
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

    NoRightClickDirective,

    FtBopEventsComponent,

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
    MatSelectModule,
    MatSlideToggleModule,
    MatFormFieldModule,
    MatNativeDateModule,
    MatRadioModule,
  ],
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
    translateService.use("ru");
  }
}
