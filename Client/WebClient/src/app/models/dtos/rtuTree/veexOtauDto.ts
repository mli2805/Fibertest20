export class VeexOtauDto {
    connected: boolean;
    id: string;
    connectionParameters: VeexOtauConnectionParameters;
    inputPortCount: number;
    isFwdm: boolean;
    model: string;
    portCount: number;
    protocol: string;
    serialNumber: string;
}

export class VeexOtauConnectionParameters {
    address: string;
    port: number;
    protocol: string;
}