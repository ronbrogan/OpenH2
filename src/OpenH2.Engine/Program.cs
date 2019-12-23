namespace OpenH2.Engine
{
    class Program
    {
        static void Main(string[] args)
        {
            var engine = new Engine();
            var startArgs = new EngineStartParameters();

            if(args.Length > 0)
            {
                startArgs.LoadPathOverride = args[0];
            }

            engine.Start(startArgs);
        }
    }
}
