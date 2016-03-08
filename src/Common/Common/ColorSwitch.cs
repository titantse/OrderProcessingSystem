
namespace OrderProcessing.Logger
{
    using System;
    using System.Threading;
    using OrderProcessing.Domain;
    /// <summary>
    /// A helper class for the word color of console output.
    /// </summary>
    public class ColorSwitch : BaseDisposable
    {
        private static readonly object lockObj = new object();
        private readonly ConsoleColor oldColor;

        public ColorSwitch(ConsoleColor color)
        {
            Monitor.Enter(lockObj);
            oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
        }

        protected override void Disposing()
        {
            Console.ForegroundColor = oldColor;
            Monitor.Exit(lockObj);
        }
    }
}