Feature: AddEquipment

Background:
	Given A container-node created
	Given An Add Equipment window opened for said node

Scenario: Save
	When When Save button on Add Equipment window pressed
	Then The new piece of equipment gets saved
	Then The Add Equipment window gets closed
