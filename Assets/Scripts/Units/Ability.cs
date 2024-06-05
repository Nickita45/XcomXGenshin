using System.Collections;

// An ability the unit can use in the battle, which makes.
public abstract class Ability
{

    public abstract string AbilityName { get; }
    public abstract string Description { get; }
    public virtual int ActualCooldown { get; set; }
    public abstract int MaxCooldown { get; }

    public virtual string Icon => AbilityName;

    public virtual bool IsAvailable => ActualCooldown <= 0;

    // the amount of action points the ability costs
    public abstract int ActionCost { get; } 
    public abstract TargetType TargetType { get; }
    public abstract IEnumerator Activate(Unit unit, object target);

    public override string ToString()
    {
        return AbilityName;
    }

}

// Defines what kind of target do you need to choose to
// be able to use this ability.
//
// Some abilities target your enemies, some have an AOE effect.
// Others may target the ability user themselves, or their allies.
public enum TargetType
{
    // targets the user (e.g. Overwatch, Hunker Down)
    Self,
    // targets a single enemy (e.g. Shoot)
    Enemy,
    // target is placing something
    Summon,
}