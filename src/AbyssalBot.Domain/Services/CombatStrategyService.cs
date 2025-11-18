using AbyssalBot.Domain.Enums;
using AbyssalBot.Domain.Interfaces;
using AbyssalBot.Domain.Models;

namespace AbyssalBot.Domain.Services;

/// <summary>
/// Main combat strategy service that coordinates all combat decisions
/// </summary>
public class CombatStrategyService
{
    private readonly NpcInformationService _npcInfoService;
    private readonly TargetPriorityService _targetPriorityService;
    private readonly ManeuverDecisionService _maneuverService;
    private readonly DroneControlService _droneService;
    private readonly TankingDecisionService _tankingService;

    public CombatStrategyService(
        NpcInformationService npcInfoService,
        TargetPriorityService targetPriorityService,
        ManeuverDecisionService maneuverService,
        DroneControlService droneService,
        TankingDecisionService tankingService)
    {
        _npcInfoService = npcInfoService;
        _targetPriorityService = targetPriorityService;
        _maneuverService = maneuverService;
        _droneService = droneService;
        _tankingService = tankingService;
    }

    /// <summary>
    /// Generates combat decisions based on current state
    /// </summary>
    public IEnumerable<CombatDecision> GenerateCombatDecisions(
        IShipStateProvider shipState,
        ITargetProvider targetProvider)
    {
        var fitting = shipState.GetShipFitting();
        var hitpoints = shipState.GetHitpointsAndEnergy();
        var currentManeuver = shipState.GetCurrentManeuver();
        var targets = shipState.GetCurrentTargets();
        var droneState = shipState.GetDroneState();
        var availableTargets = targetProvider.GetEnemyTargets();

        // Calculate incoming DPS
        var incomingDps = _targetPriorityService.CalculateIncomingDps(availableTargets);

        // 1. Always activate hardeners first
        foreach (var decision in _tankingService.DecideHardeners(fitting.GetAlwaysActiveModules()))
        {
            yield return decision;
        }

        // 2. Find orbit beacon
        var coreCache = targetProvider.FindTargetByName("Bioadaptive")
                        ?? targetProvider.FindTargetByName("Biocombinative");
        var conduit = targetProvider.FindTargetByName("Conduit");
        var orbitBeacon = _maneuverService.SelectOrbitBeacon(
            availableTargets,
            coreCache,
            conduit,
            _npcInfoService
        );

        // 3. Decide maneuver
        var maneuverDecision = _maneuverService.DecideManeuver(
            currentManeuver,
            incomingDps,
            orbitBeacon,
            orbitBeacon?.Distance ?? 0
        );
        if (maneuverDecision != null)
        {
            yield return maneuverDecision;
        }

        // 4. Decide MWD usage
        var mwd = fitting.GetMWD();
        if (mwd != null && orbitBeacon != null)
        {
            var shouldActivateMwd = _maneuverService.ShouldActivateMWD(
                incomingDps,
                orbitBeacon.Distance,
                currentManeuver
            );

            if (mwd.IsActive != shouldActivateMwd && !mwd.IsBusy)
            {
                yield return new ModuleDecision(
                    ModuleType.MWD,
                    shouldActivateMwd,
                    false,
                    shouldActivateMwd
                        ? $"Activating MWD - distance to beacon: {orbitBeacon.Distance}m"
                        : "Deactivating MWD - close to beacon"
                );
            }
        }

        // 5. Tanking decisions
        foreach (var decision in _tankingService.DecideShieldBoosting(
                     hitpoints,
                     incomingDps,
                     fitting.GetShieldBoosters()))
        {
            yield return decision;
        }

        // 6. Target locking decisions
        var targetsToLock = _targetPriorityService.GetTargetsToLock(
            availableTargets,
            fitting.MaxTargetingRange,
            fitting.MaxTargets,
            targets.Count
        );

        foreach (var target in targetsToLock)
        {
            yield return new LockTargetDecision(
                target,
                $"Locking priority target: {target.Name} at {target.Distance}m"
            );
        }

        // 7. Weapon decisions
        var activeTarget = targets.GetActiveCombatTarget();
        if (activeTarget != null)
        {
            var weapon = fitting.GetWeapon();
            if (weapon != null && !weapon.IsBusy)
            {
                var shouldFire = activeTarget.Distance <= fitting.OptimalAttackRange;
                if (weapon.IsActive != shouldFire)
                {
                    yield return new ModuleDecision(
                        ModuleType.Weapon,
                        shouldFire,
                        false,
                        shouldFire
                            ? $"Firing weapons at {activeTarget.Name}"
                            : $"Target out of range ({activeTarget.Distance}m > {fitting.OptimalAttackRange}m)"
                    );
                }

                // Orbit target if out of optimal range
                if (activeTarget.Distance > weapon.OptimalRange
                    && currentManeuver != ShipManeuverType.Orbit)
                {
                    yield return new ManeuverDecision(
                        ShipManeuverType.Orbit,
                        activeTarget,
                        weapon.OptimalRange,
                        $"Target out of optimal range, orbiting at {weapon.OptimalRange}m"
                    );
                }
            }
        }

        // 8. Drone decisions
        var noEnemies = availableTargets.Count == 0;
        foreach (var decision in _droneService.GetDroneDecisions(
                     droneState,
                     activeTarget,
                     noEnemies))
        {
            yield return decision;
        }
    }

    /// <summary>
    /// Checks if combat is complete (no enemies remaining and drones returned)
    /// </summary>
    public bool IsCombatComplete(ITargetProvider targetProvider, DroneState droneState)
    {
        var enemies = targetProvider.GetEnemyTargets();
        return enemies.Count == 0 && droneState.AllDronesInBay;
    }
}
