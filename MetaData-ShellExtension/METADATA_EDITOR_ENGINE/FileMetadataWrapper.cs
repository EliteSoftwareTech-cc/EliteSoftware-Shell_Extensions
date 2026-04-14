#pragma warning disable CS8618, CS8600, CS8602, CS8604, CS8622, CS8625
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Reflection;

namespace MetadataEditor.Engine
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class FileMetadataWrapper : ICustomTypeDescriptor
    {
        private string _path;
        private bool _isDirectory;
        private Dictionary<string, string> _shellProperties = new Dictionary<string, string>();
        private List<AdsEngine.AdsStreamInfo> _adsStreams = new List<AdsEngine.AdsStreamInfo>();
        
        private static Dictionary<string, string> _cachedSystemProperties = new Dictionary<string, string>();
        private static readonly object _initLock = new object();
        private static string _cacheFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MetadataEditor", "property_cache.txt");

        public static event EventHandler SchemaUpdated;

        private static void EnsurePropertiesLoaded()
        {
            if (_cachedSystemProperties.Count > 0) 
            {
                // Even if loaded, fire a background refresh to catch system changes
                global::System.Threading.Tasks.Task.Run(() => BackgroundScan());
                return;
            }

            lock (_initLock)
            {
                if (_cachedSystemProperties.Count > 0) return;
                LoadFromCache();
                global::System.Threading.Tasks.Task.Run(() => BackgroundScan());
            }
        }

        private static void LoadFromCache()
        {
            try
            {
                if (global::System.IO.File.Exists(_cacheFilePath))
                {
                    var lines = global::System.IO.File.ReadAllLines(_cacheFilePath);
                    foreach (var line in lines)
                    {
                        var parts = line.Split('|');
                        if (parts.Length == 2) _cachedSystemProperties[parts[0]] = parts[1];
                    }
                }
            }
            catch { }

            if (_cachedSystemProperties.Count == 0)
            {
                _cachedSystemProperties["System.Author"] = "Authors";
                _cachedSystemProperties["System.Keywords"] = "Tags";
                _cachedSystemProperties["System.Title"] = "Title";
            }
        }

        private static void BackgroundScan()
        {
            Dictionary<string, string> latest = new Dictionary<string, string>();
            try
            {
                global::System.Type shellType = global::System.Type.GetTypeFromProgID("Shell.Application");
                dynamic shell = global::System.Activator.CreateInstance(shellType);
                var folder = shell.NameSpace(global::System.Environment.GetFolderPath(global::System.Environment.SpecialFolder.Windows));
                
                for (int i = 0; i < 1000; i++) // Increased scan depth
                {
                    string name = folder.GetDetailsOf(null, i);
                    if (!string.IsNullOrEmpty(name))
                    {
                        string canonical = "System." + name.Replace(" ", "");
                        latest[canonical] = name;
                    }
                }

                if (latest.Count > 0)
                {
                    bool changed = latest.Count != _cachedSystemProperties.Count;
                    lock (_initLock)
                    {
                        _cachedSystemProperties = latest;
                        SaveToCache();
                    }
                    if (changed) SchemaUpdated?.Invoke(null, EventArgs.Empty);
                }
            }
            catch { }
        }

        private static void SaveToCache()
        {
            try
            {
                global::System.IO.Directory.CreateDirectory(global::System.IO.Path.GetDirectoryName(_cacheFilePath));
                var lines = _cachedSystemProperties.Select(kvp => $"{kvp.Key}|{kvp.Value}");
                global::System.IO.File.WriteAllLines(_cacheFilePath, lines);
            }
            catch { }
        }

        public FileMetadataWrapper(string path = null)
        {
            EnsurePropertiesLoaded();
            _path = path;
            if (!string.IsNullOrEmpty(_path))
            {
                _isDirectory = Directory.Exists(_path);
                LoadShellProperties();
                LoadAdsInfo();
            }
        }

        public void RefreshValues()
        {
            if (string.IsNullOrEmpty(_path)) return;
            LoadShellProperties();
            LoadAdsInfo();
        }

        private void LoadShellProperties()
        {
            try
            {
                Type shellType = Type.GetTypeFromProgID("Shell.Application");
                dynamic shell = Activator.CreateInstance(shellType);
                string dir = Path.GetDirectoryName(_path);
                string file = Path.GetFileName(_path);
                if (string.IsNullOrEmpty(dir)) dir = Environment.CurrentDirectory;

                var folder = shell.NameSpace(dir);
                var folderItem = folder.ParseName(file);

                // Scan all known properties for values
                for (int i = 0; i < 1000; i++)
                {
                    string name = folder.GetDetailsOf(null, i);
                    if (string.IsNullOrEmpty(name)) continue;
                    string value = folder.GetDetailsOf(folderItem, i);
                    if (!string.IsNullOrEmpty(value))
                        _shellProperties[name] = value;
                }
            }
            catch { }
        }

        private void LoadAdsInfo()
        {
            if (string.IsNullOrEmpty(_path)) return;
            try
            {
                _adsStreams = AdsEngine.EnumerateStreams(_path);
            }
            catch { }
        }

        // --- ICustomTypeDescriptor Implementation ---
        public AttributeCollection GetAttributes() => TypeDescriptor.GetAttributes(this, true);
        public string GetClassName() => "File Metadata Wrapper";
        public string GetComponentName() => _path ?? "System Template";
        public TypeConverter GetConverter() => TypeDescriptor.GetConverter(this, true);
        public EventDescriptor GetDefaultEvent() => null;
        public PropertyDescriptor GetDefaultProperty() => null;
        public object GetEditor(Type editorBaseType) => null;
        public EventDescriptorCollection GetEvents() => null;
        public EventDescriptorCollection GetEvents(Attribute[] attributes) => null;

        public PropertyDescriptorCollection GetProperties() => GetProperties(null);

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            List<PropertyDescriptor> props = new List<PropertyDescriptor>();

            // 1. General
            props.Add(CreateReflectProp("Name", "1. General", false));
            props.Add(CreateReflectProp("FullPath", "1. General", true));
            
            // 2. Timestamps
            props.Add(CreateReflectProp("CreationTime", "2. Timestamps", false));
            props.Add(CreateReflectProp("LastAccessTime", "2. Timestamps", false));
            props.Add(CreateReflectProp("LastWriteTime", "2. Timestamps", false));
            
            // 3. Attributes
            props.Add(CreateReflectProp("ReadOnly", "3. Attributes", false));
            props.Add(CreateFlagProp("Hidden", "3. Attributes"));
            props.Add(CreateFlagProp("System", "3. Attributes"));
            props.Add(CreateFlagProp("Archive", "3. Attributes"));

            // 4. System Metadata (Editable Everywhere)
            lock (_initLock)
            {
                foreach (var kvp in _cachedSystemProperties)
                {
                    props.Add(new CustomPropertyDescriptor(kvp.Value, "4. System Metadata", false, $"Canonical: {kvp.Key}", kvp.Key));
                }
            }

            // 5. Alternate Data Streams (Only if path exists)
            if (!string.IsNullOrEmpty(_path))
            {
                foreach (var stream in _adsStreams)
                {
                    props.Add(new CustomPropertyDescriptor(stream.Name, "5. Alternate Data Streams", false, $"Size: {stream.Size} bytes", null));
                }
            }

            return new PropertyDescriptorCollection(props.ToArray());
        }

        private PropertyDescriptor CreateReflectProp(string name, string category, bool isReadOnly)
        {
            return new ReflectivePropertyDescriptor(this.GetType().GetProperty(name), category, isReadOnly);
        }

        private PropertyDescriptor CreateFlagProp(string name, string category)
        {
            return new ReflectivePropertyDescriptor(this.GetType().GetProperty(name), category, false);
        }

        public object GetPropertyOwner(PropertyDescriptor pd) => this;

        // --- Actual Properties for Reflection ---
        public string Name 
        { 
            get => string.IsNullOrEmpty(_path) ? "[TEMPLATE]" : (_isDirectory ? new DirectoryInfo(_path).Name : new FileInfo(_path).Name);
            set { /* Rename could go here */ }
        }
        public string FullPath => _path ?? "[NONE]";

        public DateTime CreationTime
        {
            get => string.IsNullOrEmpty(_path) ? DateTime.Now : (_isDirectory ? Directory.GetCreationTime(_path) : File.GetCreationTime(_path));
            set { if (!string.IsNullOrEmpty(_path)) { try { if (_isDirectory) Directory.SetCreationTime(_path, value); else File.SetCreationTime(_path, value); } catch {} } }
        }
        public DateTime LastAccessTime
        {
            get => string.IsNullOrEmpty(_path) ? DateTime.Now : (_isDirectory ? Directory.GetLastAccessTime(_path) : File.GetLastAccessTime(_path));
            set { if (!string.IsNullOrEmpty(_path)) { try { if (_isDirectory) Directory.SetLastAccessTime(_path, value); else File.SetLastAccessTime(_path, value); } catch {} } }
        }
        public DateTime LastWriteTime
        {
            get => string.IsNullOrEmpty(_path) ? DateTime.Now : (_isDirectory ? Directory.GetLastWriteTime(_path) : File.GetLastWriteTime(_path));
            set { if (!string.IsNullOrEmpty(_path)) { try { if (_isDirectory) Directory.SetLastWriteTime(_path, value); else File.SetLastWriteTime(_path, value); } catch {} } }
        }

        private FileAttributes CurrentAttributes
        {
            get => string.IsNullOrEmpty(_path) ? FileAttributes.Normal : (_isDirectory ? new DirectoryInfo(_path).Attributes : new FileInfo(_path).Attributes);
            set { if (!string.IsNullOrEmpty(_path)) { if (_isDirectory) new DirectoryInfo(_path).Attributes = value; else new FileInfo(_path).Attributes = value; } }
        }
        private bool HasFlag(FileAttributes flag) => (CurrentAttributes & flag) == flag;
        private void SetFlag(FileAttributes flag, bool value)
        {
            if (string.IsNullOrEmpty(_path)) return;
            var attrs = CurrentAttributes;
            if (value) attrs |= flag; else attrs &= ~flag;
            CurrentAttributes = attrs;
        }

        public bool ReadOnly { get => HasFlag(FileAttributes.ReadOnly); set => SetFlag(FileAttributes.ReadOnly, value); }
        public bool Hidden { get => HasFlag(FileAttributes.Hidden); set => SetFlag(FileAttributes.Hidden, value); }
        public bool System { get => HasFlag(FileAttributes.System); set => SetFlag(FileAttributes.System, value); }
        public bool Archive { get => HasFlag(FileAttributes.Archive); set => SetFlag(FileAttributes.Archive, value); }

        // --- Helper Descriptors ---
        private class CustomPropertyDescriptor : PropertyDescriptor
        {
            private string _category;
            private bool _readOnly;
            private string _description;
            private string _canonicalName;

            public CustomPropertyDescriptor(string name, string category, bool readOnly, string description, string canonical) : base(name, null)
            {
                _category = category;
                _readOnly = readOnly;
                _description = description;
                _canonicalName = canonical;
            }
            public override bool CanResetValue(object component) => false;
            public override Type ComponentType => typeof(FileMetadataWrapper);
            public override object GetValue(object component)
            {
                var w = (FileMetadataWrapper)component;
                if (_category.Contains("Metadata")) return w._shellProperties.ContainsKey(Name) ? w._shellProperties[Name] : "";
                if (_category.Contains("Streams"))
                {
                    try { return AdsEngine.ReadStream(w._path, Name); } catch { return "[Binary/Error]"; }
                }
                return "";
            }
            public override void ResetValue(object component) { }
            public override void SetValue(object component, object value)
            {
                var w = (FileMetadataWrapper)component;
                if (string.IsNullOrEmpty(w._path)) return;

                if (_category.Contains("Metadata") && !string.IsNullOrEmpty(_canonicalName))
                {
                    PropertySystemEngine.WriteProperty(w._path, _canonicalName, value?.ToString() ?? "");
                    w._shellProperties[Name] = value?.ToString() ?? "";
                }
                else if (_category.Contains("Streams"))
                {
                    try { AdsEngine.WriteStream(w._path, Name, value?.ToString() ?? ""); } catch { }
                }
            }
            public override bool ShouldSerializeValue(object component) => false;
            public override Type PropertyType => typeof(string);
            public override bool IsReadOnly => _readOnly;
            public override string Category => _category;
            public override string Description => _description;
        }

        private class ReflectivePropertyDescriptor : PropertyDescriptor
        {
            private PropertyInfo _pi;
            private string _cat;
            private bool _ro;
            public ReflectivePropertyDescriptor(PropertyInfo pi, string cat, bool ro) : base(pi.Name, null)
            {
                _pi = pi;
                _cat = cat;
                _ro = ro;
            }
            public override bool CanResetValue(object component) => false;
            public override Type ComponentType => typeof(FileMetadataWrapper);
            public override object GetValue(object component) => _pi.GetValue(component);
            public override void ResetValue(object component) { }
            public override void SetValue(object component, object value) => _pi.SetValue(component, value);
            public override bool ShouldSerializeValue(object component) => false;
            public override Type PropertyType => _pi.PropertyType;
            public override bool IsReadOnly => _ro;
            public override string Category => _cat;
        }
    }
}
