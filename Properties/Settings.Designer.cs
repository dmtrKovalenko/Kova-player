﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Kova.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public int SelectedThemeIndex {
            get {
                return ((int)(this["SelectedThemeIndex"]));
            }
            set {
                this["SelectedThemeIndex"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int SelectedAccentIndex {
            get {
                return ((int)(this["SelectedAccentIndex"]));
            }
            set {
                this["SelectedAccentIndex"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("\r\n                    <ArrayOfString xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-" +
            "instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" />\r\n                ")]
        public global::System.Collections.ObjectModel.ObservableCollection<System.String> MusicFolderPath {
            get {
                return ((global::System.Collections.ObjectModel.ObservableCollection<System.String>)(this["MusicFolderPath"]));
            }
            set {
                this["MusicFolderPath"] = value;
            }
        }
    }
}
