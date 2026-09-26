# 2× Retribution + Deacon

Текущий состав задаётся в `fleet.config.json`. Роли определяются по имени персонажа, а не по PID:

- `Gil-Gelad` — `tank-retri`, fleet commander;
- `Fenreire` — `wing-retri`;
- `Tiara Parvi` — `deacon`.

PID меняются после перезапуска EVE и не должны попадать в постоянную конфигурацию.

> **Live-пауза (28 августа 2026):** не возобновлять live-запуск, пока не исправлено и не проверено
> подтверждение активного состояния `Explosive Armor Hardener II` (`typeId 11646`) на Deacon.
> Последний оркестратор остановлен. Подробности — в `session-2026-08-28.md`.

## Проверки и режимы

Все doctrine-сценарии без подключения к EVE:

```powershell
dotnet run --project src/FleetOrchestrator/FleetOrchestrator.csproj -- --brain-selftest
```

Read-only репетиция текущей сетки:

```powershell
dotnet run --project src/FleetOrchestrator/FleetOrchestrator.csproj -- --setup retri-deacon --phase anomaly
```

Live-бой без автоматической навигации (использовать только после снятия live-паузы):

```powershell
dotnet run --project src/FleetOrchestrator/FleetOrchestrator.csproj -- --live --setup retri-deacon --phase anomaly --actuate-retri
```

Маршруты навигации взаимоисключающие:

```powershell
# Serpentis combat anomalies
dotnet run --project src/FleetOrchestrator/FleetOrchestrator.csproj -- --live --setup retri-deacon --phase anomaly --actuate-retri --auto-serpentis

# один круг по всем asteroid belts системы
dotnet run --project src/FleetOrchestrator/FleetOrchestrator.csproj -- --live --setup retri-deacon --phase anomaly --actuate-retri --auto-belts
```

`--phase abyss` включает только room policy и остаётся без live-актуатора.

## Навигация

Оба маршрута управляются через ПКМ по свободному месту в космосе и требуют именно fleet warp.
Personal warp не используется как fallback.

Anomalies:

1. `ПКМ → Anomalies`.
2. Первый пункт, содержащий `Serpentis`.
3. В следующем уровне — `Warp Fleet`.
4. После посадки Gil-Gelad делает `ПКМ на себе в Fleet window → Regroup`.
5. После полной зачистки выбирается следующая аномалия.

Belts:

1. `ПКМ → Asteroid Belts`.
2. Первый ещё не посещённый `… - Asteroid Belt N`.
3. `Warp Fleet (Point)`.
4. После посадки — `Regroup`.
5. Пустой белт считается проверенным через 8 секунд; белт с NPC — после полной зачистки.
6. Имена посещённых белтов хранятся в `ClientAgent` до завершения процесса.
7. После последнего белта состояние маршрута становится `complete`; новый круг автоматически не начинается.

Контекстные меню и кнопки popup-окон пишутся в `fleet-orchestrator.log` уровнями `CONTEXT L0/L1/...`.

## Боевой цикл

- В новую сетку первым двигается tank-Retri; wing-Retri и Deacon ждут его аггро.
- После наблюдаемого повреждения armor Deacon включает нужные RR. При 100% armor RR выключены.
- Если Deacon ловит аггро, его ремонтирует wing-Retri.
- Оба Retri продолжают атаковать: RR wing-Retri не заменяет его боевую роль.
- Wreck никогда не является боевой целью; случайно залоченные wreck снимаются.
- Afterburner включается только по positioning order.
- Protection modules должны оставаться включёнными постоянно в космосе.
- Перед переходом/следующей сеткой: снять теги, перезапустить reactive, восстановить cap, закончить loot,
  выровнять позиции и только потом двигаться дальше.

Физический ввод сериализован общим межпроцессным mutex: один клиент получает короткий burst действий,
затем фокус переходит к следующему. Модули активируются попеременно hotkey и кликом; клик попадает только
в нижние 70% круга модуля, чтобы не включать overheat.

Глобальный аварийный стоп: `Ctrl+Alt+K`. Он работает независимо от активного окна.

