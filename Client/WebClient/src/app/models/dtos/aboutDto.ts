export class AboutDto {
  dcSoftware: string;
  webApiSoftware: string;
  webClientSoftware: string;
  rtus: AboutRtuDto[];
}

export class AboutRtuDto {
  title: string;
  version: string;
  version2: string;
}
