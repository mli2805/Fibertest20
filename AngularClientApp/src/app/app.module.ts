import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

import { MatMenuModule } from '@angular/material/menu';
import { MatListModule } from '@angular/material/list';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule } from '@angular/material/sort';
import { MatIconModule } from '@angular/material/icon';
import { FlexLayoutModule } from '@angular/flex-layout';
import { TranslateService, TranslateModule } from '@ngx-translate/core';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

import { TranslateLoader } from './Utils/translate-loader';
import { languages } from 'src/lang/strings';

import { MainTabComponent } from './components/maintab/maintab.component';
import { RtuListComponent } from './components/maintab/rtulist/rtulist.component';
import { AboutComponent } from './components/maintab/about/about.component';

import { MonitoringModePipe } from './pipes/monitoring-mode.pipe';
import { MonitoringModeUriPipe } from './pipes/monitoring-mode-uri.pipe';
import { RtuPartStateUriPipe } from './pipes/rtu-part-state-uri.pipe';
import { FiberStatePipe } from './pipes/fiber-state.pipe';
import { EventStatusPipe } from './pipes/event-status.pipe';
import {
  BaseRefTypePipe,
  BaseRefTypeFemalePipe
} from './pipes/base-ref-type.pipe';
import { OptEventsComponent } from './components/maintab/opt-events/opt-events.component';

@NgModule({
  declarations: [
    AppComponent,
    MainTabComponent,
    RtuListComponent,
    AboutComponent,
    MonitoringModePipe,
    MonitoringModeUriPipe,
    RtuPartStateUriPipe,
    FiberStatePipe,
    EventStatusPipe,
    BaseRefTypePipe,
    BaseRefTypeFemalePipe,
    OptEventsComponent
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
    MatIconModule,
    MatMenuModule,
    MatListModule,
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
