# Skibidi Brainrot Fruit

3D casual arcade endless runner built with Unity 2022.3 LTS + URP.

## Setup

1. Open the project in Unity 2022.3.20f1 (or any 2022.3 LTS)
2. Unity will import packages automatically (URP, Input System, TextMeshPro)
3. Switch to URP: Edit > Project Settings > Graphics > set URP pipeline asset
4. Set Active Input Handler to "Both" or "Input System" in Player Settings
5. Create two scenes: `MainMenu` and `Game`, add them to Build Settings

## Project Structure

```
Assets/
├── Scripts/
│   ├── Core/           # GameConfig, GameEvents, GameState
│   ├── Player/         # PlayerController
│   ├── Input/          # PlayerInputHandler (touch + keyboard)
│   ├── Track/          # TrackGenerator, TrackChunk
│   ├── Obstacles/      # Obstacle, ObstacleSpawner, ObstacleType
│   ├── Coins/          # Coin, CoinSpawner, CoinCollector
│   ├── Scoring/        # ScoreManager
│   ├── GameManagement/ # GameManager, GameSceneInit
│   ├── Effects/        # ScreenShake, NearMissDetector, DeathEffect
│   └── UI/             # HUDController, GameOverUI, MainMenuUI
├── Scenes/
├── Prefabs/
├── Materials/
└── Settings/
```

## Scene Setup Guide

### Game Scene
1. Create Player: Sphere + CharacterController + PlayerController + CoinCollector + NearMissDetector + DeathEffect + PlayerInputHandler
2. Create Track: Empty GO + TrackGenerator
3. Create Obstacles: Empty GO + ObstacleSpawner
4. Create Coins: Empty GO + CoinSpawner
5. Create Managers: GameManager + ScoreManager + GameSceneInit
6. Create Camera: Main Camera + ScreenShake (child of player or follow script)
7. Create Canvas: HUD + GameOverUI panels with TextMeshPro elements

### Prefabs to Create
- **Track Chunks**: Plane (30m long, 3 lanes wide) with collider
- **Obstacles**: Tagged "Obstacle"
  - LowWall: Short box (slide under)
  - HighWall: Tall box (jump over)
  - LaneBlock: Full-lane box (dodge sideways)
  - MovingBlock: Box with Obstacle(MovingBlock) component
- **Coin**: Cylinder/Disc + Coin component, tagged "Coin"
- **GameConfig**: Create via Assets > Create > SkibidiBrainrotFruit > Game Config

## Controls
- **Keyboard**: WASD / Arrow Keys + Space (jump)
- **Touch**: Swipe left/right (lane), up (jump), down (slide)

## Tags Required
- `Player` (on player object)
- `Obstacle` (on all obstacle prefabs)

## Target
- 60 FPS on mobile
- WebGL build supported
