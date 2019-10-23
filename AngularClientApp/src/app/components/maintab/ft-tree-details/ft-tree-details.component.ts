import { Component, OnInit, Input } from '@angular/core';
import { DetailsViewModel } from './detailsViewModel';
import { InteractionsCommandType } from '../interactionsCommandType';

@Component({
  selector: 'ft-tree-details',
  templateUrl: './ft-tree-details.component.html',
  styleUrls: ['./ft-tree-details.component.scss']
})
export class FtTreeDetailsComponent implements OnInit {
  @Input() detailsVm: DetailsViewModel;

  InteractionsCommandType = InteractionsCommandType;

  constructor() {}

  ngOnInit() {
    this.detailsVm = new DetailsViewModel(InteractionsCommandType.Nothing);
  }
}
