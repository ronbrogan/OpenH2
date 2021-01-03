namespace OpenH2.Core.Scripting
{
    public static class ScriptOps
    {
        public const ushort Begin = 0;
        public const ushort BeginRandom = 1;
        public const ushort If = 2;
        public const ushort Set = 4;
        public const ushort And = 5;
        public const ushort Or = 6;
        public const ushort Add = 7;
        public const ushort Subtract = 8;
        public const ushort Multiply = 9;
        public const ushort Divide = 10;
        public const ushort Min = 11;
        public const ushort Max = 12;
        public new const ushort Equals = 13;
        public const ushort GreaterThan = 15;
        public const ushort LessThan = 16;
        public const ushort GreaterThanOrEqual = 17;
        public const ushort LessThanOrEqual = 18;
        public const ushort Sleep = 19;
        public const ushort SleepForever = 20;
        public const ushort SleepUntil = 21;
        public const ushort Not = 25;
        public const ushort Print = 27;
        public const ushort GameIsPlaytest = 569;
    }
}
