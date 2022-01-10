using Newtonsoft.Json;

namespace BlazorReviews.Data
{
    public class Review
    {
        [JsonProperty("reviewText")]
        public string reviewText { get; set; }

        [JsonProperty("reviewRating")]
        public decimal reviewRating { get; set; }
    }
}
