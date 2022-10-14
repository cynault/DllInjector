// See https://aka.ms/new-console-template for more information

using DllInject;

var injectArgParser = new InjectArgParser(args);
var pid = Convert.ToUInt32(injectArgParser.GetOption("-pid"));
var dllPath = injectArgParser.GetOption("-dll");
var injector = Injector.BindProcess(pid);
var remoteHandle = injector.InjectLibrary(dllPath);
Console.WriteLine(remoteHandle.DangerousGetHandle().ToString());