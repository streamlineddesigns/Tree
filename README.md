Copyright 2026 @Streamlined Designs

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

# Color Flow

**A Unity/C# architecture case study for game developers**

Color Flow is a connect-the-colors puzzle played as physics traversal. You slingshot a ball between colored nodes, connecting matching parent pairs from inside the graph. The shortest description is "connect the pairs" wearing a physics skin: the player is both the pen drawing the solution and the traveler moving through it.

Formerly **Color Flow Arcade Puzzles**, the project lives in the `Tree` repository, uses the `StudioByStorm` namespace, and contains roughly 476 C# files.

---

## The design pillar

The game's central metaphor is structural, not decorative:

- The player ball is an **electrical impulse**.
- The nodes are **brain cells**.
- The dynamic tentacle-edge is an **axon**.
- A completed level is a thought carried all the way through its network.

Winning means lighting up the whole network, every pathway firing at once. The implementation makes that literal: on level completion, `EdgeLight` pulses travel down every finished edge into the player, nodes glow, and fireworks fire.

The movement system, graph model, feedback, and win state all express the same idea. The metaphor does not sit on top of the mechanics; it explains them.

---

## Architecture at a glance

The runtime reads as five layers, bottom-up.

### 1. Foundation

- `GameManager`: singleton service locator.
- `GameEventPublisher`: static C# event bus.
- Nine typed, self-registering registries: indexes of live runtime objects.
- `ProgressManager`: player progress persistence.

### 2. World

- `LevelManager`: JSON --> pooled nodes --> adjacency --> win or lose.
- `Node` and `Edge`: the playable graph.
- `AdjacencyList`, `DFS`, and `DFSPaths`: graph algorithms.
- `GraphConstructionManager`: in-Unity touch level editor.

### 3. Play

- `PlayerController`: drives the impulse through the graph.
- `MobileInput`: hand-rolled gesture state machine.
- `Projectile`: participates in color-typed collision rules.

Movement and graph construction are the same interaction viewed from two sides.

### 4. Presentation

- `FXManager`: roughly 15 `ScriptableObject`-backed pools.
- Obstacles: color-typed and animated.
- `CameraController`: frames levels from the node axis-aligned bounding box.

The camera treats the graph as the world boundary; the effects expose state changes.

### 5. Interface

- `UIController`: navigation through `View` and `Controller` registries.
- `TutorialController`: mechanic-specific tutorial scripts.
- `Translator`: a 15-language enum.
- `FTUEManager`: first-time-user flow.
- `AgeVerification`: gate before analytics initialization.

---

## Patterns worth stealing

These ideas travel well. None requires adopting the whole architecture.

### Solver-backed procedural generation

The procedural generator does not assume a random level is playable. It proves it:

1. Generate a random graph.
2. DFS-enumerate all simple paths for each color pair.
3. Form the cartesian product of those path choices.
4. Use `Parallel.For` to reject combinations whose paths overlap at nodes.
5. Keep only full-coverage solution sets.
6. Bake a guaranteed `safePath` into the level file.

The pattern is **generate --> solve --> keep**, with the solver as the test. A combinatorial guard aborts runaway searches.

**Steal this:** when generated content has a hard validity condition, store a proof beside the content instead of trusting generation-time luck.

### Difficulty as measurement, not opinion

An editor rig plays each animated obstacle for 60 seconds. A radial circle-cast sensor array distills color-run statistics into a 0–100 difficulty score, which directly ramps procedural obstacle selection.

Instead of hand-labeling an animation "easy" or "hard," `ObstacleDifficultyManager` measures what it actually does over time. Content tuning becomes metrology.

### Self-registering pooled lifecycle

Pooled objects register with typed registries on enable. On disable, they deregister and return to a factory-reset state.

The pool lifecycle **is** the index lifecycle. There is no second synchronization pass trying to remember which pooled objects are live.

### Event bus plus typed registries

`GameEventPublisher` broadcasts state changes, player actions, and reactive events. Typed registries answer discovery questions: which nodes, edges, obstacle parts, views, or controllers are active? Controllers are located by the `ViewName` enum rather than a web of scene references.

- Use events when many systems may care that something happened.
- Use registries when a system needs a specific live object or collection.

### Two-tier FX dispatch

Latency-critical hit feedback calls its pool directly. Ambient and broadly reactive effects listen through the event bus. Immediate feedback stays immediate; looser effects remain decoupled.

### Self-scaling obstacles

Obstacle animations read chapter and level progression, then speed themselves up. One prefab serves multiple difficulty bands with zero duplicated assets or parallel tuning data.

### Localization built into the content model

Every user-facing string rides a translations array, and the language enum covers 15 languages. Chapters also ship with per-language narration clips. Localization is part of the content shape, not a late string-table pass.

