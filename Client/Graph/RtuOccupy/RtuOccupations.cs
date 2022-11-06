using System;
using System.Collections.Concurrent;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Graph.RtuOccupy
{
    public class RtuOccupations
    {
        private readonly IMyLog _logFile;
        private const int TimeoutSec = 100;

        public ConcurrentDictionary<Guid, RtuOccupationState> RtuStates =
            new ConcurrentDictionary<Guid, RtuOccupationState>();

        public RtuOccupations(IMyLog logFile)
        {
            _logFile = logFile;
        }

        // checks and if possible set occupation
        public bool TrySetOccupation(Guid rtuId, RtuOccupation newRtuOccupation, string userName, out RtuOccupationState state)
        {
            var action = newRtuOccupation == RtuOccupation.None
                ? $@"free RTU {rtuId.First6()}"
                : $@"occupy RTU {rtuId.First6()} for {newRtuOccupation}";
            _logFile.AppendLine($@"Client {userName} asked to {action}");
            if (newRtuOccupation == RtuOccupation.None)
            {
                /////////////  it is a CHECK or CLEANUP  //////////////////////
                if (!RtuStates.TryGetValue(rtuId, out state))
                {
                    _logFile.AppendLine($@"RTU {rtuId.First6()} is free already");
                    return true;
                }

                if (state.UserName != userName)
                {
                    _logFile.AppendLine(
                        $@"{userName} can't free RTU {rtuId.First6()}, cos it's occupied by {state.UserName}");
                    return false;
                }

                if (RtuStates.TryRemove(rtuId, out state))
                {
                    _logFile.AppendLine($@"RTU {rtuId.First6()} is free now");
                    return true;
                }

                _logFile.AppendLine(@"Something went wrong while dictionary cleanup!");
                return false;
            }

            if (!RtuStates.TryGetValue(rtuId, out RtuOccupationState currentState)) 
            { ////////// NEW OCCUPATION /////////////////
                state = new RtuOccupationState() { RtuOccupation = RtuOccupation.None };
                if (RtuStates.TryAdd(rtuId,
                        new RtuOccupationState()
                        {
                            // RtuId = rtuId,
                            RtuOccupation = newRtuOccupation,
                            UserName = userName,
                            Expired = DateTime.Now.AddSeconds(TimeoutSec),
                        }))
                {
                    _logFile.AppendLine($@"Applied! RTU {rtuId.First6()} is occupied by {userName} for {newRtuOccupation}");
                    return true;
                }
                _logFile.AppendLine(@"Something went wrong while dictionary addition!");
                return false;
            }

            state = currentState;
            if (currentState.UserName == userName || currentState.Expired < DateTime.Now)
            {  /////////  REFRESH   //////////////////
                if (RtuStates.TryUpdate(rtuId, new RtuOccupationState()
                    {
                        // RtuId = rtuId,
                        RtuOccupation = newRtuOccupation,
                        UserName = userName,
                        Expired = DateTime.Now.AddSeconds(TimeoutSec)
                    }, state))
                {
                    _logFile.AppendLine($@"Applied! RTU {rtuId.First6()} is occupied by {userName} for {newRtuOccupation}");
                    return true;
                }
                _logFile.AppendLine(@"Something went wrong while dictionary update!");
                return false;
            }

            ///////// DENY ///////////////
            var cs = $@"(current state is {currentState.RtuOccupation}, expires at {currentState.Expired:HH:mm:ss})";
            _logFile.AppendLine($@"Denied! RTU {rtuId.First6()} is occupied by {currentState.UserName} {cs}");
            return false;
        }
    }
}