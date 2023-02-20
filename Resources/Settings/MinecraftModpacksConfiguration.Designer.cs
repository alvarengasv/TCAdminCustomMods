﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Resources.Settings {
    using System;
    
    
    /// <summary>
    /// A strongly-typed resource class, for looking up localized strings, formatting them, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilderEx class via the ResXFileCodeGeneratorEx custom tool. (with TCAdmin modifications)
    // To add or remove a member, edit your .ResX file then rerun the ResXFileCodeGeneratorEx custom tool or rebuild your VS.NET project.
    // Copyright (c) Dmytro Kryvko 2006-2023 (http://dmytro.kryvko.googlepages.com/)
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("DMKSoftware.CodeGenerators.Tools.StronglyTypedResourceBuilderEx", "2.6.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
#if !SILVERLIGHT
    [global::System.Reflection.ObfuscationAttribute(Exclude=true, ApplyToMembers=true)]
#endif
    [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
    public partial class MinecraftModpacksConfiguration {
        
        private static string _fullClassName = "TCAdminCustomMods.Resources.Settings.Resources.Settings.MinecraftModpacksConfigur" +
            "ation";
        
        private static global::System.Resources.ResourceManager _resourceManager;
        
        private static object _internalSyncObject;
        
        private static global::System.Globalization.CultureInfo _resourceCulture = System.Globalization.CultureInfo.InvariantCulture;
        
        /// <summary>
        /// Initializes a MinecraftModpacksConfiguration object.
        /// </summary>
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public MinecraftModpacksConfiguration() {
        }
        
        /// <summary>
        /// Thread safe lock object used by this class.
        /// </summary>
        public static object InternalSyncObject {
            get {
                if (object.ReferenceEquals(_internalSyncObject, null)) {
                    global::System.Threading.Interlocked.CompareExchange(ref _internalSyncObject, new object(), null);
                }
                return _internalSyncObject;
            }
        }
        
        /// <summary>
        /// Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(_resourceManager, null)) {
                    global::System.Threading.Monitor.Enter(InternalSyncObject);
                    try {
                        if (object.ReferenceEquals(_resourceManager, null)) {
                            global::System.Threading.Interlocked.Exchange(ref _resourceManager, new global::System.Resources.ResourceManager("TCAdminCustomMods.Resources.Settings.MinecraftModpacksConfiguration", typeof(MinecraftModpacksConfiguration).Assembly));
                        }
                    }
                    finally {
                        global::System.Threading.Monitor.Exit(InternalSyncObject);
                    }
                }
                return _resourceManager;
            }
        }
        
        /// <summary>
        /// Overrides the current thread's CurrentUICulture property for all
        /// resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return _resourceCulture;
            }
            set {
                _resourceCulture = value;
            }
        }
        
        /// <summary>
        /// Looks up a localized string similar to 'Jar Variable Name'.
        /// </summary>
        public static string JarVariableName {
            get {
                string translated_text = TCAdmin.SDK.Language.LanguageManager.GetTranslatedText(System.Reflection.Assembly.GetExecutingAssembly().FullName, _fullClassName, "JarVariableName", System.Globalization.CultureInfo.CurrentCulture);
                if (object.ReferenceEquals(translated_text, null)) {
                    return ResourceManager.GetString(ResourceNames.JarVariableName, _resourceCulture);
                }
                else {
                    return translated_text;
                }
            }
        }
        
        /// <summary>
        /// Looks up a localized string similar to 'Minecraft Modpacks Settings'.
        /// </summary>
        public static string MinecraftModpacksSettings {
            get {
                string translated_text = TCAdmin.SDK.Language.LanguageManager.GetTranslatedText(System.Reflection.Assembly.GetExecutingAssembly().FullName, _fullClassName, "MinecraftModpacksSettings", System.Globalization.CultureInfo.CurrentCulture);
                if (object.ReferenceEquals(translated_text, null)) {
                    return ResourceManager.GetString(ResourceNames.MinecraftModpacksSettings, _resourceCulture);
                }
                else {
                    return translated_text;
                }
            }
        }
        
        /// <summary>
        /// Looks up a localized string similar to 'The game must be configured to allow creating a custom command line with a custom jar.'.
        /// </summary>
        public static string Requirement1 {
            get {
                string translated_text = TCAdmin.SDK.Language.LanguageManager.GetTranslatedText(System.Reflection.Assembly.GetExecutingAssembly().FullName, _fullClassName, "Requirement1", System.Globalization.CultureInfo.CurrentCulture);
                if (object.ReferenceEquals(translated_text, null)) {
                    return ResourceManager.GetString(ResourceNames.Requirement1, _resourceCulture);
                }
                else {
                    return translated_text;
                }
            }
        }
        
        /// <summary>
        /// Looks up a localized string similar to 'In this module&apos;s configuration you must select the variable used to specify the jar in your command line.'.
        /// </summary>
        public static string Requirement2 {
            get {
                string translated_text = TCAdmin.SDK.Language.LanguageManager.GetTranslatedText(System.Reflection.Assembly.GetExecutingAssembly().FullName, _fullClassName, "Requirement2", System.Globalization.CultureInfo.CurrentCulture);
                if (object.ReferenceEquals(translated_text, null)) {
                    return ResourceManager.GetString(ResourceNames.Requirement2, _resourceCulture);
                }
                else {
                    return translated_text;
                }
            }
        }
        
        /// <summary>
        /// Looks up a localized string similar to 'For best compatibility you can use &lt;a href=&quot;/PluginRepository/Details/6&quot; target=&quot;_blank&quot;&gt;this minecraft game config from the plugin repository&lt;/a&gt; (if you don&apos;t have it already).'.
        /// </summary>
        public static string Requirement3 {
            get {
                string translated_text = TCAdmin.SDK.Language.LanguageManager.GetTranslatedText(System.Reflection.Assembly.GetExecutingAssembly().FullName, _fullClassName, "Requirement3", System.Globalization.CultureInfo.CurrentCulture);
                if (object.ReferenceEquals(translated_text, null)) {
                    return ResourceManager.GetString(ResourceNames.Requirement3, _resourceCulture);
                }
                else {
                    return translated_text;
                }
            }
        }
        
        /// <summary>
        /// Looks up a localized string similar to 'Requirements'.
        /// </summary>
        public static string Requirements {
            get {
                string translated_text = TCAdmin.SDK.Language.LanguageManager.GetTranslatedText(System.Reflection.Assembly.GetExecutingAssembly().FullName, _fullClassName, "Requirements", System.Globalization.CultureInfo.CurrentCulture);
                if (object.ReferenceEquals(translated_text, null)) {
                    return ResourceManager.GetString(ResourceNames.Requirements, _resourceCulture);
                }
                else {
                    return translated_text;
                }
            }
        }
        
        /// <summary>
        /// Lists all the resource names as constant string fields.
        /// </summary>
        public class ResourceNames {
            
            /// <summary>
            /// Stores the resource name 'JarVariableName'.
            /// </summary>
            public const string JarVariableName = "JarVariableName";
            
            /// <summary>
            /// Stores the resource name 'MinecraftModpacksSettings'.
            /// </summary>
            public const string MinecraftModpacksSettings = "MinecraftModpacksSettings";
            
            /// <summary>
            /// Stores the resource name 'Requirement1'.
            /// </summary>
            public const string Requirement1 = "Requirement1";
            
            /// <summary>
            /// Stores the resource name 'Requirement2'.
            /// </summary>
            public const string Requirement2 = "Requirement2";
            
            /// <summary>
            /// Stores the resource name 'Requirement3'.
            /// </summary>
            public const string Requirement3 = "Requirement3";
            
            /// <summary>
            /// Stores the resource name 'Requirements'.
            /// </summary>
            public const string Requirements = "Requirements";
        }
    }
}