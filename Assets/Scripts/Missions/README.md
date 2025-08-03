# Mission ScriptableObject System

This system provides a comprehensive way to define and manage mission data for the Hold rail runner game.

## Overview

The mission system supports the 20 introductory missions with full configuration for:
- Mission metadata (ID, name, description)
- Route information (start/end stations)
- Parcel details with type-specific effects
- Time limits and reward structures
- Objectives and learning goals
- Obstacle configurations and difficulty settings

## Key Components

### MissionData ScriptableObject
The main data container with these key features:
- **CreateAssetMenu** integration for easy asset creation
- **Auto-validation** of parcel effects based on type
- **Helper methods** for UI integration
- **Development notes** for internal documentation

### Parcel System
Four parcel types with automatic effect configuration:
- **Standard**: Normal gameplay (1.0x speed)
- **Lightweight**: Minor speed boost (1.2x speed)  
- **Heavy**: Speed reduction (0.8x speed)
- **Fragile**: Fails on ANY collision (zero tolerance)

### Objectives System
20+ objective types covering:
- Basic mechanics (lane switching, jumping, sliding)
- Advanced mechanics (parkour flow, near miss bonuses)
- Special challenges (fragile deliveries, time pressure)
- Environmental hazards (trains, laser grids, police radar)

### Obstacle Configuration
Flexible obstacle system supporting:
- Multiple obstacle types per mission
- Density controls for difficulty scaling
- Special features (near miss, police radar, zero tolerance)
- Track bonus cash configuration

## Usage

### Creating Mission Assets
1. Right-click in Project window
2. Choose "Create > Hold > Mission Data"
3. Configure all mission properties
4. Auto-validation handles parcel-specific effects

### Using Factory Methods
For programmatic creation:
```csharp
var mission = IntroductoryMissionsFactory.CreateMission01FirstTrackRun();
```

### Integration with Game Systems
```csharp
// Access mission properties
string title = mission.GetFormattedTitle();
string route = mission.GetRouteDescription();
int maxReward = mission.GetTotalPossibleReward();
bool hasTimeLimit = mission.HasTimeLimit();

// Configure gameplay based on parcel type
float speedMultiplier = mission.parcel.speedMultiplier;
bool isFragile = mission.parcel.failsOnCollision;

// Set up obstacles
var obstacleTypes = mission.obstacleConfig.obstacleTypes;
float difficulty = mission.obstacleConfig.density;
```

## Validation

Use `MissionDataValidator` component to test mission integrity:
- Add component to any GameObject
- Use context menu "Run Mission Data Tests"
- Check console for detailed validation results

## Mission Examples

### Mission 1: First Track Run (Tutorial)
- **Objective**: Learn basic lane switching
- **Parcel**: Standard bootleg music chip
- **Obstacles**: Few static obstacles only
- **Reward**: ₹50, no time limit

### Mission 6: Handle With Care (Fragile)
- **Objective**: Perfect run required
- **Parcel**: Fragile antique data crystal  
- **Obstacles**: Mixed obstacles requiring precision
- **Reward**: ₹150, zero tolerance mode

### Mission 16: Jatin Das Deadweight (Heavy)
- **Objective**: Heavy haul mechanics
- **Parcel**: Heavy server components (0.8x speed)
- **Obstacles**: Standard density, harder due to speed
- **Reward**: ₹150, 95 second time limit

## Extension Points

The system is designed for easy extension:
- Add new `ParcelType` values
- Extend `ObjectiveType` for new mechanics  
- Add `ObstacleType` entries for new hazards
- Modify `ObstacleConfiguration` for new features

## Integration with Existing Systems

This system is designed to work with the existing DOTS architecture:
- Compatible with existing `Mission` struct in `Maps.Maps.Data`
- Uses same namespace pattern as other game systems
- Follows BovineLabs framework conventions
- Supports Unity's Asset workflow