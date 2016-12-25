Feature: UpdateNode

Background:
	Given A node created
	Given An update window opened for said node

# TODO: what is the title initialized with?



Scenario: Save without changes
	When Save button pressed
	Then Nothing gets saved
	Then The window gets closed

Scenario: Save with changes
	Given Title was set to blah-blah
	When Save button pressed
	Then The change gets saved
	Then The window gets closed

Scenario: Save with an existing title
	When Save button pressed
	Then Title field is red
	Then The window is not closed
