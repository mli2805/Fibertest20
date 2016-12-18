Feature: Nodes
	In order to know where geographically placed my equipment
	As a root user
	I want to be able to create nodes

Scenario: Create node
	Given I call CreateNode(1.23, 1.23)
	When I call GetGraph()
	Then the return value should be
	"""
	   { 
	      "Nodes": [
	             { 
	                "Coordinates": {
						"Latitude" : 1.23,
						"Longitude" : 1.23
					}
	             }
	      ]
	   }
	"""
