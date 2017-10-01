using Iit.Fibertest.UtilsLib;
using RtuManagement;


namespace ConsoleAppOtdr
{
    internal class Program
    {
        static void Main()
        {
            var iniFile35 = new IniFile();
            iniFile35.AssignFile("service.ini");
//            var culture = iniFile35.Read(IniSection.General, IniKey.Culture, "ru-RU");
//            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
//            Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
//
//
            var logger35 = new LogFile(iniFile35).WithFile("service.log");
//
//            logger35.EmptyLine();
//            logger35.EmptyLine('-');
//            logger35.AppendLine("Application started.");

            var rtuManager = new RtuManager(logger35, iniFile35);
            rtuManager.Initialize(); 
        }

    }
}