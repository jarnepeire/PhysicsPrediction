# PhysicsPrediction
This is my exam project for Gameplay Programming (2020-2021), with "physics predictions" being the researched topic.
This readme will have the following structure (in case you want to skip ahead): 

1. What, how and why "Physics Prediction"
2. How did I design/implement it?
3. Results
4. Conclusion/Future Extensions? 

## What, how and why "Physics Prediction"
### Why?
To give a little bit of context, the year of creating this project (2020-2021) I'm an IOT student, meaning I have already finished some courses and am still (re)doing the remaining courses of the year, which also means I can spend more time on the courses I am following. Throughout the year I was on the look-out to push the edge wherever I could, implementing extra feature's for projects, doing extra research about topics, painting out how I want my portfolio and most importantly discovering what I like best. Gameplay Programming is no exception to this rule, and after some extended research, I figured physics prediction is a really relevant topic in many genre's of games, can have simple implementations, can also have very complex implementations and can be expanded on heavily. According to the teachers this could be a topic to showcase your knowledge of math and physics, which are very crucial skills to have and showcase as a developer. At this point I don't think I have to explain why there's a glowing lightbulb about to burst over my head.

### What?
The words "physics predictions" probably can paint a little picture in your head already. Literally predicting certain states, situations, times, locations, velocities or variables of objects due to physics being applied on them. It can vary from simply following the position of the ball in a game of Pong to know when to reflect the ball from the corners of the world to calculating the best way to move in such a way you can catch a flying ball through the air (*spoiler alert*). You may want to predict when a bullet will hit the enemy after shooting. Maybe you'd like to visualize where your ball will land in your awesome new re-make of Wii Tennis? These are the kinds of situations concerning physics predictions. In detail it involves mathematical and physical formula's based on what you know to reach what you want to know.

### How? 
I will be using Unity Engine (version 2019.4.16f1) to create some demo's I have pictured on paper. I will be using Unity's built in physics system to display what it can do, as well as making my own system based on physical formula's to display what I came up with. The idea of my own implementation is to make a cannon that can shoot projectiles through the air in a physically accurate motion. The cannon can be controlled to change direction or to change the force behind the projectile. Furthermore, there will be an AI that will observe the area, using mathematical and physical formula's to calculate when and where it will land, to then calculate the optimal velocity to intercept the projectile and catch it. It does not receive any pre-calculated information from the launcher, and uses mathematical concepts to find starting variables such as time, speed and direction, to then continue with the actual physics prediction.

# How did I design/implement it?
## Design
### Projectile Launcher
The actual launcher which will be responsible for shooting the projectiles in the air. It can be controlled as follows:

* Change Direction: ARROW KEYS
* Increase Force: X
* Decrease Force: Z
* Launch Projectile: SPACE

While it's being controlled and set-up for launch, it calculates the time until impact on the ground given its current force and direction. It's possible to calculate the time at other positions and not necessarily at impact of the ground. With this time, it calculates the position of impact. From all these given variables, we can figure out all positions starting at time X up until X + time until impact. It thens draw a line from each position to the next one in chronological order, visualizing its full trajectory until landing.

### Runner AI
#### Observations
The runner AI owns its own little camera, which it uses to observe the world. We only let it know, a projectile exists, nothing more. Using Unity's raycast system, we can figure out if the projectile is actually the closest in view, meaning nothing is blocking the AI from seeing the projectile. Next up we transform the projectile's position from world space to viewport space (expressed in range 0-1). Here we can do a check if the projectile is fully in view, setting up the trigger to start tracking and timing the first 3 observed positions of the projectile.

#### Predicting 
As mentioned before, it now knows its first 3 observed positions of this projectile and it knows how long it took the projectile to move from one position to the other. With this information we can estimate a direction of its trajectory (subtracting an older positions from a newer one). We can also figure out a certain speed -> the length of that vector divided by the time to get there. We can set up a "catch height" variable, depending on how big our runner character is, to calculate the time when this projectile will meet this height. With this remaining time variable, and our previously estimated variables, we can finally predict where the AI will catch this projectile. Furthermore, with everything known, we calculate our desired velocity get there as efficient as possible.

#### Extra Prediction Method (Analytical Solution for 2D Predictions)
An alternative predicting way is also implented, using Lagrange interpolations. Keep in mind, this way would work best for 2D predictions. To showcase that this works for our case, I use a work-around to convert our 3D problem into a 2D problem. Our 3D positions become 2D positions, and with those, we can form a quadratic polynomial using Lagrange interpolations. With this quadratic equation, we can substitute it into finding our 2 intersection points. The furthest intersection point having a projected X value of the landing position. With these variables, we can also estimate a direction, a "2d speed" and "2d distance" -> giving us the time and so on. This method has its limitations for 3D problems though, such as invalid discriminants when the positions are in the negative area of our world, which is caused by this little work-around from 3D to 2D. You won't encounter this in original 2D problems, and could perfectly predict positions in a 2D world (take Angry Birds as an example). 

#### Start Prediction With Drag
An implementation of drag forces are in a fourth demo, prediction the physics trajectory of a projectile with drag. Also able to calculate a desired direction in order to hit a given target. Further steps would be to take the drag into account and refine the direction, which is there, but has issues and is not 100% completed.

# Results
Now the most exciting part after all the technical explanations, the results!

Demo 1: Showcasing Unity's physics system, the projectile launcher, additional forces on contact with walls and simulated full trajectory visualization.

![Unity Demo](/ImagesReadme/UnityDemoGif.gif)

Demo 1: Showcasing self inplemented physics predictions, the projectile launcher, the runner AI and predicted locations for visualized trajectory. 

![Interception Demo 1](/ImagesReadme/RunnerDemo1Gif.gif)

Demo 2: Similar to demo 1, showcasing the observation of the AI more (starts predicting and running on projectile in sight)

![Interception Demo 2](/ImagesReadme/RunnerDemo2Gif.gif)

# Conclusion/Future Extensions?
While working on these demo's, I learned about what is accurate in real world physics, and how it can be translated into code and implementations. Unity's interesting physics system and some mathematical concepts I didn't know existed. I have a few fully working demo's showcasing different simple to more complex predictions that can directly be used or recognized in modern games of many genres. 

It's a topic that can definitely be expanded upon, for instance:
* Implementing acceleration and smoother movement
* Implementing drag (air resistance)
* Implementing rotating objects in flights and how it impacts the trajectory

It was a very fun project to work on, and I hope to be expanding on it soon for an even slicker portfolio piece.
