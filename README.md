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
