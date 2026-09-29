using Fusion;

public enum EInputButton
{
    Jump
}

public struct GameplayInput : INetworkInput
{
    public float Horizontal;
    public NetworkButtons Buttons;
}