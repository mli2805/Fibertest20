import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtTestComponentComponent } from './ft-test-component.component';

describe('FtTestComponentComponent', () => {
  let component: FtTestComponentComponent;
  let fixture: ComponentFixture<FtTestComponentComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtTestComponentComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtTestComponentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
