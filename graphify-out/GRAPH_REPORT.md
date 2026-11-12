# Graph Report - Powered-Wheelchair-Soccer  (2026-11-11)

## Corpus Check
- Corpus is ~24,524 words - fits in a single context window. You may not need a graph.

## Summary
- 1259 nodes · 2221 edges · 112 communities (55 shown, 57 thin omitted)
- Extraction: 97% EXTRACTED · 3% INFERRED · 0% AMBIGUOUS · INFERRED: 75 edges (avg confidence: 0.85)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- XR Simulator Device Modes
- Game Settings & VR Menu
- Simulator UI Theme & Teleport Input
- Sample Project Validation
- Teleport Reticles
- Controller Input Actions
- Locomotion Move Providers
- Package Lock Root
- Package: searcher
- Package: modules.video
- Poke Follow Affordance
- Sample Editor Inspectors
- Package: modules.vr
- Script Imports Hub
- Teams & Player Awareness
- Package: modules.subsystems
- Player Roles & Motor
- Package: modules.cloth
- Simulated Hand Expressions
- XR Rig Controller
- Object Spawner
- Simulated Controller UI
- State Machine Core
- Device Simulator Namespace
- Package: modules.screencapture
- Starter Assets Namespace
- World Registry Items
- Score Keeper & UI
- Package Manifest
- Manifest Dependencies
- Package: test-framework
- Package: modules.ui
- Package: inputsystem
- Package: xr.openxr
- Package: modules.assetbundle
- Package: ugui
- Package: xr.mock-hmd
- XR Platform Detection
- Package: multiplayer.center
- Package: modules.imgui
- Package: modules.tilemap
- Toggle Component Zone
- AI State Enum
- Package: modules.umbra
- Mesh & Lifetime Utilities
- Gaze Input Manager
- World Reset Flow
- Solution & Assemblies
- Toggle Color UI
- Registry Registration
- Ball Prop
- Center Spot
- Pens
- Field Zones
- Navigation Agent Setup
- Package: burst
- Package: ext.nunit
- Package: mathematics
- Package: modules.terrain
- Package: nuget.mono-cecil
- Field
- Package: ai.navigation
- Package: collab-proxy
- Package: collections
- Package: ide.rider
- Package: ide.visualstudio
- Increment UI Text
- Controller Animator
- Package: modules.wind
- Package: ide.rider
- Package: ide.visualstudio
- Package: modules.accessibility
- Package: modules.adaptiveperformance
- Package: modules.ai
- Package: modules.androidjni
- Package: modules.animation
- Package: modules.audio
- Package: modules.cloth
- Package: modules.director
- Package: modules.imageconversion
- Package: modules.imgui
- Package: modules.jsonserialize
- Package: modules.particlesystem
- Package: modules.physics2d
- Package: modules.screencapture
- Package: modules.terrainphysics
- Package: modules.tilemap
- Package: modules.ui
- Package: modules.uielements
- Package: modules.unityanalytics
- Package: modules.unitywebrequest
- Package: modules.unitywebrequestassetbundle
- Package: modules.unitywebrequestaudio
- Package: modules.unitywebrequesttexture
- Package: modules.unitywebrequestwww
- Package: modules.vehicles
- Package: modules.wind
- Package: modules.xr
- Package: multiplayer.center
- Package: render-pipelines.universal
- Package: test-framework
- Package: timeline
- Package: xr.interaction.toolkit
- Package: xr.management
- Package: xr.mock-hmd
- Package: xr.openxr

## God Nodes (most connected - your core abstractions)
1. `XRDeviceSimulatorUI` - 89 edges
2. `Player` - 42 edges
3. `ControllerInputActionManager` - 38 edges
4. `XRInteractionSimulatorInputFeedbackUI` - 36 edges
5. `PlayerStateMachine` - 34 edges
6. `VRMainMenu` - 33 edges
7. `XRInteractionSimulatorPlayModeMenu` - 31 edges
8. `XRPokeFollowAffordance` - 27 edges
9. `PlayerAwareness` - 26 edges
10. `RegistryItem` - 25 edges

