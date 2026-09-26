# AbotMcp — управление ботом из Claude Code через MCP

`src/AbotMcp` — MCP-сервер (Streamable HTTP) поверх общего движка `AbotEngine.ClientAgent`.
Процесс держит подключение к клиенту(ам) EVE (чтение памяти + ввод), а Claude Code подключается
к нему как к обычному MCP-серверу и управляет действиями по одному шагу: прочитал UI → кликнул →
посмотрел, что изменилось.

## Запуск

```powershell
# из корня репозитория (там лежит fleet.config.json и туда пишутся дампы)
dotnet run --project src/AbotMcp            # dry-run: input-тулы только отчитываются
dotnet run --project src/AbotMcp -- --live  # реальный ввод мыши/клавиатуры
```

Флаги: `--live`, `--port 5030`, `--pid N` (несколько раз), `--profile Worm_T1`, `--rescan`, `--no-hotkey`.

Сервер стартует и без запущенного клиента; когда клиент залогинен — тул `attach_clients`.
Первое подключение к новому клиенту — скан UIRoot (~10–90 с), потом адреса берутся из кеша.

Endpoint: `http://127.0.0.1:5030/mcp`. Статус: `http://127.0.0.1:5030/` и `/state.json`.

Регистрация в Claude Code уже сделана в `/.mcp.json` (project scope, имя `abot`). После первого
запуска Claude Code спросит разрешение на этот сервер; при необходимости `/mcp` для переподключения.

## Безопасность

- По умолчанию dry-run: `click`, `press_keys`, `context_menu`, `type_text`, `click_at` возвращают
  «dry-run (no input sent): …». Реальный ввод только с `--live`.
- `Ctrl+Alt+K` — глобальный аварийный стоп (как в SingleRunner/FleetOrchestrator) + тул `emergency_stop`.
  Защёлкивается: весь ввод и автопилоты остановлены до `resume`.
- Ввод идёт тем же путём, что у бота: именованный mutex `Local\A-Bot.ScreenInput` (не дерётся с другим
  раннером), occlusion-модель окон, `stopRequested`.
- На каждого клиента один lock: ручные тулы и автопилот никогда не перемешиваются внутри одного действия.

## Тулы

| Тул | Что делает |
|---|---|
| `list_clients` | pid/title/role, состояние attach, автопилот, режим live/dry-run, аварийный стоп |
| `attach_clients [pid] [rescan]` | подхватить клиентов, появившихся после старта; attach в фоне |
| `status [client]` | свежий компактный стейт: HP/cap/скорость/манёвр, локи, враги на гриде, активные модули, открытые меню + последнее решение бота |
| `get_ui [client] [sections] [maxOverview]` | структурированный UI с **id элементов**: `ship,targets,overview,menu,windows,drones,inventory,messages,probe,chat` или `all` |
| `find_ui query [client] [limit]` | поиск по **сырому** дереву UI (тип/имя/текст) — для окон, которые парсер ещё не понимает; найденное тоже кликабельно |
| `screenshot [client] [maxWidth]` | PNG окна (inline + файл в `%TEMP%\abot-mcp`), с масштабом и origin для `click_at` |
| `click id [client] [button] [modifiers] [doubleClick]` | клик по id (`ctrl` = лок цели в overview), `button`: left/right/middle/hover |
| `click_at x y …` | клик по абсолютным экранным координатам (без occlusion-проверки) |
| `press_keys "ctrl+f1 f2 esc" [client]` | аккорды клавиш |
| `type_text text [client]` | ввод текста в сфокусированное поле |
| `context_menu id "Warp to Within > Within 0 m" [client] [settleMs]` | ПКМ по элементу и проход по пути меню (regex по сегментам; промежуточные — hover, последний — клик) |
| `watch [client] [seconds] [sampleMs]` | наблюдать несколько секунд, вернуть только изменения состояния |
| `wait ms` | пауза на сервере (≤ 15 с) |
| `bot_step [client]` | один шаг штатной стратегии (run profile); в live исполняет её motions |
| `autopilot start\|stop\|status [client] [intervalMs]` | фоновый цикл `bot_step` |
| `emergency_stop [reason]` / `resume` | защёлка аварийного стопа |
| `dump_diagnostics [client]` | `rawtree.json` + `parsed.json` в `mcp-dump-<pid>-<ts>/` |

`client` — pid, подстрока заголовка окна («Gil») или роль из `fleet.config.json` («deacon»);
при одном клиенте не нужен. Id элементов действительны только для последнего чтения — каждый
action-тул перечитывает UI перед действием и падает с понятной ошибкой, если элемент исчез.

## Что добавлено в движок

`ClientAgent` получил публичный «ручной» слой: `Perceive()` (чтение+парсинг без мозга бота),
`PerceiveRawWithRegion()`, `ExecuteManual(motions, parsed, action)` (тот же `ExecuteMotions`, что у
бота: lease, occlusion, стоп), `MainWindowHandle`.

## Проверено / не проверено

- Проверено без EVE: протокол (initialize/tools/list/tools/call), все тулы отвечают ошибками
  корректно при отсутствии клиента, attach-fail путь, `screenshot` (валидный PNG inline).
- **Не проверено на живом клиенте**: `get_ui`/`find_ui` наполнение, реальные клики. Первый прогон —
  в dry-run: `status` → `get_ui` → `click` (должен вернуть «dry-run … would click») → потом `--live`.
