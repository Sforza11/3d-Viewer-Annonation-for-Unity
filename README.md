# 3d-Viewer-Annonation
This guide outlines the steps to implement an enhanced 3D annotation system in a Unity project. The system allows for interactive annotations on 3D models with customizable display options.

Enhanced Annotation System Implementation Guide
This guide outlines the steps to implement an enhanced 3D annotation system in a Unity project. The system allows for interactive annotations on 3D models with customizable display options.
Prerequisites

Unity (version used in development)
Basic knowledge of Unity editor and C# scripting

Implementation Steps
1. Camera Setup

Create a Main Camera in your scene if it doesn't exist already.
Attach the Camera_controller_new.cs script to the Main Camera.
Configure the camera controller script with the following settings:

Orbit Speed: 50
Pan Speed: 0.01
Zoom Speed: 0.15
Smoothness: 15
Min Zoom Distance: 0.1
Max Zoom Distance: 700
Max Pan Distance: 10



2. Model Container Setup

Create an empty GameObject and name it "Model Container".
Attach the ModelBoundCalculator.cs script to the Model Container.
Make your 3D model a child of the Model Container.
Apply Mesh Colliders to each part of your 3D model that you want to be annotatable.

3. Event System Setup

Create an Event System in the hierarchy if it doesn't exist already.

In Unity menu: GameObject -> UI -> Event System



4. Canvas Setup

Create a new Canvas in the hierarchy.
Rename the Canvas to "World Space".
Set the Canvas Render Mode to "World Space".
In the Canvas component, set additional shader channels:

Texcoord1
Normal
Tangent


Set the Canvas size to 1920x1080.
Create a Button (TextMeshPro) as a child of the World Space canvas.
Convert the Button to a prefab by dragging it from the hierarchy into the Project window.

5. Annotation System Setup

Create a new GameObject and name it "AnnotationSystem".
Attach the Annotation_updated.cs script to the AnnotationSystem GameObject.
In the Inspector, set the correct Obstacle Layer for the annotation system.

Script Descriptions
Camera_controller_new.cs
This script manages camera movement, allowing for orbit, pan, and zoom functionalities.
ModelBoundCalculator.cs
Calculates the bounds of the 3D model, which is used by the annotation system for positioning and visibility checks.
Annotation_updated.cs
The main script for the annotation system. It handles the creation, display, and interaction with annotations on the 3D model.
Usage
After setting up the system:

Enter Play mode in Unity.
Click on parts of your 3D model to add annotations.
Interact with the annotation points to view and edit annotations.
Use the camera controls to navigate around your 3D model and view annotations from different angles.

Customization
You can customize the appearance and behavior of annotations by modifying the public variables in the Annotation_updated.cs script through the Unity Inspector.
Troubleshooting

Ensure all scripts are properly attached to the correct GameObjects.
Check that your 3D model has the correct colliders for interaction.
Verify that the Obstacle Layer is set correctly in the annotation system.