## Surprising Connections (you probably didn't know these)
- `PermissionRequestGroup` --references--> `XRPlatformType`  [EXTRACTED]
  Assets/Samples/XR Interaction Toolkit/3.3.2/Starter Assets/Scripts/PermissionsManager.cs → Assets/Samples/XR Interaction Toolkit/3.3.2/Starter Assets/Scripts/PlatformUnderstanding.cs
- `XRDeviceSimulatorControllerUI` --references--> `XRDeviceSimulatorUI`  [EXTRACTED]
  Assets/Samples/XR Interaction Toolkit/3.3.2/XR Device Simulator/Scripts/XRDeviceSimulatorControllerUI.cs → Assets/Samples/XR Interaction Toolkit/3.3.2/XR Device Simulator/Scripts/XRDeviceSimulatorUI.cs
- `XRDeviceSimulatorHandsUI` --references--> `XRDeviceSimulatorUI`  [EXTRACTED]
  Assets/Samples/XR Interaction Toolkit/3.3.2/XR Device Simulator/Scripts/XRDeviceSimulatorHandsUI.cs → Assets/Samples/XR Interaction Toolkit/3.3.2/XR Device Simulator/Scripts/XRDeviceSimulatorUI.cs
- `Ball` --implements--> `IRegistryProvider`  [EXTRACTED]
  Assets/Scripts/TrackedItems/Ball.cs → Assets/Scripts/GameState/WorldRegister.cs
- `Player` --implements--> `IRegistryProvider`  [EXTRACTED]
  Assets/Scripts/TrackedItems/Player.cs → Assets/Scripts/GameState/WorldRegister.cs

## Import Cycles
- None detected.

## Communities (112 total, 57 thin omitted)

### Community 0 - "XR Simulator Device Modes"
Cohesion: 0.07
Nodes (11): ActiveDeviceMode, BothControllers, BothHands, HMD, LeftController, LeftHand, None, RightController (+3 more)

### Community 1 - "Game Settings & VR Menu"
Cohesion: 0.06
Nodes (9): GameSettings, MasterVolume, MasterVolumePercent, MenuScreen, GameSelect, Home, Settings, VRMainMenu (+1 more)

### Community 2 - "Simulator UI Theme & Teleport Input"
Cohesion: 0.07
Nodes (15): XRDeviceSimulatorUI, backgroundColor, buttonColor, deviceColor, disabledButtonColor, disabledColor, disabledDeviceColor, enabledColor (+7 more)

### Community 3 - "Sample Project Validation"
Cohesion: 0.06
Nodes (10): StarterAssetsSampleProjectValidation, MaterialPipelineHandler, RenderPipelineValidation, ShaderContainer, PermissionRequest, PermissionRequestGroup, PermissionsManager, XRInteractionSimulatorProjectValidation (+2 more)

### Community 4 - "Teleport Reticles"
Cohesion: 0.08
Nodes (12): MultiAnchorTeleportReticle, destinationIndicator, potentialDestinationIndicator, potentialIndicatorUpdateFrequency, timerProgressFilledImage, ClimbTeleportDestinationIndicator, climbTeleportInteractor, pointerDistance (+4 more)

### Community 5 - "Controller Input Actions"
Cohesion: 0.12
Nodes (4): ControllerInputActionManager, smoothMotionEnabled, smoothTurnEnabled, uiScrollingEnabled

### Community 6 - "Locomotion Move Providers"
Cohesion: 0.07
Nodes (12): DynamicMoveProvider, headTransform, leftControllerTransform, leftHandMovementDirection, rightControllerTransform, rightHandMovementDirection, MovementDirection, HandRelative (+4 more)

### Community 7 - "Package Lock Root"
Cohesion: 0.07
Nodes (30): dependencies, depth, source, version, dependencies, depth, source, version (+22 more)

