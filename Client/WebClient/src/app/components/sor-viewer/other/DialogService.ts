import { IDialogService } from "@veex/common";
export class DialogService implements IDialogService {
  askYesNo(message: string): Promise<boolean> {
    const result = confirm(message);
    return Promise.resolve(result);
  }
}
