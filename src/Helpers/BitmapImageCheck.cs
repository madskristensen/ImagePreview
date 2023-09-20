// Class adapted from: http://james-ramsden.com/get-file-types-supported-by-bitmapimage/
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;

namespace WpfApplication1.Classes
{
    /// <summary>
    /// Provides methods for checking whether a file can likely be opened as a BitmapImage, based upon its file extension
    /// </summary>
    public class BitmapImageCheck : IDisposable
    {
        private readonly string _baseKeyPath;
        private readonly RegistryKey _baseKey;
        private const string _wICDecoderCategory = "{7ED96837-96F0-4812-B211-F13C24117ED3}";

        public BitmapImageCheck()
        {
            _baseKeyPath = Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess ? "Wow6432Node\\CLSID" : "CLSID";
            _baseKey = Registry.ClassesRoot.OpenSubKey(_baseKeyPath, false);
            recalculateExtensions();
        }

        public static BitmapImageCheck Instance { get; } = new();

        #region properties
        /// <summary>
        /// File extensions that are supported by decoders found elsewhere on the system
        /// </summary>
        public string[] CustomSupportedExtensions { get; private set; }

        /// <summary>
        /// File extensions that are supported natively by .NET
        /// </summary>
        public string[] NativeSupportedExtensions { get; private set; }

        /// <summary>
        /// File extensions that are supported both natively by NET, and by decoders found elsewhere on the system
        /// </summary>
        public string[] AllSupportedExtensions { get; private set; }
        public string AllSupportedExtensionsString { get; private set; }
        #endregion

        #region public methods
        /// <summary>
        /// Check whether a file is likely to be supported by BitmapImage based upon its extension
        /// </summary>
        /// <param name="extension">File extension (with or without leading full stop), file name or file path</param>
        /// <returns>True if extension appears to contain a supported file extension, false if no suitable extension was found</returns>
        public bool IsExtensionSupported(string extension)
        {
            //prepare extension, should a full path be given
            if (extension.Contains("."))
            {
                extension = extension.Substring(extension.LastIndexOf('.') + 1);
            }
            extension = extension.ToUpper();
            extension = extension.Insert(0, ".");

            return AllSupportedExtensions.Contains(extension);
        }
        #endregion

        #region private methods
        /// <summary>
        /// Re-calculate which extensions are available on this system. It's unlikely this ever needs to be called outside of the constructor.
        /// </summary>
        private void recalculateExtensions()
        {
            CustomSupportedExtensions = GetSupportedExtensions().ToArray();
            NativeSupportedExtensions = new string[] { ".BMP", ".GIF", ".ICO", ".JPEG", ".PNG", ".TIF", ".TIFF", ".DDS", ".JPG", ".JXR", ".HDP", ".WDP", ".SVG", ".WMP" };

            string[] cse = CustomSupportedExtensions;
            string[] nse = NativeSupportedExtensions;
            string[] ase = new string[cse.Length + nse.Length];
            Array.Copy(nse, ase, nse.Length);
            Array.Copy(cse, 0, ase, nse.Length, cse.Length);
            AllSupportedExtensions = ase;
            AllSupportedExtensionsString = string.Join("|", ase.Select(a => a.TrimStart('.')));
        }

        /// <summary>
        /// Represents information about a WIC decoder
        /// </summary>
        private struct DecoderInfo
        {
            public string FriendlyName;
            public string FileExtensions;
        }

        /// <summary>
        /// Gets a list of additionally registered WIC decoders
        /// </summary>
        /// <returns></returns>
        private IEnumerable<DecoderInfo> GetAdditionalDecoders()
        {
            List<DecoderInfo> result = [];

            foreach (RegistryKey codecKey in GetCodecKeys())
            {
                DecoderInfo decoderInfo = new()
                {
                    FriendlyName = Convert.ToString(codecKey.GetValue("FriendlyName", "")),
                    FileExtensions = Convert.ToString(codecKey.GetValue("FileExtensions", ""))
                };
                result.Add(decoderInfo);
            }
            return result;
        }

        private List<string> GetSupportedExtensions()
        {
            IEnumerable<DecoderInfo> decoders = GetAdditionalDecoders();
            List<string> rtnlist = [];

            foreach (DecoderInfo decoder in decoders)
            {
                string[] extensions = decoder.FileExtensions.Split(',');
                foreach (string extension in extensions)
                {
                    rtnlist.Add(extension);
                }
            }
            return rtnlist;
        }

        private IEnumerable<RegistryKey> GetCodecKeys()
        {
            List<RegistryKey> result = [];

            if (_baseKey != null)
            {
                RegistryKey categoryKey = _baseKey.OpenSubKey(_wICDecoderCategory + "\\instance", false);
                if (categoryKey != null)
                {
                    // Read the guids of the registered decoders
                    _ = categoryKey.GetSubKeyNames();

                    foreach (string codecGuid in GetCodecGuids())
                    {
                        // Read the properties of the single registered decoder
                        RegistryKey codecKey = _baseKey.OpenSubKey(codecGuid);
                        if (codecKey != null)
                        {
                            result.Add(codecKey);
                        }
                    }
                }
            }

            return result;
        }

        private string[] GetCodecGuids()
        {
            if (_baseKey != null)
            {
                RegistryKey categoryKey = _baseKey.OpenSubKey(_wICDecoderCategory + "\\instance", false);
                if (categoryKey != null)
                {
                    // Read the guids of the registered decoders
                    return categoryKey.GetSubKeyNames();
                }
            }
            return null;
        }

        #endregion

        public void Dispose()
        {
            _baseKey.Dispose();
        }
    }
}