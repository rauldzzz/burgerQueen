# Burger Queen

Burger Queen is a cooperative restaurant-management game built around a repeating kitchen loop. Players enter a start screen, begin a timed cooking round, prepare burger orders through different kitchen stations, spend earnings on upgrades, and repeat the process until the session ends.

This document focuses on the game systems, scene flow, content structure, and the scripts that implement each part of the loop. The tracking implementation is intentionally omitted.

## High-Level Design

The game is built as a round-based loop with persistent progression inside a single session.

The flow is:

1. Start in the intro scene.
2. Begin a gameplay round.
3. Fulfil orders under time pressure.
4. Move to the upgrade shop when the timer ends.
5. Apply upgrades and return to gameplay.
6. Repeat until the final round is complete.
7. Show session statistics in the final scene.

The important point is that the game is not just a kitchen sandbox. It is a stateful loop where money, upgrades, and unlocked recipes persist across rounds until the run ends.

## Scene Map And Implementation Files

The main scenes live in [Assets/Scenes](Assets/Scenes).

The scripts that drive the scene flow are:

- [Assets/Scripts/New Scripts/Rut_Scripts/StartScreenManager.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/StartScreenManager.cs)
- [Assets/Scripts/New Scripts/Rut_Scripts/GameManager.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/GameManager.cs)
- [Assets/Scripts/New Scripts/Timer.cs](Assets/Scripts/New%20Scripts/Timer.cs)
- [Assets/Scripts/New Scripts/Rut_Scripts/UpgradeShopManager.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/UpgradeShopManager.cs)
- [Assets/Scripts/New Scripts/Rut_Scripts/FinalSceneManager.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/FinalSceneManager.cs)

The following scene files are the entry points for those systems:

- [Inici scene](Assets/Scenes/Inici.unity)
- [Gameplay scene](Assets/Scenes/CanvisProbes.unity)
- [Upgrade shop scene](Assets/Scenes/UpgradeShop.unity)
- [Final scene](Assets/Scenes/FinalScene.unity)

## Game Flow

The session flow is controlled mainly by [GameManager.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/GameManager.cs).

That script keeps the current round counter, total rounds, current money, total earned money, and the persistent upgrade flags. It also owns the scene names used for the transition chain.

The actual flow is:

1. [StartScreenManager.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/StartScreenManager.cs) resets the run when the intro scene starts.
2. When the start conditions are met, it loads the gameplay scene.
3. [GameManager.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/GameManager.cs) increments the round with `StartRound()`.
4. [Timer.cs](Assets/Scripts/New%20Scripts/Timer.cs) counts down the active round.
5. When time reaches zero, the game loads the upgrade scene.
6. [UpgradeShopManager.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/UpgradeShopManager.cs) finalizes selected upgrades and returns to gameplay.
7. After the final round, [GameManager.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/GameManager.cs) calls `EndSession()` and loads the final scene.

## Start Scene

The start flow is implemented in [StartScreenManager.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/StartScreenManager.cs) and the [Inici scene](Assets/Scenes/Inici.unity).

This scene is not a passive menu. It has a small state machine:

- It caches the initial instruction position.
- It caches the raw image positions for the intro videos.
- It creates unique runtime render textures for the video players so each one gets its own output target.
- It waits for both starting player spots to be occupied.
- It plays the intro motion sequence by moving the instruction block forward in Z.
- It supports a keyboard skip path through the `S` key.

Relevant files:

- [StartScreenManager.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/StartScreenManager.cs)
- [PlayerSpot.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/PlayerSpot.cs)
- [Inici.unity](Assets/Scenes/Inici.unity)

Technical note: `StartScreenManager` also calls `GameManager.ResetGame()` at startup so the session begins from a clean state.

## Round Timer

The round timer is implemented in [Timer.cs](Assets/Scripts/New%20Scripts/Timer.cs).

It uses a simple countdown loop:

- `timeRemaining` starts at 120 seconds by default.
- `Update()` subtracts `Time.deltaTime` while the timer is active.
- When the timer hits zero, the script stops the round and triggers the scene transition.

The timer also provides warning-state UI through `SetWarningState(bool)`, which changes the timer color to red when the player enters a hazard state.

Relevant files:

- [Timer.cs](Assets/Scripts/New%20Scripts/Timer.cs)
- [TimePenaltyZone.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/TimePenaltyZone.cs)

## Orders And Rewards

Orders are managed in [OrdersManager.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/OrdersManager.cs).

