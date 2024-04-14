# FinalYearProject - Procedural Generation of a Planet with optimisation
Final Year Project. A procedurally generated planet with optimisation. Created in Unity 2021.3.8f1 

- Developed a mesh generation system that uses a cube to sphere mapping to generate a spherical mesh.
- Developed a chunked level of detail system, where the mesh of the planet is split into smaller segments to help reduce polygon count.
- Developed, but not implemented into the final version, threading for noise generation using Unity’s job system, where each chunk needing noise generation to be completed are processed on different threads helping to improve performance.

