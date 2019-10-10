import { Component, OnInit, Input } from '@angular/core';
import { RtuDto } from 'src/app/models/dtos/rtuDto';

@Component({
  selector: 'ft-about',
  templateUrl: './about.component.html',
  styleUrls: ['./about.component.scss']
})
export class AboutComponent implements OnInit {
  @Input() rtuArray: RtuDto[];
  columnsToDisplay = ['title', 'version', 'version2'];

  constructor() {}

  ngOnInit() {}
}