This system owns the active order, the pool of possible recipes, and the UI presentation for the request panel. Each order contains:

- a burger name
- a reward value
- a final burger sprite
- the ingredient icons used by the UI

When a new order is generated, the script:

1. Picks a recipe from `possibleOrders`.
2. Updates the order text.
3. Updates the main burger image.
4. Rebuilds the ingredient icon list.
5. Updates the reward label.

When the correct dish is delivered, `CompleteOrder()` awards money through [ScoreManager.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/ScoreManager.cs) and writes statistics into [SessionStatistics.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/SessionStatistics.cs).

Relevant files:

- [OrdersManager.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/OrdersManager.cs)
- [BurgerRecipe.cs](Assets/Scripts/New%20Scripts/BurgerRecipe.cs)
- [ScoreManager.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/ScoreManager.cs)
- [SessionStatistics.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/SessionStatistics.cs)

## Burger Data

The content of each burger is defined with ScriptableObject assets based on two layers:

### Visible Order Data

[BurgerRecipe.cs](Assets/Scripts/New%20Scripts/BurgerRecipe.cs) holds the data used by the order UI and the reward system.

It stores:

- `burgerName`
- `burger`
- `reward`
- `finalBurgerImage`
- `ingredientImages`

### Assembly Logic

[BurgerAssemblyRecipe.cs](Assets/Scripts/New%20Scripts/BurgerAssemblyRecipe.cs) defines the burger build sequence.

It stores:

- `startingStatePrefab`
- a list of build steps
- each step’s current state
- the required ingredient
- the next burger state

This is what lets the hostess station behave like a state machine instead of a simple combine trigger.

Relevant files:

- [BurgerRecipe.cs](Assets/Scripts/New%20Scripts/BurgerRecipe.cs)
- [BurgerAssemblyRecipe.cs](Assets/Scripts/New%20Scripts/BurgerAssemblyRecipe.cs)
- [Counter_Hostess.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/Counter_Hostess.cs)

## Kitchen Interaction Model

The whole kitchen interaction layer is built on top of [Counter.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/Counter.cs) and [PlayerInteraction.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/PlayerInteraction.cs).

### Counter Base

[Counter.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/Counter.cs) is the base interaction controller for every station.

It handles:

- player detection through collider overlap
- hold-to-interact timing
- interaction gating so a player cannot immediately reclaim the same item they placed
- optional UI feedback via `CounterTimerUI`
- placing and taking items from counters

That base class is extended by the station-specific scripts.

### Player Item Handling

[PlayerInteraction.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/PlayerInteraction.cs) handles the player inventory state.

It owns:

- `heldItem`
- `heldItemName`
- `sourceCounter`
- the grab/release animation triggers

The important part is that item pickup is delayed until the grab animation completes. That makes the interaction feel like a physical action instead of an instant inventory swap.

Relevant files:

- [Counter.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/Counter.cs)
- [PlayerInteraction.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/PlayerInteraction.cs)
- [CounterTimerUI.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/CounterTimerUI.cs)
- [PlayerController.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/PlayerController.cs)

## Ingredient And Processing Stations

### Ingredient Spawn

[Counter_Ingredient.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/Counter_Ingredient.cs) spawns a configured ingredient prefab when the player interacts without already holding something.

This is the simplest entry point into the kitchen loop.

### Generic Processor

[Counter_Processor.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/Counter_Processor.cs) is the shared implementation for processing stations.

It works with a list of `ProcessRecipe` entries that define:

- input prefab
- output prefab
- output scale

The station checks the player’s held item, matches it against a recipe, destroys the input, spawns the output, and returns the processed item to the player.

### Cutting Station

[Counter_Cutting.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/Counter_Cutting.cs) inherits from the generic processor and is used for cut transformations.

### Grill Station

[Counter_Gril.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/Counter_Gril.cs) inherits from the generic processor and is used for cooked transformations.

Relevant files:

- [Counter_Ingredient.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/Counter_Ingredient.cs)
- [Counter_Processor.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/Counter_Processor.cs)
- [Counter_Cutting.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/Counter_Cutting.cs)
- [Counter_Gril.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/Counter_Gril.cs)
- [SoundController.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/SoundController.cs)

## Burger Assembly Station

The assembly logic is in [Counter_Hostess.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/Counter_Hostess.cs).

This script is the most complex station in the game. It supports:

- spawning the starting burger state
- advancing to the next burger state when the correct ingredient is added
- taking the final burger state back into the player’s hands
- stepping backward in supported recipes when the current state allows it
- syncing to the current order from [OrdersManager.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/OrdersManager.cs)

