OpenH2 - Airplane notes

- Write Interal Represntation of a .map file
- Parse .map file into the internal representation
- Transpile/convert shaders to OpenGL-compatible programs
	- Probably write an interpreter shader that handles the raw shader data, similar to UberShader for (dolphin?) some emulator
- Bitmap conversion from DDS(?) to OpenGL hardware textures
- Model parsing, export/import to/from OBJ or AutoDesk's "open format" that I can't think of at the moment
- Display tree of tags
	- Visualise textures
	- Visualise models
	- Visualise shaders on model
	- Animation system

Once all that is done, work on the engine can start
	- Rendering
	- Physics (CD/CA/CR)
	- FP Camera w/ weapons system
