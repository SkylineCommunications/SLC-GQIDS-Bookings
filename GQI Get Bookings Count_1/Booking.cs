namespace GQI_Get_Bookings_Count_1
{
    using System;
    using Newtonsoft.Json;

    public class Booking
    {
        public CustomData CustomData { get; set; }
    }

    public class CustomData
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("start")]
        public DateTime Start { get; set; }

        [JsonProperty("end")]
        public DateTime End { get; set; }
    }
}
