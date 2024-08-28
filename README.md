# KCiv

KCiv at first glance merely mimics the turn-based gameplay mechanics of Civilization II and Civilization III. However, the computer AI that plays against the human is composed of a mesh network of Arduinos representing a "horde" of barbarians that attempts to topple early player civilizations.

Each Arduino is its own barbarian "unit" on the world map. Each barbarian unit can find other barbarian units over the ZigBee network protocol. Each barbarian works together over the radio to collectively defeat the human player. The player also has a ZigBee-compatible radio which is used to transmit the player's moves and receive the barbarian horde's moves. 

This is the repository for the game UI (minus graphics, which might be copyrighted), game logic, and most of the non-AI game rules. The C code for the Arduino AI logic is elsewhere.

The player starts with a single city, much like in a Civilization-type video game, and the city's population goes up as the player harvests food and resources. The player can built military units using the resources they harvest and the increase in population over time. The player must use some intelligence in determining what to build and when in order to prepare for the arrival of the barbarian horde. 

An A* path-finding algorithm is used so the player can move units to a desired point on a hex-based world map. Like in Civilization-type games, player units have a limited number of moves per turn, and some units can move further than other units.

## Context

This project was developed in about 2 months as part of a solo project for an MS degree program in 2017. The intent was to demonstrate mesh networking over ZigBee wireless. The outcome was that ZigBee "barbarians" could be positioned out of radio range of the player's laptop but still participate in the attacks against the player, provided some ZigBee radios on the same PAN were located inbetween the player and the out-of-range unit.

Barbarian units could also have their corresponding Arduino board and XBee S2C radio powered by 9v battery, at least for a short period of time.

