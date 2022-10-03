using System;
using System.Collections.Concurrent;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph.RtuOccupy
{
    public class RtuOccupations
    {
        public ConcurrentDictionary<Guid, RtuOccupationState> RtuStates =
            new ConcurrentDictionary<Guid, RtuOccupationState>();

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
                        return false;
                    }
                }
            }
        }
    }
}