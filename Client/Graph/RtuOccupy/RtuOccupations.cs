using System;
using System.Collections.Concurrent;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Graph.RtuOccupy
{
    public class RtuOccupations
    {
        private readonly IMyLog _logFile;

        public ConcurrentDictionary<Guid, RtuOccupationState> RtuStates =
            new ConcurrentDictionary<Guid, RtuOccupationState>();

        public RtuOccupations(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public bool TrySetOccupation(Guid rtuId, RtuOccupation rtuOccupation, string userName, out RtuOccupationState state)
        {
            if (rtuOccupation == RtuOccupation.None)
            {
                state = new RtuOccupationState();
                return RtuStates.TryRemove(rtuId, out RtuOccupationState _);
            }
            else
            {
                var isBusy = RtuStates.TryGetValue(rtuId, out RtuOccupationState currentState);
                if (!isBusy)
                {
                    state = new RtuOccupationState() { RtuOccupation = RtuOccupation.None };
                    return RtuStates.TryAdd(rtuId,
                        new RtuOccupationState()
                        {
                            RtuId = rtuId,
                            RtuOccupation = rtuOccupation,
                            UserName = userName,
                            Expired = DateTime.Now.AddMinutes(3)
                        });
                }
                else
                {
                    state = currentState;
                    if (currentState.UserName == userName || currentState.Expired < DateTime.Now)
                    {
                        return RtuStates.TryUpdate(rtuId, new RtuOccupationState()
                        {
                            RtuId = rtuId,
                            RtuOccupation = rtuOccupation,
                            UserName = userName,
                            Expired = DateTime.Now.AddMinutes(3)
                        }, state);
                    }
                    else
                    {
                        var cs = $@"(current state is {currentState.RtuOccupation}, expiration {currentState.Expired})";
                        _logFile.AppendLine($@"RTU {rtuId.First6()} is occupied by {currentState.UserName} {cs}");
                        return false;
                    }
                }
            }
        }
    }
}