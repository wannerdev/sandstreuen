## Sandstreuen


A mobile AR isosurface manipulation prototype by handtracking using <a href="https://www.manomotion.com/mobile-ar"> Manomotion CE </a> handtracking.
The goal was to simulate spreading sand in some way.

The Voxel algorithm is based on Dual contouring generally it is a implementation of <a href="http://www.boristhebrave.com/2018/04/15/dual-contouring-tutorial/"> Boris tutorial</a> with some modifications.
The performance is bad, it is not efficient since unitys job system wasn't used. The Project was built with Unity 2021.1.14f1
It was tested on a oneplus 5.

There are 2 settings that can be changed by gestures the material and the edit mode, they can be seen in the images in small white text.  
#### Materials (They influence the angle of the cone):  
sanddry, sandwetstart, sandwetend, sandwet

#### Edit modes:
cone, single, remove, grow

The camera view is changed by the direction the hand moves towards but this can be annoying thats why there is the button which toggles this feature.
With the controllers the camera can be controlled more easily.

### Demo of the single mode 
The scale of 0.1 was used

https://user-images.githubusercontent.com/7975579/155970278-de06a796-0ab4-4e46-bcf0-39eac89f5343.mp4



### Change Material by fist gesture
<img src="https://github.com/wannerdev/sandstreuen/blob/main/img/Screenshot_20211112-053008.jpg?raw=true" width="256" height="256" />

### Create sand by click gesture
<div>
  <img src="https://github.com/wannerdev/sandstreuen/blob/main/img/Screenshot_20211112-053021.jpg?raw=true" width="256" height="256" />
  <img src="https://github.com/wannerdev/sandstreuen/blob/main/img/Screenshot_20211112-053030.jpg?raw=true" width="256" height="256" />
  
  <img src="https://github.com/wannerdev/sandstreuen/blob/main/img/image35.gif?raw=true" width="256" height="256" />
</div>

### Change edit mode by palm switch
<div>
  <img src="https://github.com/wannerdev/sandstreuen/blob/main/img/Screenshot_20211112-053105.jpg?raw=true" width="200" height="200" />
  <img src="https://github.com/wannerdev/sandstreuen/blob/main/img/Screenshot_20211112-053107.jpg?raw=true" width="200" height="200" />
  <img src="https://github.com/wannerdev/sandstreuen/blob/main/img/Screenshot_20211112-053109.jpg?raw=true" width="200" height="200" />
  <img src="https://github.com/wannerdev/sandstreuen/blob/main/img/Screenshot_20211112-053112.jpg?raw=true" width="200" height="200" />
<div>
