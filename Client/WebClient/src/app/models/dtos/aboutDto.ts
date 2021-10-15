export class AboutDto {
  dcSoftware: string;
  webApiSoftware: string;
  webClientSoftware: string;
  rtus: AboutRtuDto[];
}

export class AboutRtuDto {
  title: string;
  model: string;
  serial: string;
  version: string;
}
