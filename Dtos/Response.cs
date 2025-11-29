using System.Net;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace EdgePMO.API.Dtos
{
    public record Response
    {
        [JsonPropertyName("isSuccess")]
        public bool IsSuccess { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("code")]
        public HttpStatusCode Code { get; set; }

        [JsonPropertyName("result")]
        public JsonObject Result { get; set; } = new JsonObject();
    }
}
