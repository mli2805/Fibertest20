export class GetSorDataParams {
    isSorFile: string; // for sor file - true, for measurement client - false
    sorFileId: string;
    measGuid: string;
    isBase: string;
    isInVxSorFormat: string; // for display - true, for save - false
    rtuGuid: string;
}