using System;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using Foundation;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Solitaire.Utils;
using Solitaire.ViewModels;
using UIKit;

namespace Solitaire.iOS;

public class Application
{
    // This is the main entry point of the application.
    static void Main(string[] args)
    {
        // if you want to use a different Application Delegate class from "AppDelegate"
        // you can specify it here.
        UIApplication.Main(args, null, typeof(AppDelegate));
    }


}