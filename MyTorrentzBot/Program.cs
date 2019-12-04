using System;
using Telegram.Bot;
using System.Threading.Tasks;
using System.Data.SQLite;
using Telegram.Bot.Types.Enums;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace MyTorrentzBot
{
    class Program
    {
        private const string BotToken = "858759117:AAHPjdQEW1NScJlVnZRjKFdccEr0SopRoeM";

        private static TelegramBotClient bot;

        private static SQLiteConnection usersDb;
        private static SQLiteConnection torrentsDb;
        private static SQLiteConnection linksDb;

        private static void Main()
        {
            bot = new TelegramBotClient(BotToken);

            usersDb = new SQLiteConnection("Data Source=users.db; Version=3");
            usersDb.Open();

            torrentsDb = new SQLiteConnection("Data Source=torrents.db3; Version=3");
            torrentsDb.Open();
            
            linksDb = new SQLiteConnection("Data Source=links.db; Version=3");
            linksDb.Open();

            while (true)
            {
                try
                {
                    HandleMessages().Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex);
                }
            }
        }

        private static async Task HandleMessages()
        {
            int offset = 0;

            await bot.SetWebhookAsync(""); //Убираем вебхук

            while (true)
            {
                var updates = await bot.GetUpdatesAsync(offset); //Получили обновления

                foreach (var update in updates)
                {
                    if (update.Type != UpdateType.Message) // Может быть и не сообщение
                        continue;

                    var message = update.Message;
                    var words = message.Text.Split().ToList();

                    var command = words[0];
                    var args = words.Count > 1 ? words.GetRange(1, words.Count - 1) : new List<string>();

                    switch (command)
                    {
                        case "/start":
                            if (message.Chat.Username != null)
                            {
                                SignUpUser(message.Chat.Id.ToString(), message.Chat.Username.ToString());
                                await bot.SendTextMessageAsync(message.Chat.Id, "Приветствую, я Торрент бот, который облегчит тебе поиски фильмов, которые ты так хочешь посмотреть.\nВсе мои команды можешь узнать с помощью /help");
                                Console.WriteLine($"Новый пользователь: {message.Chat.Username}; ID чата: {message.Chat.Id}; Дата,время(GMT 0): {DateTime.UtcNow}\n");
                                break;
                            }
                            else
                            {
                                await bot.SendTextMessageAsync(message.Chat.Id, "Приветствую, я Торрент бот, который облегчит тебе поиски фильмов, которые ты так хочешь посмотреть.\nВсе мои команды можешь узнать с помощью /help");
                                await bot.SendTextMessageAsync(message.Chat.Id, "У тебя не указан юзернейм в настройках аккаунта. Без него мы не сможем отслеживать твою историю запросов и предлагать самые сочные фильмы.");
                                await bot.SendTextMessageAsync(message.Chat.Id, "Укажи свой юзернейм и пропиши снова команду /start");
                                Console.WriteLine($"Пользователь без юзернейма; ID чата: {message.Chat.Id}; Дата,время(GMT 0): {DateTime.UtcNow}\n");
                                break;
                            }

                        case "/help":
                            Console.WriteLine($"Получена команда: {message.Text}; ID отправителя: {message.Chat.Username}; ID чата: {message.Chat.Id}; Дата,время(GMT 0): {DateTime.UtcNow}\n");
                            await bot.SendTextMessageAsync(message.Chat.Id, "Привет, сейчас я покажу тебе все свои возможности...");
                            await bot.SendTextMessageAsync(message.Chat.Id, "/find - позволит начать процесс поиска нужного тебе фильма;");
                            break;
                        
                        case "magnet":
                            var link = words[1];
                            var comment = string.Join(' ', words.Skip(2));
                            Console.WriteLine($"Command: {command}; User: {message.Chat.Username}; Chat ID: {message.Chat.Id}; Time (GMT 0): {DateTime.UtcNow}\n");
                            var addLink = linksDb.CreateCommand();
                            addLink.CommandText = "INSERT INTO Links VALUES(@Link, @Comment)";
                            addLink.Parameters.AddWithValue("@Link", link);
                            addLink.Parameters.AddWithValue("@Comment", comment);
                            addLink.ExecuteNonQuery();
                            await bot.SendTextMessageAsync(message.Chat.Id, $"Комментарий _{comment}_ к ссылке *{link}* добавлен", ParseMode.Markdown);
                            break;

                        default:
                            var filmTitle = string.Join(' ', words);

                            Console.WriteLine($"Command: {command}; User: {message.Chat.Username}; Chat ID: {message.Chat.Id}; Time (GMT 0): {DateTime.UtcNow}\n; Film title: {filmTitle}");

                            var regcmd = torrentsDb.CreateCommand();
                            regcmd.CommandText = $"SELECT hash_info, title FROM torrent WHERE title LIKE @film_title LIMIT 5";
                            regcmd.Parameters.AddWithValue("@film_title", $"%{filmTitle}%");

                            var dbReader = regcmd.ExecuteReader();
                            if (dbReader.HasRows)
                            {
                                var sb = new StringBuilder();

                                sb.AppendLine("Вот, что нашлось:");

                                while (dbReader.Read())
                                {
                                    var description = dbReader.GetString(1);
                                    var magnetHash = dbReader.GetString(0);

                                    sb.AppendLine($"{description}\n*mаgnet:?xt=urn:btih:{dbReader.GetString(0)}*");
                                    sb.AppendLine();
                                }

                                await bot.SendTextMessageAsync(message.Chat.Id, sb.ToString(), ParseMode.Markdown);
                            }
                            else
                            {
                                await bot.SendTextMessageAsync(message.Chat.Id, "Ничего не нашел. Попробовать еще раз?");
                            }

                            break;
                    }

                    offset = update.Id + 1; // Чтобы не приходило много обновлений
                }
            }
        }

        public static void SignUpUser(string chatId, string username) // Метод для регистрации пользователя в базе данных
        {
            try
            {
                var regcmd = usersDb.CreateCommand(); // Создаём команду добавления пользователя
                regcmd.CommandText = "INSERT INTO Regusers VALUES(@chatId, @username)"; // Запрос на добавление пользователя
                regcmd.Parameters.AddWithValue("@chatId", chatId); // Добавляем параметры
                regcmd.Parameters.AddWithValue("@username", username);
                regcmd.ExecuteNonQuery(); // Подставить и выполнить
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }
        }
    }
}