### Tutorials as event-driven scripts

Tutorials are small scripts gated by level and chapter. Completion follows the real mechanic event:

- `JumpTutorial` finishes on `OnPlayerJump`.
- `PhaseTutorial` finishes on a correct-obstacle hit.
- Other tutorials use the same event-driven shape.

Progress is saved only when the player beats the level. Seeing a prompt is not treated as learning; successful play is.

### Small, hand-rolled spatial tools

The per-frame nearest-node query uses a hand-rolled spatial hash followed by brute-force KNN over nearby candidates. No external ML or geometry library is required.

For a bounded game problem, a small transparent implementation can be easier to tune than a generalized dependency.

### Story pacing encoded as level rhythm

Every third level ID diverts to a cutscene. The ten-chapter arc from **Darkness** to **Clarity** is paced by the level grid itself, not separate content metadata.

Narrative cadence can be a property of progression structure, not only a script layered over gameplay.

---

## Honest gotchas

These are the places where the codebase shows its history, or where a future pass should choose differently.

### `Edge` means two different things

`Edge` is the dynamic FABRIK tentacle tether from a node toward the player. Static graph lines are pooled `EdgeRenderer` objects. Clearer names such as `AxonTether` and `GraphEdgeRenderer` would reduce cognitive load.

### `ControllerRegistry` gives up type safety

Most registries are typed. `ControllerRegistry` stores untyped `object`, moving errors into casts at call sites. A generic registry or typed controller interface would preserve compiler help.

### Two serialization eras coexist

Levels are human-readable JSON. Progress uses legacy `BinaryFormatter`. Moving progress to an explicit, versioned format would improve safety, debuggability, and forward compatibility.

### The archaeology is still in the tree

- A dead 3D-platformer `Gravity/` folder.
- Vendored gesture recognizers that the active game does not reference.
- An empty `SpatialPartitionRegistry.cs`.

These files preserve development history but blur the supported architecture. Archive them outside the runtime tree or label them explicitly.

### The spatial hash rebuilds every frame

`SpatialHashManager` reconstructs the full hash every frame. It is straightforward and correct, but potentially wasteful when most objects have not moved.

If profiling shows pressure, update dirty entries incrementally or rebuild only when topology changes. Keep the simple implementation until measurement justifies the complexity.

---

## Repository layout

The main code lives under `Assets/Scripts/` in the `StudioByStorm` namespace.

- `UI/` — views, controllers, navigation, and player actions.
- `TouchKit/` — touch input support.
- `Data/` — serializable level, progress, obstacle, tutorial, text, and vector data.
- `Tutorials/` — mechanic-specific event-driven tutorial scripts.
- `Obstacles/` — obstacle behavior and difficulty measurement.
- `FX/` — pooled edge lights, trails, fireworks, and hit feedback.
- `Registries/` — typed indexes for active runtime objects.
- `Gravity/` — older, inactive 3D-platformer code.
- `Helpers/` — edge placement, circle casts, player positioning, capture, and utilities.
- `GestureRecognition/` — gesture input and vendored recognizer work.
- `PCG/` — graph generation, cartesian path selection, obstacle placement, and shuffling.
- `Optimizations/` — pooling, quadtree, and spatial-hash implementations.
- `Graph/` — adjacency, reachability, all-path enumeration, and the touch level editor.
- `ML/` — hand-rolled math and KNN utilities.

Content is separate:

- `Assets/Level Files/` — JSON levels by shape pack: Square, Line, ZigZig, Shape, Polygon, Loop, Group, and Rectangle.
- `Assets/Narrations/` — per-chapter narration clips for Chapters 1 through 10.

---

## The entire game in four verbs

At the interaction boundary, Color Flow collapses to the `NodeAction` enum:

```csharp
public enum NodeAction
{
    TravelNode,
    GetEdge,
    SetEdge,
    TravelEdge
}
```

Those four verbs are the whole loop:

- `TravelNode` — move through the field of brain cells.
- `GetEdge` — take hold of an axon-like connection.
- `SetEdge` — commit that connection into the network.
- `TravelEdge` — move along a connection already in play.

The architecture is large, but the player's language stays small. That is a useful test for any game system: as the implementation grows, can the verbs remain legible?

---

## Closing note

Color Flow's strongest lesson is not a particular Unity pattern. It is coherence.

The player is an electrical impulse. Movement is traversal. Connections are axons. Levels are regions of a brain. Completion is a network lighting up. The PCG system proves the network can be traversed, the camera frames it as the world, and the effects make the completed thought visible.

A game becomes easier to reason about when its fiction, verbs, data, algorithms, and feedback all describe the same thing.

---

## License

Copyright 2026 @Streamlined Designs

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
Copyright 2026 Streamlined Designs.
