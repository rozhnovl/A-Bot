using AbyssalBot.Domain.Enums;
using AbyssalBot.Domain.Models;

namespace AbyssalBot.Tactics.Tests.Builders;

/// <summary>
/// Builder for creating test ship hitpoints and energy data
/// </summary>
public class ShipHitpointsBuilder
{
    private double _shield = 1000;
    private double _armor = 1000;
    private double _hull = 500;
    private double _capacitor = 1000;
    private double _maxShield = 1000;
    private double _maxArmor = 1000;
    private double _maxHull = 500;
    private double _maxCapacitor = 1000;

    public ShipHitpointsBuilder WithShield(double shield)
    {
        _shield = shield;
        return this;
    }

    public ShipHitpointsBuilder WithMaxShield(double maxShield)
    {
        _maxShield = maxShield;
        return this;
    }

    public ShipHitpointsBuilder WithArmor(double armor)
    {
        _armor = armor;
        return this;
    }

    public ShipHitpointsBuilder WithHull(double hull)
    {
        _hull = hull;
        return this;
    }

    public ShipHitpointsBuilder WithCapacitor(double capacitor)
    {
        _capacitor = capacitor;
        return this;
    }

    public ShipHitpointsBuilder WithMaxCapacitor(double maxCapacitor)
    {
        _maxCapacitor = maxCapacitor;
        return this;
    }

    public ShipHitpointsBuilder WithCapacitorPercentage(double percentage)
    {
        _capacitor = _maxCapacitor * (percentage / 100.0);
        return this;
    }

    public ShipHitpointsBuilder WithShieldPercentage(double percentage)
    {
        _shield = _maxShield * (percentage / 100.0);
        return this;
    }

    public ShipHitpointsBuilder WithCriticalShield()
    {
        _shield = 100;
        return this;
    }

    public ShipHitpointsBuilder WithLowCapacitor()
    {
        _capacitor = 300;
        return this;
    }

    public ShipHitpointsAndEnergy Build()
    {
        return new ShipHitpointsAndEnergy(
            _shield,
            _armor,
            _hull,
            _capacitor,
            _maxShield,
            _maxArmor,
            _maxHull,
            _maxCapacitor
        );
    }
}

/// <summary>
/// Builder for creating test targets
/// </summary>
public class TargetBuilder
{
    private long _id = 1;
    private string _name = "Test Enemy";
    private string _type = "Cruiser";
    private int _distance = 5000;
    private bool _isEnemy = true;
    private bool _isTargeted = false;
    private bool _isTargeting = false;
    private bool _isSelected = false;
    private bool _weaponAssigned = false;
    private bool _droneAssigned = false;

    public TargetBuilder WithId(long id)
    {
        _id = id;
        return this;
    }

    public TargetBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public TargetBuilder WithType(string type)
    {
        _type = type;
        return this;
    }

    public TargetBuilder WithDistance(int distance)
    {
        _distance = distance;
        return this;
    }

    public TargetBuilder AsEnemy()
    {
        _isEnemy = true;
        return this;
    }

    public TargetBuilder AsTargeted()
    {
        _isTargeted = true;
        return this;
    }

    public TargetBuilder AsTargeting()
    {
        _isTargeting = true;
        return this;
    }

    public TargetBuilder AsSelected()
    {
        _isSelected = true;
        return this;
    }

    public TargetBuilder WithWeaponAssigned()
    {
        _weaponAssigned = true;
        return this;
    }

    public TargetBuilder WithDroneAssigned()
    {
        _droneAssigned = true;
        return this;
    }

    public TargetBuilder AsHighThreat()
    {
        _type = "Frigate";
        _name = "Harrowing Scythe";
        return this;
    }

    public TargetBuilder AsLowThreat()
    {
        _type = "Cruiser";
        _name = "Standard Cruiser";
        return this;
    }

    public TargetBuilder AsCoreCache()
    {
        _name = "Bioadaptive Cache";
        _isEnemy = false;
        return this;
    }

    public TargetBuilder AsConduit()
    {
        _name = "Conduit";
        _isEnemy = false;
        return this;
    }

    public Target Build()
    {
        return new Target(
            _id,
            _name,
            _type,
            _distance,
            _isEnemy,
            _isTargeted,
            _isTargeting,
            _isSelected,
            _weaponAssigned,
            _droneAssigned
        );
    }
}

/// <summary>
/// Builder for creating test ship modules
/// </summary>
public class ShipModuleBuilder
{
    private ModuleType _type = ModuleType.Weapon;
    private int _optimalRange = 4000;
    private bool _isActive = false;
    private bool _isBusy = false;
    private bool _isOverloaded = false;

    public ShipModuleBuilder WithType(ModuleType type)
    {
        _type = type;
        return this;
    }

