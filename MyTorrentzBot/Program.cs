using System;
using Telegram.Bot;//Библиотека Telegram.Bot
using System.Threading.Tasks;//библиотека для ассинхронных потоков
using System.Data.SQLite;//Библиотека поддержики SQLite баз данных
using System.Net;//Получение ссылок со страницы сайта
using MihaZupan;//Обход блокировки телеграм через прокси4   
namespace MyTorrentzBot
{
    class Program
    {
        const string TOKEN = "858759117:AAHPjdQEW1NScJlVnZRjKFdccEr0SopRoeM";//переменная с токеном бота типа константа. TOKEN с ошибкой, так как ещё его не использовали
        public static SQLiteConnection DB;//Переменная с базой данных
        static void Main(string[] args)
        {
            while (true)
            {
                try//обработчик  исключений. Отлавливает исключения, вызванные в ходе выполнения работы
                {
                    GetMessage().Wait();//здесь будет основная задача бота, метод
                }
                catch (Exception ex)//здесь ошибка, если срабатывает исключение. Выводит в консоль
                {
                    Console.WriteLine("Error: " + ex);
                }
            }
        }
        static async Task GetMessage()//метод, выполняющие авторизацию и получающий обновления от телеграм бота. Он статический и ассинхронный. Второй поток типа
        {
            var socksProxy = new HttpToSocks5Proxy("183.102.71.77", 8888);
            TelegramBotClient bot = new TelegramBotClient(TOKEN, socksProxy);//Переменная авторизации бота.
            int offset = 0;//тупа нужно
            int timeout = 0;
            try
            {
                await bot.SetWebhookAsync("");//Убираем вебхук
                while (true)//Вытаскиваем сообщения из обновления бота
                {
                    var updates = await bot.GetUpdatesAsync(offset, timeout);//Получили обновления
                    foreach (var update in updates)//
                    {
                        var message = update.Message;//Получили сообщение
                        /*if (message.Text == "MyFirstBot")//проверяем содержимое сообщения
                        {
                            Console.WriteLine("Получено сообщение: " + message.Text + " ; ID отправителя: " + message.Chat.Username + " ; ID чата: " + message.Chat.Id + " ; Дата,время(GMT 0): " + DateTime.UtcNow + "\n");//Вывели сообщение в консоль
                            await bot.SendTextMessageAsync(message.Chat.Id, "Привет, создатель, я твой бот!");//ответ на сообщение
                            await bot.SendTextMessageAsync(message.Chat.Id, "Сообщение отправлено пользователем - " + message.Chat.Username);
                        }//MyFirstBot
                        if (message.Text == "/reg")
                        {
                            Registration(message.Chat.Id.ToString(), message.Chat.Username.ToString());
                            await bot.SendTextMessageAsync(message.Chat.Id, "Пользователь зарегистрирован");
                            Console.WriteLine("Зарегистрирован пользователь с ID: " + message.Chat.Username + " ; ID чата: " + message.Chat.Id + "  ; Дата, время(GMT 0): " + DateTime.UtcNow + "\n");
                        }// /reg
                        if (message.Text == "/help")
                        {
                            Console.WriteLine("Получено сообщение: " + message.Text + " ; ID отправителя: " + message.Chat.Username + " ; ID чата: " + message.Chat.Id + " ; Дата,время(GMT 0): " + DateTime.UtcNow + "\n");
                            await bot.SendTextMessageAsync(message.Chat.Id, "Привет, сейчас я покажу тебе все свои возможности...");
                            await bot.SendTextMessageAsync(message.Chat.Id, "/reg - позволит тебе зарегистрироваться у нас, чтобы в дальнейшем получать рекомендации по самым лучшим фильмам, которые могут тебе понравится;");

                        }// /help*/

                        switch (message.Text)
                        {
                            case "/start":
                                Registration(message.Chat.Id.ToString(), message.Chat.Username.ToString());
                                await bot.SendTextMessageAsync(message.Chat.Id, "Приветствую, я я Торрент бот, который облегчит тебе поиски фильмов, которые ты так хочешь посмотреть.\nВсе мои команды можешь узнать с помощью /help");
                                Console.WriteLine("Новый пользователь: " + message.Chat.Username + " ; ID чата: " + message.Chat.Id + " ; Дата,время(GMT 0): " + DateTime.UtcNow + "\n");
                                break;
                            case "MyFirstBot":
                                Console.WriteLine("Получено сообщение: " + message.Text + " ; ID отправителя: " + message.Chat.Username + " ; ID чата: " + message.Chat.Id + " ; Дата,время(GMT 0): " + DateTime.UtcNow + "\n");
                                await bot.SendTextMessageAsync(message.Chat.Id, "Привет, создатель, я твой бот!");
                                await bot.SendTextMessageAsync(message.Chat.Id, "Сообщение отправлено пользователем - " + message.Chat.Username);
                                break;
                            case "/help":
                                Console.WriteLine("Получена команда: " + message.Text + " ; ID отправителя: " + message.Chat.Username + " ; ID чата: " + message.Chat.Id + " ; Дата,время(GMT 0): " + DateTime.UtcNow + "\n");
                                await bot.SendTextMessageAsync(message.Chat.Id, "Привет, сейчас я покажу тебе все свои возможности...");
                                await bot.SendTextMessageAsync(message.Chat.Id, "/find - позволит начать процесс поиска нужного тебе фильма;");
                                break;
                            /*case "/reg":
                                Registration(message.Chat.Id.ToString(), message.Chat.Username.ToString());
                                await bot.SendTextMessageAsync(message.Chat.Id, "Пользователь зарегистрирован");
                                Console.WriteLine("Зарегистрирован пользователь с ID: " + message.Chat.Username + " ; ID чата: " + message.Chat.Id + "  ; Дата, время(GMT 0): " + DateTime.UtcNow + "\n");
                                break;*/
                            case "/find":
                                Console.WriteLine("Получена команда: " + message.Text + " ; ID отправителя: " + message.Chat.Username + " ; ID чата: " + message.Chat.Id + " ; Дата,время(GMT 0): " + DateTime.UtcNow + "\n");
                                await bot.SendTextMessageAsync(message.Chat.Id, "Напиши название фильма, который ты хочешь найти");
                                string messageTextOne = message.Text;
                                string nameFilm = null;
                                while (true)
                                {
                                    var up = await bot.GetUpdatesAsync(offset, timeout);
                                    foreach (var upp in up)
                                    {
                                        message = upp.Message;
                                    }
                                    if (messageTextOne != message.Text)
                                    {
                                        await bot.SendTextMessageAsync(message.Chat.Id, "Сейчас глянем, что у нас есть по запросу " + message.Text);
                                        Console.WriteLine("Получено сообщение: " + message.Text + " ; ID отправителя: " + message.Chat.Username + " ; ID чата: " + message.Chat.Id + " ; Дата,время(GMT 0): " + DateTime.UtcNow + "\n");
                                        nameFilm = message.Text;
                                        // Поиск по качеству фильма И выдёргивание торрент файла, крч всё осноное здесь должно быть
                                        message.Text = "С этим разобрались. Жду твоих команд, друг";
                                        await bot.SendTextMessageAsync(message.Chat.Id, message.Text);
                                        break;
                                    }
                                }
                                break;
                            default:
                                Console.WriteLine("Получено незарегистрированное сообщение: " + message.Text + " ; ID отправителя: " + message.Chat.Username + " ; ID чата: " + message.Chat.Id + " ; Дата,время(GMT 0): " + DateTime.UtcNow + "\n");
                                //await bot.SendTextMessageAsync(message.Chat.Id, "Я не знаю такой команды. Воспользуйтесь функцией /help");
                                break;
                        }

                        offset = update.Id + 1;//чтобы не приходило много обновлений
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }
        }
        public static void Registration(string chatId, string username)//Метод для регистрации пользователя в базе данных
        {
            try
            {
                DB = new SQLiteConnection("Data Source=DB.db; Version=3");//Присваиваем переменной значение базы данных
                DB.Open();//Открыли базу данных
                SQLiteCommand regcmd = DB.CreateCommand();//Создаём команду добавления пользователя
                regcmd.CommandText = "INSERT INTO Regusers VALUES(@chatId, @username)";//Запрос на добавление пользователя
                regcmd.Parameters.AddWithValue("@chatId", chatId);//Добавляем параметры
                regcmd.Parameters.AddWithValue("@username", username);
                regcmd.ExecuteNonQuery();//Подставить и выполнить
                DB.Close();//Закрыли базу данных
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }
        }// /reg
    }
}
