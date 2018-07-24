/*
* Copyright © 2018 Cloudveil Technology Inc.  
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

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
                return null;
            }

            if(!tryParseTime(times[1], out endTime))
            {
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
                    time = LocalTime.MinValue;
                    return false;
                }
            }

            if (timeParts.Length >= 2)
            {
                if(!int.TryParse(timeParts[1], out minutes))
                {
                    time = LocalTime.MinValue;
                    return false;
                }
            }

            if(timeParts.Length >= 1)
            {
                if(!int.TryParse(timeParts[0], out hours))
                {
                    time = LocalTime.MinValue;
                    return false;
                }
            }

            time = LocalTime.FromHourMinuteSecondMillisecondTick(hours, minutes, seconds, 0, 0);
            return true;
        }
    }
}
