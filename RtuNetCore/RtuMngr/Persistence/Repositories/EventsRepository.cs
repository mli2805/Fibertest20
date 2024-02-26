﻿using Microsoft.EntityFrameworkCore;

namespace Iit.Fibertest.RtuMngr;

public class EventsRepository
{
    private readonly RtuContext _rtuContext;

    public EventsRepository(RtuContext rtuContext)
    {
        _rtuContext = rtuContext;
    }

    public async Task Add(string json)
    {
        _rtuContext.Events.Add(new DtoInDbEf() { Registered = DateTime.Now, Json = json });
        await _rtuContext.SaveChangesAsync();
    }

    public async Task<List<DtoInDbEf>> GetPortion(int portion)
    {
        try
        {
            IQueryable<DtoInDbEf> query = _rtuContext.Events;
            query = query.OrderBy(e => e.Registered).Take(portion);
            return await query.ToListAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}