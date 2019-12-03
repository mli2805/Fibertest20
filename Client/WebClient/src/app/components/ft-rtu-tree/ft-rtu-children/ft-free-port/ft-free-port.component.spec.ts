import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtFreePortComponent } from './ft-free-port.component';

describe('FtFreePortComponent', () => {
  let component: FtFreePortComponent;
  let fixture: ComponentFixture<FtFreePortComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtFreePortComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtFreePortComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
