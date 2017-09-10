# Dactylea

Dactylea is a didactic chemistry simulation in VR. With the help of Fabienne, Dactylea's AI, make a Pharao's Serpent.  
You can talk to Fabienne, she may not understand everything but she will do her best to help you. For example, you can ask her to repeat the last instructions or to explain you what you should do next.  
Be careful when manipulating chemicals, Ethanol burns and glass breaks. You have to finish the simulation without doing any mistake !  

Dactylea was made for the [LAMPA laboratory](http://lampa.ensam.eu/).
Here is the [presentation video (on Youtube)](https://www.youtube.com/watch?v=ghqWI-w7M_k).

Most graphical elements were made by [Emilien Grude](https://www.artstation.com/scraick).

## Table of contents
[The simulation](#the-simulation)  
[Fluid simulation](#fluid-simulation)  
[Other interactions](#other-interactions)  
* [Voice recognition](#voice-recognition)
* [Controller selection](#controller-selection)
* [Elevating platform](#elevating-platform)
* [Whiteboard drawing](#whiteboard-drawing)
[Illustrations](#illustrations)  

## The simulation

## Fluid simulation

## Other interactions

### Voice recognition
Voice recognition was achieved by using Microsoft's API with :
```csharp
using UnityEngine.Windows.Speech;
```
This API "only" does Keyword recognition, not Speech-to-Text.

### Controller selection
The user has the choice between 4 controllers, each controller is there to improve immersion, trying to explain interactions in different ways:
* Default controller  
* Hands  
* Default controller with Claws
* Default controller with Laser beam

### Elevating platform
It was decided during development to present this project at the Science Festival. We wanted children to be able to participate, to keep it immersive, we added a platform that can rise up.

### Whiteboard drawing
There was a whiteboard since ealy development, we wanted to scene to feel more "complete" so we added pens and interaction to be able to draw on the whiteboard.  
By default, you only have few pens but you can ask Fabienne to bring you the pen distributor, allowing you to make any color (using RGB). Be careful not to flood the floor with too many pens, it may slow down the program at some point !  
The technique I used for whiteboard is not very good and I didn't have time to update it. If you're interested  in Texture painting, you should check [this tutorial](http://codeartist.mx/tutorials/dynamic-texture-painting/).

## Illustrations