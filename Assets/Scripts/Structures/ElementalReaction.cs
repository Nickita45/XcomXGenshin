public enum ElementalReaction
{
    MeltStrong,
    MeltWeak,
    VaporizeStrong,
    VaporizeWeak,
    ElectroCharged,
    Overloaded,
    Superconduct,
    // A reaction that occurs when attacking a unit with superconduct
    // using physical damage,
    // may refactor in future
    SuperconductActivate,
    Freeze,
    Shatter,
    // 4 versions of Swirl and Crystallize
    SwirlPyro,
    SwirlCryo,
    SwirlHydro,
    SwirlElectro,
    CrystallizePyro,
    CrystallizeCryo,
    CrystallizeHydro,
    CrystallizeElectro
}