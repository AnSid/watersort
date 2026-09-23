\# Water Sort



Головоломка-сортировка жидкостей по колбам. Unity 6, 2D, Android + Windows.



\## Стек



\- \*\*Unity 6\*\* (6000.0.6f1), 2D

\- \*\*Input:\*\* старый Input Manager

\- \*\*Прогресс:\*\* PlayerPrefs

\- \*\*Рейтинг:\*\* PocketBase (локальный сервер) + CloudPub (туннель)



\## Управление



\- \*\*Клик по колбе\*\* — выбрать / перелить

\- \*\*Клик во время анимации\*\* — пропустить анимацию

\- \*\*Верхняя панель\*\* — настройки, рейтинг, отмена хода, подсказка, обновить уровень



\## Скрипты



| Файл | Назначение |

|---|---|

| `WaterSort.cs` | Игровая логика: уровни, клики, перелив, победа |

| `TubeVisual.cs` | Визуал одной колбы |

| `PourAnimator.cs` | Анимация перелива |

| `LevelGenerator.cs` | Генерация уровней (BFS-проверка решаемости) |

| `ColorPalette.cs` | Палитра контрастных цветов |

| `Toolbar.cs` | Верхняя панель с кнопками |

| `SettingsPanel.cs` | Панель настроек |

| `ProfilePanel.cs` | Панель профиля |

| `LeaderboardPanel.cs` | Панель рейтинга |

| `LeaderboardAPI.cs` | API рейтинга |

| `LeaderboardCache.cs` | Офлайн-кэш рейтинга |

| `CountryData.cs` | Список стран |

| `CountrySelectPanel.cs` | Выбор страны |

| `PlayerProfile.cs` | Профиль игрока |

| `GameSettings.cs` | Настройки |

| `UIHelper.cs` | Хелперы UI |



\## Бэкенд



\- \*\*PocketBase\*\* 0.40.4 (`C:\\Distr\\pocketbase\_0.40.4\\`)

\- Коллекция `leaderboard`: `device\_id` (unique), `player\_name`, `score`, `country\_code`

\- \*\*CloudPub\*\* — публичный туннель



\## Сборка



\- \*\*Android:\*\* IL2CPP, ARM64 + ARMv7

\- \*\*Windows:\*\* Standalone



\## Лицензия



Приватный проект.

