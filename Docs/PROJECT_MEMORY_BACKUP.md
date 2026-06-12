# MonGame Project Memory Backup

Last refreshed: 2026-06-13

This is a backup copy of the root `PROJECT_MEMORY.md`. Keep this file as a second source of project context if the root memory file is damaged.

## Identity And Deployment

- Project: `MonGame`
- Unity version: `2022.3.57f1`
- Product name: `MonGame`
- Organization ID: `tobide98`
- Main scene: `Assets/Scenes/SampleScene.unity`
- Primary target: WebGL
- Itch.io page: `https://tobide98.itch.io/practicemon`
- Inferred Butler WebGL target: `tobide98/practicemon:html5`
- Existing local WebGL output: `Build/`

## Game Flow

MonGame is a 2D educational quiz-battle game. The player starts from `MenuUI`, presses Start, and enters a monster battle. `GameManager` selects a question through `QuestionManager`, `GameController` resets monsters and starts `GameUI`, and `GameUI` displays question image/text/body plus answer buttons.

Answer selection is two-step: clicking an answer only stores a pending answer and highlights it; pressing OK confirms it. Correct answers cause the player monster to attack the enemy. Wrong answers cause the enemy monster to attack the player. Damage updates HP, plays monster hit feedback, and triggers win/lose if a monster reaches 0 HP. If the battle continues, `GameUI` shows NEXT and transitions to another random question.

## Runtime Owners

- `GameManager`: singleton, menu/game/end state, gameplay fade, audio facade.
- `MenuUI`: Start button and menu fade.
- `GameController`: answer result routing, monster attack ownership, defeat handling.
- `GameUI`: question display, answer selection, OK/NEXT modes, feedback timing, image preview.
- `QuestionManager`: random question selection with optional immediate-repeat avoidance.
- `QuestionList`: ScriptableObject question/answer data.
- `AnswerButton`: answer text, click callback, selected/correct/wrong visuals.
- `AnswerFeedbackUI`: correct/wrong popup with fade/punch/SFX.
- `MonsterController`: monster HP, attack, damage, hit particles, defeated event.
- `MonsterUI`: HP display, HP fill tween, intro/idle/attack/hit animation.
- `GameEndUI`: win/lose screen and return-to-menu.
- `AudioManager` and `AudioSO`: enum-driven BGM/SFX setup and playback.
- `UIParticleEffect`: UI particle helper.
- `PlayerController`, `MobileButton`, `CameraFollow`: movement/prototype support outside the main quiz-battle flow.

## Current Image Preview Feature

`GameUI` has a `QuestionImageButton` overlay. Clicking the question image opens `ImagePreview`, copies the current `questionImage.sprite` into `imagePreviewImage`, preserves aspect, activates the preview root, and fades the preview canvas group to alpha 1. The close button fades alpha to 0, disables interaction/raycasting, clears the sprite, disables the preview image, and sets the preview root inactive. `ResetGameplayUI()` hides the preview immediately.

Scene bindings in `SampleScene.unity` connect:

- `questionImageButton`
- `imagePreviewRoot`
- `imagePreviewCanvasGroup`
- `imagePreviewImage`
- `imagePreviewCloseButton`

## Audio Enums

`BGM`: `BGM_MainMenu`, `BGM_Gameplay`, `COUNT`

`SFX`: `SFX_Hit`, `SFX_NegativeClick`, `SFX_Point`, `SFX_PositiveClick`, `SFX_Correct`, `SFX_Wrong`, `SFX_Win`, `SFX_Lose`, `SFX_Show`, `COUNT`

## Build And Upload Notes

- C# build check: `dotnet build MonGame.sln --no-restore`
- WebGL rebuild requires Unity. Close open Unity editor instances before batchmode building.
- Butler upload target for itch.io WebGL: `tobide98/practicemon:html5`
- Upload command once `Build/` is ready:

```powershell
butler push Build tobide98/practicemon:html5
```

- Status command:

```powershell
butler status tobide98/practicemon
```

## Safety Notes

- Do not revert user changes in the dirty working tree.
- Root `PROJECT_MEMORY.md` was previously all null bytes and has been rebuilt.
- Keep `Docs/PROJECT_FLOW.md` as the older architecture walkthrough.
