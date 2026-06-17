# Wobbi: Reinforcement Learning for Character-Based Bipedal Movement and Footprint Visualisation

## Project Overview

This project explores reinforcement learning for a character-inspired bipedal robot named **Wobbi**. The aim was to train a simplified legged robot in Unity ML-Agents and connect its learned movement with a creative output through footprint traces and movement documentation.

The original idea was to make Wobbi follow a designed route and leave footprints that could form a visible pattern. During development, I realised that even basic bipedal movement was already a complex reinforcement learning problem. The final direction therefore became more focused: training Wobbi to stay upright, move toward simple waypoint targets, and use its imperfect forward shuffling movement as part of the robot's character.

The final result is not a perfect human-like walking gait. Instead, Wobbi produces a limited but usable forward shuffling movement. It can remain upright for longer periods and move slowly toward simple waypoint targets. Its main limitations are weak turning ability and unclear left-right foot alternation. These limitations are documented as part of the project process and creative reflection.

## Final Selected Model

The final selected model is:

```text
Models/final/WobbiGait_SpeedFinal02.onnx
```

This model was selected because it produced the most acceptable inference behaviour in Unity. Although the TensorBoard reward curve was not fully stable, the final model showed usable forward movement and better visual behaviour than several earlier versions.

An earlier useful comparison model is stored in:

```text
Models/archive/WobbiGait_Speed_02.onnx
```

The model selection process is documented in:

```text
Models/archive/README_model_selection.md
```

## Technical Summary

* Engine: Unity 6.0.74f1
* Framework: Unity ML-Agents
* Algorithm: PPO
* Robot type: 8-DOF bipedal robot
* Observation size: 35
* Continuous actions: 8
* Task: waypoint-following locomotion
* Creative output: footprint / movement trace visualisation
* Final model: `WobbiGait_SpeedFinal02.onnx`

## Robot Structure

Wobbi is represented as a simplified 8-DOF bipedal robot. Each leg has four controllable joints:

| Joint index | Joint name        | Function                   |
| ----------: | ----------------- | -------------------------- |
|           0 | Left hip roll     | Side balance               |
|           1 | Left hip pitch    | Forward/backward leg swing |
|           2 | Left knee pitch   | Knee bending               |
|           3 | Left ankle pitch  | Foot compensation          |
|           4 | Right hip roll    | Side balance               |
|           5 | Right hip pitch   | Forward/backward leg swing |
|           6 | Right knee pitch  | Knee bending               |
|           7 | Right ankle pitch | Foot compensation          |

The training body uses Unity Articulation Bodies and simplified colliders. A separate imported Wobbi visual model is included as part of the presentation direction, while the simplified body is used as the physics proxy for reinforcement learning.

## Reward Design

The final reward function combines several terms:

* upright reward
* survival reward
* waypoint progress reward
* velocity-to-waypoint reward
* target speed reward
* standing-still penalty
* low-body penalty
* crawling penalty
* action penalty
* fall penalty
* weak anti-phase hip reward

The reward design was developed through multiple iterations. Earlier versions could stand but did not move enough. Later versions moved more actively but sometimes slid, crawled, or became unstable. The final model uses a speed-oriented reward to encourage slow forward movement while still maintaining body stability.

## Project Structure

```text
FINAL_PROJECT/
├── Config/
│   └── wobbi_gait_path.yaml
│
├── Models/
│   ├── archive/
│   │   ├── README_model_selection.md
│   │   └── WobbiGait_Speed_02.onnx
│   └── final/
│       └── WobbiGait_SpeedFinal02.onnx
│
├── Report/
│   └── Wobbi_RL_Report.pdf
│
├── Results/
│   ├── logs/
│   │   └── logs.txt
│   ├── screenshots/
│   │   ├── behavior_parameters.jpg
│   │   ├── body.jpg
│   │   ├── final_inference_result.jpg
│   │   ├── imported_visual_model.jpg
│   │   ├── pathmanager_waypoints.jpg
│   │   ├── unity_scene_overview.jpg
│   │   └── wobbi_agent_hierarchy.jpg
│   └── tensorboard/
│       ├── 01_final_reward.jpg
│       ├── 02_important_reward3.jpg
│       ├── 03_policy_loss.jpg
│       ├── 04_entropy.jpg
│       ├── 05_value_loss.jpg
│       ├── 06_SpeedFinal02_Environment.jpg
│       ├── 07_successfull_reward.jpg
│       ├── 08_successfull_length.jpg
│       ├── 09_speed02_loss.jpg
│       ├── 10_speed02_reward.jpg
│       ├── 11_train03_stability_reward.jpg
│       └── 12_train03_stability_length.jpg
│
├── Scripts/
│   ├── FootprintManager.cs
│   ├── PathManager.cs
│   └── WobbiGaitAgent.cs
│
├── Unity_Project/
│   ├── Assets/
│   ├── Packages/
│   └── ProjectSettings/
│
├── Videos/
│   ├── final_inference_speedfinal02.mp4
│   └── limitation_turning_failure.mp4
│
└── README.md
```

