public enum ForceType
{
    Force = 0,
    Acceleration = 1,
    Impulse = 2,
    VelocityChange = 3,
    Gravity = 4, // gravity is essentially the same as acceleration but operates by itself
    Generic = 5 // generic is not linked with a defined behavior but is to be defined in scripts
}