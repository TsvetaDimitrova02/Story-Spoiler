using System;
using System.Text.Json.Serialization;

namespace StorySpoiler.Models
{
	public class StoryDTO
	{
        [JsonPropertyName("title")]

        public string? Title { get; set; }

        [JsonPropertyName("description")]

        public string? Description { get; set; }

        [JsonPropertyName("url")]

        public string? url { get; set; }
    }
}

