using System;

namespace ExternalConfiguration
{
    /// <exclude />
    public abstract class ProviderConfig
    {
        private string _environment;

        /// <exclude />
        protected ProviderConfig()
        {
            UseCache = true;
        }

        /// <summary>
        /// The environment where app runs.
        /// </summary>
        public string Environment
        {
            get => _environment;
            set
            {
                if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(Environment));

                _environment = value;
            }
        }

        /// <summary>
        /// If provider caches settings. Default: true.
        /// </summary>
        public bool UseCache { get; set; }
    }
}