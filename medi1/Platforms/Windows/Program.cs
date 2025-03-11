using System;
using Microsoft.UI.Xaml;

namespace medi1.WinUI
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Microsoft.UI.Xaml.Application.Start(_ => new App());
        }
    }
}
