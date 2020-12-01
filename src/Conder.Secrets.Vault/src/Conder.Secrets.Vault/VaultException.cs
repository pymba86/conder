using System;

namespace Conder.Secrets.Vault
{
    internal sealed class VaultException : Exception
    {
        public string Key { get; }

        public VaultException(string key) : this(null, key)
        {
        }

        public VaultException(Exception exception, string key) : this(string.Empty, exception, key)
        {
        }

        public VaultException(string message, Exception exception, string key) : base(message, exception)
        {
            Key = key;
        }
    }
}