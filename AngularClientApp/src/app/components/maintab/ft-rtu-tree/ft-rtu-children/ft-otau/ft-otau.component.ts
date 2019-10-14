import { Component, OnInit, Input } from '@angular/core';
import { OtauWebDto } from 'src/app/models/dtos/otauWebDto';

@Component({
  selector: 'ft-otau',
  templateUrl: './ft-otau.component.html',
  styleUrls: ['./ft-otau.component.scss']
})
export class FtOtauComponent implements OnInit {
  @Input() otau: OtauWebDto;

  constructor() {}

  ngOnInit() {
  }
}
