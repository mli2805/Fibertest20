export class GetSorDataParams {
    isSorFileOrMeasurementClient: string; // for sor file - true, for measurement client - false
    sorFileId: string;
    measGuid: string;
    isBase: string;
    isForDisplayOrSaveFormat: string; // for display - true, for save - false
    rtuGuid: string;
}