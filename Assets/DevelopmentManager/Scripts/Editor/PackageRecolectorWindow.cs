using UnityEditor;
using UnityEditor.AssetPackage;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace dev.PackageRecolector
{
    public class PackageRecolectorWindow : EditorWindow
    {
        private PackageProfile currentProfile;
        private Vector2 scrollPosition;

        [MenuItem("Tools/Dev/Package Recolector")]
        public static void ShowWindow()
        {
            GetWindow<PackageRecolectorWindow>("Package Recolector");
        }

        private void OnGUI()
        {
            GUILayout.Label("Package Recolector - Triple List Builder", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Profile Selector
            currentProfile = (PackageProfile)EditorGUILayout.ObjectField("Active Profile", currentProfile, typeof(PackageProfile), false);

            if (currentProfile == null)
            {
                EditorGUILayout.HelpBox("Select or create a PackageProfile to begin.", MessageType.Info);
                if (GUILayout.Button("Create New Profile", GUILayout.Height(30)))
                {
                    CreateNewProfile();
                }
                return;
            }

            EditorGUILayout.Space();

            // Draw profile properties dynamically
            SerializedObject serializedObject = new SerializedObject(currentProfile);
            SerializedProperty propPackageName = serializedObject.FindProperty("packageName");
            SerializedProperty propAuthor = serializedObject.FindProperty("author");
            SerializedProperty propOutput = serializedObject.FindProperty("outputFolder");
            SerializedProperty propSourceFolders = serializedObject.FindProperty("sourceFolders");
            SerializedProperty propDemoFolders = serializedObject.FindProperty("demoFolders");
            SerializedProperty propSharedDemoFolders = serializedObject.FindProperty("sharedDemoFolders");

            EditorGUILayout.PropertyField(propPackageName);
            EditorGUILayout.PropertyField(propAuthor);
            EditorGUILayout.PropertyField(propOutput);

            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Core Folders Section
            GUILayout.Label("Package Core Folders:", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(propSourceFolders, true);

            EditorGUILayout.Space();

            // Demo Folders Section
            GUILayout.Label("Demo Package Folders:", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(propDemoFolders, true);

            EditorGUILayout.Space();

            // Shared Demo Folders Section
            GUILayout.Label("Shared Demo Folders (Demo/shared):", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(propSharedDemoFolders, true);

            EditorGUILayout.EndScrollView();

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            // Export button
            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.3f);
            if (GUILayout.Button("Build .unitypackage (Triple Lists)!", GUILayout.Height(45)))
            {
                BuildPackage();
            }
            GUI.backgroundColor = Color.white;
        }

        private void CreateNewProfile()
        {
            PackageProfile asset = ScriptableObject.CreateInstance<PackageProfile>();
            string path = EditorUtility.SaveFilePanelInProject("Save Profile", "DefaultPackageProfile", "asset", "Choose where to save the profile");
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                currentProfile = asset;
            }
        }

        private void BuildPackage()
        {
            if (currentProfile == null) return;

            if (string.IsNullOrEmpty(currentProfile.author) || string.IsNullOrEmpty(currentProfile.packageName))
            {
                EditorUtility.DisplayDialog("Error", "Author and Package Name cannot be empty.", "OK");
                return;
            }

            bool hasSource = currentProfile.sourceFolders != null && currentProfile.sourceFolders.Count > 0;
            bool hasDemo = currentProfile.demoFolders != null && currentProfile.demoFolders.Count > 0;
            bool hasSharedDemo = currentProfile.sharedDemoFolders != null && currentProfile.sharedDemoFolders.Count > 0;

            if (!hasSource && !hasDemo && !hasSharedDemo)
            {
                EditorUtility.DisplayDialog("Error", "No folders selected in the profile.", "OK");
                return;
            }

            string authorRoot = Path.Combine("Assets", currentProfile.author);

            // 1. Check if the Author folder already exists in Assets to prevent overwriting or conflicts
            if (Directory.Exists(authorRoot) || AssetDatabase.IsValidFolder(authorRoot))
            {
                EditorUtility.DisplayDialog(
                    "Collision Error",
                    $"A folder named '{currentProfile.author}' already exists in your Assets root! Operation aborted to prevent overwriting existing project structures.",
                    "OK"
                );
                return;
            }

            // 2. Setup directory structure directly in Assets
            string packageFolder = Path.Combine(authorRoot, currentProfile.packageName);
            string demosFolder = Path.Combine(authorRoot, "Demos");
            string demoPackageFolder = Path.Combine(demosFolder, currentProfile.packageName);
            string sharedDemoFolder = Path.Combine(demosFolder, "shared");

            Directory.CreateDirectory(packageFolder);

            try
            {
                // 3. Process Source Folders (Flattened into PackageName root)
                if (hasSource)
                {
                    foreach (var folderAsset in currentProfile.sourceFolders)
                    {
                        if (folderAsset == null) continue;

                        string sourcePath = AssetDatabase.GetAssetPath(folderAsset);
                        if (string.IsNullOrEmpty(sourcePath)) continue;

                        if (!ProcessFolderFlattened(sourcePath, packageFolder, authorRoot))
                            return;
                    }
                }

                // 4. Process Demo Folders (Flattened into Demos/PackageName root)
                if (hasDemo)
                {
                    Directory.CreateDirectory(demosFolder);
                    Directory.CreateDirectory(demoPackageFolder);

                    foreach (var folderAsset in currentProfile.demoFolders)
                    {
                        if (folderAsset == null) continue;

                        string sourcePath = AssetDatabase.GetAssetPath(folderAsset);
                        if (string.IsNullOrEmpty(sourcePath)) continue;

                        if (!ProcessFolderFlattened(sourcePath, demoPackageFolder, authorRoot))
                            return;
                    }
                }

                // 5. Process Shared Demo Folders (Flattened into Demos/shared root)
                if (hasSharedDemo)
                {
                    Directory.CreateDirectory(demosFolder);
                    Directory.CreateDirectory(sharedDemoFolder);

                    foreach (var folderAsset in currentProfile.sharedDemoFolders)
                    {
                        if (folderAsset == null) continue;

                        string sourcePath = AssetDatabase.GetAssetPath(folderAsset);
                        if (string.IsNullOrEmpty(sourcePath)) continue;

                        if (!ProcessFolderFlattened(sourcePath, sharedDemoFolder, authorRoot))
                            return;
                    }
                }

                AssetDatabase.Refresh();

                // 6. Prepare output folder
                if (!Directory.Exists(currentProfile.outputFolder))
                {
                    Directory.CreateDirectory(currentProfile.outputFolder);
                }

                string fileName = $"{currentProfile.packageName}_{System.DateTime.Now:yyyyMMdd_HHmmss}.unitypackage";
                string fullPath = Path.Combine(currentProfile.outputFolder, fileName);

                // 7. Export package using modern Unity API
                var parameters = new ExportPackageParameters(
                    new string[] { authorRoot },
                    fullPath,
                    CloudProjectSettings.organizationKey,
                    ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies
                );

                Package.Export(parameters);

                // 8. Thorough cleanup (removes folder and .meta file completely)
                Cleanup(authorRoot);

                Debug.Log($"<color=green>Package successfully generated with triple lists and cleaned!</color> Path: {fullPath}");
                EditorUtility.RevealInFinder(fullPath);
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("Export Error", $"An error occurred during export: {ex.Message}", "OK");
                Cleanup(authorRoot);
            }
        }

        private bool ProcessFolderFlattened(string sourcePath, string targetDestinationFolder, string authorRoot)
        {
            string absoluteSourcePath = Path.GetFullPath(sourcePath);
            string absoluteTargetFolder = Path.GetFullPath(targetDestinationFolder);

            // Copy subdirectories
            foreach (string subDir in Directory.GetDirectories(absoluteSourcePath))
            {
                string dirName = Path.GetFileName(subDir);
                string destSubDir = Path.Combine(absoluteTargetFolder, dirName);

                if (File.Exists(destSubDir))
                {
                    EditorUtility.DisplayDialog(
                        "Conflict Error",
                        $"Conflict detected: '{destSubDir}' already exists as a file. Operation stopped.",
                        "OK"
                    );
                    Cleanup(authorRoot);
                    return false;
                }

                FileUtil.CopyFileOrDirectory(subDir, destSubDir);
            }

            // Copy files
            foreach (string filePath in Directory.GetFiles(absoluteSourcePath))
            {
                if (Path.GetExtension(filePath).Equals(".meta", System.StringComparison.OrdinalIgnoreCase)) continue;

                string destFile = Path.Combine(absoluteTargetFolder, Path.GetFileName(filePath));

                if (Directory.Exists(destFile))
                {
                    EditorUtility.DisplayDialog(
                        "Conflict Error",
                        $"Conflict detected: '{destFile}' already exists as a folder. Operation stopped.",
                        "OK"
                    );
                    Cleanup(authorRoot);
                    return false;
                }

                FileUtil.CopyFileOrDirectory(filePath, destFile);
            }

            return true;
        }

        private void Cleanup(string authorRoot)
        {
            string absolutePath = Path.GetFullPath(authorRoot);
            string metaPath = absolutePath + ".meta";

            // Delete physical directory and all contents recursively
            if (Directory.Exists(absolutePath))
            {
                Directory.Delete(absolutePath, true);
            }

            // Delete the associated .meta file if it exists
            if (File.Exists(metaPath))
            {
                File.Delete(metaPath);
            }

            // Refresh Unity database to reflect full removal
            AssetDatabase.Refresh();
        }
    }
}