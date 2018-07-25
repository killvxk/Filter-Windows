/*
* Copyright © 2018 Cloudveil Technology Inc.  
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

using Citadel.Core.Windows.Util;
using Newtonsoft.Json;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CitadelService.Data.Time
{
    public class StringToTimeRangeConverter : JsonConverter
    {
        private NLog.Logger m_logger;

        public StringToTimeRangeConverter() : base()
        {
            m_logger = LoggerUtil.GetAppWideLogger();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TimeRange);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string timespanString = reader.Value.ToString();

            // We expect a string in the format 15:00-7:00
            string[] times = timespanString.Split('-');

            LocalTime startTime, endTime;

            if(!tryParseTime(times[0], out startTime))
            {
                m_logger.Error($"Failed to parse start time {times[0]}");
                return null;
            }

            if(!tryParseTime(times[1], out endTime))
            {
                m_logger.Error($"Failed to parse end time {times[1]}");
                return null;
            }

            return new TimeRange(startTime, endTime);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // TODO: Maybe someday we'll need to serialize the JSON, but not today.
            throw new NotImplementedException();
        }

        private bool tryParseTime(string timeString, out LocalTime time)
        {
            string[] timeParts = timeString.Split(':');

            int hours = 0, minutes = 0, seconds = 0;

            if(timeParts.Length >= 3)
            {
                if(!int.TryParse(timeParts[2], out seconds))
                {
                    m_logger.Error($"seconds parse failed {timeParts[2]}");
                    time = LocalTime.MinValue;
                    return false;
                }
            }

            if (timeParts.Length >= 2)
            {
                if(!int.TryParse(timeParts[1], out minutes))
                {
                    m_logger.Error($"minutes parse failed {timeParts[1]}");
                    time = LocalTime.MinValue;
                    return false;
                }
            }

            if(timeParts.Length >= 1)
            {
                if(!int.TryParse(timeParts[0], out hours))
                {
                    m_logger.Error($"hours parse failed {timeParts[0]}");
                    time = LocalTime.MinValue;
                    return false;
                }
            }

            time = LocalTime.FromHourMinuteSecondMillisecondTick(hours, minutes, seconds, 0, 0);
            return true;
        }
    }
}
