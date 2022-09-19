tool
extends "res://addons/virtualcamera/TransformModifiers/TransformModifier.gd"

class_name Shaker

export var shake_target : NodePath
export var noise : OpenSimplexNoise
export var shake_speed : float = 1.0
export var shake_strength_translation : Vector3 = Vector3.ZERO
export var shake_strength_rotation : Vector3 = Vector3.ZERO
var shake_time : float = 0.0

func _physics_process(delta : float):
    shake_time += delta * self.shake_speed * 50
    var target = get_node_or_null(shake_target)
    target.translation = Vector3(
        noise.get_noise_4d(shake_time, 0, 0, 0) * self.shake_strength_translation.x,
        noise.get_noise_4d(0, shake_time, 0, 0) * self.shake_strength_translation.y,
        noise.get_noise_4d(0, 0, shake_time, 0) * self.shake_strength_translation.z
        )
    target.rotation = Vector3(
        deg2rad(noise.get_noise_4d(shake_time, 0, 0, 1) * self.shake_strength_rotation.x),
        deg2rad(noise.get_noise_4d(0, shake_time, 0, 1) * self.shake_strength_rotation.y),
        deg2rad(noise.get_noise_4d(0, 0, shake_time, 1) * self.shake_strength_rotation.z)
        )
