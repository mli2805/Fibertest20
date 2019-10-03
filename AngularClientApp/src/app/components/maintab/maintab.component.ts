import { Component, OnInit } from '@angular/core';
import { RtuApiService } from 'src/app/api/rtu-api.service';
import { RtuDto } from 'src/app/models/rtuDto';

@Component({
  selector: 'ft-maintab',
  templateUrl: './maintab.component.html',
})

export class MainTabComponent implements OnInit {

  private rtus: RtuDto[];

  constructor(private rtuService: RtuApiService) {}

  ngOnInit() {
    this.rtuService.getAllRtu().subscribe((res: RtuDto[]) => {
      console.log(res);
      this.rtus = res;
    });
  }
}
