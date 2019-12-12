import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtRtuListComponent } from './ft-rtu-list.component';

describe('FtRtuListComponent', () => {
  let component: FtRtuListComponent;
  let fixture: ComponentFixture<FtRtuListComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtRtuListComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtRtuListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
