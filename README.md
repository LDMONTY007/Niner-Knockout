# This readme is unfinished. Do not follow instructions yet.

# Niner Knockout

Use Unity version 2022.3.16f1 to open this project.

Fighting game base similar to Smash bros. 

Trello Board for progress: https://trello.com/b/X9kT38ji/untitled-fighting-game


## Contributing

Please use Pull requests when developing your character. See [Character Development](#character-development)

For major changes, please open an issue first
to discuss what you would like to change.

Please make sure to update tests as appropriate.

## Installation

First, install [Unity](https://unity.com/download), then install Unity version 2022.3.16f1 or 2022.3.X to open this project.

If you plan on contributing do not simply clone this repo, make a fork so that you can use version control before ever making a pull request. 

That is, select "Fork" in the top right of the GitHub page to create a fork on your account. 

## Character Development

In this tutorial, I will use words that come from the unity manual [here](https://docs.unity3d.com/Manual/UsingTheEditor.html). Please reference this if you don't understand which part of the interface I am referring to. 

The only time you will ever make a pull request is when you are finalizing your character. Do not make pull requests unless you are satisfied with their current state and consider them "Complete" for the time being. 

First, follow the [Installation](#installation) to set up your unity project. 

Now that you've forked this repo you can begin making your character. 

Navigate to Assets/Characters and create a new folder using the name of your character.

-Insert images and how to create a character folder here-

Now, add your character folder to your "favorites" in the [Project window](https://docs.unity3d.com/Manual/ProjectView.html) so it's easier to access.

### Create Character Files

> [!NOTE]
  > All files created should be placed within your Character Folder. 

1. Create a "Moveset", by selecting Create -> Character -> Moveset
    - Moveset naming convention "MovesetCharacterName" (PascalCase).
    - The Moveset stores parameters for every attack.
    - You can find the list of attacks in the [Required Moves](#required-moves) table.

2. Create a "Icon", by selecting Create -> Character -> Icon
    - Icon naming convention "IconCharacterName" (PascalCase).
    - Contains a reference to your player prefab (we'll set that up later)
    - Contains settings for how to display your character in the "selection" menu.
    - Contains a reference for the image shown in the "selection" menu. 

![image](https://github.com/LDMONTY007/Niner-Knockout/assets/69697953/b15fddfb-8f16-40ff-8a0d-b0b74553823a)

### Create Character Script

  1. Create a new file in your Character Folder called "CharacterName.cs"
    -Select Create -> C# Script 
    -Put an image here LD-
     
  3. Open the script in your preferred IDE (Visual Studios is reccomended) either by double clicking or right clicking and selecting "open" 

  4. Change the class decleration so that it inherits from "Player" instead of "Monobehavior"
      -Put an image here LD-
  5. LD write the rest of this after you are done with your HW. This is where you left off.

## Art

### Required Moves
  
  > [!NOTE]
  > Move methods are automatically called within Player.cs,
  > All you have to do is override the method.
  > for more info see [Create Character Script](#create-character-script)
  
  | Move | Required Input | 
  |     :---:      |     :---:      |
  | git diff       | git diff       |

## License 

[MIT](https://choosealicense.com/licenses/mit/)
