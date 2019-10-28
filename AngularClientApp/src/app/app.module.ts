import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

import { MatMenuModule } from '@angular/material/menu';
import { MatListModule } from '@angular/material/list';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule } from '@angular/material/sort';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatGridListModule } from '@angular/material/grid-list';
import {
  MatExpansionModule,
  MatPaginatorModule,
  MatProgressSpinnerModule
} from '@angular/material';

import { FlexLayoutModule } from '@angular/flex-layout';
import { TranslateService, TranslateModule } from '@ngx-translate/core';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

import { TranslateLoader } from './Utils/translate-loader';
import { languages } from 'src/lang/strings';

import { FtMainTabComponent } from './components/maintab/maintab.component';
import { FtAboutComponent } from './components/maintab/about/about.component';

import { TraceMonitoringModeUriPipe } from './pipes/trace-monitoring-mode-uri.pipe';
import { MonitoringModePipe } from './pipes/monitoring-mode.pipe';
import { MonitoringModeUriPipe } from './pipes/monitoring-mode-uri.pipe';
import { RtuPartStateUriPipe } from './pipes/rtu-part-state-uri.pipe';
import { FiberStatePipe } from './pipes/fiber-state.pipe';
import { FiberStateUriPipe } from './pipes/fiber-state-uri.pipe';
import { EventStatusPipe } from './pipes/event-status.pipe';
import {
  BaseRefTypePipe,
  BaseRefTypeFemalePipe
} from './pipes/base-ref-type.pipe';
import { FtOptEventsComponent } from './components/maintab/opt-events/opt-events.component';
import { FtRtuTreeComponent } from './components/maintab/ft-rtu-tree/ft-rtu-tree.component';
import { FtRtuChildrenComponent } from './components/maintab/ft-rtu-tree/ft-rtu-children/ft-rtu-children.component';
import { FtFreePortComponent } from './components/maintab/ft-rtu-tree/ft-rtu-children/ft-free-port/ft-free-port.component';
import { FtAttachedLineComponent } from './components/maintab/ft-rtu-tree/ft-rtu-children/ft-attached-line/ft-attached-line.component';
import { FtDetachedLineComponent } from './components/maintab/ft-rtu-tree/ft-rtu-children/ft-detached-line/ft-detached-line.component';
import { FtOtauComponent } from './components/maintab/ft-rtu-tree/ft-rtu-children/ft-otau/ft-otau.component';
import { BooleanUriPipe } from './pipes/boolean-uri.pipe';
import { FtTreeDetailsComponent } from './components/maintab/ft-tree-details/tree-details.component';
import { FtTraceInformationComponent } from './components/maintab/ft-tree-details/trace-information/trace-information.component';
import { FtTraceStatisticsComponent } from './components/maintab/ft-tree-details/trace-statistics/trace-statistics.component';
import { FtNetworkEventsComponent } from './components/maintab/network-events/network-events.component';
import { FtRtuStateComponent } from './components/maintab/ft-tree-details/rtu-state/rtu-state.component';

@NgModule({
  declarations: [
    AppComponent,

    MonitoringModePipe,
    MonitoringModeUriPipe,
    RtuPartStateUriPipe,
    FiberStatePipe,
    FiberStateUriPipe,
    EventStatusPipe,
    BaseRefTypePipe,
    BaseRefTypeFemalePipe,
    TraceMonitoringModeUriPipe,
    BooleanUriPipe,

    FtMainTabComponent,
    FtAboutComponent,
    FtOptEventsComponent,
    FtRtuTreeComponent,
    FtRtuChildrenComponent,
    FtFreePortComponent,
    FtAttachedLineComponent,
    FtDetachedLineComponent,
    FtOtauComponent,
    FtTreeDetailsComponent,
    FtTraceInformationComponent,
    FtTraceStatisticsComponent,
    FtNetworkEventsComponent,
    FtRtuStateComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    FormsModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    FlexLayoutModule,
    MatToolbarModule,
    MatTabsModule,
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
    TranslateModule.forRoot()
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule {
  private translateLoader: TranslateLoader;

  constructor(translateService: TranslateService) {
    this.translateLoader = new TranslateLoader(translateService);
    this.translateLoader.init(languages);
    translateService.setDefaultLang('en');
    translateService.use('ru');
  }
}
