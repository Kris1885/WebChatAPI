using System.Net.WebSockets;
using System.Text.Json.Serialization;

namespace WebChatAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [JsonIgnore]
        public string Login {  get; set; }
        
        [JsonIgnore]
        public string Password { get; set; }
                
        [JsonIgnore]
        public WebSocket Socket { get; init; }
        
        [JsonIgnore]
        public TaskCompletionSource AliveSource { get; private set; } = new TaskCompletionSource();
    }
}
