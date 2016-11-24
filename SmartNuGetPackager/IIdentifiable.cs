using System;
using System.IO;
using System.Windows.Input;
using RoboNuGet.Data;

namespace RoboNuGet
{
    internal interface IIdentifiable
    {
        string Name { get; }
    }
}