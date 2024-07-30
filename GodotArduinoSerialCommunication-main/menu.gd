extends Control


# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with functio

func _on_start_button_pressed():
	get_tree().change_scene_to_file("res://Scenes/GameScene.tscn") 



func _on_options_button_pressed():
	get_tree().change_scene_to_file("res://Scenes/OptionsMenu.tscn")


func _on_quit_button_pressed():
	get_tree().quit()
