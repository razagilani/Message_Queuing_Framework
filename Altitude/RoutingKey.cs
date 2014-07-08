using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Altitude
{
    public sealed class RoutingKey : ConfigurationSection
    {
        #region Member Variables
        private static RoutingKey settings = ConfigurationManager.GetSection("RoutingKey") as RoutingKey;
        #endregion
        #region Fields
        /// <summary>
        /// Settings Property
        /// </summary>
        public static RoutingKey Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// The Id
        /// </summary>
        [ConfigurationProperty("Name", DefaultValue = "", IsRequired = true)]
        public string Name
        {
            get { return (string)this["Name"]; }
            set { this["Name"] = value; }
        }

        /// <summary>
        /// The collection of services / methods
        /// </summary>
        [ConfigurationProperty("Parameters", IsDefaultCollection = true, IsKey = false, IsRequired = true)]
        public ParametersCollection Parameters
        {
            get { return base["Parameters"] as ParametersCollection; }
        }
        #endregion
        #region Methods
        #endregion
    }
    /// <summary>
    /// The ParametersCollection class
    /// </summary>
    public sealed class ParametersCollection : ConfigurationElementCollection
    {
        #region Member Variables
        #endregion
        #region Fields
        /// <summary>
        /// Create method
        /// </summary>
        /// <returns></returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new KeyElement();
        }
        /// <summary>
        /// Get element key
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((KeyElement)element).Name;   //key is method name for us
        }
        /// <summary>
        /// Name element
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        new public KeyElement this[string name]
        {
            get { return (KeyElement)base.BaseGet(name); }
        }
        /// <summary>
        /// Collection type setting
        /// </summary>
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }
        /// <summary>
        /// Index element
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public KeyElement this[int index]
        {
            get { return (KeyElement)base.BaseGet(index); }
        }
        /// <summary>
        /// Override the name
        /// </summary>
        protected override string ElementName
        {
            get
            {
                return "Key"; //force name of Key for elements
            }
        }
        #endregion
        #region Methods
        #endregion
    }
    /// <summary>
    /// The KeyElement class
    /// </summary>
    public sealed class KeyElement : ConfigurationElement
    {
        #region Member Variables
        #endregion
        #region Fields
        /// <summary>
        /// The Name
        /// </summary>
        [ConfigurationProperty("Name", DefaultValue = "", IsRequired = true)]
        public string Name
        {
            get { return (string)base["Name"]; }
            set { base["Name"] = value; }
        }
        /// <summary>
        /// Redeliver Attempts
        /// </summary>
        [ConfigurationProperty("redeliverAttempts", DefaultValue = "", IsRequired = true)]
        public string redeliverAttempts
        {
            get { return (string)base["redeliverAttempts"]; }
            set { base["redeliverAttempts"] = value; }
        }
        /// <summary>
        /// Redeliver Delay
        /// </summary>
        [ConfigurationProperty("redeliverDelay", DefaultValue = "", IsRequired = true)]
        public string redeliverDelay
        {
            get { return (string)base["redeliverDelay"]; }
            set { base["redeliverDelay"] = value; }
        }
        /// <summary>
        /// Manual Ack
        /// </summary>
        [ConfigurationProperty("manualAck", DefaultValue = "", IsRequired = true)]
        public string manualAck
        {
            get { return (string)base["manualAck"]; }
            set { base["manualAck"] = value; }
        }
        #endregion
        #region Methods
        #endregion
    }
 
}
