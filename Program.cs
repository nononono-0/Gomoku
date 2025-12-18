using play;
using System.Text.Json;

namespace play
{
    //Создаем базовый класс игрока
    class Player
    {
        //Создаем приватные поля
        private string name;
        private int color;

        //Создание конструктора игрока
        public Player(string playerName, int playerColor)
        {
            color = playerColor;
            name = playerName ?? "Игрок"; //Проверяем playerName, если равен null, будет равен "Игрок"
        }

        //Свойства для доступа (инкапсуляция)
        public string Name
        {
            get { return name; }
        }

        public int Color
        {
            get { return color; }
            set { color = value; } //Создаем возможность менять цвет после создания
        }

        //Метод получения цвета фишек игрока
        public string PlayerColor()
        {
            switch (color)
            {
                case 1: return "черные";
                case 2: return "белые";
                default: return "цвет отсутствует";
            }
        }
    }

    //Базовый класс для наследования
    class Game
    {
        public virtual string GameName()//Два вида инициализации
        {
            return "Игра";
        }

        //Виртуальный метод - полиморфизм
        public virtual void Rules()
        {
            Console.WriteLine("Правила игры.");
        }
    }

    //Класс наследования - Гомоку 
    class Gomoku : Game
    {

        //Переопределение метода - полиморфизм
        public override string GameName()
        {
            return "Гомоку";
        }

        public override void Rules()
        {
            Console.WriteLine("ПРАВИЛА ИГРЫ ГОМОКУ");
            Console.WriteLine("Игра ведется на поле размером 15×15.");
            Console.WriteLine("Играют две стороны - 'черные' и 'белые'.");
            Console.WriteLine("Первым делает ход игрок с черными фишками,");
            Console.WriteLine("далее ходы делаются по очереди.");
            Console.WriteLine("Цель игры - первым построить камнями своего цвета");
            Console.WriteLine("непрерывный ряд из пяти камней в горизонтальном,");
            Console.WriteLine("вертикальном или диагональном направлении.");
        }
    }

    //Класс для сохранения результатов игры
    class GameResult
    {
        //Пустой конструктор для десериализации
        public GameResult()
        {

        }
        
        public string Player1 { get; set; }
        public string Player2 { get; set; }
        public string Winner { get; set; }
        public DateTime Date { get; set; }
        public int MovesCounter { get; set; }
        public string Result { get; set; } //"победа" или "ничья"
        public int BoardSize { get; set; }
        public string BlackPlayer { get; set; }  //Кто играл черными
        public string WhitePlayer { get; set; }  //Кто играл белыми
        public string FirstPlayer { get; set; }  //Кто ходил первым

        public GameResult(string player1, string player2, string winner, int moves, string result,
                         string blackPlayer, string whitePlayer, string firstPlayer)
        {
            Player1 = player1;
            Player2 = player2;
            Winner = winner;
            Date = DateTime.Now;
            MovesCounter = moves;
            Result = result;
            BoardSize = 15; //Размер доски фиксированный в нашей игре - 15 на 15
            BlackPlayer = blackPlayer;
            WhitePlayer = whitePlayer;
            FirstPlayer = firstPlayer;
        }
    }

    //Класс для сохранения текущей игры
    class GameSave
    {
        public int[][] Board { get; set; } //Используем int[][] вместо int[,] для совместимости с json
        public string Player1Name { get; set; }
        public string Player2Name { get; set; }
        public string BlackPlayer { get; set; }
        public string WhitePlayer { get; set; }
        public string CurrentPlayer { get; set; }
        public int CurrentPlayerColor { get; set; }
        public int MoveCounter { get; set; }
        public DateTime SaveTime { get; set; }

        //Конструктор
        public GameSave()
        {
            //Создаем массив строк и для каждой строки создаем массив столбцов
            Board = new int[15][];
            for (int i = 0; i < 15; i++)
            {
                Board[i] = new int[15];
            }
            //Инициализируем строковые свойства пустыми строками для избежания исключений с null
            Player1Name = "";
            Player2Name = "";
            BlackPlayer = "";
            WhitePlayer = "";
            CurrentPlayer = "";
            CurrentPlayerColor = 0;
            MoveCounter = 0;
            SaveTime = DateTime.MinValue;
        }
        //Создаем конструктор для объекта с готовыми данными
        public GameSave(int[,] board, string player1Name, string player2Name,
                       string blackPlayer, string whitePlayer,
                       string currentPlayer, int currentPlayerColor, int moveCounter)
        {
            //Преобразуем двумерный массив в массив массивов
            Board = new int[15][];
            for (int i = 0; i < 15; i++)
            {
                Board[i] = new int[15];
                for (int j = 0; j < 15; j++)
                {
                    Board[i][j] = board[i, j]; //Копируем данные для удобства в массив вида [][]
                }
            }

            Player1Name = player1Name;
            Player2Name = player2Name;
            BlackPlayer = blackPlayer;
            WhitePlayer = whitePlayer;
            CurrentPlayer = currentPlayer;
            CurrentPlayerColor = currentPlayerColor;
            MoveCounter = moveCounter;
            SaveTime = DateTime.Now;
        }