## Выбор огня

Приоритет: neut/starving → web/painter → прочий EWAR → repairers → DPS-frigates → cruisers → BC → BS.

`focusFire`/`shuffle` считается по реальному урону текущего crystal:

- берутся shield/armor/hull NPC и resonance каждого слоя из встроенной NPC DB;
- учитывается профиль EM/thermal текущего crystal и множители двух конкретных Retribution;
- после lock полный HP заменяется остатками HP из target bar;
- harmless peers можно делить только когда каждому Retri требуется не больше пяти solo volleys;
- неизвестный crystal, отсутствующая NPC-защита, опасный EWAR или цель тяжелее пяти залпов → `focusFire`.

Базовый fit: четыре `Small Focused Beam Laser II`, `perTurretDamageMultiplier=11.7142`.
Фактические значения задаются отдельно для каждого пилота в `fleet.config.json`.

### Кристаллы и дистанция

Диапазоны сняты с текущего build Gil/Fen. Второе число — `optimal + accuracy falloff`, а не отдельная
ширина falloff; у всех шести кристаллов ширина равна 3 км.

| Crystal | Optimal | Optimal + falloff |
|---|---:|---:|
| Aurora | 45 км | 48 км |
| Xray | 19 км | 22 км |
| Standard | 25 км | 28 км |
| Gamma | 15 км | 18 км |
| Gleam | 6 км | 9 км |
| Multifrequency | 12 км | 15 км |

Селектор оценивает `raw crystal damage × range-only hit chance` на большей из двух дистанций:
текущей дистанции до цели и ожидаемой дистанции боя/орбиты. Это не даёт преждевременно зарядить
короткий кристалл во время сближения и заранее учитывает врага, который будет держать дальнюю орбиту.
Текущий кристалл сохраняется, если его оценка не хуже 85% от лучшей, чтобы не дёргать reload на границах.
Gleam используется только для явно заданной близкой boss-orbit.

До optimal штрафа за дальность нет. На `optimal + falloff` range-only chance to hit равен 50%,
на `optimal + 2 × falloff` — 6.25%; tracking и signature дают отдельные дополнительные штрафы.
Нормальный режим поэтому стреляет не дальше второго числа в таблице. За этой границей лазеры
останавливаются, корабль продолжает сближение, а огонь возобновляется после входа в полезный диапазон.
Источник механики: https://www.eveonline.com/eve-academy/ships/combat-mechanics

### pyfa как источник fit-профиля

`pyfa` планируется использовать офлайн: импортировать точные fit/skills/implants и генерировать для
бота компактный проверяемый профиль (fit hash/version, optimal/falloff, volley, cycle time, tracking,
capacitor и resists). Live-loop не должен зависеть от Python/wxWidgets или запускать pyfa во время боя.
Предпочтительная граница интеграции — экспорт данных, а не копирование GPL-кода в процесс бота.
Проект: https://github.com/pyfa-org/pyfa

## Локальные базы данных

- `npc-stats.json` — NPC HP, layer resonance, EWAR, DPS и служебные признаки.
- `module-types.json` — 4 655 EVE module type IDs с именами, group ID и published-флагом.

Module DB встроена в сборку и не требует сети. Неизвестный ID остаётся читаемым как `module type N`.
Диагностика `parsed.json` содержит `typeId` и `typeName`; action log пишет имя вместе с ID.
Обновление базы: `gen-module-types.py <reference-data.tar.xz>`.

## Abyss policy (пока read-only)

Room policy покрывает Kikimora, Vet'akh/Vhetaguth, Leshak, Deepwatcher, Karybdis, Overmind,
Angels/Sleepers, rogue-drone frigates/BC и Vedmak. Учтены tank-first aggro, all-in reps,
blue-cloud/tracking-pylon avoidance, нужная дистанция, crystal и MWD discipline.

Live Abyss остаётся закрыт: ещё не подтверждены sequence tags, безопасный reset reactive hardener,
fleetmate distance, cache-looted state и переход через gate. При неизвестной готовности policy держит gate.