The station chooses the active recipe in this order:

1. Current order from the order manager.
2. Default recipe assigned in the inspector.
3. Fallback recipes in the local list.

That priority makes the station adaptable: it can follow the active order, but it still works if the scene is using a fallback setup.

Relevant files:

- [Counter_Hostess.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/Counter_Hostess.cs)
- [BurgerAssemblyRecipe.cs](Assets/Scripts/New%20Scripts/BurgerAssemblyRecipe.cs)
- [OrdersManager.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/OrdersManager.cs)

## Plating And Disposal

### Plate Assembly

[Counter_Plate.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/Counter_Plate.cs) handles plate-based recipe assembly.

It keeps two pieces of state:

- whether a plate is already present
- the list of ingredients currently on the plate

When the ingredient set matches one of the configured `PlateRecipe` entries, the script replaces the plate contents with the completed dish prefab.

### Trash Counter

[Counter_Trash.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/Counter_Trash.cs) is the cleanup station.

It destroys the item the player is holding and clears the hand state. This is a useful fail-safe so the player can recover from a wrong pickup or a dead-end item chain.

Relevant files:

- [Counter_Plate.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/Counter_Plate.cs)
- [Counter_Trash.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/Counter_Trash.cs)
- [ItemNameUtility.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/ItemNameUtility.cs)

## Delivery

[Counter_Delivery.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/Counter_Delivery.cs) is the handoff point between cooking and scoring.

The flow is:

1. The player brings a completed dish to the counter.
2. The counter normalizes the item name.
3. `OrdersManager.CompleteOrder()` checks whether it matches the active order.
4. On success, the game rewards money and logs statistics.
5. On failure, the item is discarded.

This is the point where the restaurant work becomes session progress.

Relevant files:

- [Counter_Delivery.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/Counter_Delivery.cs)
- [OrdersManager.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/OrdersManager.cs)
- [ScoreManager.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/ScoreManager.cs)

## Money, Score, And Session Stats

The money flow is split into two systems:

### Real-Time Money

[ScoreManager.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/ScoreManager.cs) keeps the current money visible during gameplay.

It synchronizes with [GameManager.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/GameManager.cs) when the singleton exists, and it falls back to cached values when it does not.

### Session Summary

[SessionStatistics.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/SessionStatistics.cs) stores the run summary.

It tracks:

- total money earned
- total money spent
- burgers prepared per type

The final scene uses this data to present the end-of-run summary.

Relevant files:

- [ScoreManager.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/ScoreManager.cs)
- [SessionStatistics.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/SessionStatistics.cs)
- [FinalSceneManager.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/FinalSceneManager.cs)
- [GameManager.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/GameManager.cs)

## Upgrade Shop

The upgrade phase is implemented in [UpgradeShopManager.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/UpgradeShopManager.cs) and [UpgradeButton.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/UpgradeButton.cs).

The shop works through a hold-to-select system:

1. The player enters the trigger area of an upgrade button.
2. The button accumulates hold time.
3. If the player can afford the price, the shop reserves that money.
4. Once the hold is complete, the upgrade becomes selected.
5. The continue action finalizes the chosen upgrades.
6. The game returns to gameplay.

This keeps the upgrade phase diegetic and physical instead of using a generic menu click.

The shop can work in two paths:

- with [GameManager.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/GameManager.cs) present, where the current run state is applied directly
- with the cached fallback path handled by [UpgradeCache.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/UpgradeCache.cs)

Relevant files:

- [UpgradeShopManager.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/UpgradeShopManager.cs)
- [UpgradeButton.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/UpgradeButton.cs)
- [UpgradeCache.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/UpgradeCache.cs)
- [GameManager.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/GameManager.cs)
- [UpgradeShop.unity](Assets/Scenes/UpgradeShop.unity)

## Upgrade Effects

The available upgrades are tracked inside [GameManager.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/GameManager.cs).

The script stores four persistent gameplay flags:

- `unlockCheeseBurger`
- `betterPan`
- `extraCuttingZone`
- `extraServingZone`

When those flags change, the manager can:

- save the state into PlayerPrefs
- rebind scene references after loading a new scene
- activate or deactivate kitchen objects
- inject unlocked recipes into the active `OrdersManager`

Relevant files:

- [GameManager.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/GameManager.cs)
- [UpgradeCache.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/UpgradeCache.cs)
- [OrdersManager.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/OrdersManager.cs)

## Penalty And Warning Flow

