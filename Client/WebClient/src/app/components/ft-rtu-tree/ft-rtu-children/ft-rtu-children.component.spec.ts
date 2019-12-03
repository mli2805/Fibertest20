import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtRtuChildrenComponent } from './ft-rtu-children.component';

describe('FtRtuChildrenComponent', () => {
  let component: FtRtuChildrenComponent;
  let fixture: ComponentFixture<FtRtuChildrenComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtRtuChildrenComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtRtuChildrenComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
