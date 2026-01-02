using EdgePMO.API.Models;
using EdgePMO.API.Settings;
using System.Text.Json.Serialization;

namespace EdgePMO.API.Dtos
{
    public record CreateContentBlockDto
    {
        public string Type { get; set; } = null!;

        [JsonConverter(typeof(ContentConverter))]
        public object Content { get; set; } = null!;

        public int Order { get; set; }
    }

    public static class ContentBlockExtensions
    {
        public static string GetParagraphContent(this ContentBlock block)
        {
            if (block.Type != "paragraph")
                throw new InvalidOperationException("Block type is not 'paragraph'");

            return (string)ContentBlockSerializer.Deserialize(block.Content, block.Type);
        }

        public static List<string> GetListContent(this ContentBlock block)
        {
            if (block.Type != "list")
                throw new InvalidOperationException("Block type is not 'list'");

            return (List<string>)ContentBlockSerializer.Deserialize(block.Content, block.Type);
        }

        public static object GetContent(this ContentBlock block)
        {
            return ContentBlockSerializer.Deserialize(block.Content, block.Type);
        }
    }
}
