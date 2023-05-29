using UnityEngine.InputSystem;

namespace NobetaTrainer.Shortcuts;

public static class NobetaInputs
{

    public static InputActionMap GameplayActionMap => Game.input?.gameplayActionMap;

    public static InputAction JumpAction => GameplayActionMap["Jump"];
    public static InputAction DodgeAction => GameplayActionMap["Dodge"];
}