### Community 8 - "Package: searcher"
Cohesion: 0.07
Nodes (29): depth, source, version, dependencies, depth, source, version, dependencies (+21 more)

### Community 9 - "Package: modules.video"
Cohesion: 0.08
Nodes (28): dependencies, depth, source, version, dependencies, depth, source, version (+20 more)

### Community 10 - "Poke Follow Affordance"
Cohesion: 0.09
Nodes (8): XRPokeFollowAffordance, applyIfChildIsTarget, clampToMaxDistance, initialPosition, maxDistance, pokeFollowTransform, returnToInitialPosition, smoothingSpeed

### Community 11 - "Sample Editor Inspectors"
Cohesion: 0.10
Nodes (8): MaterialPipelineHandlerEditor, ShaderContainerDrawer, ReadmeEditor, BodyStyle, ButtonStyle, HeadingStyle, LinkStyle, TitleStyle

### Community 12 - "Package: modules.vr"
Cohesion: 0.09
Nodes (26): dependencies, depth, source, version, depth, source, version, dependencies (+18 more)

### Community 15 - "Teams & Player Awareness"
Cohesion: 0.12
Nodes (7): Team, Away, Home, PlayerAwareness, Ball, BubbleBounds, NearbyPlayers

### Community 16 - "Package: modules.subsystems"
Cohesion: 0.09
Nodes (23): dependencies, depth, source, version, dependencies, depth, source, version (+15 more)

### Community 17 - "Player Roles & Motor"
Cohesion: 0.17
Nodes (9): Role, Goaly, LeftFielder, RightFielder, Player, item, PossessedPlayer, Role (+1 more)

### Community 18 - "Package: modules.cloth"
Cohesion: 0.10
Nodes (22): dependencies, depth, source, version, dependencies, depth, source, version (+14 more)

### Community 19 - "Simulated Hand Expressions"
Cohesion: 0.17
Nodes (3): HandExpressionUI, sprite, XRDeviceSimulatorHandsUI

### Community 21 - "Object Spawner"
Cohesion: 0.15
Nodes (11): ObjectSpawner, applyRandomAngleAtSpawn, cameraToFace, isSpawnOptionRandomized, objectPrefabs, onlySpawnInView, spawnAngleRange, spawnAsChildren (+3 more)

### Community 23 - "State Machine Core"
Cohesion: 0.21
Nodes (3): PlayerStateMachine, Home, Opponent

### Community 26 - "Package: modules.screencapture"
Cohesion: 0.12
Nodes (16): dependencies, depth, source, version, dependencies, depth, source, version (+8 more)

### Community 28 - "World Registry Items"
Cohesion: 0.21
Nodes (7): IRegistryProvider, item, RegistryItem, gameObject, id, Goal, item

### Community 30 - "Package Manifest"
Cohesion: 0.15
Nodes (12): com.unity.inputsystem@1.20.0, com.unity.modules.assetbundle@1.0.0, com.unity.modules.physics@1.0.0, com.unity.modules.vectorgraphics@1.0.0, com.unity.ugui@2.0.0, com.unity.visualscripting@1.9.12, com.unity.inputsystem, com.unity.modules.assetbundle (+4 more)

### Community 31 - "Manifest Dependencies"
Cohesion: 0.15
Nodes (13): dependencies, com.unity.ai.navigation@2.0.14, com.unity.collab-proxy@2.13.6, com.unity.modules.terrain@1.0.0, com.unity.modules.umbra@1.0.0, com.unity.modules.video@1.0.0, com.unity.modules.vr@1.0.0, com.unity.modules.terrain (+5 more)

### Community 32 - "Package: test-framework"
Cohesion: 0.17
Nodes (13): dependencies, depth, dependencies, depth, source, url, version, source (+5 more)

### Community 33 - "Package: modules.ui"
Cohesion: 0.15
Nodes (13): dependencies, depth, source, version, dependencies, depth, source, version (+5 more)

