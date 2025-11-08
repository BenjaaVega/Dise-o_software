namespace Shin_Megami_Tensei_View
{
    internal sealed class ConsoleIO
    {
        public const string Sep = "----------------------------------------";
        private readonly dynamic _view;

        public ConsoleIO(dynamic view) => _view = view;

        public void WL(string s) => _view.WriteLine(s);

        public void SepLine() => _view.WriteLine(Sep);

        public int ReadIntOrDefault(int fallback)
        {
            var s = _view.ReadLine();
            int n;
            return int.TryParse(s, out n) ? n : fallback;
        }
    }
}