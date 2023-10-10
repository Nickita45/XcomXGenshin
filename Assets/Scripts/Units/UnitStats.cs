using UnityEngine;

public abstract class UnitStats : MonoBehaviour
{
    public abstract int MaxHP();

    // the base chance of aim for the units with ranged weapons
    public abstract int BaseAimPercent();

    // the base amount of action points that can be used for movement (dashing) or abilities
    public abstract int BaseActions();

    public abstract int MovementDistance();
    public abstract float VisionDistance();

    // movement speed (doesn't affect gameplay)
    public abstract float Speed();
}
