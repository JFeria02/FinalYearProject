# FinalYearProject - Procedural Generation of a Planet with optimisation
Final Year Project. A procedurally generated planet with optimisation. Created in Unity 2021.3.8f1 

- Developed a mesh generation system that uses a cube to sphere mapping to generate a spherical mesh.
- Developed a chunked level of detail system, where the mesh of the planet is split into smaller segments to help reduce polygon count.
- Developed, but not implemented into the final version, threading for noise generation using Unity’s job system, where each chunk needing noise generation to be completed are processed on different threads helping to improve performance.

## Early Development / Testing

### Perturbed Perlin Noise
<img src="https://github.com/JFeria02/FinalYearProject/assets/78926685/210b3861-bf9f-46a4-8b16-21fbd9544728" width="500" height="500">

### Worley Noise
<img src="https://github.com/JFeria02/FinalYearProject/assets/78926685/41088011-8683-4d54-8f8f-c49029b33f39" width="500" height="500">

### Perlin and Worley Combined Texture
<img src="https://github.com/JFeria02/FinalYearProject/assets/78926685/5279a2d7-f112-414a-92ea-5b1f6bd307a0" width="500" height="500">

### Billow Texture
<img src="https://github.com/JFeria02/FinalYearProject/assets/78926685/76280a7d-b155-4ae3-821f-2d9eab0b229a" width="500" height="500">

### 3D Simplex Noise Slice
<img src="https://github.com/JFeria02/FinalYearProject/assets/78926685/3e5b7777-9824-4488-b7c9-ed5072cc7e08" width="500" height="500">

### Sphere Generation Test (Single Mesh)
https://drive.google.com/file/d/1CBGzQnDOydDUTxVj7ZDztQmGzpvf3gjT/view?usp=drive_link

### Early Chunked LOD Test
https://drive.google.com/file/d/1ORMaal4270COOlXxBwEDn2xnGxhaIruK/view?usp=drive_link

### Crack Removal Test
https://drive.google.com/file/d/1X_0J7aTyQdD6c4a9doxo5IqYr9uRswCD/view?usp=drive_link

### Noise Threadding Test
https://drive.google.com/file/d/1URRqNoiwqXUsrZV-2OcxRmL-Xa8gVqDR/view?usp=drive_link

## Demo Videos

### Crack Fix Demo
https://drive.google.com/file/d/1zvKWluv1xxaIY7P7MAB9XcfJGN4SidLi/view?usp=drive_link

### Noise Demo
https://drive.google.com/file/d/1743nNNiwctYy4a6NqdyR4aegKi5kTkRK/view?usp=drive_link
