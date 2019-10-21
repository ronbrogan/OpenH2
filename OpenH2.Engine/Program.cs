namespace OpenH2.Engine
{
    class Program
    {
        static void Main(string[] args)
        {
            var engine = new Engine();

            engine.Start(new EngineStartParameters());
        }
    }
}
