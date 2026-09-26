# Activity supervisor

Да, боту нужен отдельный слой «чем заняться». Combat brain не должен одновременно решать,
искать ли аномалию, заканчивать ли belt-route или входить ли в Abyss. Эти задачи имеют разные
риски и разные признаки завершения.

## Три уровня

1. **Perception/survey** сообщает факты: доступность anomalies/belts, local roster, состояние флота,
   cargo/filaments, текущий warp/combat и качество чтения UI.
2. **Activity supervisor** выбирает только деятельность: `Hold`, `Anomalies`, `Belts`, `Abyss`.
3. **Strategy/doctrine** исполняет выбранную деятельность и не может самостоятельно повысить риск.

Реализация чистого второго слоя находится в `AbotEngine/Fleet/FleetActivitySupervisor.cs`.
Она уже участвует в `--brain-selftest`, но пока намеренно не подключена к live actuator.

## Приоритет

```text
опасность / плохое чтение / неверный состав / 3 ошибки подряд
    -> HOLD
уже начат warp/site/combat
    -> закончить текущую атомарную деятельность
есть Serpentis anomalies
    -> Anomalies
anomalies подтверждённо закончились, есть непосещённые belts
    -> Belts
anomalies и belts подтверждённо закончились + весь Abyss preflight зелёный
    -> Abyss
иначе
    -> HOLD
```

`Unknown` не равно `Exhausted`. Один нераспарсенный context menu, закрывшийся Local или пропавший
UIRoot никогда не разрешает перейти к более рискованной деятельности.

## Reliability rules

- Новый кандидат должен повториться минимум в трёх последовательных observations.
- Между переключениями действует cooldown (по умолчанию 15 секунд).
- Пока кандидат стабилизируется, command — `Hold`, а не повтор старой исчерпанной стратегии.
- Warp, посадка и начатый бой атомарны: supervisor не меняет activity посреди выполнения.
- Emergency stop, critical damage, invalid composition или unreadable client дают немедленный `Hold`.
- Три последовательных ошибки activity открывают circuit breaker и дают немедленный `Hold`.
- Решение содержит candidate, blockers и человекочитаемую причину для dashboard/log.

## Abyss gate

Маленький Local — только один из сигналов, не разрешение войти. Для `Abyss` одновременно нужны:

- anomalies и belts имеют достоверный статус `Exhausted`;
- Local count известен и не выше настроенного порога;
- Local roster стабилен несколько observations;
- нет hostile/suspect safety flags;
- Abyss явно разрешён конфигурацией;
- live actuator откалиброван;
- все три корабля полностью здоровы и состав валиден;
- capacitor готов;
- filaments и cargo/preflight подтверждены из UI;
- хватает безопасного session time на полный заход.

Любой `false` или `unknown` → `Hold`. Сейчас `ActuatorCalibrated=false`, поэтому supervisor физически
не должен выбирать live Abyss даже при пустом Local.

## Что ещё нужно подключить

1. Read-only survey anomalies, который отличает «меню прочитано, Serpentis нет» от parse failure.
2. Belt availability из полного списка и persisted visited-set (сейчас set живёт только до рестарта).
3. Парсер Local member count + roster stability + standings/flags.
4. Fleet preflight: HP, cap, module confirmation, cargo, filaments и session time.
5. Публикация supervisor decision/blockers в dashboard и status journal.
6. Только после исправления hardener `11646` и Abyss actuator — wiring решения в live route.

## Offline coverage

Self-test проверяет:

- `Anomalies > Belts`;
- fallback `Exhausted anomalies -> Belts`;
- `Unknown -> Hold`;
- запрет Abyss по одному лишь маленькому Local;
- разрешение Abyss только при полном preflight;
- атомарность committed activity;
- immediate Hold по safety/circuit breaker;
- hysteresis при прыгающих сигналах.
