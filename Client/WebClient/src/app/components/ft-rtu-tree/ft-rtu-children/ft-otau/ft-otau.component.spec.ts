import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtOtauComponent } from './ft-otau.component';

describe('FtOtauV2Component', () => {
  let component: FtOtauComponent;
  let fixture: ComponentFixture<FtOtauComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtOtauComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtOtauComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
