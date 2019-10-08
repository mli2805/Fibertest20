import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtRtuTreeComponent } from './ft-rtu-tree.component';

describe('FtRtuTreeComponent', () => {
  let component: FtRtuTreeComponent;
  let fixture: ComponentFixture<FtRtuTreeComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtRtuTreeComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtRtuTreeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
