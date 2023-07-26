namespace iPanel.InputProcess
{
    internal static class InputLine
    {
        public static void Input(string? line)
        {
            line = line?.Trim();
            if (line is null)
            {
                return;
            }

            Processor.Process(line);
        }
    }
}