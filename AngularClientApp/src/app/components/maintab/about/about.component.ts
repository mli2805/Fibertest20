import { Component, OnInit, Input } from '@angular/core';
import { RtuDto } from 'src/app/models/dtos/rtuDto';
import { globalVars } from 'src/app/global-vars';
import { GlobalVarSet } from 'src/app/models/globalVarSet';

@Component({
  selector: 'ft-about',
  templateUrl: './about.component.html',
  styleUrls: ['./about.component.scss']
})
export class FtAboutComponent implements OnInit {
  @Input() rtuArray: RtuDto[];
  columnsToDisplay = ['title', 'version', 'version2'];

  globalVarSet: GlobalVarSet;

  constructor() {
    this.globalVarSet = globalVars.globalVarSet;
  }

  ngOnInit() {}
}