### Community 35 - "Package: inputsystem"
Cohesion: 0.17
Nodes (12): depth, source, url, version, dependencies, depth, source, url (+4 more)

### Community 36 - "Package: xr.openxr"
Cohesion: 0.17
Nodes (12): dependencies, depth, source, version, dependencies, depth, source, url (+4 more)

### Community 37 - "Package: modules.assetbundle"
Cohesion: 0.17
Nodes (12): dependencies, depth, source, version, dependencies, depth, source, version (+4 more)

### Community 38 - "Package: ugui"
Cohesion: 0.17
Nodes (12): dependencies, depth, source, version, dependencies, depth, source, url (+4 more)

### Community 39 - "Package: xr.mock-hmd"
Cohesion: 0.17
Nodes (12): depth, source, url, version, dependencies, depth, source, url (+4 more)

### Community 40 - "XR Platform Detection"
Cohesion: 0.24
Nodes (7): XRPlatformType, OpenXRAndroidXR, OpenXRMeta, OpenXROther, Other, XRPlatformUnderstanding, CurrentPlatform

### Community 41 - "Package: multiplayer.center"
Cohesion: 0.18
Nodes (11): dependencies, depth, source, version, dependencies, depth, source, version (+3 more)

### Community 42 - "Package: modules.imgui"
Cohesion: 0.18
Nodes (11): dependencies, depth, source, version, dependencies, depth, source, version (+3 more)

### Community 43 - "Package: modules.tilemap"
Cohesion: 0.18
Nodes (11): dependencies, depth, source, version, dependencies, depth, source, version (+3 more)

### Community 44 - "Toggle Component Zone"
Cohesion: 0.24
Nodes (5): ToggleComponentZone, activationObject, componentsToToggle, enableOnEntry, gameObjectsToToggle

### Community 45 - "AI State Enum"
Cohesion: 0.20
Nodes (10): State, Attack, ChaseBall, Defend, Disabled, Disperse, GuardGoal, HoldPosition (+2 more)

### Community 46 - "Package: modules.umbra"
Cohesion: 0.18
Nodes (10): dependencies, depth, source, version, dependencies, depth, source, version (+2 more)

### Community 47 - "Mesh & Lifetime Utilities"
Cohesion: 0.22
Nodes (3): DestroySelf, lifetime, MeshToggler

### Community 50 - "Solution & Assemblies"
Cohesion: 0.25
Nodes (7): Assembly-CSharp, Assembly-CSharp-Editor, Unity.XR.Interaction.Toolkit.Samples.DeviceSimulator, Unity.XR.Interaction.Toolkit.Samples.InteractionSimulator, Unity.XR.Interaction.Toolkit.Samples.InteractionSimulator.Editor, Unity.XR.Interaction.Toolkit.Samples.StarterAssets, Unity.XR.Interaction.Toolkit.Samples.StarterAssets.Editor

### Community 51 - "Toggle Color UI"
Cohesion: 0.29
Nodes (3): ToggleColorToggler, offColor, onColor

### Community 53 - "Ball Prop"
Cohesion: 0.32
Nodes (4): Prop, Ball, Ball, item

### Community 57 - "Field Zones"
Cohesion: 0.29
Nodes (6): Zone, Center, Goal, LeftField, Pen, RightField

### Community 58 - "Navigation Agent Setup"
Cohesion: 0.33
Nodes (3): PlayerNavigation, AgentReady, Direction

### Community 59 - "Package: burst"
Cohesion: 0.29
Nodes (7): dependencies, depth, source, url, version, com.unity.burst, com.unity.burst

### Community 60 - "Package: ext.nunit"
Cohesion: 0.29
Nodes (7): dependencies, depth, source, version, dependencies, com.unity.ext.nunit, com.unity.ext.nunit

### Community 61 - "Package: mathematics"
Cohesion: 0.29
Nodes (7): dependencies, depth, source, url, version, com.unity.mathematics@1.2.1, com.unity.mathematics

