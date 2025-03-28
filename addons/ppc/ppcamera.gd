"""
  _____ _            _       _____
 |  __ (_)          | |     |  __
 | |__) | _ __   ___| |__   | |__) |_ _ _ __
 |  ___/ | '_ | / __| '_ |  |  ___/ _` | '_ |
 | |   | | | | | (__| | | | | |  | (_| | | | |
 |_|   |_|_| |_| ___|_| |_| |_|    __,_|_| |_|
   _____
  / ____|
 | |     __ _ _ __ ___   ___ _ __ __ _
 | |    / _` | '_ ` _ | / _ | '__/ _` |
 | |___| (_| | | | | | |  __/ | | (_| |
   _____ __,_|_| |_| |_| ___|_|   __,_|

A touchscreen optimized camera control system
for common 2D top-down strategy games.

Licensed under MIT

v. 0.4

Author: Max Schmitt
		from
		Divirad - Kepper, Lösing, Schmitt GbR
"""

@icon("res://addons/ppc/icon.png")
class_name PinchPanCamera extends Marker2D


@export var enable_pinch_pan: bool = true
@export var enable_drag: bool = true

@export_enum("Normal", "Horizontal", "Vertical") var slide_mode: int
@export var enabled: bool = true
@export var natural_slide: bool = true

@export_group("Smoothing")
@export var smoothing: bool = true
@export var position_smoothing_speed: int = 10

@export_group("Zoom Options")
@export var invert_mousewheel_zoom: bool = false
@export var invert_touch_zoom: bool = false
@export var min_zoom_factor: float = 0.6
@export var max_zoom_factor: float = 2
@export var drag_deadzone: Vector2 = Vector2(0.1, 0.1)

@export_group("Debug Options")
@export var show_debug_icon: bool = false

var start_position: Vector2
var already_pressed: bool = false
var min_zoom: Vector2 = Vector2(0, 0)
var max_zoom: Vector2 = Vector2(0, 0)
var naturalizer: int = 1

var camera: TouchZoomCamera2D

signal zoom_in()
signal zoom_out()
signal just_pressed()
signal dragging()

signal input_number(num: int)

## initializes all the export variables
func _enter_tree() -> void:
	min_zoom = Vector2(min_zoom_factor, min_zoom_factor)
	max_zoom = Vector2(max_zoom_factor, max_zoom_factor)
	
	#scale = Vector2(5, 5)
	
	var touch_camera := TouchZoomCamera2D.new()
	touch_camera.name = "TouchZoomCamera2D"
	add_child(touch_camera)
	camera = touch_camera

	camera.drag_left_margin = drag_deadzone.x
	camera.drag_right_margin = drag_deadzone.x
	camera.drag_top_margin = drag_deadzone.y
	camera.drag_bottom_margin = drag_deadzone.y
	
	camera.enabled = enabled
	camera.position_smoothing_enabled = smoothing
	camera.position_smoothing_speed = position_smoothing_speed
	camera.invert_zoom = invert_touch_zoom
	
	if show_debug_icon:
		var di: PackedScene = load("res://addons/ppc/testicon.tscn")
		add_child(di.instantiate())

func _process(_delta: float) -> void:
	camera.enable_drag = self.enable_drag
	if !self.enable_drag:
		return
	
	if camera.drag_left_margin != drag_deadzone.x \
	and camera.drag_right_margin != drag_deadzone.x \
	and enable_drag:
		camera.drag_left_margin = drag_deadzone.x
		camera.drag_right_margin = drag_deadzone.x
	
	if camera.drag_top_margin != drag_deadzone.y \
	and camera.drag_bottom_margin != drag_deadzone.y \
	and enable_drag	:
		camera.drag_top_margin = drag_deadzone.y
		camera.drag_bottom_margin = drag_deadzone.y
	
	if camera.enabled != enabled:
		camera.enabled = enabled
		
	if smoothing != camera.position_smoothing_enabled:
		camera.position_smoothing_enabled = smoothing

	if camera.position_smoothing_speed != position_smoothing_speed:
		camera.position_smoothing_speed = position_smoothing_speed

	if min_zoom != Vector2(min_zoom_factor, min_zoom_factor):
		min_zoom = Vector2(min_zoom_factor, min_zoom_factor)

	if max_zoom != Vector2(max_zoom_factor, max_zoom_factor):
		max_zoom = Vector2(max_zoom_factor, max_zoom_factor)
	
	# inverts inputs
	if natural_slide and naturalizer != 1:
		naturalizer = 1
	elif !natural_slide and naturalizer != -1:
		naturalizer = -1

	if camera.invert_zoom != invert_touch_zoom:
		camera.invert_zoom = invert_touch_zoom

func _input(event: InputEvent) -> void:
	if !enable_drag:
		return
	if !enable_pinch_pan:
		return

	## Handle MouseWheel for Zoom
	if event is InputEventMouseButton and event.is_pressed():
		var invert_modifyer: int = 1
		if invert_mousewheel_zoom:
			invert_modifyer = -1

		if invert_mousewheel_zoom:
			if event.button_index == MOUSE_BUTTON_WHEEL_UP and camera.zoom >= min_zoom:
				zoom_out.emit()
				camera.zoom -= Vector2(0.1, 0.1)
			if event.button_index == MOUSE_BUTTON_WHEEL_DOWN and camera.zoom <= max_zoom:
				zoom_in.emit()
				camera.zoom += Vector2(0.1, 0.1)
		else:
			if event.button_index == MOUSE_BUTTON_WHEEL_UP and camera.zoom <= max_zoom:
				zoom_in.emit()
				camera.zoom += Vector2(0.1, 0.1)
			if event.button_index == MOUSE_BUTTON_WHEEL_DOWN and camera.zoom >= min_zoom:
				zoom_out.emit()
				camera.zoom -= Vector2(0.1, 0.1)

	## Handle Touch
	if event is InputEventScreenTouch and enable_drag:
		if event.is_pressed() and !already_pressed:
			just_pressed.emit()
			start_position = get_norm_coordinate() * naturalizer
			already_pressed = true
		if !event.is_pressed():
			already_pressed = false

	## Handles ScreenDragging
	if event is InputEventScreenDrag and enable_drag:
		if camera.input_count == 1:
			dragging.emit()
			if natural_slide:
				position += get_movement_vector_from(get_local_mouse_position())
				start_position = get_local_mouse_position()
			else:
				var coord: Vector2 = get_movement_vector_from(-get_norm_coordinate())
				position += coord

	## Handles releasing
	if camera.input_count == 0:
		position = camera.get_camera_center()
		enable_drag = true


## calculates a vector for camera movement
func get_movement_vector_from(vec: Vector2) -> Vector2:
	var move_vec: Vector2 = start_position - vec

	if slide_mode == 1:
		return Vector2(move_vec.x, 0)
	if slide_mode == 2:
		return Vector2(0, move_vec.y)
	else:
		return move_vec


## gets the normalized coordinate of touch
func get_norm_coordinate() -> Vector2:
	var result: Vector2 = Vector2.ZERO
	if natural_slide:
		result = get_global_mouse_position() - camera.get_camera_center()
	else:
		result = get_local_mouse_position() - camera.get_camera_center()
	return result


func invert_vector(vec: Vector2) -> Vector2:
	return Vector2(-vec.x, -vec.y)
