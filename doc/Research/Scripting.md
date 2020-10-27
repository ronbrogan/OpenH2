#Halo 2 Script Info

## Bytecode description
The compiled bytecode found in the map is arranged as a sequence of 'nodes'. These nodes essentially represent the parse tree of the original script, not linearized instructions.
A node has values to define the node type, data type, and data. (There's also values to reference originating data, such as the token that the node was created from)
Each node contains an index of the node's sub-exression. In order to represent a sibling node, a 'scope' node is used, which uses the data bits of the node to store the sibling index.
Each node has a 'checkval' that is embedded in referencing nodes to prevent incorrect sequences from executing. 

### Runtime
Patching the game scripts has yielded the following findings:
 - Originating token index is not used
 - OP field maps 1:1 to built-in and other script indexes
 - Scope OP values must match the child's OP
 - Unknown field 'h' is not used

## Limits
 - Squads are limited to 255, as when referring to a contained starting location a single byte is available/used for addressing
 - (Presumed) Node index is likely a signed short value, indicating a maximum of 32,767 nodes in a scenario.