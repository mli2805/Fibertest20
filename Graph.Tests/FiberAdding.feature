Feature: FiberAdding

Background: 
	Given Left and right nodes created

@mytag
Scenario: Add fiber
	When User clicked Add fiber
	Then New event persisted