## How to Open the Unity Project

Open the following folder in Unity Hub:

```text
Unity_Project/
```

The Unity project contains the scene, assets, scripts, ML-Agents setup, and training environment.

## How to Run the Final Model

1. Open the Unity project.
2. Open the Wobbi training scene.
3. Select `WobbiAgent`.
4. In **Behavior Parameters**, set:

```text
Behavior Name: WobbiGait
Vector Observation Space Size: 35
Continuous Actions: 8
Behavior Type: Inference Only
Model: Models/final/WobbiGait_SpeedFinal02.onnx
```

5. Press Play in Unity.

The final model should show Wobbi maintaining balance and moving slowly toward simple waypoint targets. The movement is best described as forward shuffling rather than complete bipedal walking.

## How to Train

The training configuration is stored in:

```text
Config/wobbi_gait_path.yaml
```

To train a new model:

```bat
cd /d path\to\FINAL_PROJECT
conda activate mlagents
mlagents-learn Config\wobbi_gait_path.yaml --run-id=WobbiGait_NewRun
```

To train from a previous checkpoint, use `--initialize-from` with the name of an existing run inside the `results` folder:

```bat
mlagents-learn Config\wobbi_gait_path.yaml --run-id=WobbiGait_NewRun --initialize-from=WobbiGait_Speed_02
```

During training, Unity should be set to:

```text
Behavior Type: Default
Model: None
Vector Observation Space Size: 35
Continuous Actions: 8
```

## TensorBoard Results

The TensorBoard screenshots are stored in:

```text
Results/tensorboard/
```

The main figures include:

* cumulative reward comparison
* episode length
* policy loss
* value loss
* entropy
* final model comparison

The reward curves show that training was not fully stable. Some runs reached higher reward values but did not always produce better visual behaviour. For this reason, the final model was selected using both TensorBoard results and direct inference observation in Unity.

## Screenshots and Demo Videos

Project screenshots are stored in:

```text
Results/screenshots/
```

These include the Unity scene, robot hierarchy, Behavior Parameters, PathManager waypoint setup, imported visual model, and final inference result.

Demo videos are stored in:

```text
Videos/
```

The two main videos are:

```text
final_inference_speedfinal02.mp4
limitation_turning_failure.mp4
```

The first video shows the final selected model. The second video documents a limitation case, especially weak turning behaviour.

## Main Results

The final model achieved:

* improved balance compared with early runs
* slow forward shuffling movement
* basic waypoint-oriented progress
* visible movement traces
* usable final inference behaviour for the demo

The final model did not achieve:

* clean human-like bipedal walking
* robust turning
* clear left-right alternating footsteps
* a complete footprint pattern or symbol

The result is therefore presented as a creative reinforcement learning prototype rather than a finished walking controller.

## Creative Integration

The creative output of this project comes from Wobbi’s movement and footprint traces. The imperfect shuffling gait is not hidden as a failure. Instead, it is treated as part of the robot’s character.

The movement suggests a small body trying to move forward within its physical and learned limitations. The footprint trace makes this process visible. In this way, the project connects reinforcement learning, robotic embodiment, and creative movement expression.

## Limitations and Future Work

The main limitations are:

* weak turning ability
* unclear alternating gait
* unstable reward curves
* sensitivity to waypoint placement
* simplified robot morphology
* no explicit foot contact sensing
* no imitation learning or reference gait

Future work could include:

* local target direction observations
* heading alignment reward
* turning reward
* foot contact sensors
* gait phase tracking
* imitation learning from reference motion
* improved physical model and collider setup
* curriculum learning from straight paths to curved paths

## AI Usage

Generative AI tools were used to support project planning, debugging, report organisation, reward design discussion, TensorBoard interpretation, and language refinement. The Unity scene setup, training runs, model testing, screenshots, videos, and final decisions were carried out by the author.

## Author

Tingting Liao
University of the Arts London
Applied Machine Learning Final Project
2026
