using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace dev.PackageRecolector { 
    [CreateAssetMenu(fileName = "NewPackageProfile", menuName = "PackageRecolector/Package Profile")]
    public class PackageProfile : ScriptableObject
    {
        [Tooltip("Base name for the generated .unitypackage file.")]
        public string packageName = "MyPackage";
        public string author = "PublisherName";

        [Tooltip("Destination folder where the package will be saved.")]
        public string outputFolder = "Exports";

        [Header("Folders to Include in the Package")]
        [Tooltip("Drag any folder here that should be part of this package.")]
        public List<DefaultAsset> sourceFolders = new List<DefaultAsset>();

        [Header("Demo folders to Include in the Package demo")]
        [Tooltip("Drag any folder here that should be part of the demo of this package.")]
        public List<DefaultAsset> demoFolders = new List<DefaultAsset>();
        [Header("Shared Demo data folder")]
        [Tooltip("Drag any folder here that should be part of the demo of this package.")]
        public List<DefaultAsset> sharedDemoFolders = new List<DefaultAsset>();
    }
}