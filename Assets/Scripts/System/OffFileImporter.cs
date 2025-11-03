using UnityEditor;
using UnityEngine;

public class OffFileImporter
{
    // NOTE : 1 = Version of the importer (so Unity can reimport files if there's an upgrade to the importer
    // NOTE : "off" = File extension this importer is destined to
    [UnityEditor.AssetImporters.ScriptedImporter(1, "off")]
    public class OffImporter : UnityEditor.AssetImporters.ScriptedImporter
    {
        public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx)
        {
            // Read the file content (loads it as plain text) then wrap it in a TextAsset
            string text = System.IO.File.ReadAllText(ctx.assetPath);
            TextAsset asset = new TextAsset(text);

            // Add the TextAsset to Unity then mark it as the main object of the .off file
            ctx.AddObjectToAsset("main", asset);
            ctx.SetMainObject(asset);
        }
    }
}
