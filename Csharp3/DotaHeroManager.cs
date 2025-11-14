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
            ["Изменить размер отступа"] = ChangeSpaceSize,
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
                                return currentFlags;
                        }
                        break;
                    case ConsoleKey.Escape:
                        return currentFlags;
                }
            }
        }
    }

    private void ChangeSpaceSize()
    {
        _console.WriteColoredLine($"Текущий размер отступа: {_settings.IndentSize}", ConsoleColor.Yellow);
        var size = _console.ReadInt("Введите новый размер отступа (2-8): ", 2, 8);
        _settings.IndentSize = size;
        _console.WriteColoredLine($"Размер отступа изменен на: {size}", ConsoleColor.Green);
    }

    private void ShowCurrentSettings()
    {
        _console.WriteColoredLine("=== ТЕКУЩИЕ НАСТРОЙКИ ===", ConsoleColor.Cyan);
        _console.WriteColoredLine($"FormatOutput: {_settings.FormatOutput}");
        _console.WriteColoredLine($"IgnoreNullValues: {_settings.IgnoreNullValues}");
        _console.WriteColoredLine($"PropertyBindingFlags: {_settings.PropertyBindingFlags}");
        _console.WriteColoredLine($"FieldBindingFlags: {_settings.FieldBindingFlags}");
        _console.WriteColoredLine($"IndentSize: {_settings.IndentSize}");
        _console.Pause();
    }

    private void AddHero()
    {
        _console.WriteColoredLine("=== ДОБАВЛЕНИЕ НОВОГО ГЕРОЯ ===", ConsoleColor.Cyan);

        var name = _console.ReadString("Введите имя героя: ");

        if (_heroes.Any(h => h.Name == name))
        {
            _console.WriteColoredLine($"{name} УЖЕ СУЩЕСТВУЕТ!", _console.ErrorColor);
            return;
        }

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
            AttackRange = 500,
            AttackRate = 1.7,
            AbilityPointsAvailable = 0,
            Lore = "Лор героя"
        };

        _heroes.Add(hero);

        _console.WriteColoredLine($"Герой '{name}' успешно добавлен!", ConsoleColor.Green);
    }

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
            ["Изменить роли"] = () => hero.Roles = SelectEnum<HeroRole>("Роли", hero.Roles),
            ["Изменить здоровье"] = () => hero.CurrentHealth = _console.ReadDouble("Введите здоровье: ", 0, 5000),
            ["Изменить ману"] = () => hero.CurrentMana = _console.ReadDouble("Введите ману: ", 0, 3000),
            ["Изменить золото"] = () => hero.Gold = _console.ReadInt("Введите золото: ", 0, 50000),
            ["Изменить скорость"] = () => hero.MoveSpeed = _console.ReadInt("Введите скорость: ", 200, 550),
            ["Изменить дальность атаки"] = () => hero.AttackRange = _console.ReadInt("Введите дальность атаки: ", 100, 800),
            ["Изменить скорость атаки"] = () => hero.AttackRate = _console.ReadDouble("Введите скорость атаки: ", 0.1, 5.0),
            ["Изменить доступные очки способностей"] = () => hero.AbilityPointsAvailable = _console.ReadInt("Введите очки способностей: ", 0, 25),
            ["Изменить лор"] = () => hero.Lore = _console.ReadString("Введите лор героя: "),
            ["Изменить способности"] = () => EditAbilities(hero),
            ["Изменить инвентарь"] = () => EditInventory(hero),
            ["Изменить нейтральный предмет"] = () => EditNeutralItem(hero),
            ["Изменить статистику матча"] = () => EditMatchStats(hero),
            ["Изменить пользовательские атрибуты"] = () => EditCustomAttributes(hero),
            ["Назад"] = () => { menuActive = false; }
        };

        _console.Menu(editMenu, ref menuActive, $"РЕДАКТИРОВАНИЕ: {hero.Name}");
    }

    private void EditAbilities(DotaHero hero)
    {
        var abilitiesMenuActive = true;
        var abilitiesMenu = new Dictionary<string, Action>
        {
            ["Добавить способность"] = () => hero.Abilities.Add(CreateNewAbility()),
            ["Удалить способность"] = () => RemoveAbility(hero),
            ["Редактировать существующую"] = () => EditExistingAbility(hero),
            ["Назад"] = () => { abilitiesMenuActive = false; }
        };

        _console.Menu(abilitiesMenu, ref abilitiesMenuActive, $"РЕДАКТИРОВАНИЕ СПОСОБНОСТЕЙ: {hero.Name}");
    }

    private void EditCustomAttributes(DotaHero hero)
    {
        var attributesMenuActive = true;
        var attributesMenu = new Dictionary<string, Action>
        {
            ["Добавить/изменить атрибут"] = () =>
            {
                var key = _console.ReadString("Введите ключ атрибута: ");
                var value = _console.ReadString("Введите значение атрибута: ");
                hero.CustomAttributes[key] = value;
            },
            ["Удалить атрибут"] = () =>
            {
                if (hero.CustomAttributes.Count == 0)
                {
                    _console.WriteColoredLine("Нет пользовательских атрибутов для удаления!", ConsoleColor.Yellow);
                }
                else
                {
                    var keys = hero.CustomAttributes.Keys.ToList();
                    var selectedIndex = ShowSelectionMenu(keys, "ВЫБЕРИТЕ АТРИБУТ ДЛЯ УДАЛЕНИЯ");
                    if (selectedIndex >= 0 && selectedIndex < keys.Count)
                    {
                        hero.CustomAttributes.Remove(keys[selectedIndex]);
                        _console.WriteColoredLine("Атрибут удален!", ConsoleColor.Green);
                    }
                }
            },
            ["Просмотреть все атрибуты"] = () =>
            {
                if (hero.CustomAttributes.Count == 0)
                {
                    _console.WriteColoredLine("Нет пользовательских атрибутов", ConsoleColor.Yellow);
                }
                else
                {
                    _console.WriteColoredLine("=== ПОЛЬЗОВАТЕЛЬСКИЕ АТРИБУТЫ ===", ConsoleColor.Cyan);
                    foreach (var kvp in hero.CustomAttributes)
                    {
                        _console.WriteColoredLine($"{kvp.Key}: {kvp.Value}", ConsoleColor.White);
                    }
                }
                _console.Pause();
            },
            ["Назад"] = () => { attributesMenuActive = false; }
        };

        _console.Menu(attributesMenu, ref attributesMenuActive, $"РЕДАКТИРОВАНИЕ АТРИБУТОВ: {hero.Name}");
    }

    private Ability CreateNewAbility()
    {
        var ability = new Ability
        {
            Name = _console.ReadString("Введите имя способности: "),
            Description = _console.ReadString("Введите описание способности: ")
        };

        var manaCostCount = _console.ReadInt("Введите количество значений маны (1-10): ", 1, 10);
        for (int i = 0; i < manaCostCount; i++)
        {
            ability.ManaCost.Add(_console.ReadInt($"Введите стоимость маны для уровня {i + 1}: ", 0, 1000));
        }

        var cooldownCount = _console.ReadInt("Введите количество значений кулдауна (1-10): ", 1, 10);
        for (int i = 0; i < cooldownCount; i++)
        {
            ability.Cooldown.Add(_console.ReadDouble($"Введите кулдаун для уровня {i + 1}: ", 0, 100));
        }

        return ability;
    }

    private void RemoveAbility(DotaHero hero)
    {
        if (hero.Abilities.Count == 0)
        {
            _console.WriteColoredLine("Нет способностей для удаления!", ConsoleColor.Yellow);
            _console.Pause();
            return;
        }

        var abilityNames = hero.Abilities.Select(a => a.Name).ToList();
        var selectedIndex = ShowSelectionMenu(abilityNames, "ВЫБЕРИТЕ СПОСОБНОСТЬ ДЛЯ УДАЛЕНИЯ");

        if (selectedIndex >= 0 && selectedIndex < hero.Abilities.Count)
        {
            hero.Abilities.RemoveAt(selectedIndex);
            _console.WriteColoredLine("Способность удалена!", ConsoleColor.Green);
        }
    }

    private void EditExistingAbility(DotaHero hero)
    {
        if (hero.Abilities.Count == 0)
        {
            _console.WriteColoredLine("Нет способностей для редактирования!", ConsoleColor.Yellow);
            _console.Pause();
            return;
        }

        var abilityNames = hero.Abilities.Select(a => a.Name).ToList();
        var selectedIndex = ShowSelectionMenu(abilityNames, "ВЫБЕРИТЕ СПОСОБНОСТЬ ДЛЯ РЕДАКТИРОВАНИЯ");

        if (selectedIndex >= 0 && selectedIndex < hero.Abilities.Count)
        {
            var ability = hero.Abilities[selectedIndex];
            ability.Name = _console.ReadString($"Введите имя способности (текущее: {ability.Name}): ");
            ability.Description = _console.ReadString($"Введите описание способности (текущее: {ability.Description}): ");

            ability.ManaCost.Clear();
            var manaCostCount = _console.ReadInt("Введите количество значений маны (1-10): ", 1, 10);
            for (int i = 0; i < manaCostCount; i++)
            {
                ability.ManaCost.Add(_console.ReadInt($"Введите стоимость маны для уровня {i + 1}: ", 0, 1000));
            }

            ability.Cooldown.Clear();
            var cooldownCount = _console.ReadInt("Введите количество значений кулдауна (1-10): ", 1, 10);
            for (int i = 0; i < cooldownCount; i++)
            {
                ability.Cooldown.Add(_console.ReadDouble($"Введите кулдаун для уровня {i + 1}: ", 0, 100));
            }
        }
    }

    private void EditInventory(DotaHero hero)
    {
        var inventoryMenuActive = true;
        var inventoryMenu = new Dictionary<string, Action>
        {
            ["Добавить предмет"] = () => hero.Inventory.Add(CreateNewItem()),
            ["Удалить предмет"] = () => RemoveItem(hero),
            ["Редактировать существующий"] = () => EditExistingItem(hero),
            ["Назад"] = () => { inventoryMenuActive = false; }
        };

        _console.Menu(inventoryMenu, ref inventoryMenuActive, $"РЕДАКТИРОВАНИЕ ИНВЕНТАРЯ: {hero.Name}");
    }

    private DotaItem CreateNewItem()
    {
        var item = new DotaItem
        {
            Name = _console.ReadString("Введите имя предмета: "),
            Cost = _console.ReadInt("Введите стоимость: ", 0, 10000),
            InBackpack = _console.ReadBool("В рюкзаке? (y/n): "),
            CooldownRemaining = _console.ReadDouble("Введите оставшееся время кулдауна: ", 0, 100)
        };

        if (_console.ReadBool("Есть заряды? (y/n): "))
        {
            item.Charges = _console.ReadInt("Введите количество зарядов: ", 1, 100);
        }

        return item;
    }

    private void RemoveItem(DotaHero hero)
    {
        if (hero.Inventory.Count == 0)
        {
            _console.WriteColoredLine("Инвентарь пуст!", ConsoleColor.Yellow);
            _console.Pause();
            return;
        }

        var itemNames = hero.Inventory.Select(i => i.Name).ToList();
        var selectedIndex = ShowSelectionMenu(itemNames, "ВЫБЕРИТЕ ПРЕДМЕТ ДЛЯ УДАЛЕНИЯ");

        if (selectedIndex >= 0 && selectedIndex < hero.Inventory.Count)
        {
            hero.Inventory.RemoveAt(selectedIndex);
            _console.WriteColoredLine("Предмет удален!", ConsoleColor.Green);
        }
    }

    private void EditExistingItem(DotaHero hero)
    {
        if (hero.Inventory.Count == 0)
        {
            _console.WriteColoredLine("Инвентарь пуст!", ConsoleColor.Yellow);
            _console.Pause();
            return;
        }

        var itemNames = hero.Inventory.Select(i => i.Name).ToList();
        var selectedIndex = ShowSelectionMenu(itemNames, "ВЫБЕРИТЕ ПРЕДМЕТ ДЛЯ РЕДАКТИРОВАНИЯ");

        if (selectedIndex >= 0 && selectedIndex < hero.Inventory.Count)
        {
            var item = hero.Inventory[selectedIndex];
            item.Name = _console.ReadString($"Введите имя предмета (текущее: {item.Name}): ");
            item.Cost = _console.ReadInt($"Введите стоимость (текущая: {item.Cost}): ", 0, 10000);
            item.InBackpack = _console.ReadBool($"В рюкзаке? (текущее: {(item.InBackpack ? "y" : "n")}) (y/n): ");
            item.CooldownRemaining = _console.ReadDouble($"Введите оставшееся время кулдауна (текущее: {item.CooldownRemaining}): ", 0, 100);

            if (_console.ReadBool("Изменить заряды? (y/n): "))
            {
                if (_console.ReadBool("Есть заряды? (y/n): "))
                {
                    item.Charges = _console.ReadInt("Введите количество зарядов: ", 1, 100);
                }
                else
                {
                    item.Charges = null;
                }
            }
        }
    }

    private void EditNeutralItem(DotaHero hero)
    {
        var neutralItemMenuActive = true;
        var neutralItemMenu = new Dictionary<string, Action>
        {
            ["Добавить/изменить нейтральный предмет"] = () => hero.NeutralItem = CreateNewItem(),
            ["Удалить нейтральный предмет"] = () => { hero.NeutralItem = null; _console.WriteColoredLine("Нейтральный предмет удален!", ConsoleColor.Green); },
            ["Редактировать текущий"] = () =>
            {
                if (hero.NeutralItem != null)
                {
                    var item = hero.NeutralItem;
                    item.Name = _console.ReadString($"Введите имя предмета (текущее: {item.Name}): ");
                    item.Cost = _console.ReadInt($"Введите стоимость (текущая: {item.Cost}): ", 0, 10000);
                    item.InBackpack = _console.ReadBool($"В рюкзаке? (текущее: {(item.InBackpack ? "y" : "n")}) (y/n): ");
                    item.CooldownRemaining = _console.ReadDouble($"Введите оставшееся время кулдауна (текущее: {item.CooldownRemaining}): ", 0, 100);

                    if (_console.ReadBool("Изменить заряды? (y/n): "))
                    {
                        if (_console.ReadBool("Есть заряды? (y/n): "))
                        {
                            item.Charges = _console.ReadInt("Введите количество зарядов: ", 1, 100);
                        }
                        else
                        {
                            item.Charges = null;
                        }
                    }
                }
                else
                {
                    _console.WriteColoredLine("Нет нейтрального предмета для редактирования!", ConsoleColor.Yellow);
                }
            },
            ["Назад"] = () => { neutralItemMenuActive = false; }
        };

        _console.Menu(neutralItemMenu, ref neutralItemMenuActive, $"РЕДАКТИРОВАНИЕ НЕЙТРАЛЬНОГО ПРЕДМЕТА: {hero.Name}");
    }

    private void EditMatchStats(DotaHero hero)
    {
        var stats = hero.MatchStats;
        stats.Kills = _console.ReadInt($"Введите убийства (текущие: {stats.Kills}): ", 0, 50);
        stats.Deaths = _console.ReadInt($"Введите смерти (текущие: {stats.Deaths}): ", 0, 50);
        stats.Assists = _console.ReadInt($"Введите assists (текущие: {stats.Assists}): ", 0, 50);
        _console.WriteColoredLine("Статистика матча обновлена!", ConsoleColor.Green);
    }

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

    private int ShowSelectionMenu(List<string> items, string title)
    {
        var menuItems = items.ToList();
        menuItems.Add("Назад");

        int selectedIndex = 0;
        while (true)
        {
            Console.Clear();
            _console.WriteColoredLine($"=== {title} ===", ConsoleColor.Cyan);
            Console.WriteLine();

            for (int i = 0; i < menuItems.Count; i++)
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
                    selectedIndex = (selectedIndex - 1 + menuItems.Count) % menuItems.Count;
                    break;
                case ConsoleKey.DownArrow:
                    selectedIndex = (selectedIndex + 1) % menuItems.Count;
                    break;
                case ConsoleKey.Enter:
                    if (selectedIndex < items.Count)
                        return selectedIndex;
                    return -1;
                case ConsoleKey.Escape:
                    return -1;
            }
        }
    }
}
