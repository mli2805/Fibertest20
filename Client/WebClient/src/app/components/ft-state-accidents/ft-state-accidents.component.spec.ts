import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtStateAccidentsComponent } from './ft-state-accidents.component';

describe('FtStateAccidentsComponent', () => {
  let component: FtStateAccidentsComponent;
  let fixture: ComponentFixture<FtStateAccidentsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtStateAccidentsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtStateAccidentsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
