tool
extends "res://addons/virtualcamera/TransformModifiers/TransformModifier.gd"

class_name LerpFollow

export var follow_target : NodePath
export(float, 0.0, 1.0) var follow_lerp_t : float = 1.0
export var follow_radius : float = 1
export var follow_offset : Vector3 = Vector3(0, 0, 0)

func has_follow_target() -> bool:
    return not follow_target.is_empty()
    
func _physics_process(delta : float):
    if not self.follow_target.is_empty():
        var target = get_node_or_null(self.follow_target)
        if target != null:
            var target_pos = target.global_transform.origin + target.global_transform.basis.xform(follow_offset)
            var dir : Vector3 = (target_pos - global_transform.origin)
            if dir.length_squared() > follow_radius * follow_radius:
                dir = dir.normalized() * follow_radius
            global_transform.origin = lerp(global_transform.origin, target_pos + dir, follow_lerp_t)
        else:
            self.follow_target = NodePath()
