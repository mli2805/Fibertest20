﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GMap.NET.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("GMap.NET.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to CREATE TABLE IF NOT EXISTS Tiles (id INTEGER NOT NULL PRIMARY KEY, X INTEGER NOT NULL, Y INTEGER NOT NULL, Zoom INTEGER NOT NULL, Type UNSIGNED INTEGER  NOT NULL, CacheTime DATETIME);
        ///CREATE INDEX IF NOT EXISTS IndexOfTiles ON Tiles (X, Y, Zoom, Type);
        ///
        ///CREATE TABLE IF NOT EXISTS TilesData (id INTEGER NOT NULL PRIMARY KEY CONSTRAINT fk_Tiles_id REFERENCES Tiles(id) ON DELETE CASCADE, Tile BLOB NULL);
        ///
        ///-- Foreign Key Preventing insert
        ///CREATE TRIGGER fki_TilesData_id_Tiles_id
        ///BEFORE INSERT ON [TilesDat [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string CreateTileDb {
            get {
                return ResourceManager.GetString("CreateTileDb", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to (own/total) {0} / {1}.
        /// </summary>
        internal static string SID__own_total___0_____1_ {
            get {
                return ResourceManager.GetString("SID__own_total___0_____1_", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Automatic.
        /// </summary>
        internal static string SID_Automatic {
            get {
                return ResourceManager.GetString("SID_Automatic", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Critical.
        /// </summary>
        internal static string SID_Critical {
            get {
                return ResourceManager.GetString("SID_Critical", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Exceeded the number of RTU for an existing license.
        /// </summary>
        internal static string SID_Exceeded_the_number_of_RTU_for_an_existing_license {
            get {
                return ResourceManager.GetString("SID_Exceeded_the_number_of_RTU_for_an_existing_license", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to License could not be applied twice!.
        /// </summary>
        internal static string SID_License_could_not_be_applied_twice_ {
            get {
                return ResourceManager.GetString("SID_License_could_not_be_applied_twice_", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Manual.
        /// </summary>
        internal static string SID_Manual {
            get {
                return ResourceManager.GetString("SID_Manual", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to RTU initialized successfully.
        /// </summary>
        internal static string SID_RTU_initialized_successfully {
            get {
                return ResourceManager.GetString("SID_RTU_initialized_successfully", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to RTU with address {0} doesn&apos;t support BOP.
        /// </summary>
        internal static string SID_RTU_with_address__0__doesn_t_support_BOP {
            get {
                return ResourceManager.GetString("SID_RTU_with_address__0__doesn_t_support_BOP", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There is a trace with the same title.
        /// </summary>
        internal static string SID_There_is_a_trace_with_the_same_title {
            get {
                return ResourceManager.GetString("SID_There_is_a_trace_with_the_same_title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        internal static byte[] System_Data_SQLite_x64_NET2_dll {
            get {
                object obj = ResourceManager.GetObject("System_Data_SQLite_x64_NET2_dll", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        internal static byte[] System_Data_SQLite_x64_NET4_dll {
            get {
                object obj = ResourceManager.GetObject("System_Data_SQLite_x64_NET4_dll", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        internal static byte[] System_Data_SQLite_x86_NET2_dll {
            get {
                object obj = ResourceManager.GetObject("System_Data_SQLite_x86_NET2_dll", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        internal static byte[] System_Data_SQLite_x86_NET4_dll {
            get {
                object obj = ResourceManager.GetObject("System_Data_SQLite_x86_NET4_dll", resourceCulture);
                return ((byte[])(obj));
            }
        }
    }
}
