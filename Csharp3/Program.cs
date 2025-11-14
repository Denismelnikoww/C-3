using CWrite;
using DotaNamespace;
using System.Reflection;

public class DotaHeroManager
{
    private List<DotaHero> _heroes = new List<DotaHero>() { TestHelper.GetHoodwink() };
    private MiniJsonSerializerSettings _settings = new MiniJsonSerializerSettings();
    private readonly MiniJsonSerializer _serializer;
    private readonly ConsoleHelper _console;

    public DotaHeroManager()
    {
        _serializer = new MiniJsonSerializer(_settings);
        _console = new ConsoleHelper();
    }

    public void ShowMainMenu()
    {
        var menuItems = new Dictionary<string, Action>
        {
            ["Переключить FormatOutput"] = () => _settings.FormatOutput = !_settings.FormatOutput,
            ["Переключить IgnoreNullValues"] = () => _settings.IgnoreNullValues = !_settings.IgnoreNullValues,
            ["Изменить Property BindingFlags"] = () =>
                _settings.PropertyBindingFlags = ChangeBindingFlags(_settings.PropertyBindingFlags, "Property"),
            ["Изменить Field BindingFlags"] = () =>
                _settings.FieldBindingFlags = ChangeBindingFlags(_settings.FieldBindingFlags, "Field"),
            ["Изменить размер отступа"] = ChangeIndentSize,
            ["Показать текущие настройки"] = ShowCurrentSettings,
            ["Добавить персонажа"] = AddHero,
            ["Редактировать персонажа"] = EditHero,
            ["Сериализовать в консоль"] = SerializeToConsole,
            ["Сериализовать в файл"] = SerializeToFile,
            ["Выход"] = () => Environment.Exit(0)
        };
        var t = true;

        _console.Menu(menuItems, ref t, "DOTA 2 HERO MANAGER");
    }

