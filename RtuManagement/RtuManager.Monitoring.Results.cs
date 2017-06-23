using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.Utils35;

namespace RtuManagement
{
    public partial class RtuManager
    {
        private void SendMoniResult(MoniResult moniResult)
        {
            _rtuLog.AppendLine($"Sending monitoring result {moniResult.BaseRefType} to server...");

        }

        // only is trace OK or not, without details of breakdown if any
        private PortMeasResult GetPortState(MoniResult moniResult)
        {
            if (!moniResult.IsFailed && !moniResult.IsFiberBreak && !moniResult.IsNoFiber)
                return PortMeasResult.Ok;

            return moniResult.BaseRefType == BaseRefType.Fast
                ? PortMeasResult.BrokenByFast
                : PortMeasResult.BrokenByPrecise;
        }

    }
}

/* 
private static void SendMoniResult(MoniResult moniResult)
{
//                            var queueName = @".\private$\F20";
    var queueName = @"FormatName:Direct=TCP:192.168.96.8\private$\F20";
    //                var queueName = @"FormatName:Direct=TCP:192.168.96.52\private$\F22";
    //                var queueName = @"FormatName:Direct=OS:opx-lmarholin\private$\F22";

    // if (!MessageQueue.Exists(queueName)) // works only for local machine queue/address format
    //     break;

    using (MessageQueue queue = new MessageQueue(queueName))
    {
        var binaryMessageFormatter = new BinaryMessageFormatter();
        using (var message = new Message(moniResult, binaryMessageFormatter))
        {
            message.Recoverable = true;
            queue.Send(message, MessageQueueTransactionType.Single);
//                    queue.Send(message);
        }
    }

}
*/