    public ShipModuleBuilder WithOptimalRange(int range)
    {
        _optimalRange = range;
        return this;
    }

    public ShipModuleBuilder Active()
    {
        _isActive = true;
        return this;
    }

    public ShipModuleBuilder Inactive()
    {
        _isActive = false;
        return this;
    }

    public ShipModuleBuilder Busy()
    {
        _isBusy = true;
        return this;
    }

    public ShipModuleBuilder Overloaded()
    {
        _isOverloaded = true;
        return this;
    }

    public ShipModule Build()
    {
        return new ShipModule(_type, _optimalRange, _isActive, _isBusy, _isOverloaded);
    }
}

/// <summary>
/// Builder for creating test ship fittings
/// </summary>
public class ShipFittingBuilder
{
    private List<ShipModule> _highSlots = new();
    private List<ShipModule> _midSlots = new();
    private List<ShipModule> _lowSlots = new();
    private int _maxTargetingRange = 100000;
    private int _maxTargets = 7;
    private int _maxDronesInSpace = 5;
    private int _optimalAttackRange = 11000;

    public ShipFittingBuilder WithWeapon(int optimalRange = 4000, bool active = false)
    {
        _highSlots.Add(new ShipModuleBuilder()
            .WithType(ModuleType.Weapon)
            .WithOptimalRange(optimalRange)
            .Active()
            .Build());
        return this;
    }

    public ShipFittingBuilder WithShieldBoosters(int count, bool active = false)
    {
        for (int i = 0; i < count; i++)
        {
            var module = new ShipModuleBuilder()
                .WithType(ModuleType.ShieldBooster);

            if (active)
                module.Active();

            _midSlots.Add(module.Build());
        }
        return this;
    }

    public ShipFittingBuilder WithHardeners(int count, bool active = true)
    {
        for (int i = 0; i < count; i++)
        {
            var module = new ShipModuleBuilder()
                .WithType(ModuleType.Hardener);

            if (active)
                module.Active();

            _midSlots.Add(module.Build());
        }
        return this;
    }

    public ShipFittingBuilder WithMWD(bool active = false)
    {
        var module = new ShipModuleBuilder()
            .WithType(ModuleType.MWD);

        if (active)
            module.Active();

        _midSlots.Add(module.Build());
        return this;
    }

    public ShipFittingBuilder WithMaxTargets(int maxTargets)
    {
        _maxTargets = maxTargets;
        return this;
    }

    public ShipFittingBuilder WithMaxTargetingRange(int range)
    {
        _maxTargetingRange = range;
        return this;
    }

    public ShipFittingBuilder WithDrones(int maxDrones = 5)
    {
        _maxDronesInSpace = maxDrones;
        return this;
    }

    public ShipFittingBuilder WithOptimalAttackRange(int range)
    {
        _optimalAttackRange = range;
        return this;
    }

    public ShipFitting Build()
    {
        return new ShipFitting(
            _highSlots,
            _midSlots,
            _lowSlots,
            _maxTargetingRange,
            _maxTargets,
            _maxDronesInSpace,
            _optimalAttackRange
        );
    }
}

/// <summary>
/// Builder for creating test drone states
/// </summary>
public class DroneStateBuilder
{
    private int _dronesInBay = 5;
    private int _dronesInSpace = 0;
    private int _maxDronesInSpace = 5;
    private List<DroneStatus> _droneStatuses = new();

    public DroneStateBuilder WithDronesInBay(int count)
    {
        _dronesInBay = count;
        return this;
    }

    public DroneStateBuilder WithDronesInSpace(int count)
    {
        _dronesInSpace = count;
        return this;
    }

    public DroneStateBuilder WithMaxDrones(int max)
    {
        _maxDronesInSpace = max;
        return this;
    }

    public DroneStateBuilder WithIdleDrones(int count)
    {
        for (int i = 0; i < count; i++)
        {
            _droneStatuses.Add(DroneStatus.Idle);
        }
        return this;
    }

    public DroneStateBuilder WithEngagingDrones(int count)
    {
        for (int i = 0; i < count; i++)
        {
            _droneStatuses.Add(DroneStatus.Engaging);
        }
        return this;
    }

    public DroneStateBuilder WithReturningDrones(int count)
    {
        for (int i = 0; i < count; i++)
        {
            _droneStatuses.Add(DroneStatus.Returning);
        }
        return this;
    }

    public DroneStateBuilder AllDronesIdle()
    {
        _droneStatuses.Clear();
        for (int i = 0; i < _dronesInSpace; i++)
        {
            _droneStatuses.Add(DroneStatus.Idle);
        }
        return this;
    }

    public DroneState Build()
    {
        return new DroneState(
            _dronesInBay,
            _dronesInSpace,
            _maxDronesInSpace,
            _droneStatuses
        );
    }
}
