using UnityEngine.InputSystem;

namespace NobetaTrainer.Utils;

public static class InputUtils
{

    public static InputActionMap GameplayActionMap => Game.input?.gameplayActionMap;

    public static InputAction JumpAction => GameplayActionMap["Jump"];
    public static InputAction DodgeAction => GameplayActionMap["Dodge"];
}