    // Оставляем методы настроек как есть, но теперь они вызываются из главного меню
    private BindingFlags ChangeBindingFlags(BindingFlags currentFlags, string type)
    {
        var flags = currentFlags;

        while (true)
        {
            Console.Clear();
            _console.WriteColoredLine($"=== BINDING FLAGS ДЛЯ {type} ===", ConsoleColor.Cyan);
            _console.WriteColoredLine($"Текущие флаги: {flags}", ConsoleColor.Yellow);
            Console.WriteLine();

            var menuItems = new[]
            {
                "Public",
                "NonPublic",
                "Instance",
                "Static",
                "Сбросить",
                "Применить",
                "Назад"
            };

            int selectedIndex = 0;
            bool inMenu = true;

            while (inMenu)
            {
                Console.Clear();
                _console.WriteColoredLine($"=== BINDING FLAGS ДЛЯ {type} ===", ConsoleColor.Cyan);
                _console.WriteColoredLine($"Текущие флаги: {flags}", ConsoleColor.Yellow);
                Console.WriteLine();

                for (int i = 0; i < menuItems.Length; i++)
                {
                    if (i == selectedIndex)
                    {
                        _console.WriteColoredLine($" ► {menuItems[i]}", ConsoleColor.Magenta);
                    }
                    else
                    {
                        Console.WriteLine($"   {menuItems[i]}");
                    }
                }

                var key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex - 1 + menuItems.Length) % menuItems.Length;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex + 1) % menuItems.Length;
                        break;
                    case ConsoleKey.Enter:
                        var selectedItem = menuItems[selectedIndex];
                        switch (selectedItem)
                        {
                            case "Public":
                                flags ^= BindingFlags.Public;
                                break;
                            case "NonPublic":
                                flags ^= BindingFlags.NonPublic;
                                break;
                            case "Instance":
                                flags ^= BindingFlags.Instance;
                                break;
                            case "Static":
                                flags ^= BindingFlags.Static;
                                break;
                            case "Сбросить":
                                flags = BindingFlags.Default;
                                break;
                            case "Применить":
                                return flags;
                            case "Назад":
                                return currentFlags; // Возврат к предыдущему значению
                        }
                        break;
                    case ConsoleKey.Escape:
                        return currentFlags; // Возврат к предыдущему значению
                }
            }
        }
    }

    private void ChangeIndentSize()
    {
        _console.WriteColoredLine($"Текущий размер отступа: {_settings.IndentSize}", ConsoleColor.Yellow);
        var size = _console.ReadInt("Введите новый размер отступа (2-8): ", 2, 8);
        _settings.IndentSize = size;
        _console.WriteColoredLine($"Размер отступа изменен на: {size}", ConsoleColor.Green);
    }

    private void ShowCurrentSettings()
    {
        _console.WriteColoredLine("=== ТЕКУЩИЕ НАСТРОЙКИ ===", ConsoleColor.Cyan);
        _console.WriteColoredLine($"FormatOutput: {_settings.FormatOutput}", ConsoleColor.White);
        _console.WriteColoredLine($"IgnoreNullValues: {_settings.IgnoreNullValues}", ConsoleColor.White);
        _console.WriteColoredLine($"PropertyBindingFlags: {_settings.PropertyBindingFlags}", ConsoleColor.White);
        _console.WriteColoredLine($"FieldBindingFlags: {_settings.FieldBindingFlags}", ConsoleColor.White);
        _console.WriteColoredLine($"IndentSize: {_settings.IndentSize}", ConsoleColor.White);
        _console.Pause();
    }

    private void AddHero()
    {
        _console.WriteColoredLine("=== ДОБАВЛЕНИЕ НОВОГО ГЕРОЯ ===", ConsoleColor.Cyan);

        var name = _console.ReadString("Введите имя героя: ");

        var hero = new DotaHero
        {
            Name = name,
            Attribute = PrimaryAttribute.Agility,
            Complexity = HeroComplexity.Moderate,
            Roles = HeroRole.Support,
            CurrentLevel = 1,
            CurrentHealth = 500,
            CurrentMana = 200,
            Gold = 625,
            MatchStats = new MatchStats(),

            MoveSpeed = 300,
            AttackRange = 500
        };

        _heroes.Add(hero);

        _console.WriteColoredLine($"Герой '{name}' успешно добавлен!", ConsoleColor.Green);
        _console.Pause();
    }

    // 3. Редактировать персонажа
    private void EditHero()
    {
        if (_heroes.Count == 0)
        {
            _console.WriteColoredLine("Нет героев для редактирования!", ConsoleColor.Red);
            _console.Pause();
            return;
        }

        var hero = SelectHero("ВЫБЕРИТЕ ГЕРОЯ ДЛЯ РЕДАКТИРОВАНИЯ");
        if (hero != null)
        {
            EditHeroProperties(hero);
        }
    }

    private void EditHeroProperties(DotaHero hero)
    {
        var menuActive = true;
        var editMenu = new Dictionary<string, Action>
        {
            ["Изменить имя"] = () => hero.Name = _console.ReadString("Введите новое имя: "),
            ["Изменить уровень"] = () => hero.CurrentLevel = _console.ReadInt("Введите уровень: ", 1, 30),
            ["Изменить основной атрибут"] = () => hero.Attribute = SelectEnum<PrimaryAttribute>("Основной атрибут", hero.Attribute),
            ["Изменить сложность"] = () => hero.Complexity = SelectEnum<HeroComplexity>("Сложность", hero.Complexity),
            ["Изменить здоровье"] = () => hero.CurrentHealth = _console.ReadDouble("Введите здоровье: ", 0, 5000),
            ["Изменить ману"] = () => hero.CurrentMana = _console.ReadDouble("Введите ману: ", 0, 3000),
            ["Изменить золото"] = () => hero.Gold = _console.ReadInt("Введите золото: ", 0, 50000),
            ["Изменить скорость"] = () => hero.MoveSpeed = _console.ReadInt("Введите скорость: ", 200, 550),
            ["Изменить дальность атаки"] = () => hero.AttackRange = _console.ReadInt("Введите дальность атаки: ", 100, 800),
            ["Назад"] = () => { menuActive = false; }
        };

        _console.Menu(editMenu, ref menuActive,$"РЕДАКТИРОВАНИЕ: {hero.Name}");
    }

    // 4. Сериализовать в консоль
    private void SerializeToConsole()
    {
        if (_heroes.Count == 0)
        {
            _console.WriteColoredLine("Нет героев для сериализации!", ConsoleColor.Red);
            _console.Pause();
            return;
        }

        var selectedHeroes = SelectMultipleHeroes("ВЫБЕРИТЕ ГЕРОЕВ ДЛЯ СЕРИАЛИЗАЦИИ");
        if (selectedHeroes.Count == 0) return;

        _console.WriteColoredLine("=== РЕЗУЛЬТАТ СЕРИАЛИЗАЦИИ ===", ConsoleColor.Cyan);

        foreach (var hero in selectedHeroes)
        {
            var json = _serializer.Serialize(hero);
            _console.WriteColoredLine($"\n=== {hero.Name} ===", ConsoleColor.Yellow);
            Console.WriteLine(json);
            _console.WriteColoredLine(new string('=', 50), ConsoleColor.Gray);
        }

        _console.Pause();
    }

    // 5. Сериализовать в файл
    private void SerializeToFile()
    {
        if (_heroes.Count == 0)
        {
            _console.WriteColoredLine("Нет героев для сериализации!", ConsoleColor.Red);
            _console.Pause();
            return;
        }

        var selectedHeroes = SelectMultipleHeroes("ВЫБЕРИТЕ ГЕРОЕВ ДЛЯ СОХРАНЕНИЯ");
        if (selectedHeroes.Count == 0) return;

        var fileName = _console.ReadString("Введите имя файла (без расширения): ");
        fileName = Path.ChangeExtension(fileName, ".json");

        try
        {
            using var writer = new StreamWriter(fileName);
            foreach (var hero in selectedHeroes)
            {
                var json = _serializer.Serialize(hero);
                writer.WriteLine($"// {hero.Name}");
                writer.WriteLine(json);
                writer.WriteLine();
            }

            _console.WriteColoredLine($"Данные успешно сохранены в файл: {fileName}", ConsoleColor.Green);
        }
        catch (Exception ex)
        {
            _console.WriteColoredLine($"Ошибка при сохранении файла: {ex.Message}", ConsoleColor.Red);
        }

        _console.Pause();
    }

    // Вспомогательные методы
    private DotaHero SelectHero(string title)
    {
        var heroMenu = _heroes.ToDictionary(
            h => $"{h.Name} (Уровень {h.CurrentLevel})",
            h => (Action)(() => { })
        );
        heroMenu["Назад"] = () => { };

        int selectedIndex = 0;
        while (true)
        {
            Console.Clear();
            _console.WriteColoredLine($"=== {title} ===", ConsoleColor.Cyan);
            Console.WriteLine();

            int i = 0;
            foreach (var item in heroMenu.Keys)
            {
                if (i == selectedIndex)
                {
                    _console.WriteColoredLine($" ► {item}", ConsoleColor.Magenta);
                }
                else
                {
                    Console.WriteLine($"   {item}");
                }
                i++;
            }

            var key = Console.ReadKey(true).Key;
            switch (key)
            {
                case ConsoleKey.UpArrow:
                    selectedIndex = (selectedIndex - 1 + heroMenu.Count) % heroMenu.Count;
                    break;
                case ConsoleKey.DownArrow:
                    selectedIndex = (selectedIndex + 1) % heroMenu.Count;
                    break;
                case ConsoleKey.Enter:
                    if (selectedIndex < _heroes.Count)
                        return _heroes[selectedIndex];
                    return null;
                case ConsoleKey.Escape:
                    return null;
            }
        }
    }

    private List<DotaHero> SelectMultipleHeroes(string title)
    {
        var selectedHeroes = new List<DotaHero>();
        var selectionStates = new bool[_heroes.Count];

        int selectedIndex = 0;
        bool selecting = true;

        while (selecting)
        {
            Console.Clear();
            _console.WriteColoredLine($"=== {title} ===", ConsoleColor.Cyan);
            _console.WriteColoredLine("Space - выбрать/снять, Enter - подтвердить, Esc - отмена", ConsoleColor.Yellow);
            Console.WriteLine();

            for (int i = 0; i < _heroes.Count; i++)
            {
                var marker = selectionStates[i] ? "[✓] " : "[ ] ";
                if (i == selectedIndex)
                {
                    _console.WriteColoredLine($" ► {marker}{_heroes[i].Name} (Уровень {_heroes[i].CurrentLevel})", ConsoleColor.Magenta);
                }
                else
                {
                    Console.WriteLine($"   {marker}{_heroes[i].Name} (Уровень {_heroes[i].CurrentLevel})");
                }
            }

            var key = Console.ReadKey(true).Key;
            switch (key)
            {
                case ConsoleKey.UpArrow:
                    selectedIndex = (selectedIndex - 1 + _heroes.Count) % _heroes.Count;
                    break;
                case ConsoleKey.DownArrow:
                    selectedIndex = (selectedIndex + 1) % _heroes.Count;
                    break;
                case ConsoleKey.Spacebar:
                    selectionStates[selectedIndex] = !selectionStates[selectedIndex];
                    break;
                case ConsoleKey.Enter:
                    selecting = false;
                    break;
                case ConsoleKey.Escape:
                    return new List<DotaHero>();
            }
        }

        for (int i = 0; i < _heroes.Count; i++)
        {
            if (selectionStates[i])
                selectedHeroes.Add(_heroes[i]);
        }

        return selectedHeroes;
    }

    private T SelectEnum<T>(string title, T currentValue) where T : Enum
    {
        var values = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
        var menu = values.ToDictionary(
            v => v.ToString(),
            v => (Action)(() => { })
        );
        menu["Назад"] = () => { };

        int currentIndex = Array.IndexOf(values, currentValue);
        int selectedIndex = currentIndex >= 0 ? currentIndex : 0;

        while (true)
        {
            Console.Clear();
            _console.WriteColoredLine($"=== {title} ===", ConsoleColor.Cyan);
            Console.WriteLine();

            int i = 0;
            foreach (var item in menu.Keys)
            {
                if (i == selectedIndex)
                {
                    _console.WriteColoredLine($" ► {item}", ConsoleColor.Magenta);
                }
                else
                {
                    Console.WriteLine($"   {item}");
                }
                i++;
            }

            var key = Console.ReadKey(true).Key;
            switch (key)
            {
                case ConsoleKey.UpArrow:
                    selectedIndex = (selectedIndex - 1 + menu.Count) % menu.Count;
                    break;
                case ConsoleKey.DownArrow:
                    selectedIndex = (selectedIndex + 1) % menu.Count;
                    break;
                case ConsoleKey.Enter:
                    if (selectedIndex < values.Length)
                        return values[selectedIndex];
                    return currentValue;
                case ConsoleKey.Escape:
                    return currentValue;
            }
        }
    }
}

// Обновленный Program для запуска
public class Program
{
    public static void Main()
    {
        var manager = new DotaHeroManager();
        manager.ShowMainMenu();
    }
}
