Feature: Nodes
	In order to know where geographically placed my equipment
	As a root user
	I want to be able to create, update and delete nodes

Scenario: Create node
	Given I call CreateNode(1.23, 1.23)
	When I call GetGraph()
	Then the return value should be
	"""
	   { 
	      "Nodes": [
	             { 
					"Id" : 0,
					"Title" : null,
	                "Coordinates": {
						"Latitude" : 1.23,
						"Longitude" : 1.23
					}
	             }
	      ]
	   }
	"""

Scenario: Update node
	Given I call CreateNode(1.23, 1.23)
	And I call UpdateNode
	"""
	{
					"Id" : 0,
					"Title" : "Hello world!",
	                "Coordinates": {
						"Latitude" : 1.23,
						"Longitude" : 1.23
					}
	}
	"""
	When I call GetGraph()
	Then the return value should be
	"""
	   { 
	      "Nodes": [
	             { 
					"Id" : 0,
					"Title" : "Hello world!",
	                "Coordinates": {
						"Latitude" : 1.23,
						"Longitude" : 1.23
					}
	             }
	      ]
	   }
	"""
