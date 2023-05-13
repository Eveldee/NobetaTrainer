using System.Numerics;

namespace NobetaTrainer.Overlay;

public partial class TrainerOverlay
{
    private static readonly Vector4 ValueColor = new(252 / 255f, 161 / 255f, 3 / 255f, 1f);
    private static readonly Vector4 InfoColor = new(3 / 255f, 148 / 255f, 252 / 255f, 1f);
    private static readonly Vector4 WarningColor = new(252 / 255f, 211 / 255f, 3 / 255f, 1f);
    private static readonly Vector4 ErrorColor = new(255 / 255f, 0 / 255f, 0 / 255f, 1f);
    private static readonly Vector4 TitleColor = new(173 / 255f, 3 / 255f, 252 / 255f, 1f);

    private static readonly Vector4 SuccessButtonColor = new(30 / 255f, 199 / 255f, 24 / 255f, 1f);
    private static readonly Vector4 ErrorButtonColor = new(179 / 255f, 54 / 255f, 54 / 255f, 1f);
    private static readonly Vector4 PrimaryButtonColor = new(222 / 255f, 171 / 255f, 20 / 255f, 1f);
}