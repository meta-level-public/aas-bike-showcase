using System;
using System.IO;

namespace AasDemoapp.Production
{
    public class HandoverDocumentationTest
    {
        public static void TestConversion()
        {
            try
            {
                Console.WriteLine("Testing HandoverDocumentationCreator...");
                
                // Test the conversion
                var submodel = HandoverDocumentationCreator.CreateHandoverDocumentationFromJson();
                
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
                
                Console.WriteLine("Test completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Test failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