        //Метод для преобразования обратно в int[,]
        public int[,] TransformToArray()
        {
            //Копируем данные из зубчатого массива в двумерный
            int[,] result = new int[15, 15];
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    result[i, j] = Board[i][j];
                }
            }
            return result;
        }
    }

    //Класс-менеджер для сохранения результатов
    class ScoreManager
    {
        //Создаем приватные поля
        private const string ScoreFile = "gomoku_results.json"; //Хранит ВСЕ завершённые игры (история)
        private const string SaveFile = "gomoku_save.json"; //Хранит ТЕКУЩУЮ незавершённую игру (сохранение)
        private List<GameResult> results; //Динамический список для хранения объектов GameResult в оперативной памяти.
        private int moveCounter;

        //Конструктор класса
        public ScoreManager()
        {
            results = new List<GameResult>(); //Создание нового экземпляра List<GameResult>
            moveCounter = 0;
            LoadingResults(); //Вызов приватного метода загрузки результатов
        }

        //Метод для сохранения текущей игры по просьбе игрока
        public void SaveCurrentGame(int[,] board, string player1Name, string player2Name,
                               string blackPlayer, string whitePlayer,
                               string currentPlayer, int currentPlayerColor)
        {
            //Блок try-catch для ловли ошибок
            try
            {
                Console.WriteLine($"Сохранение в файл: {Path.GetFullPath(SaveFile)}"); //Выводим сообщение о сохранении для уведомления игрока
                GameSave save = new GameSave(board, player1Name, player2Name,
                                            blackPlayer, whitePlayer,
                                            currentPlayer, currentPlayerColor, moveCounter);

                var options = new JsonSerializerOptions { WriteIndented = true }; //Используем с отступами для читаемости
                string json = JsonSerializer.Serialize(save, options); //Преобразуем в текстовый формат для хранения
                File.WriteAllText(SaveFile, json); //Запись в файл

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Игра сохранена! ({save.SaveTime:HH:mm:ss})"); //В случае успешного сохранения выводим сообщение игроку
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                //В случае пойманной ошибки выводим сообщение игроку
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка при сохранении игры: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                Console.ResetColor();
            }
        }

        //Загрузить сохраненную игру
        public GameSave LoadingSavedGame()
        {
            //Блок try-catch для ловли ошибок
            try
            {
                //Проверка существования файла
                if (File.Exists(SaveFile))
                {
                    string json = File.ReadAllText(SaveFile); //Читаем данные из файла
                    GameSave save = JsonSerializer.Deserialize<GameSave>(json)!; //Преобразуем текстовые данные обратно в объект GameSave

                    //Проверка результата
                    if (save != null)
                    {
                        //В случае успешной проверки выводим сообщение игроку и возвращаем объект  
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Загружена игра от {save.SaveTime:dd.MM.yyyy HH:mm}");
                        Console.ResetColor();
                        return save;
                    }
                }
            }
            catch (Exception ex)
            {
                //В случае пойманной ошибки выводим сообщение игроку
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка при загрузке игры: {ex.Message}");
                Console.ResetColor();
            }

            return null!; //Возвращаем пустое значение 
        }

        //Проверить наличие сохраненной игры
        public bool IsThereGame()
        {
            //Возвращаем true-false значение для проверки наличия сохраненной игры
            return File.Exists(SaveFile);
        }

        //Удалить сохраненную игру (после завершения или отмены)
        public void DeleteSavedGame()
        {
            try
            {
                if (File.Exists(SaveFile))
                {
                    File.Delete(SaveFile);
                }
            }
            catch { }
        }

        //Получаем текущий счетчик ходов
        public int GetMoveCounter()
        {
            return moveCounter;
        }

        //Устанавливаем счетчик ходов при загрузке сохранения (используется при загрузке сохранения)
        public void SetMoveCounter(int moves)
        {
            moveCounter = moves;
        }

        //Приватный метод загрузки результатов из файла
        private void LoadingResults()
        {
            //Блок try-catch для обработки исключений
            try
            {
                if (File.Exists(ScoreFile)) //Проверка существования файла
                {
                    string json = File.ReadAllText(ScoreFile); //Чтение содержимого файла в строку
                    results = JsonSerializer.Deserialize<List<GameResult>>(json) ?? new List<GameResult>(); //Проверка на null, если так, то создаст новый пустой список
                }
            }
            catch (Exception) //Обработка исключений
            {
                results = new List<GameResult>(); //В случае ошибки новый пустой список
            }
        }

        //Приватный метод сохранения результатов в файл
        private void SaveResults()
        {
            //Блок try-catch для обработки исключений
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true }; //WriteIndented = true делает файл читаемым (с отступами)
                string json = JsonSerializer.Serialize(results, options); //Преобразование списка результатов в строку json
                File.WriteAllText(ScoreFile, json); //Запись строки в файл (перезапись всего файла)
            }
            catch (Exception ex) //Обработка исключений
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка при сохранении результатов: {ex.Message}"); //При пойманной ошибке выводит сообщение о ней
                Console.ResetColor();
            }
        }

        //Публичный метод для начала новой игры
        public void StartNewGame()
        {
            moveCounter = 0; //Сброс счетчика ходов для новой игры
        }

        //Публичный метод для увеличения счетчика ходов
        public void MoveCounterIncreaser()
        {
            moveCounter++;
        }

        //Публичный метод для сохранения результата игры
        public void SaveGameResult(string player1, string player2, string winner, string resultType,
                                 string blackPlayer, string whitePlayer, string firstPlayer)
        {
            //Создание нового объекта GameResult с переданными параметрами
            GameResult result = new GameResult(player1, player2, winner, moveCounter, resultType,
                                              blackPlayer, whitePlayer, firstPlayer);
            results.Add(result); //Добавление результата в список
            SaveResults(); //Вызов приватного метода для сохранения в файл

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Результат игры сохранен! Всего сохранено игр: {results.Count}");
            Console.ResetColor();
        }

        //Публичный метод для показа истории игр
        public void GameHistory()
        {
            if (results.Count == 0) //Проверка, есть ли сохраненные результаты
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("История игр пуста."); //Если сохраненных результатов нет, выводит сообщение об этом
                Console.ResetColor();
                return; //Выход из метода досрочно
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"ИСТОРИЯ ИГР (всего: {results.Count})");
            Console.ResetColor();

            //Цикл по всем результатам
            for (int i = 0; i < results.Count; i++)
            {
                GameResult result = results[i]; //Получение результата по индексу
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write($"{i + 1}. "); //Номер записи
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"{result.Date:dd.MM.yyyy HH:mm} - "); //Дата и время

                //Показываем кто черные, кто белые
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("(Черный игрок: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(result.BlackPlayer);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(", Белый игрок: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(result.WhitePlayer);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(") ");

                //Проверка результата (победа или ничья)
                if (result.Result == "Победа") //Если кто-то победил, победитель выводится на экран
                {
                    Console.Write("→ Победитель: ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(result.Winner);
                }
                else //Если ничья - выводится на экран
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("→ Ничья");
                }

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($" (всего ходов: {result.MovesCounter})");
            }
        }

        //Публичный метод для показания статистики сыгранных игр
        public void Statistics()
        {
            if (results.Count == 0) //Проверка,есть ли данные статистики
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Статистика отсутствует."); //В случае отсутствия данных, выводится сообщение об этом на экран
                Console.ResetColor();
                return;
            }

            //Инициализируем массивы для хранения данных игроков
            int maxPlayers = results.Count * 2; //Максимальное количество уникальных игроков
            string[] playerNames = new string[maxPlayers]; //Массив для хранения имен игроков
            int[] playerWins = new int[maxPlayers]; //Массив для подсчета побед каждого игрока
            int[] playerGames = new int[maxPlayers]; //Массив для подсчета игр каждого игрока
            int playerCounter = 0; //Счетчик реального количества уникальных игроков

            int draws = 0; //Подсчет ничьи
            int totalMoves = 0; //Подсчет общего количества ходов
            int totalGames = results.Count; //Подсчет общего количества игр

            foreach (var result in results) //Цикл по всем результатам игр
            {
                //Добавляем текущих игроков в массивы
                NewPlayer(playerNames, playerGames, ref playerCounter, result.BlackPlayer);
                NewPlayer(playerNames, playerGames, ref playerCounter, result.WhitePlayer);

                //Поиск индексов игроков в массивах
                int indexBlack = PlayerIndex(playerNames, playerCounter, result.BlackPlayer);
                int indexWhite = PlayerIndex(playerNames, playerCounter, result.WhitePlayer);
                //Увеличение счетчика игр для обоих участников
                playerGames[indexBlack]++;
                playerGames[indexWhite]++;

                if (result.Result == "Победа") //Обрабатываем результаты игры
                {
                    //Находим индекс победителя
                    int winnerIndex = PlayerIndex(playerNames, playerCounter, result.Winner);

                    //Увеличиваем количество побед
                    if (winnerIndex != -1)
                    {
                        playerWins[winnerIndex]++;
                    }
                }
                else //Если победы не нашлось, значит ничья
                {
                    draws++;
                }
                totalMoves += result.MovesCounter; //Добавление ходов текущей игры к общему количеству
            }

            //Вывод общей статистики
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\nОБЩАЯ СТАТИСТИКА");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Всего игр: {totalGames}");
            Console.WriteLine($"Ничьих: {draws}");
            Console.WriteLine($"Всего ходов: {totalMoves}");

            if (totalGames > 0) //Расчет и вывод среднего количества ходов
            {
                Console.WriteLine($"Среднее кол-во ходов за игру: {(double)totalMoves / totalGames:F1}");
            }

            //Статистика по игрокам
            if (playerCounter > 0)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\nСТАТИСТИКА ИГРОКОВ");
                Console.ForegroundColor = ConsoleColor.White;

                //Ищем лучшего игрока
                int bestIndex = -1;  //Индекс лучшего игрока
                double WinnerRate = -1; //Наилучший процент побед

                //Цикл по всем уникальным игрокам
                for (int i = 0; i < playerCounter; i++)
                {
                    //Расчет процента побед для текущего игрока
                    double winRate = playerGames[i] > 0 ? (double)playerWins[i] / playerGames[i] * 100 : 0;

                    //Вывод статистики игрока
                    Console.WriteLine($"\n{playerNames[i]}:");
                    Console.WriteLine($"  Игр: {playerGames[i]}");
                    Console.WriteLine($"  Побед: {playerWins[i]} ({winRate:F1}%)");

                    //Обновление лучшего игрока
                    if (winRate > WinnerRate)
                    {
                        WinnerRate = winRate;
                        bestIndex = i;
                    }
                }

                //Вывод информации о лучшем игроке
                if (bestIndex != -1 && WinnerRate > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\nЛучший игрок: {playerNames[bestIndex]} ({WinnerRate:F1}% побед)");
                    Console.ResetColor();
                }
            }
        }

        //Вспомогательный метод для добавления нового игрока
        private void NewPlayer(string[] playerNames, int[] playerGames, ref int playerCount, string playerName)
        {
            //Проверяем, есть ли уже игрок в массиве
            for (int i = 0; i < playerCount; i++)
            {
                if (playerNames[i] == playerName)
                {
                    return; //Игрок уже есть
                }
            }

            //Добавляем нового игрока
            if (playerCount < playerNames.Length)
            {
                playerNames[playerCount] = playerName;
                playerGames[playerCount] = 0;
                playerCount++;
            }
        }

        //Вспомогательный метод для поиска индекса игрока
        private int PlayerIndex(string[] playerNames, int playerCount, string playerName)
        {
            //Поиск игрока в массиве
            for (int i = 0; i < playerCount; i++)
            {
                if (playerNames[i] == playerName)
                {
                    return i; //Возвращаем индекс найденного игрока
                }
            }
            Console.ResetColor();
            return -1; //Игрок не найден, тогда возвращаем -1 
        }

        //Публичный метод очищения истории игр
        public void ClearHistory()
        {
            //Запрос подтверждения действия для избежания случайного удаления
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Вы уверены, что хотите удалить всю историю игр? (да/нет)/(yes/no): ");
            Console.ResetColor();

            //Чтение результата пользователя
            string answer = Console.ReadLine()?.ToLower()!; //Вызывает ToLower только если ReadLine не равно null

            //Проверка ответа пользователя
            if (answer == "да" || answer == "yes")
            {
                //Если ответ - "да", то очищаем список результатов
                results.Clear();
                SaveResults(); //Сохраняем пустой спискок в файл
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("История игр очищена.");
                Console.ResetColor();
            }
            else
            {
                //В случае отрицательного ответа - отмена операции
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Отменено.");
                Console.ResetColor();
            }
        }
    }

    //Основной класс программы
    class Program
    {
        private static ScoreManager scoreManager; //Приватное поле для менеджера результатов

        //Главный метод программы
        static void Main()
        {
            //Создание объекта для управления результатами игр
            scoreManager = new ScoreManager();

            //Создаем объект игры (вызов конструктора Gomoku)
            Gomoku game = new Gomoku();

            bool run = true; //Переменная-флаг для управления главным циклом программы
            while (run) //Главный цикл программы
            {
                //Выведение меню
                Console.Clear();
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($" {game.GameName()}"); //Используем полиморфизм
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("1. Начать новую игру");
                //Проверяем, есть ли сохранение
                if (scoreManager.IsThereGame())
                {
                    //При наличии сохраненной игры выводим доп пункт для продолжения игры
                    Console.WriteLine("2. Продолжить сохраненную игру");
                    Console.WriteLine("3. Прочитать правила игры");
                    Console.WriteLine("4. История игр");
                    Console.WriteLine("5. Статистика");
                    Console.WriteLine("6. Выход");
                }
                else
                {
                    //В случае отсутствия - обычное меню
                    Console.WriteLine("2. Прочитать правила игры");
                    Console.WriteLine("3. История игр");
                    Console.WriteLine("4. Статистика");
                    Console.WriteLine("5. Выход");
                }
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Используйте TAB для возврата в меню, F1 для выхода из игры");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("Выберите опцию: ");
                Console.ForegroundColor = ConsoleColor.White;

                //Блок для полного выхода из игры и выхода в главное меню
                var key = Console.ReadKey(true); //Чтение нажатой клавиши без отображения в консоли

                if (key.Key == ConsoleKey.F1) //Если нажатая клавиша - F1, то выход из игры
                {
                    ExitGame(); //Вызов метода для выхода из игры
                    return;
                }

                if (key.Key == ConsoleKey.Tab) //Если нажатая клавиша - Tab, то возвращаемся в меню
                {
                    Console.ResetColor();
                    continue; //Продолжаем программу - переходим к следующему шагу "показать меню"
                }

                //Проверяем, была ли нажата цифровая клавиша
                if (char.IsDigit(key.KeyChar))
                {
                    //Если была, то преобразуем символ в число
                    if (int.TryParse(key.KeyChar.ToString(), out int choice2))
                    {
                        bool hasSave = scoreManager.IsThereGame();

                        //Обрабатываем значение
                        switch (choice2)
                        {
                            case 1:
                                Console.ResetColor();
                                StartGame(game);
                                break;
                            case 2:
                                if (hasSave)
                                {
                                    Console.ResetColor();
                                    ContinueSavedGame(game);
                                }
                                else
                                {
                                    Console.Clear();
                                    Console.ResetColor();
                                    game.Rules();
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine("\nНажмите любую клавишу для возврата в меню...");
                                    Console.ResetColor();
                                    Console.ReadKey(true);
                                }
                                break;
                            case 3:
                                if (hasSave)
                                {
                                    Console.Clear();
                                    Console.ResetColor();
                                    game.Rules();
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine("\nНажмите любую клавишу для возврата в меню...");
                                    Console.ResetColor();
                                    Console.ReadKey(true);
                                }
                                else
                                {
                                    HistoryMenu();
                                }
                                break;
                            case 4:
                                if (hasSave)
                                {
                                    HistoryMenu();
                                }
                                else
                                {
                                    ShowStatistics();
                                }
                                break;
                            case 5:
                                if (hasSave)
                                {
                                    ShowStatistics();
                                }
                                else
                                {
                                    Console.ResetColor();
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("Вы вышли из игры");
                                    Console.ResetColor();
                                    run = false;
                                }
                                break;
                            case 6:
                                if (hasSave)
                                {
                                    Console.ResetColor();
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("Вы вышли из игры");
                                    Console.ResetColor();
                                    run = false;
                                }
                                break;
                        }

                    }
                }
                //Если вводится некорректная цифра (не от 1 до 5), то меню просто перерисовывается
            }
        }

        //Метод для продолжения сохраненной игры
        static void ContinueSavedGame(Gomoku game)
        {
            //Проверяем наличие сохраненной игры
            if (!scoreManager.IsThereGame())
            {
                //В случае остутсивия наличия найденной игры выводим сообщение игроку
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Сохраненная игра не найдена.");
                Console.WriteLine("\nНажмите любую клавишу для возврата в меню...");
                Console.ResetColor();
                Console.ReadKey(true);
                return;
            }

            GameSave save = scoreManager.LoadingSavedGame();
            if (save == null)
            {
                //Если сохранение пусто, выводим сообщение игроку
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ошибка загрузки сохранения.");
                Console.ResetColor();
                Console.ReadKey(true);
                return;
            }

            //Восстанавливаем имена игроков
            Player player1 = new Player(save.Player1Name, 0);
            Player player2 = new Player(save.Player2Name, 0);

            //Определяем кто черные, кто белые
            Player blackPlayer = (save.BlackPlayer == player1.Name) ? player1 : player2;
            Player whitePlayer = (save.WhitePlayer == player1.Name) ? player1 : player2;

            //Устанавливаем цвета
            blackPlayer.Color = 1;
            whitePlayer.Color = 2;

            //Определяем текущего игрока
            Player currentPlayer = (save.CurrentPlayer == blackPlayer.Name) ? blackPlayer : whitePlayer;

            //Восстанавливаем счетчик ходов
            scoreManager.SetMoveCounter(save.MoveCounter);

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"ПРОДОЛЖЕНИЕ ИГРЫ {game.GameName()}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Игра от: {save.SaveTime:dd.MM.yyyy HH:mm}");
            Console.WriteLine($"Ход: {save.CurrentPlayer}");
            Console.WriteLine($"Сделано ходов: {save.MoveCounter}");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Нажмите любую клавишу для продолжения игры (TAB - меню, F1 - выход)...");
            Console.ForegroundColor = ConsoleColor.White;

            //Блок для выхода из игры и выхода в главное меню
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.F1)
            {
                //Если нажатая кнопка - F1, вызываем метод для выхода из игры
                ExitGame();
                Environment.Exit(0);
            }
            else if (key.Key == ConsoleKey.Tab)
            {
                //Если нажатая кнопка - Tab, выходим в главное меню
                Console.ResetColor();
                return;
            }

            //Запускаем игру с восстановленным состоянием
            int[,] board2D = save.TransformToArray();
            RunSavedGame(game, board2D, player1, player2, blackPlayer, whitePlayer,
                           currentPlayer, save.Player1Name, save.Player2Name,
                           save.BlackPlayer, save.WhitePlayer, save.CurrentPlayer);
        }

        //Метод для запуска игры из сохранения
        static void RunSavedGame(Gomoku game, int[,] matrix, Player player1, Player player2,
                                   Player blackPlayer, Player whitePlayer, Player currentPlayer,
                                   string player1Name, string player2Name,
                                   string blackPlayerName, string whitePlayerName, string currentPlayerName)
        {
            bool gameRunning = true;
            //Запускаем игровой цикл
            while (gameRunning)
            {
                Console.Clear();
                Board(matrix);
                Console.WriteLine();
                if (currentPlayer.Color == 1)
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($" {currentPlayer.Name} ходит! ({currentPlayer.PlayerColor()}) ");
                }
                else
                {
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write($" {currentPlayer.Name} ходит! ({currentPlayer.PlayerColor()}) ");
                }

                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Введите координаты через пробел в формате: сначала номер строки, потом номер столбца. Например: '7 7'");
                Console.WriteLine("S - Сохранить игру и выйти в меню");
                Console.WriteLine("TAB - вернуться в меню (без сохранения)");
                Console.WriteLine("F1 - выйти из игры");
                Console.WriteLine("S - сохранить игру");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("> ");
                Console.ForegroundColor = ConsoleColor.White;

                string inputCoords = ReadInputWithSave(); //Читаем координаты из сохранения

                if (inputCoords == null) //F1
                {
                    ExitGame();
                    Environment.Exit(0);
                }
                else if (inputCoords == "") //Tab
                {
                    Console.ResetColor();
                    return;
                }
                else if (inputCoords.ToLower() == "s") //Сохранить
                {
                    //Сохраняем текущее состояние
                    scoreManager.SaveCurrentGame(matrix, player1Name, player2Name,
                                                blackPlayerName, whitePlayerName,
                                                currentPlayer.Name, currentPlayer.Color);
                    Console.WriteLine("\nИгра сохранена. Нажмите любую клавишу для возврата в меню...");
                    Console.ReadKey(true);
                    return;
                }

                string[] coords = inputCoords.Split(' '); //Разбиваем координаты по пробелу
                if (coords.Length == 2 && int.TryParse(coords[0], out int rowInput) &&
                    int.TryParse(coords[1], out int colInput) && rowInput >= 1 && rowInput <= 15 &&
                    colInput >= 1 && colInput <= 15) //Проверяем, верно ли введены координаты
                {
                    int row = rowInput - 1;
                    int col = colInput - 1;

                    if (matrix[row, col] == 0)
                    {
                        matrix[row, col] = currentPlayer.Color;
                        scoreManager.MoveCounterIncreaser();

                        if (CheckWin(matrix, row, col, currentPlayer.Color))
                        {
                            Console.Clear();
                            Board(matrix);
                            Console.ResetColor();
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"\n{currentPlayer.Name} победил(-а)!");

                            //Сохраняем результат игры
                            scoreManager.SaveGameResult(
                                player2Name,
                                player1Name,
                                currentPlayer.Name,
                                "Победа",
                                blackPlayerName,
                                whitePlayerName,
                                currentPlayerName
                            );

                            //Удаляем сохранение
                            scoreManager.DeleteSavedGame();

                            Console.ResetColor();
                            gameRunning = false;
                        }
                        else if (IsFullBoard(matrix))
                        {
                            Console.Clear();
                            Board(matrix);
                            Console.ResetColor();
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("\nНичья!");

                            //Сохраняем результат игры (ничья)
                            scoreManager.SaveGameResult(
                                player1Name,
                                player2Name,
                                "Ничья",
                                "Ничья",
                                blackPlayerName,
                                whitePlayerName,
                                currentPlayerName
                            );

                            //Удаляем сохранение
                            scoreManager.DeleteSavedGame();

                            Console.ResetColor();
                            gameRunning = false;
                        }
                        else
                        {
                            currentPlayer = (currentPlayer == blackPlayer) ? whitePlayer : blackPlayer;
                        }
                    }
                    else
                    {
                        Console.ResetColor();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Эта клетка занята! Нажмите любую клавишу...");
                        Console.ResetColor();
                        Console.ReadKey(true);
                    }
                }
                else
                {
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Некорректный ввод! Используйте числа от 1 до 15. Нажмите любую клавишу...");
                    Console.ResetColor();
                    Console.ReadKey(true);
                }
            }

            //Завершение игры
            Console.ResetColor();
            Console.WriteLine("\nИгра окончена! Нажмите любую клавишу, чтобы продолжить...");
            Console.ReadKey(true);
        }

        //Метод для чтения ввода с поддержкой сохранения
        static string ReadInputWithSave()
        {
            string input = "";

            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.F1)
                {
                    return null!;
                }
                else if (key.Key == ConsoleKey.Tab)
                {
                    return "";
                }
                else if (key.Key == ConsoleKey.S) //Клавиша S для сохранения
                {
                    Console.Write("s");
                    return "s";
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    if (input == "")
                    {
                        continue;
                    }
                    return input;
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (input.Length > 0)
                    {
                        input = input.Substring(0, input.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    input += key.KeyChar;
                    Console.Write(key.KeyChar);
                }
            }
        }

        //Метод подменю для истории игр
        static void HistoryMenu()
        {
            //Цикл для меню, пока пользователь не выберет "Назад"
            while (true)
            {
                //Выводим содержание подменю
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("ИСТОРИЯ ИГР");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("1. Показать историю игр");
                Console.WriteLine("2. Очистить историю");
                Console.WriteLine("3. Назад в главное меню");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Выберите опцию: ");
                Console.ForegroundColor = ConsoleColor.White;

                //Ждем нажатия клавиши
                var key = Console.ReadKey(true);
                Console.WriteLine();

                //Проверяем, была ли нажата цифровая клавиша
                if (char.IsDigit(key.KeyChar))
                {
                    //Если была, то преобразуем символ в число
                    if (int.TryParse(key.KeyChar.ToString(), out int choice))
                    {
                        //Выбираем действие в зависимости от нажатой клавиши
                        switch (choice)
                        {
                            case 1:
                                Console.Clear();
                                scoreManager.GameHistory(); //В случае нажатии единицы выводим историю игр
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                                Console.ResetColor();
                                Console.ReadKey(true);
                                break;
                            case 2:
                                Console.Clear();
                                scoreManager.ClearHistory(); //В случае нажатия двойки очищаем историю
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                                Console.ResetColor();
                                Console.ReadKey(true);
                                break;
                            case 3:
                                return;
                        }
                    }
                }
                else if (key.Key == ConsoleKey.F1) //Если нажатая клавиша - F1, выходим из игры
                {
                    ExitGame(); //Вызываем метод для выхода из игры
                    Environment.Exit(0);
                }
                else if (key.Key == ConsoleKey.Tab) //Если нажатая клавиша - Tab, возвращаемся в главное меню
                {
                    return;
                }
            }
        }

        //Метод показа статистики
        static void ShowStatistics()
        {
            Console.Clear();
            scoreManager.Statistics();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ResetColor();
            Console.ReadKey(true); //Ожидание любой клавиши
        }

        //Метод отрисовки поля
        static void Board(int[,] board)
        {
            Console.ResetColor();

            //Основное поле (16x16, включая заголовки)
            for (int row = -1; row < 15; row++)  //-1 для заголовков строк, 0-14 включительно для игровых строк
            {
                for (int col = -1; col < 15; col++)  //-1 для заголовков столбцов, 0-14 включительно для игровых столбцов
                {
                    if (row == -1 && col == -1)
                    {
                        //Левый верхний угол - пробелы для выравнивания
                        Console.Write("   ");
                    }
                    else if (row == -1) //Верхняя строка - заголовки столбцов (1-15)
                    {
                        //Верхняя строка - номера столбцов (1-15)
                        Console.ForegroundColor = ConsoleColor.Gray;
                        if (col + 1 < 10) //Для однозначных чисел добавляем пробел спереди для выравнивания
                            Console.Write($" {col + 1} ");
                        else
                            Console.Write($"{col + 1} ");
                        Console.ResetColor();  //Сбрасываем цвет после вывода
                    }
                    else if (col == -1)
                    {
                        //Левый столбец - номера строк (1-15)
                        Console.ForegroundColor = ConsoleColor.Gray;
                        if (row + 1 < 10)
                            Console.Write($" {row + 1} ");
                        else
                            Console.Write($"{row + 1} ");
                        Console.ResetColor();  //Сбрасываем цвет после вывода
                    }
                    else
                    {
                        //Игровые клетки (15x15)
                        switch (board[row, col])
                        {
                            case 1: //Игрок, делающий первый ход - черные фишки
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.BackgroundColor = ConsoleColor.Black;
                                Console.Write(" O ");
                                break;
                            case 2: //Игрок, делающий первый ход - белые фишки
                                Console.ForegroundColor = ConsoleColor.Black;
                                Console.BackgroundColor = ConsoleColor.White;
                                Console.Write(" O ");
                                break;
                            default: //Пустая клетка
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.BackgroundColor = ConsoleColor.DarkGreen;
                                Console.Write(" · ");
                                break;
                        }
                        Console.ResetColor();  //Сбрасываем цвет после каждой клетки
                    }
                }

                //Переход на новую строку
                Console.WriteLine();
            }

            Console.ResetColor();
        }

        //Метод для переполненной доски
        static bool IsFullBoard(int[,] board)
        {
            //Двойной цикл по всем клеткам
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    if (board[i, j] == 0)
                    {
                        //Если найдена пустая клетка, то доска не полная, соответственно возвращаем false
                        return false;
                    }
                }
            }
            return true; //Иначе доска заполнена, возвращаем true
        }

        //Метод для проверки победы
        static bool CheckWin(int[,] board, int row, int col, int currName)
        {
            //Проверка по вертикали
            int counter1 = 0; //Счетчик подряд идущих фишек для вертикальной победы
            for (int i = 0; i < 15; i++) //Проходимся по строкам
            {
                if (board[i, col] == currName)
                {
                    //Если в клетке стоит фишка текущего игрока, увеличиваем счетчик
                    counter1++;
                    if (counter1 >= 5) return true; //Если набрано 5 и болеее подряд идущих фишек, победа
                }
                else counter1 = 0; //Иначе - сбрасываем счетчик
            }

            //Проверка по горизонтали
            int counter2 = 0; //Счетчик подряд идущих фишек для горизонтальной победы
            for (int i = 0; i < 15; i++) //Проходимся по столбцам
            {
                if (board[row, i] == currName)
                {
                    //Если в клетке стоит фишка текущего игрока, увеличиваем счетчик
                    counter2++;
                    if (counter2 >= 5) return true; //Если набрано 5 и болеее подряд идущих фишек, победа
                }
                else counter2 = 0; //Иначе - сбрасываем счетчик
            }
            //Проверка диагонали слева-направо
            int counter3 = 0; //Счетчик подряд идущих фишек для диагональной (\) победы
            for (int i = -4; i < 5; i++) //Проверка 9 клеток вокруг последнего хода
            {
                //Инициализируем новые строки и столбцы по диагонали
                int row1 = row + i;
                int col1 = col + i;
                //Проверка границ массива
                if (row1 >= 0 && col1 >= 0 && row1 < 15 && col1 < 15)
                {
                    if (board[row1, col1] == currName)
                    {
                        counter3++;
                        if (counter3 >= 5) return true;
                    }
                    else counter3 = 0;
                }
            }
            //Проверка диагонали справа-налево
            int counter4 = 0; //Счетчик подряд идущих фишек для диагональной (/) победы
            for (int i = -4; i <= 4; i++) //Проверка 9 клеток вокруг последнего хода
            {
                //Инициализируем новые строки и столбцы по диагонали
                int row1 = row + i;
                int col1 = col - i;
                //Проверка границ массива
                if (row1 >= 0 && col1 >= 0 && row1 < 15 && col1 < 15)
                {
                    if (board[row1, col1] == currName)
                    {
                        counter4++;
                        if (counter4 >= 5) return true;
                    }
                    else counter4 = 0;
                }
            }
            return false;
        }

        //Метод для начала игры
        static void StartGame(Gomoku game)
        {
            Console.Clear();
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"НОВАЯ ИГРА В {game.GameName()}"); //Используем полиморфизм
            Console.ForegroundColor = ConsoleColor.White;

            //Ввод имени первого игрока
            Console.Write("Введите имя первого игрока (TAB - меню, F1 - выход): ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            string name1 = ReadInput();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();

            if (name1 == null) //null - нажата F1, нужно выйти из игры
            {
                ExitGame();
                Environment.Exit(0);
            }
            if (name1 == "") //"" - нажата Tab, нужно вернуться в меню
            {
                Console.ResetColor();
                return;
            }

            //Ввод имени второго игрока
            Console.Write("Введите имя второго игрока (TAB - меню, F1 - выход): ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            string name2 = ReadInput();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            if (name2 == null) //null - нажата F1, нужно выйти из игры
            {
                ExitGame();
                Environment.Exit(0);
            }
            if (name2 == "") //"" - нажата Tab, нужно вернуться в меню
            {
                Console.ResetColor();
                return;
            }

            //Используем констркутор класса Player
            Player player1 = new Player(name1, 0);
            Player player2 = new Player(name2, 0);

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Начинаем!");
            Console.WriteLine("Выбираем игрока для первого хода...");
            Console.ForegroundColor = ConsoleColor.White;

            Random random = new Random(); //Инициализируем генератор случайных чисел для жеребьевки
            int[,] matrix = new int[15, 15];
            bool gameRunning = true; //Флаг для продолжения игры

            //Жеребьевка: кто будет ходить первым (и получает фишки черного цвета)
            int firstPlayerIndex = random.Next(1, 3);

            //Определяем кто будет черными, а кто белыми
            Player blackPlayer; //Черные (цвет 1)
            Player whitePlayer; //Белые (цвет 2)
            Player firstPlayer; //Тот, кто ходит первым
            Player secondPlayer; //Тот, кто ходит вторым
            if (firstPlayerIndex == 1)
            {
                //Первый игрок ходит первым и получает черные
                firstPlayer = player1;
                secondPlayer = player2;
                blackPlayer = player1;
                whitePlayer = player2;
            }
            else
            {
                //Второй игрок ходит первым и получает черные
                firstPlayer = player2;
                secondPlayer = player1;
                blackPlayer = player2;
                whitePlayer = player1;
            }

            //Устанавливаем цвета игрокам (через поля)
            blackPlayer.Color = 1; //Черные
            whitePlayer.Color = 2; //Белые

            //Текущий игрок всегда начинает с черных (по правилам игры)
            Player currentPlayer = blackPlayer;

            //Сброс счетчика ходов для новой игры
            scoreManager.StartNewGame();

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"По результатам жеребьевки первым ходит {firstPlayer.Name}!");
            Console.WriteLine($"{blackPlayer.Name} играет черными, {whitePlayer.Name} играет белыми.");
            Console.ResetColor();

            //Ожидание подтверждения начала игры
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nНажмите любую клавишу для начала игры (TAB - меню, F1 - выход)...");
            Console.ForegroundColor = ConsoleColor.White;

            //Блок для выхода из игры
            var key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.F1) //Нажата клавиша F1 - выходим из игры
            {
                ExitGame(); //Вызов метода для выхода из игры
                Environment.Exit(0);
            }
            else if (key.Key == ConsoleKey.Tab) //Нажата клавиша Tab - выходим в главное меню
            {
                Console.ResetColor();
                return;
            }

            //Переменные для сохранения результатов
            string blackPlayerName = blackPlayer.Name;
            string whitePlayerName = whitePlayer.Name;
            string firstPlayerName = firstPlayer.Name;
            string player1Name = player1.Name; //Сохраняем исходные имена
            string player2Name = player2.Name;

            //Основной игровой цикл
            while (gameRunning)
            {
                Console.Clear();
                Board(matrix); //Вызываем метод для отрисовки игрового поля 
                Console.WriteLine();

                if (currentPlayer.Color == 1) //Игрок с черными фишками
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($" {currentPlayer.Name} ходит! ({currentPlayer.PlayerColor()}) ");
                }
                else //Игрок с белыми фишками
                {
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write($" {currentPlayer.Name} ходит! ({currentPlayer.PlayerColor()}) ");
                }

                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Введите координаты через пробел в формате: сначала номер строки, потом номер столбца. Например: '7 7'");
                Console.WriteLine("TAB - вернуться в меню, F1 - выйти из игры, S - сохранить игру");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("> ");
                Console.ForegroundColor = ConsoleColor.White;
                //Чтение ввода координат
                string inputCoords = ReadInputWithSave();

                //Обработка специальных команд
                if (inputCoords == null) //F1
                {
                    ExitGame();
                    Environment.Exit(0);
                }
                else if (inputCoords == "") //Tab
                {
                    Console.ResetColor();
                    return;
                }
                else if (inputCoords.ToLower() == "s") //Сохранить
                {
                    //Сохраняем текущее состояние
                    scoreManager.SaveCurrentGame(matrix, player1Name, player2Name,
                                                blackPlayerName, whitePlayerName,
                                                currentPlayer.Name, currentPlayer.Color);
                    Console.WriteLine("\nИгра сохранена. Нажмите любую клавишу для возврата в меню...");
                    Console.ReadKey(true);
                    return;
                }
                //Разбор введенных координат
                string[] coords = inputCoords.Split(' '); //Разбиваем по пробелу
                //Проверяем верность введенных координат
                if (coords.Length == 2 && int.TryParse(coords[0], out int rowInput) &&
                    int.TryParse(coords[1], out int colInput) && rowInput >= 1 && rowInput <= 15 &&
                    colInput >= 1 && colInput <= 15) //Должно быть 2 числа в корректном диапазоне
                {
                    int row = rowInput - 1;
                    int col = colInput - 1;

                    //Проверяем, свободна ли клетка
                    if (matrix[row, col] == 0)
                    {
                        matrix[row, col] = currentPlayer.Color; //Ставим фишку текущего игрока

                        //Увеличиваем счетчик ходов
                        scoreManager.MoveCounterIncreaser();

                        //Проверка победы
                        if (CheckWin(matrix, row, col, currentPlayer.Color))
                        {
                            Console.Clear();
                            Board(matrix);
                            Console.ResetColor();
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"\n{currentPlayer.Name} победил(-а)!");

                            //Сохраняем результат игры
                            scoreManager.SaveGameResult(
                                player2Name, //Исходное имя игрока 2
                                player1Name, //Исходное имя игрока 1
                                currentPlayer.Name, //Имя победителя
                                "Победа",
                                blackPlayerName, //Кто играл черными
                                whitePlayerName, //Кто играл белыми
                                firstPlayerName //Кто ходил первым
                            );
                            scoreManager.DeleteSavedGame();
                            Console.ResetColor();
                            gameRunning = false; //Заканчиваем цикл, завершаем игру
                        }
                        //Поверка ничьи - доска переполнена и никто не победил
                        else if (IsFullBoard(matrix))
                        {
                            Console.Clear();
                            Board(matrix);
                            Console.ResetColor();
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine("\nНичья!");

                            //Сохраняем результат игры (ничья)
                            scoreManager.SaveGameResult(
                                player1Name,
                                player2Name,
                                "Ничья",
                                "Ничья",
                                blackPlayerName,
                                whitePlayerName,
                                firstPlayerName
                            );
                            scoreManager.DeleteSavedGame();
                            Console.ResetColor();
                            gameRunning = false; //Заканчиваем цикл, завершаем игру
                        }
                        else
                        {
                            //Смена игрока
                            currentPlayer = (currentPlayer == blackPlayer) ? whitePlayer : blackPlayer;
                        }
                    }
                    else //Клетка занята
                    {
                        Console.ResetColor();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Эта клетка занята! Нажмите любую клавишу...");
                        Console.ResetColor();
                        Console.ReadKey(true); //Пауза для сообщения об ошибке
                    }
                }
                else //Некорректный ввод (не числа или вне диапазона)
                {
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Некорректный ввод! Используйте числа от 1 до 15. Нажмите любую клавишу...");
                    Console.ResetColor();
                    Console.ReadKey(true);
                }
            }
            //Завершение игры (после победы или ничьи)
            Console.ResetColor();
            Console.WriteLine("\nИгра окончена! Нажмите любую клавишу, чтобы продолжить...");
            Console.ReadKey(true);
        }

        //Специальный метод для чтения ввода с обработкой F1 и Tab
        static string ReadInput()
        {
            string input = ""; //Переменная для накопления ввода

            //Цикл чтения
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.F1)
                {
                    //В случае нажатия F1 возвращаем null как признак выхода
                    return null!;
                }
                else if (key.Key == ConsoleKey.Tab)
                {
                    //В случае нажатия Tab возвращаем пустую строку как признак возврата в меню
                    return "";
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    //Если ввод пустой, продолжаем ввод
                    if (input == "")
                    {
                        continue; //Продолжаем ввод, не возвращаем пустую строку
                    }
                    return input; //Возвращаем введенную строку
                }
                else if (key.Key == ConsoleKey.Backspace) //Удаление последнего символа
                {
                    if (input.Length > 0) //Если есть что удалять
                    {
                        input = input.Substring(0, input.Length - 1); //Удаляем последний символ
                        Console.Write("\b \b"); //Удаляем символ с экрана
                    }
                }
                else if (!char.IsControl(key.KeyChar)) //Если символ не управляющий
                {
                    //Добавляем символ к строке и выводим на экран
                    input += key.KeyChar;
                    Console.Write(key.KeyChar);
                }
            }
        }

        //Метод для выхода из игры
        static void ExitGame()
        {
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nВыход из игры...");
            Console.ResetColor();
        }
    }
}
