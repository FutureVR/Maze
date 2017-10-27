# Maze

<img src="https://raw.github.com/FutureVR/Maze/master/media/snapshot_1.PNG" width="350" height="350" />
<img src="https://raw.github.com/FutureVR/Maze/master/media/snapshot_3.PNG" width="350" height="350" />

<b>INTRODUCTION:</b> This simulation creates a maze and then “floods” it slowly over time, marking flooded territories with a red color, and walls with black. I was inspired to make this small Unity application by months of playing dungeon crawlers like “Dungeon Crawl Stone Soup.” I wanted to somehow generate the maze-like architecture from my childhood games. After creating the maze, I thought it would be interesting to “flood” it, and watch how the “water” slipped through all the passages, in order to better understand the flow of the randomly-generated paths.

<b>IMPLEMENTATION:</b> The maze was created by first selecting a random block within a 2D array, and a direction in which to create the “hallway”. Every subsequent block in that direction became a part of this hallway until it reached an arbitrarily-set length, or the next block would have had more than one empty neighbor. This process continued until no more walls could be turned into empty spaces. Once the maze was created, Breadth First Search was used to fill in the “hallways” of the maze, starting at the bottom left corner. 

<b>CONCLUSION:</b> I found it enjoyable to watch the “water” flow through the maze, unearthing its hidden passageways. Through the process, I practiced some fundamental CS algorithms, and had fun doing it. If I were to add more mechanics to this program, I could add rooms to the maze, instead of only having passageways, like a real rogue-like would do. I could also add simple AI characters that run away from the flooding hallways to empty spaces, and finally burn in the oncoming lava. 