### Community 62 - "Package: modules.terrain"
Cohesion: 0.29
Nodes (7): dependencies, depth, source, version, dependencies, com.unity.modules.terrain@1.0.0, com.unity.modules.terrain

### Community 63 - "Package: nuget.mono-cecil"
Cohesion: 0.29
Nodes (7): dependencies, depth, source, url, version, com.unity.nuget.mono-cecil@1.11.6, com.unity.nuget.mono-cecil

### Community 66 - "Package: ai.navigation"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, url, version, com.unity.ai.navigation

### Community 67 - "Package: collab-proxy"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, url, version, com.unity.collab-proxy

### Community 68 - "Package: collections"
Cohesion: 0.33
Nodes (6): depth, source, url, version, com.unity.collections, com.unity.collections

### Community 69 - "Package: ide.rider"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, url, version, com.unity.ide.rider

### Community 70 - "Package: ide.visualstudio"
Cohesion: 0.33
Nodes (6): dependencies, depth, source, url, version, com.unity.ide.visualstudio

### Community 74 - "Package: modules.wind"
Cohesion: 0.40
Nodes (5): dependencies, depth, source, version, com.unity.modules.wind

## Knowledge Gaps
- **117 isolated node(s):** `text`, `timerProgressFilledImage`, `destinationIndicator`, `potentialDestinationIndicator`, `potentialIndicatorUpdateFrequency` (+112 more)
  These have ≤1 connection - possible missing edges. (Counts symbols only; 477 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **57 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `XRDeviceSimulatorUI` connect `Simulator UI Theme & Teleport Input` to `XR Simulator Device Modes`, `Teleport Reticles`, `Simulator Device Toggles`, `Mesh & Lifetime Utilities`, `Simulated Hand Expressions`, `Simulated Controller UI`, `Device Simulator Namespace`, `Simulator UI Init`?**
  _High betweenness centrality (0.083) - this node is a cross-community bridge._
- **What connects `text`, `timerProgressFilledImage`, `destinationIndicator` to the rest of the system?**
  _117 weakly-connected nodes found - possible missing edges._
- **Should `XR Simulator Device Modes` be split into smaller, more focused modules?**
  _Cohesion score 0.06558441558441558 - nodes in this community are weakly interconnected._
- **Why does `PlayerStateMachine` connect `State Machine Core` to `Zone Bounds`, `AI State Enum`, `Teams & Player Awareness`, `Mesh & Lifetime Utilities`, `Player Roles & Motor`, `XR Rig Controller`, `AI Steering Helpers`, `Navigation Agent Setup`, `Score Keeper & UI`?**
  _High betweenness centrality (0.066) - this node is a cross-community bridge._
- **Should `Game Settings & VR Menu` be split into smaller, more focused modules?**
  _Cohesion score 0.05952380952380952 - nodes in this community are weakly interconnected._
- **Why does `dependencies` connect `Package: modules.cloth` to `Package Lock Root`, `Package: searcher`, `Package: modules.video`, `Package: modules.vr`, `Package: modules.subsystems`, `Package: modules.screencapture`, `Package: test-framework`, `Package: modules.ui`, `Package: inputsystem`, `Package: xr.openxr`, `Package: modules.assetbundle`, `Package: ugui`, `Package: xr.mock-hmd`, `Package: multiplayer.center`, `Package: modules.imgui`, `Package: modules.tilemap`, `Package: modules.umbra`, `Package: burst`, `Package: ext.nunit`, `Package: mathematics`, `Package: modules.terrain`, `Package: nuget.mono-cecil`, `Package: ai.navigation`, `Package: collab-proxy`, `Package: collections`, `Package: ide.rider`, `Package: ide.visualstudio`, `Package: modules.wind`?**
  _High betweenness centrality (0.057) - this node is a cross-community bridge._
- **Should `Simulator UI Theme & Teleport Input` be split into smaller, more focused modules?**
  _Cohesion score 0.06648936170212766 - nodes in this community are weakly interconnected._