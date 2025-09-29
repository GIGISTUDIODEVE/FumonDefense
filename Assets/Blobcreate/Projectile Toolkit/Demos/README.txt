Please read the Manual.pdf to learn how to set up the demos correctly ("Explore the demos > In editor" section).

These demos use "Input Manager" instead of "Input System", if you are using Input System, you can change the input setting to "Both" to make the demos work. The setting is under "Project Settings > Player > Other Settings > Active Input Handling".

The demos are a great learning resource about how to achieve different gameplays with Projectile Toolkit. You can also use the scripts and prefabs of the demos directly for fast prototyping.

------

Starting from version 3.0, some demo scripts have been rewritten to have "Control API", which enhances their reusability. You can invoke them through your own scripts or visual scripts.

– JumpTester:
1. In scene "00 Jump", drag and drop "cube1" into your folder to make a prefab,
2. turn on "Controlled By Another Script" of "JumpTester" component on the prefab,
3. create an instance of the prefab in your scene, and now you are able to call "JumpToPosition(...)" instance method in your own script / visual script.

– CannonLike:
1. In scene "03 Cannon-Like Weapon", drag and drop "Cannon" into your folder to make a prefab,
2. turn on "Controlled By Another Script" of "CannonLike" component on the prefab,
3. create an instance of the prefab in your scene, and now you are able to call "Aim(...)" and "Launch()" instance methods in your own script / visual script.

------

Starting from version 3.1, new items are provided as templates, no more manual setups will be required.

– AeroProjectileLauncher:
This class, which used to be named CurvyTest, is now part of a template.
See the online documentation "Projectile Aerodynamics".
