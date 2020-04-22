import { Component, OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { FtComponentDataProvider } from "src/app/providers/ft-component-data-provider";
import { RtuDto } from "src/app/models/dtos/rtuTree/rtuDto";
import { OtauPortDto } from "src/app/models/underlying/otauPortDto";
import { AttachOtauDto } from "src/app/models/dtos/port/attachOtauDto";
import { NetAddress } from "src/app/models/underlying/netAddress";
import { OtauAttachedDto } from "src/app/models/dtos/port/otauAttachedDto";
import { OneApiService } from "src/app/api/one.service";

@Component({
  selector: "ft-port-attach-otau",
  templateUrl: "./ft-port-attach-otau.component.html",
  styleUrls: ["./ft-port-attach-otau.component.css"],
})
export class FtPortAttachOtauComponent implements OnInit {
  rtu: RtuDto;
  mainPort: number;
  otau: OtauPortDto = new OtauPortDto();
  serial;
  portCount;

  public isSpinnerVisible = false;
  public isButtonDisabled = false;
  public resultMessage: string;

  ipAddress = "192.168.96.57";

  constructor(
    private router: Router,
    private dataStorage: FtComponentDataProvider,
    private oneApiService: OneApiService
  ) {}

  /* tslint:disable:no-string-literal */
  ngOnInit() {
    this.rtu = this.dataStorage.data["selectedRtu"];
    this.mainPort = this.dataStorage.data["selectedPort"].opticalPort;
  }

  attachOtau() {
    this.isButtonDisabled = true;
    this.isSpinnerVisible = true;
    const cmd = new AttachOtauDto();
    cmd.RtuId = this.rtu.rtuId;
    cmd.RtuMaker = this.rtu.rtuMaker;
    cmd.OpticalPort = this.mainPort;
    cmd.OtauAddress = new NetAddress();
    cmd.OtauAddress.Ip4Address = this.ipAddress;
    cmd.OtauAddress.IsAddressSetAsIp = true;
    cmd.OtauAddress.Port = 11834;
    console.log(cmd);
    this.oneApiService
      .postRequest("port", "attach-otau", cmd)
      .subscribe((res: OtauAttachedDto) => {
        console.log(res);
        if (res.isAttached) {
          this.resultMessage = "Otau attached succesfully!";
          this.serial = res.serial;
          this.portCount = res.portCount;
          this.isButtonDisabled = false;
          this.isSpinnerVisible = false;
        } else {
          this.resultMessage = res.errorMessage;
          this.isButtonDisabled = false;
          this.isSpinnerVisible = false;
        }
      });
  }

  cancel() {
    this.router.navigate(["/rtu-tree"]);
  }
}
