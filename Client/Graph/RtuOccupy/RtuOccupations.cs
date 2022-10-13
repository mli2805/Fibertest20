﻿using System;
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

        public bool TrySetOccupation(Guid rtuId, RtuOccupation newRtuOccupation, string userName, out RtuOccupationState state)
        {
            if (newRtuOccupation == RtuOccupation.None)
            {  /////////////  CLEAR  //////////////////////
                state = new RtuOccupationState() { RtuOccupation = RtuOccupation.None };
                return RtuStates.TryRemove(rtuId, out RtuOccupationState _);
            }
            else
            {
                var isBusy = RtuStates.TryGetValue(rtuId, out RtuOccupationState currentState);
                if (!isBusy) ////////// NEW /////////////////
                {
                    state = new RtuOccupationState() { RtuOccupation = RtuOccupation.None };
                    return RtuStates.TryAdd(rtuId,
                        new RtuOccupationState()
                        {
                            RtuId = rtuId,
                            RtuOccupation = newRtuOccupation,
                            UserName = userName,
                            Expired = DateTime.Now.AddSeconds(TimeoutSec),
                        });
                }
                else
                {
                    state = currentState;
                    if (currentState.UserName == userName || currentState.Expired < DateTime.Now)
                    {  /////////  REFRESH   //////////////////
                        return RtuStates.TryUpdate(rtuId, new RtuOccupationState()
                        {
                            RtuId = rtuId,
                            RtuOccupation = newRtuOccupation,
                            UserName = userName,
                            Expired = DateTime.Now.AddSeconds(TimeoutSec)
                        }, state);
                    }
                    else
                    {   ///////// DENY ///////////////
                        var cs = $@"(current state is {currentState.RtuOccupation}, expires at {currentState.Expired:HH:mm:ss})";
                        _logFile.AppendLine($@"RTU {rtuId.First6()} is occupied by {currentState.UserName} {cs}");
                        return false;
                    }
                }
            }
        }
    }
}