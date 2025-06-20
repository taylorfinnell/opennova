namespace OpenNova.Model;

public enum NodeType
{
    Unrecognized = 0,       // Default or unrecognized prefix

    // "C" + second char:
    CB = 1,                 // "CB" Generic Collision Box
    CS = 2,                 // "CS"
    CC = 3,                 // "CC"
    CL = 4,                 // "CL" Ladder Collision
    CV = 5,                 // "CV" 
    CA = 6,                 // "CA" Armory Collision
    CD = 9,                 // "CD" Collision For Doors
    CT = 10,                // "CT" Change team box
    CM = 11,                // "CM"
    CF = 13,                // "CF" Activate special functions
    CP = 19,                // "CP" Collision that only affects players (not AI)

    // "V" + second char:
    VC = 7,                 // "VC" Collision For Vehicles
    VK = 12,                // "VK"

    // "B" + second char:
    BB = 8,                 // "BB" (Blink Box)

    // "L" + second char:
    LP = 14,                // "LP"

    // "D" + second char:
    DH = 16,                // "DH"
    DM = 17,                // "DM"
    DL = 18,                // "DL"

    // "O" + second char:
    OB = 20,                // "OB"
    OS = 21,                // "OS"
    OP = 22,                // "OP"
    OH = 23,                // "OH"
}
