# House of Trinity

## Game Overview
House of trinity is top-down maze escape game where player must navigate and escape different rooms(levels) while avoiding enemies. Player must choose 3 gameplay rules before starting each levels, of which 2 are beneficial rules that help player and one is restriction that will give player unique challenge

## Game Concept
In each level, the player:

1. Selects three rules from table of all the rules
2. Navigates the maze, runs from enemies and tries to reach exit.
3. If player dies he can try again with different (or same) rules but if player however wins he will be able to select new rules for the next level.
   

## Technical Implementation
### Architecture Highlights
- **Rules system**: Abstract base class with ScriptableObject implementation enables modular rule creation. New rules can be added by simply inheriting from the base class and implementing three core methods (Activate, Deactivate, Update), making the system highly extensible without modifying existing code.
- **Managers**: Singleton managers handle unique responsibilities. This separation of concerns ensures clean system boundaries while providing seamless level transitions and robust state management.
- **Event-Driven**: Decoupled communication between systems using C# events and delegates. Rules broadcast their state changes (activation/deactivation), player actions trigger events that multiple systems can respond to independently, and UI components subscribe to specific rule events (like TimeBend countdown updates). This architecture allows adding new features without modifying existing code for example, adding particle effects or audio cues to rule activations requires no changes to the core rule system.
### Key Algorithms
- **A\* pathfinding**: Implemented efficient grid-based pathfinding using the A* algorithm with Manhattan + diagonal heuristic. The system consists of two modular components: PathFindingGrid creates a virtual grid that automatically detects obstacles via Physics2D overlap detection, and AStarPathfinding2D executes the pathfinding with open/closed set management and optimal path reconstruction. The algorithm supports 8-directional movement with appropriate cost weighting (10 for cardinal, 14 for diagonal directions) and includes visual debugging tools for development.
- **Enemy vision and hearing**: Cone-based Ai vision system that draws a cone at specific angle and then rotates it basically like radar if the cone overlaps with player it will flag him and check if they are any walls or obstacles in between. Also added hearing system so when player approaches enemy to hearing radius even if they are not seeing you they will start chasing player.
- **Use of state management**: All behaviors are handled by specific enemies state manager. Every player has three states: Patrolling, Chasing, Searching. Patrolling state when enemy looks around and searches for player in their vision. If it finds player it goes into chasing mode and it lock in on the player and automatically. When ai loses player it will go into searching mode and pathfind to last known position look around and then either go back to Patrolling or Chasing.
- **Rule queueing**: When player choses rules, at that point up until level is fully loaded there is no player component(PlayerController) there. After locking in the rules Rule manager will setup a queue, and check every frame if there is player there and if there is it will then "release" all of the rules and activate them. If we didn't use this system manager would load rules as it is and every rule would not have a player to attach themselves to.
### Design Patterns Used
- Singleton
- Strategy
- Observer
- State
- Command

## Game Design Philosophy
The game explores the concept of voluntary constraints by forcing players to accept limitations alongside benefits, every decision becomes meaningful. The "three rules" structure creates:
- **Strategic Depth**: There is different strategy for each level
- **Replayability**: Different rule lead to different gameplay of each level.
- **Player Agency**: Success feels earned through smart planning.

## Retrospective
