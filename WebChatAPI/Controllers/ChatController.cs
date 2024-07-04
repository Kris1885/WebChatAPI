using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using System.Buffers.Text;
using System.IO;
using System.Net.WebSockets;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using WebChatAPI.DataBase;
using WebChatAPI.Models;

namespace WebChatAPI.Controllers
{
    [ApiController]
    [Route("chat")]
    public class ChatController : ControllerBase
    {
        private static readonly List<User> UsersConnections = new List<User>();

        [Route("connect")]
        public async Task WebSocket()
        {
            var authUser = TokenAuthorization();
            if (authUser == null)
                return;

            var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();
            using var connection = DbConnect.New();
            var command = connection.ExecuteAndRead($"SELECT Messages.Message, Users.Name FROM Messages JOIN Users ON Users.Id=Messages.UserId");
            if (command.HasData)
            {
                do
                {
                    string messagetext = command.GetString(0);
                    string userName = command.GetString(1);
                    string message = userName + ": " + messagetext;

                    byte[] array1 = Encoding.UTF8.GetBytes(message);
                    await ws.SendAsync(array1, WebSocketMessageType.Text, true, CancellationToken.None);
                    Console.WriteLine($"{userName}:  {messagetext}");
                }
                while (await command.NextRow());
            }

            var newUser = new User
            {
                Id = UsersConnections.Count + 1,
                Name = "User",
                Socket = ws,
            };
            UsersConnections.Add(newUser);

            await newUser.AliveSource.Task;
        }

        [HttpPost]
        [Route("")]
        public IActionResult SentMessage(Message message)
        {
            var authUser = TokenAuthorization();
            if (authUser == null)
                return NoAuthResult();

            Console.WriteLine($"Message was sent: {message.Text}");
            byte[] array = Encoding.UTF8.GetBytes(message.Text);
            foreach (var user in UsersConnections)
            {
                user.Socket.SendAsync(array, WebSocketMessageType.Text, true, CancellationToken.None);
            }

            using var connection = DbConnect.New();
            connection.Execute($"INSERT INTO Messages (Message, UserId) VALUES ('{message.Text}',1)");

            string base64 = "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";
            byte[] binImage = Convert.FromBase64String(base64);
            return File(binImage, "image/png");
        }

        [HttpPost]
        [Route("registration")]
        public IActionResult RegistrationUser(RegistrationForm userRegistration)
        {
            using var connection = DbConnect.New();
            var command = connection.ExecuteAndRead($"SELECT * FROM Users WHERE Login = '{userRegistration.Login}'");
            if (command.HasData)
            {
                Console.WriteLine("Такой пользователь уже существует");
                return BadRequest();
            }

            connection.Execute($"INSERT INTO Users (Name, Login, Password) " +
                $"VALUES ('{userRegistration.Name}', '{userRegistration.Login}', '{userRegistration.Password}')");
            Console.WriteLine($"New user: {userRegistration.Name} {userRegistration.Password}");
            return Ok();
        }



        [HttpPost("login")]
        public string Autorization(LoginForm loginForm)
        {
            using var connection = DbConnect.New();
            var matchUser = connection.ExecuteAndRead($"SELECT * FROM Users Where Login = '{loginForm.Login}' AND Password = '{loginForm.Password}'");
            if (!matchUser.HasData)
                return null;

            var user = new User
            {
                Id = matchUser.GetInt32(0),
                Name = matchUser.GetString(1),
            };

            var matchExistToken = connection.ExecuteAndRead($"SELECT * FROM Tokens WHERE UserId ='{user.Id}'");
            if (matchExistToken.HasData)
            {
                return matchExistToken.GetString(0);
            }

            string token = Guid.NewGuid().ToString();
            connection.Execute($"INSERT INTO Tokens(Token, UserId) VALUES ('{token}', {user.Id})");
            return token;
        }

        /// <summary>
        /// Этот метод, проверяет текущий HTTP запрос на наличие заголовка авторизации.
        /// После этого пытается по значению (AuthToken) получить модель пользователя, который ранее
        /// был авторизован по этому токену.
        /// Если ничего этого небыло, то этот метод вернет NULL!
        /// </summary>
        public User TokenAuthorization()
        {
            if (!HttpContext.Request.Headers.TryGetValue("Authorization", out var headValue))
                return null;

            string headToken = headValue;
            string[] split = headToken.Split(' ');
            if (split.Length != 2)
                return null;

            if (split[0] != "Bearer")
                return null;

            string token = split[1];

            using var connection = DbConnect.New();
            var command = connection.ExecuteAndRead($"SELECT * FROM Tokens WHERE Token = '{token}'");
            if (command.HasData)
            {
                int userId = command.GetInt32(1);
                var getUserCommand = connection.ExecuteAndRead($"SELECT * FROM Users Where Id={userId}");

                var user = new User
                {
                    Id = userId,
                    Name = getUserCommand.GetString(1),
                };
                return user;
            }
            return null;
        }

        private NoAuthResultModel NoAuthResult()
        {
            return new NoAuthResultModel();
        }
    }
}
