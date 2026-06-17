# Model Selection Notes

This file documents the model selection process for the Wobbi gait training project.

## Final Selected Model

Final selected model:

```text
WobbiGait_SpeedFinal02.onnx
```

This model was selected because its inference behaviour showed the most acceptable balance and forward waypoint-oriented movement. Although the reward curve was not fully stable during training, the final exported model produced usable visual behaviour in Unity.

The final behaviour is best described as:

```text
limited but usable forward shuffling locomotion
```

rather than complete human-like bipedal walking.

## Model Development History

### WobbiGait_Multi_49

This version used multiple parallel agents to speed up training. It improved data collection and showed that parallel training was useful, but the resulting movement was still weak and unstable.

### WobbiGait_Stable_01

This version focused strongly on upright posture and survival. It improved standing stability, but the robot tended to remain still rather than move forward. This showed that survival reward alone was not enough for locomotion.

### WobbiGait_Move_01

This version introduced movement reward. It encouraged more forward motion, but also created a risk of crawling or sliding because movement could be rewarded even when the body posture was poor.

### WobbiGait_MoveStrong_01

This version increased the movement reward. It produced more active shuffling movement, but the gait still did not become clearly alternating or human-like.

### WobbiGait_AltGait_01

This version introduced an anti-phase hip action reward to encourage the left and right legs to move in opposite directions. It improved the tendency toward leg alternation, but the visual gait was still not clearly structured.

### WobbiGait_Speed_02

This version built on `WobbiGait_AltGait_01` and added a target-speed reward. It became one of the strongest earlier models because it encouraged the robot to move forward more actively instead of standing still.

This model was useful as a checkpoint and comparison baseline.

### WobbiGait_SpeedFinal01

This version was another speed-oriented training run. The reward occasionally reached high values, but the standard deviation was large and the behaviour was not consistently stable. It was kept as an experiment but not selected as the final model.

### WobbiGait_SpeedFinal02

This was selected as the final model. It produced the most usable inference behaviour among the tested models. The robot could remain upright and move slowly toward waypoint targets. The movement still showed limitations, especially weak turning and unclear footstep alternation, but it was the most suitable result for the final demonstration.

## Why SpeedFinal02 Was Selected

`WobbiGait_SpeedFinal02` was selected because:

* It showed better forward movement than earlier stability-only models.
* It remained more visually usable during inference.
* It could progress toward simple waypoint targets.
* It produced movement suitable for footprint visualisation.
* It supported the creative framing of Wobbi as a character with imperfect embodied movement.

The selection was based on both:

```text
TensorBoard training curves
```

and:

```text
direct observation of inference behaviour in Unity
```

The reward curve alone was not treated as the only evaluation metric, because some runs showed high reward values but poor or inconsistent visual behaviour.

## Main Limitations

The final model still had several limitations:

* Turning was weak.
* Curved waypoint paths were difficult.
* The robot mainly learned forward shuffling rather than robust walking.
* The feet did not form a clear alternating step pattern.
* The reward curve showed instability during training.
* The policy was sensitive to waypoint placement and physical setup.

## Final Evaluation Statement

The final model demonstrates partial success. It does not achieve perfect bipedal walking, but it shows that PPO can generate limited forward locomotion behaviour for a simplified character-inspired robot. The imperfect shuffling movement was integrated into the creative output through footprint traces and process reflection.
