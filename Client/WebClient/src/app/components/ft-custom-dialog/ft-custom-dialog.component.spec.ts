import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtCustomDialogComponent } from './ft-custom-dialog.component';

describe('FtCustomDialogComponent', () => {
  let component: FtCustomDialogComponent;
  let fixture: ComponentFixture<FtCustomDialogComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtCustomDialogComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtCustomDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
