Hello!

This guide explains how to add a path to the projection shader!

1. **Create an image (PNG, JPG, etc.) of your path.**
	- Any color that is not white or fully transparent will appear as part of the path in Unity. Semi-transparent pixels will appear as a section of semi-transparent path.
	- The beginning of the path should be in the bottom-left corner of the image.
	- The path cannot touch the edge of the image; leave at least one pixel of white or fully transparent space around it.
2. **Upload your path image to Unity.**
	- The path image should go in the "Paths" folder. *(It will work elsewhere, but this is easier for organizing.)*
	- To upload it, navigate to the ```Assets/Art/Projector/Paths``` folder in the *Project* tab of Unity, then drag your file here from anywhere else on your PC.
3. **Adjust the settings of your path.**
	- Click your path image file in the *Project* tab.
	- Ensure the texture settings match the following:
		- Texture Type: Default
		- Texture Shape: 2D
		- Alpha Source: Input Texture Alpha
		- Alpha is Transparency: True (checked)
		- Wrap Mode: Clamp **(important!)**
4. **Add your path to the shader.**
	- Navigate back to the ```Assets/Art/Projector``` folder. *(Where you found this README!)*
	- Click on the material (looks like a sphere) called *PathProjector*.
	- In the *Inspector* tab, find the box to the right of the label *"Path"*.
	- Drag your uploaded texture from the Paths folder to this box in the Inspector.
5. **Adjust projector settings.** (optional)
	- *"Main Texture"* is the rest of the floor underneath the texture.
	- *"Path Color"* is the color that the path will appear.
	- *"Starting Point"* is the starting point of the path. To move the path around, adjust this point.
	- *"Rotation"* is the rotation of the path in radians.
	- *"Scale"* is the path's scale - 1 is largest, 0 is smallest. (The path will not be visible at 0.)
	
Enjoy! :D 