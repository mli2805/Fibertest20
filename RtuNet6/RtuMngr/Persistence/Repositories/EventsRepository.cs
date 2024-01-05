﻿using Microsoft.EntityFrameworkCore;

namespace Iit.Fibertest.RtuMngr
{
    public class EventsRepository
    {
        private readonly RtuContext _rtuContext;

        public EventsRepository(RtuContext rtuContext)
        {
            _rtuContext = rtuContext;
        }

        public async Task Add(string json)
        {
            await _rtuContext.Events.AddAsync(new EventInJsonEf(){Registered = DateTime.Now, Json = json});
            await _rtuContext.SaveChangesAsync();
        }

        public async Task<List<EventInJsonEf>> GetPortion(int portion)
        {
            IQueryable<EventInJsonEf> query = _rtuContext.Events;
            query = query.OrderBy(e => e.Registered).Take(portion);
            return await query.ToListAsync();
        }
    }
}
