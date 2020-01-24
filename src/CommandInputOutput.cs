using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using SDG.Unturned;

namespace UnturnedConsoleModule
{
    public class CommandInputOutput : ICommandInputOutput
    {
        private ConcurrentQueue<string> _pendingInputs;
        private ConcurrentQueue<ConsoleOutput> _pendingOutputs;
        
        private Task _readingTask;
        private Task _writingTask;
        private CancellationTokenSource _token;

        public event CommandInputHandler inputCommitted;

        private struct ConsoleOutput
        {
            public string Value;
            public ConsoleColor Color;
        }
        
        public void initialize(CommandWindow commandWindow)
        {
            _pendingInputs = new ConcurrentQueue<string>();
            _pendingOutputs = new ConcurrentQueue<ConsoleOutput>();
            
            _token = new CancellationTokenSource();
            _readingTask = Task.Run(DoRead);
            _writingTask = Task.Run(async () => await DoWrite());
        }

        public void shutdown(CommandWindow commandWindow)
        {
            _token.Cancel();
        }

        public void update()
        {
            while (_pendingInputs.TryDequeue(out var input))
            {
                inputCommitted?.Invoke(input);
            }
        }

        public void outputInformation(string information)
        {
            _pendingOutputs.Enqueue(new ConsoleOutput
            {
                Value = information, Color = ConsoleColor.White
            });
        }

        public void outputWarning(string warning)
        {
            _pendingOutputs.Enqueue(new ConsoleOutput
            {
                Value = warning, Color = ConsoleColor.Yellow
            });
        }

        public void outputError(string error)
        {
            _pendingOutputs.Enqueue(new ConsoleOutput
            {
                Value = error, Color = ConsoleColor.Red
            });
        }

        private static void WriteLine(ConsoleOutput output)
        {
            Console.ForegroundColor = output.Color;
            Console.WriteLine(output.Value);
            Console.ForegroundColor = ConsoleColor.White;
        }
        
        private void DoRead()
        { 
            while (!_token.IsCancellationRequested)
            {
                try
                {
                    var line = Console.ReadLine();
                    if (!_token.IsCancellationRequested && !string.IsNullOrWhiteSpace(line)) 
                        _pendingInputs.Enqueue(line);
                }
                catch (Exception ex)
                {
                    if(!_token.IsCancellationRequested)
                        Console.WriteLine(ex);
                }
            }
        }
        
        private async Task DoWrite()
        {
            while (!_token.IsCancellationRequested)
            {
                while (_pendingOutputs.TryDequeue(out var output))
                {
                    WriteLine(output);
                }

                await Task.Delay(10);
            }
        }
    }
}
