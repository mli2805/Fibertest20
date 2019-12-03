import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtAttachedLineComponent } from './ft-attached-line.component';

describe('FtAttachedLineComponent', () => {
  let component: FtAttachedLineComponent;
  let fixture: ComponentFixture<FtAttachedLineComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtAttachedLineComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtAttachedLineComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