[TimePenaltyZone.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/TimePenaltyZone.cs) is the hazard system used to pressure the player.

It tracks objects inside a trigger, measures their movement distance, and removes time from the round based on the configured loss rate.

When the first target enters the zone, the script can:

- start the warning audio loop
- switch the timer to warning mode
- blink the alert object

When the last target leaves, the warning state is cleared.

Relevant files:

- [TimePenaltyZone.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/TimePenaltyZone.cs)
- [Timer.cs](Assets/Scripts/New%20Scripts/Timer.cs)
- [SoundController.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/SoundController.cs)

## Audio And Feedback

Audio is centralized in [SoundController.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/SoundController.cs).

The script exposes dedicated clips for:

- pickup
- drop
- cut
- warning
- grill
- deliver

It also manages a dedicated warning-loop AudioSource so the warning sound can run continuously while the hazard state is active.

The main feedback channels are:

- [Timer.cs](Assets/Scripts/New%20Scripts/Timer.cs) for timer color changes
- [TimePenaltyZone.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/TimePenaltyZone.cs) for alert blinking
- [UpgradeButton.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/UpgradeButton.cs) for button color states
- [UpgradeShopManager.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/UpgradeShopManager.cs) for continue-zone feedback

## Final Scene

The ending screen is implemented in [FinalSceneManager.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/FinalSceneManager.cs).

It reads the values stored in [SessionStatistics.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/SessionStatistics.cs) and writes them into the UI:

- burger counts by type
- money earned
- money spent
- final remaining money

This is the last layer of the loop, because it turns the full session into a readable outcome instead of only showing the final cash value.

Relevant files:

- [FinalSceneManager.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/FinalSceneManager.cs)
- [SessionStatistics.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/SessionStatistics.cs)
- [RestartButton.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/RestartButton.cs)
- [FinalScene.unity](Assets/Scenes/FinalScene.unity)

## Data Persistence And Reset

The session state is intentionally split between runtime state and saved state.

[GameManager.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/GameManager.cs) owns the runtime values and can also store selected values in PlayerPrefs when persistence is enabled.

[UpgradeCache.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/UpgradeCache.cs) is used when the upgrade flow needs to survive scene transitions even if the GameManager is not immediately present.

[SessionStatistics.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/SessionStatistics.cs) is a separate persistent object that survives scene changes and is reset when a brand-new run starts.

The practical effect is:

- player progress persists inside the session
- upgrades persist across rounds
- the full run resets only when starting over

Relevant files:

- [GameManager.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/GameManager.cs)
- [UpgradeCache.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/UpgradeCache.cs)
- [SessionStatistics.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/SessionStatistics.cs)
- [StartScreenManager.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/StartScreenManager.cs)

## Player Movement And Presentation

[PlayerController.cs](Assets/Scripts/New%20Scripts/Marcel_Scripts/PlayerController.cs) orients the player based on movement direction.

The controller samples the transform delta, smooths it, and rotates the player toward the movement vector. That makes the character visually align with the direction of travel.

The broader presentation layer also includes the kitchen layout, the start-screen motion, the order UI, and the feedback loops described above.

## Burger Families Tracked In The Final Screen

The final statistics screen reports these burger families:

1. SingleBurger
2. CheeseBurger
3. DoubleCheeseBurger
4. CompleteBurger
5. Tomatonator
6. VegetalBurger

Those names are the identifiers the end-of-run UI uses when reading counters from [SessionStatistics.cs](Assets/Scripts/New%20Scripts/Rut_Scripts/SessionStatistics.cs).

## Commit History In Gameplay Terms

The commit history shows a progressive build-up of the game systems:

1. The early commits established the basic flow between scenes.
2. Burger recipes and order generation were added.
3. Player item pickup and drop interactions were implemented.
4. Cutting and grilling stations were wired into the kitchen.
5. Plate logic, trash handling, and delivery were added.
6. The upgrade shop and economy loop were introduced.
7. New recipes and extra kitchen zones expanded the content.
8. The warning/audio layer and final results screen completed the session loop.

In other words, the current game is the result of a staged progression from a simple kitchen prototype into a full round-based session loop.

## Summary

Burger Queen is a timed cooperative burger game built around a repeatable loop:

- start the session
- read the order
- process ingredients
- assemble the burger
- deliver the dish
- earn money
- buy upgrades
- repeat
- finish with a full results screen

Every part of the implementation is backed by a concrete script or scene file, so the project is easy to trace from the design level down to the exact behavior in code.
