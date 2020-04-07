import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FtPortAttachOtauComponent } from './ft-port-attach-otau.component';

describe('FtPortAttachOtauComponent', () => {
  let component: FtPortAttachOtauComponent;
  let fixture: ComponentFixture<FtPortAttachOtauComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FtPortAttachOtauComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FtPortAttachOtauComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
