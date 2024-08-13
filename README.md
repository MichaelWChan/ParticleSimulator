Course Project - Fire Simulator
CSC473, 2024-04-14
Michael Chan

Build/Run:
Make sure you have the .NET desktop development workload installed. 

You can open the solution in JetBrains Rider and execute the program in the top right (Shift + F10)

Or

You can build the solution in Visual Studio 2022.
- Build the solution.
- Run it

Features:
- Particle System
"Spacebar" to toggle between a fire centered on the screen.
"Left click" to spawn a fire on your cursor position.
"Right click" to create a wind that will blow the fire opposite to the direction of the cursor.
"Middle click" to create a collision object that will deflect the fire.
"r" to reset the particles and collision objects on the screen.
"Numbers 1-9" to change the number of particles spawned per frame. Default is Number 6.

Acknowledgements:
The majority of the rendering code was adapted from https://learnopengl.com/ to C#. 