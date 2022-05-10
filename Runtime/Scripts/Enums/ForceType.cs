public enum ForceType
{
    Force = 0,
    Acceleration = 1,
    Impulse = 2,
    VelocityChange = 3,
    Wind = 4, // wind is essentially the same as force but operates by itself
    Gravity = 5, // gravity is essentially the same as acceleration but operates by itself
    Generic = 6 // generic is not linked with a defined behavior but is to be defined in scripts
}