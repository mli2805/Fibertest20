import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import {
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
  MatPaginatorIntl
} from '@angular/material';
import { FtPaginatorLocale } from './Utils/paginator-locale';

@NgModule({
  imports: [
    CommonModule,
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
    MatTabsModule
  ],
  exports: [
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
    MatTabsModule
  ],
  providers: [{ provide: MatPaginatorIntl, useClass: FtPaginatorLocale }]
})
export class AngularMaterialModule {}
