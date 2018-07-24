/*
* Copyright © 2018 Cloudveil Technology Inc.  
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CitadelService.Data.Time
{
    public class TimeRange
    {
        private static readonly LocalTime s_endOfDay;
        private static readonly LocalTime s_startOfDay;

        static TimeRange()
        {
            s_startOfDay = new LocalTime(0, 0, 0);
            s_endOfDay = new LocalTime(23, 59, 59, 999);
        }

        public TimeRange(LocalTime start, LocalTime end)
        {
            Start = start;
            End = end;

            m_timeComparison = start.CompareTo(end);
        }

        public LocalTime Start { get; private set; }
        public LocalTime End { get; private set; }

        /// <summary>
        /// This is the result from start.CompareTo(end) so we don't have to run that every time we run IsTimeOfDayInRange
        /// </summary>
        private int m_timeComparison;

        public bool IsTimeOfDayInRange(LocalTime time)
        {
            // If Start < End then we don't have to consider whether the time of day might be between 00:00 and End

            if(m_timeComparison < 0)
            {
                // Start < End, just see if time is in between the two.
                return time.CompareTo(Start) >= 0 && time.CompareTo(End) <= 0;
            }
            else if(m_timeComparison > 0)
            {
                // Start > End, compare time against Start - s_endOfDay and s_startOfDay - End
                return (time.CompareTo(Start) >= 0 && time.CompareTo(s_endOfDay) <= 0) || (time.CompareTo(s_startOfDay) >= 0 && time.CompareTo(End) <= 0);
            }

            return false;
        }

        public bool IsTimeOfDayInRange(OffsetTime time)
        {
            return IsTimeOfDayInRange(time.TimeOfDay);
        }
    }
}
