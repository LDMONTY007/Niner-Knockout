# This readme is unfinished. Do not follow instructions yet.

  > [!NOTE]
  > I'm planning on changing most of the code so that it's coroutine-based.
  > By this I mean for things like dashing I would run a coroutine for X
  > Number of frames so that devs can specify the frame ranges for dash cancels,
  > pivots, or foxtrots. This would make it much easier to check for input changes
  > and cancel the action. It would also allow me to modify the moveset scriptable object
  > to have extra parameters that affect how other parts of the character are handled.
  > The only issue would be that for jumping, I don't know how I'd convert it to a coroutine
  > but it doesn't need one.
  > 
  > This would also allow me to just take in a "Coroutine" parameter for moves so that the
  > designers can instead design their custom move within a coroutine then pass that instead.
  > I'll have to think about this more but it seems like a better idea than all the spaghetti
  > code I currently have.

# Niner Knockout

![LaunchGif(1)](https://github.com/LDMONTY007/Niner-Knockout/assets/69697953/b222de20-8f07-4893-992a-716f557f78b7)


Use Unity version 2022.3.16f1 to open this project.

Fighting game base similar to Smash bros. 

Trello Board for progress: https://trello.com/b/X9kT38ji/untitled-fighting-game

## Glossary

| Term | Definition | 
|     :---:      |     :---:      |
|DI|Directional Influence|

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

![CreateCharacterFolder](https://github.com/LDMONTY007/Niner-Knockout/assets/69697953/92d44a4f-c7f2-4c37-9760-46b1c9d70e19)

Now, add your character folder to your "favorites" in the [Project window](https://docs.unity3d.com/Manual/ProjectView.html) so it's easier to access.

### Create Character Files

> [!NOTE]
  > All files created should be placed within your Character Folder. 

1. Create a "Moveset", by selecting Create -> Character -> Moveset
    - Moveset naming convention "MovesetCharacterName" (PascalCase).
    - The Moveset stores parameters for every attack.
    - You can find the list of attacks in the [Required Moves](#required-moves) table.
    - When selecting values for your moveset try to reference the values of attacks as shown in the [Smash Ultimate Calculator](https://rubendal.github.io/SSBU-Calculator/)

2. Create a "Icon", by selecting Create -> Character -> Icon
    - Icon naming convention "IconCharacterName" (PascalCase).
    - Contains a reference to your player prefab (we'll set that up later)
    - Contains settings for how to display your character in the "selection" menu.
    - Contains a reference for the image shown in the "selection" menu.
      
    ![CreateScriptableObjects](https://github.com/LDMONTY007/Niner-Knockout/assets/69697953/5ead37cd-e369-4ec9-b8c5-86b70e46338b)

3. Copy (Ctrl+C) the CharacterBase.prefab (Assets/Characters/CharacterBase.prefab) and paste it in your Character Folder (Ctrl-V)
4. Rename the prefab to be your character's name (PascalCase)
5. If you haven't created animations/art for your character please follow [Art](#art) and then return here.

### Create Character Script

  1. Create a new file in your Character Folder called "CharacterName.cs" (replace CharacterName with your character's name).
    -Select Create -> C# Script

     ![CreateCharacterScript](https://github.com/LDMONTY007/Niner-Knockout/assets/69697953/0620e8c2-b996-4418-bcbf-eaaacd1835f6)
     
  2. Open the script in your preferred IDE (Visual Studios is recommended) either by double-clicking or right-clicking and selecting "open" 

  3. Change the class declaration so that it inherits from "Player" instead of "MonoBehavior"
     ![ChangeParent](https://github.com/LDMONTY007/Niner-Knockout/assets/69697953/8273ee47-eda9-4e3a-b171-5253e7074bad)

  > [!WARNING]
  > If you do not have your animations set up yet, then pause here. 
  > Follow the [Art](#art) section [LD REPLACE THIS WITH A DIRECT LINK TO SETTING UP THEIR CHARACTER PREFABS/ANIMATIONS.

  4. <details>

     <summary>Override base methods.</summary>

     ### Override base methods
     > This section is only for **attack** animations.
     > You shouldn't be manually calling any movement animations,
     > those are automatically called in [Player.cs](https://github.com/LDMONTY007/Niner-Knockout/blob/4c597641987918cf219d85183b895e2a4fd8c756/Assets/Scripts/Player/Player.cs).

     > LD You should probably just not leave this here and instead just say that we'll return to the script later and then go into the prefab part.
     > Then, after making the prefab do the art.
     > Then, during the art you can include a section about this in the part where you implement animations. 
    
     You can override the base methods for all the attacks to call your animations.
     For Example:
     https://github.com/LDMONTY007/Niner-Knockout/blob/4c597641987918cf219d85183b895e2a4fd8c756/Assets/Characters/NormTheNiner/NormTheNiner.cs#L15-L19

     Always call the base attack method, as shown above, there is internal code that must be invoked for expected functionality.
     
     Try to follow how Norm's animations are called and copy his animator controller for your character.
        
     </details>

### Setup Character Prefab

> TODO:
> Write this so that they understand how to add their Character script to the prefab.
> Then, let them go into the art section, and in the art section show them how to
> set up their prefab to have their sprite and their animator/animations. 

## Art

> [!WARNING]
> In Unity you can do 2D animations in 2 ways:
> - Sprite-Based
> - 2D rig Based
> I made the animations for Norm the Niner using a 2D rig I made.
> If you attempt to use Layers with a sprite-based animation it won't work.
> You will have to make your animations by first making movement animations
> Then make attack animations.
> Then, for attacks that you can do while moving you'll have to make new animations
> that are copies of the movement animation combined with the attack animation.
> It's up to you to decide which attacks you can do when moving but typically
> most attacks are done when stationary and have different variations when moving.
> For now, just make your movement animations, then make your attack animations.
> Then we can discuss how to set up a more refined animator to match your sprite-based setup.

Inside your "Characters/CharacterName" folder create two folders named "Sprites" and "Animations".
Place your sprites/2D art into the sprites folder and add your animations to the "Animations" folder

> TODO:
> Write some collapsable sections about how to import art for different styles/sizes.
> For example, for pixel art you must have "Filter" set to "Point (No Filter)" for it
> to look like pixel art and not be blurry. 


### Setting up the character

1. At least have your character sprite made by this step.
    - Place your sprites into the "Characters/CharacterName/Sprites" Folder you made earlier.
    - Next, open your prefab.
    - Drag your Sprite onto the "Sprite Parent" object.
    - This creates a new child object which will be the object
    - you animate.
    - If you cannot see your sprite set the position to be (0, 0, 0).
    - Also, add an empty GameObject as a child of your sprite
      - Add a "BoxCollider2D" Component to it and set "isTrigger" to true.
      - Add the "HurtBox.cs" component to it.
      - I should rearrange this so that you add it later but this works as well. 
   
   ![CreateSpriteInPrefab](https://github.com/LDMONTY007/Niner-Knockout/assets/69697953/b6fd92b6-5e7c-4b4f-9efd-af7597dbe2c8)

   
2. Setting up the animator

  > In this section go over how to make a copy of the NormTheNiner animator and
  > Replace the animations in it with their animations.    
  > You also have to make sure that they already have the character sprite
  > in the game and that it has animations already set up.

  - Copy (Ctrl+C) the NormTheNiner Animator Controller and Paste (Ctrl+V) it into your "Characters/CharacterName/Animations" Folder.
  ![CopyAnimator](https://github.com/LDMONTY007/Niner-Knockout/assets/69697953/0a63212b-afee-4379-87dc-c76238ea8f50)
  - Double-click to open the animator and replace the animation references with your animations.
    - if you don't have your movement animations set up yet then take some time to make them. Refer to [Required Base Layer Animations](#required-base-layer-animations)
  ![EditAnimator](https://github.com/LDMONTY007/Niner-Knockout/assets/69697953/e5a943a5-f8ef-4521-8c6b-8f24dce23746)
  - Open your prefab and add an animator component to your sprite, set the animation controller reference to be the animator controller you just created.
  ![AddAnimatorComponent](https://github.com/LDMONTY007/Niner-Knockout/assets/69697953/46d59fdb-c8a9-4ffe-b681-4a31433df54b)

### Required Base Layer Animations

  | Animation | Conditions | 
  |     :---:      |     :---:      |
  | Crouching       | Plays when crouching, Plays before jumping       |
  | Landing       | Plays when landing     |
  | Falling       | Plays when falling     |
  | Jump       | Plays when jumping     |
  | Floating       | Plays when jumping upwards     |
  | Idle       | Plays when idle     |
  | Walking       | Plays when walking     |
  | Running       | Plays when running     |

### Required Attack Layer Animations
  
  > [!NOTE]
  > Attack methods are automatically called within Player.cs,
  > All you have to do is override the method.
  > For more info see [Create Character Script](#create-character-script)
  
  | Attack | Conditions Controller Input | Conditions Keyboard Input |
  |     :---:      |     :---:      |     :---:      |
  | Neutral       | Button East + No directional input | 'O' + No Directional input |
  | Up Tilt | Button East + Left Stick Up | 'O' + 'W' |
  | Down Tilt | Button East + Left Stick Down | 'O' + 'S' |
  | Forward Tilt | Button East + (Left Stick Left or Right) | 'O' + ('A' or 'D') |
  
  | Aerial | Conditions Controller Input | Conditions Keyboard Input |
  |     :---:      |     :---:      |     :---:      |
  | Neutral Air | inAir + Button East + No directional input | inAir + 'O' + No Directional input |
  | Up Air | inAir + Button East + Left Stick Up | inAir + 'O' + 'W' |
  | Down Air | inAir + Button East + Left Stick Down | inAir + 'O' + 'S' |
  | Forward Air | Button East + Left Stick towards character look direction | 'O' + ('A' or 'D' depending on if the character is facing the same direction as input.) |
  | Back Air | Button East + Left Stick away from character look direction | 'O' + ('A' or 'D' depending on if the character is facing the opposite direction as input.) |

  | Special | Conditions Controller Input | Conditions Keyboard Input |
  |     :---:      |     :---:      |     :---:      |
  | Neutral Special | Button South + No directional input | 'P' + No Directional input |
  | Up Special | Button South + Left Stick Up | 'P' + 'W' |
  | Down Special | Button South + Left Stick Down | 'P' + 'S' |
  | Forward Special | Button South + (Left Stick Left or Right) | 'P' + ('A' or 'D') |

  | Smash Attack | Conditions Controller Input | Conditions Keyboard Input |
  |     :---:      |     :---:      |     :---:      |
  | Up Smash | Button East + Left Stick **Tap** Up | 'O' + 'W'  |
  | Down Smash | Button East + Left Stick **Tap** Down | 'O' + 'S'  |
  | Forward Smash | Button East + Left Stick **Tap** (Left or Right) | 'I' + ('A' or 'D')  |

## License 

[MIT](https://choosealicense.com/licenses/mit/)
