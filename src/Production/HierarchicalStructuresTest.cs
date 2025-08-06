using System;
using System.IO;

namespace AasDemoapp.Production
{
    public class HierarchicalStructuresTest
    {
        public static void TestConversion()
        {
            try
            {
                Console.WriteLine("Testing HierarchicalStructuresCreator...");
                
                // Test the conversion
                var submodel = HierarchicalStructuresCreator.CreateFromJson();
                
                Console.WriteLine($"✓ Successfully created submodel with ID: {submodel.Id}");
                Console.WriteLine($"✓ IdShort: {submodel.IdShort}");
                Console.WriteLine($"✓ Number of submodel elements: {submodel.SubmodelElements?.Count ?? 0}");
                
                if (submodel.Description != null && submodel.Description.Count > 0)
                {
                    Console.WriteLine($"✓ Description: {submodel.Description[0].Text}");
                }
                
                if (submodel.Administration != null)
                {
                    Console.WriteLine($"✓ Version: {submodel.Administration.Version}");
                    Console.WriteLine($"✓ Revision: {submodel.Administration.Revision}");
                }
                
                // Test specific HierarchicalStructures content
                if (submodel.SubmodelElements != null)
                {
                    foreach (var element in submodel.SubmodelElements)
                    {
                        Console.WriteLine($"✓ Found submodel element: {element.IdShort} (Type: {element.GetType().Name})");
                        
                        // Check if it's a SubmodelElementCollection (typical for BOM structures)
                        if (element is AasCore.Aas3_0.SubmodelElementCollection collection && collection.Value != null)
                        {
                            Console.WriteLine($"  └─ Collection contains {collection.Value.Count} elements");
                            foreach (var subElement in collection.Value)
                            {
                                Console.WriteLine($"     └─ {subElement.IdShort} ({subElement.GetType().Name})");
                            }
                        }
                    }
                }
                
                Console.WriteLine("HierarchicalStructures test completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Test failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
