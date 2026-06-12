# MonGame Project Memory

Last refreshed: 2026-06-13

## Identity And Deployment

- Project: `MonGame`
- Unity version: `2022.3.57f1`
- Product name: `MonGame`
- Organization ID in Unity project settings: `tobide98`
- Primary build target: WebGL
- Runtime scene: `Assets/Scenes/SampleScene.unity`
- Itch.io page: `https://tobide98.itch.io/practicemon`
- Inferred Butler upload target for WebGL: `tobide98/practicemon:html5`
- Existing local WebGL build folder: `Build/`
- Important deployment rule: do not build this Unity project unless the user explicitly changes this rule. The user builds the project manually. Codex should only upload the already-built `Build/` folder to itch.io.
- Current user-built WebGL output path: `C:\Users\tobid\OneDrive\Documents\MyWorkspace\MonGame\Build`

## Current Game Shape

MonGame is a 2D educational quiz-battle game. The player starts on a main menu, enters a battle between two monsters, answers multiple-choice questions, and each confirmed answer causes one monster to attack. Correct answers make the player monster attack the enemy; wrong answers make the enemy attack the player. The game ends when either monster reaches 0 HP, then a win or lose screen appears and the player can return to the menu.

The game is not currently structured as a level/exploration game. `PlayerController`, `MobileButton`, and `CameraFollow` still exist as movement-support/prototype scripts, but the main shipped loop is quiz battle.

## Primary Runtime Flow

1. `GameManager.Awake()` establishes the singleton, sets `Application.targetFrameRate`, disables vSync, and initializes `AudioManager`.
2. `GameManager.Start()` shows the menu immediately, hides gameplay/end UI, and plays `BGM_MainMenu`.
3. `MenuUI` handles the Start button. It plays click SFX, fades the menu out, and calls `GameManager.StartGame()`.
4. `GameManager.StartGame()` checks required references, resets gameplay state, selects a random question through `QuestionManager`, sets `HasStarted`, starts `GameController`, and fades in the gameplay canvas group.
5. `GameController.StartGame()` resets both monsters and tells `GameUI` to start with the first question.
6. `GameUI` renders the question image/text/body, configures answer buttons, hides the physical OK/NEXT button, plays monster intro animations, then fades/moves the question UI in.
7. Selecting an answer only stores a pending answer and highlights the selected button. The answer is not applied until OK is pressed.
8. Pressing OK locks answer buttons, shows selected/correct/wrong visual state, plays `AnswerFeedbackUI`, then calls back into `GameController`.
9. `GameController.ApplyAnswerResult()` sends the attack to the correct monster side.
10. `MonsterController.Attack()` plays `MonsterUI.PlayAttack()` and then applies damage to the target.
11. `MonsterController.TakeDamage()` clamps HP, refreshes UI, plays hit feedback, plays hit particles/SFX, and raises `Defeated` if HP reaches 0.
12. `GameController` listens for monster defeat. Enemy defeat shows win UI; player defeat shows lose UI.
13. If no monster is defeated, `GameUI` shows the physical button again in NEXT mode. Pressing it selects another random question and fades the question UI between questions.
14. `GameEndUI` shows win/lose text and image, plays win/lose SFX, then the Return button fades out end UI and returns through `GameManager.ReturnToMainMenu()`.

## Core Scripts

- `Assets/Scripts/GameManager.cs`: top-level coordinator for menu/game/end states, singleton access, gameplay canvas fade, game start/reset, and audio facade methods.
- `Assets/Scripts/GameController.cs`: battle coordinator. Owns the relation between answers and monster attacks. Subscribes to player/enemy defeated events.
- `Assets/Scripts/GameUI.cs`: question rendering, answer selection/confirmation, OK/NEXT button modes, question transitions, feedback sequencing, monster intro sequencing, and question image preview.
- `Assets/Scripts/QuestionManager.cs`: selects random questions from `QuestionList`, optionally avoiding immediate repeat.
- `Assets/Scripts/QuestionList.cs`: ScriptableObject data model. Each `Question` has `questionImage`, `questionText`, `answerBodyText`, and a list of `Answer`; each `Answer` has `isCorrect` and `answerText`.
- `Assets/Scripts/AnswerButton.cs`: individual answer button state, text binding, selected/correct/wrong colors, click callback, and click SFX.
- `Assets/Scripts/AnswerFeedbackUI.cs`: correct/wrong feedback popup with optional result sprite, text color, fade, punch scale, delay, and SFX.
- `Assets/Scripts/MonsterController.cs`: monster data and rules: name, max/current HP, base attack, hit particles, reset, attack, damage, defeated event.
- `Assets/Scripts/MonsterUI.cs`: monster name/HP UI, HP fill color/tween, intro, idle loop, attack movement, hit blink, hit punch, and transform reset.
- `Assets/Scripts/MenuUI.cs`: menu root, start button, fade in/out, start guarding.
- `Assets/Scripts/GameEndUI.cs`: win/lose root, result image, return button, fade in/out.
- `Assets/Scripts/AudioManager.cs`: enum-based BGM/SFX playback with `AudioSO`, looping BGM coroutine and fade in/out.
- `Assets/Scripts/SO/AudioSO.cs`: `BGMData[]` and `SFXData[]` registry. `GatherAssets()` loads clips from `Resources/Audio/BGM` and `Resources/Audio/SFX`.
- `Assets/Scripts/UIParticleEffect.cs`: helper to spawn/follow a particle effect at a UI target.
- `Assets/Scripts/PlayerController.cs`, `MobileButton.cs`, `CameraFollow.cs`: movement/camera support, currently separate from the core quiz-battle loop.

