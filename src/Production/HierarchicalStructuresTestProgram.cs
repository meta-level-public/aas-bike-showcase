using System;
using AasDemoapp.Production;

namespace AasDemoapp.Production
{
    class HierarchicalStructuresTestProgram
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== HierarchicalStructures Test Console Application ===");
            Console.WriteLine();
            
            try
            {
                // Führe den Test aus
                HierarchicalStructuresTest.TestConversion();
                
                Console.WriteLine();
                Console.WriteLine("=== Test abgeschlossen ===");
                Console.WriteLine("Drücken Sie eine beliebige Taste zum Beenden...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Unerwarteter Fehler: {ex.Message}");
                Console.WriteLine($"Details: {ex.StackTrace}");
                Console.WriteLine();
                Console.WriteLine("Drücken Sie eine beliebige Taste zum Beenden...");
                Console.ReadKey();
            }
        }
    }
}
