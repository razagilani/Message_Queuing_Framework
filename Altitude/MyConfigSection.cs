using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Altitude
{
    public class MyConfigSection : ConfigurationSection {
    [ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
    public MyConfigInstanceCollection Instances {
        get { return (MyConfigInstanceCollection)this[""]; }
        set { this[""] = value; }
    }
}

    public class MyConfigInstanceElement : ConfigurationElement
    {
        //Make sure to set IsKey=true for property exposed as the GetElementKey above
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }


    } 
public class MyConfigInstanceCollection : ConfigurationElementCollection, IEnumerable<MyConfigInstanceElement> {
    protected override ConfigurationElement CreateNewElement() {
        return new MyConfigInstanceElement();
    }


    protected override object GetElementKey(ConfigurationElement element) {
        //set to whatever Element Property you want to use for a key
        var l_configElement = element as MyConfigInstanceElement;
        if (l_configElement != null)
            return l_configElement.Name;
        else
            return null;
    }

    public MyConfigInstanceElement this[int index]
    {
        get
        {
            return BaseGet(index) as MyConfigInstanceElement;
        }
    }
    #region IEnumerable<MyConfigInstanceElement> Members

    IEnumerator<MyConfigInstanceElement> IEnumerable<MyConfigInstanceElement>.GetEnumerator()
    {
        /*foreach(MyConfigInstanceElement item in (this as IEnumerable<MyConfigInstanceElement>))
        {
            yield return item;
        }*/
        return (from i in Enumerable.Range(0, this.Count)
                select this[i]).GetEnumerator();
    }

    #endregion
}


}
