import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtRtuLineComponent } from './ft-rtu-line.component';

describe('FtRtuLineComponent', () => {
  let component: FtRtuLineComponent;
  let fixture: ComponentFixture<FtRtuLineComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtRtuLineComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtRtuLineComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
