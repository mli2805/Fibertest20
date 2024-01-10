using System;

namespace Iit.Fibertest.Dto
{
    public static class FrequencyExt
    {
        public static TimeSpan GetTimeSpan(this Frequency frequency)
        {
            return frequency == Frequency.DoNot ? TimeSpan.Zero : TimeSpan.FromHours((int) frequency);
        }

        public static Frequency GetFrequency(this TimeSpan timeSpan)
        {
            return timeSpan == TimeSpan.Zero ? Frequency.DoNot : (Frequency) timeSpan.TotalHours;
        }
    }
}