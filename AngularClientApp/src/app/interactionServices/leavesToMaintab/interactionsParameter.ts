import { InteractionsCommandType } from './interactionsCommandType';

export class InteractionsParameter {
  commandType: InteractionsCommandType;

  rtuId: string;
  traceId: string;
  port: number;
}
