import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'ft-free-port',
  templateUrl: './ft-free-port.component.html',
  styleUrls: ['./ft-free-port.component.scss']
})
export class FtFreePortComponent implements OnInit {
  @Input() port: number;

  constructor() {}

  ngOnInit() {}
}