## Current UI Details

- Main menu uses `MenuUI` and fades out before gameplay starts.
- Gameplay uses a `CanvasGroup` controlled by `GameManager`.
- Question UI is hidden until the game starts, then appears after monster intro.
- Answers use a two-step confirmation flow: select an answer, then press OK.
- The OK and NEXT actions share one physical button in `GameUI`, using a `NextButtonMode` enum:
  - `ConfirmAnswer`: button text is `OK`, color is `okButtonColor`.
  - `NextQuestion`: button text is `NEXT`, color returns to default.
- Answer colors:
  - selected: configured per `AnswerButton`, default yellow.
  - correct: green.
  - wrong: red.
- Wrong answers reveal the correct answer by coloring the correct answer button green.
- `AnswerFeedbackUI` plays before damage resolution.
- End screen fades in with win/lose text and result sprite, then Return brings the player back to menu.

## Image Preview Feature

Added on 2026-06-12:

- The question image area has a `QuestionImageButton`.
- `GameUI` has serialized references for:
  - `questionImageButton`
  - `imagePreviewRoot`
  - `imagePreviewCanvasGroup`
  - `imagePreviewImage`
  - `imagePreviewCloseButton`
  - `imagePreviewFadeDuration`
- Clicking the question image opens `ImagePreview`.
- The preview image sprite is copied from the current question UI image.
- The preview preserves the question image aspect setting.
- Opening uses `CanvasGroup.DOFade(1f, imagePreviewFadeDuration)`.
- Closing uses `CanvasGroup.DOFade(0f, imagePreviewFadeDuration)`, then clears the sprite, disables the image, and sets the preview root inactive.
- Preview raycasts/interactable state are disabled while hidden or fading out.
- `ResetGameplayUI()` hides the preview immediately.
- `SampleScene.unity` binds the existing `ImagePreview`, preview image, question image button, and close button to `GameUI`.

## Audio

Namespace: `WhackAMole`

`BGM` enum:

- `BGM_MainMenu`
- `BGM_Gameplay`
- `COUNT`

`SFX` enum:

- `SFX_Hit`
- `SFX_NegativeClick`
- `SFX_Point`
- `SFX_PositiveClick`
- `SFX_Correct`
- `SFX_Wrong`
- `SFX_Win`
- `SFX_Lose`
- `SFX_Show`
- `COUNT`

`GameManager` plays main menu BGM on startup. Most UI clicks use `SFX_PositiveClick`; answer feedback uses correct/wrong SFX; damage uses hit SFX; end UI uses win/lose SFX.

## Build And Verification

- Unity editor version in `ProjectSettings/ProjectVersion.txt`: `2022.3.57f1`.
- `ProjectSettings/EditorBuildSettings.asset` includes only `Assets/Scenes/SampleScene.unity`.
- `ProjectSettings/ProjectSettings.asset` has WebGL configured with DOTWEEN scripting define.
- C# syntax/build check used successfully: `dotnet build MonGame.sln --no-restore`.
- Deployment rule: never run Unity build commands for this project. The user is responsible for building. For itch.io updates, upload the existing `Build/` folder only.
- Existing Butler executable path found previously: `C:\Users\tobid\OneDrive\Documents\UserFolder\Butler\butler.exe`.
- Butler credentials exist at `C:\Users\tobid\.config\itch\butler_creds`, but the API token does not permit listing games. Upload target should be provided or inferred from known itch page.

## Known Local State Notes

- `PROJECT_MEMORY.md` was previously corrupted: 7,660 bytes of null data. This file has been rebuilt from actual project files and `Docs/PROJECT_FLOW.md`.
- There are many modified/untracked assets and scripts in the working tree. Treat them as user/project changes and do not revert unless explicitly requested.
- The root `Build/` folder is a WebGL build layout with `index.html`, `Build/`, and `TemplateData/`.
- `Docs/PROJECT_FLOW.md` is the older flow document from 2026-06-08.
- A backup memory document now exists at `Docs/PROJECT_MEMORY_BACKUP.md`.

## Practical Commands

Build check:

```powershell
dotnet build MonGame.sln --no-restore
```

Expected Butler upload command for WebGL after the user has built the project:

```powershell
butler push Build tobide98/practicemon:html5
```

Check latest uploaded channel status:

```powershell
butler status tobide98/practicemon
```
