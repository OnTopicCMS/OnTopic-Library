/*==============================================================================================================================
| Author        Katherine Trunkey, Ignia LLC 
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Configuration;

namespace Ignia.Topics.Configuration {

  /*============================================================================================================================
  | CLASS: PAGE TYPE ELEMENT COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a custom implementation of a <see cref="ConfigurationElementCollection"/> responsible for encapsulating a set
  ///   of <see cref="PageTypeElement"/> elements.
  /// </summary>
  /// <remarks>
  ///   Includes the Default property that exposes the default page type (<see cref="Ignia.Topics.Web.TopicPage"/>).
  /// </remarks>
  public class PageTypeElementCollection : ConfigurationElementCollection {

    /*==========================================================================================================================
    | INDEXER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the <see cref="PageTypeElement"/> at the specified index.
    /// </summary>
    /// <param name="index">The integer index for the <see cref="PageTypeElement"/> item in the collection.</param>
    public PageTypeElement this[int index] {
      get {
        return base.BaseGet(index) as PageTypeElement;
      }
      set {
        if (base.BaseGet(index) != null) {
          base.BaseRemoveAt(index);
        }
        this.BaseAdd(index, value);
      }
    }

    /*==========================================================================================================================
    | ATTRIBUTE: DEFAULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the default page type (e.g., <see cref="Ignia.Topics.Web.TopicPage"/>).
    /// </summary>
    [ConfigurationProperty("default", DefaultValue="TopicPage", IsRequired = false)]
    public string Default {
      get {
        return (string)base["default"];
      }
    }

    /*==========================================================================================================================
    | METHOD: CREATE NEW ELEMENT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a new <see cref="PageTypeElement"/>.
    /// </summary>
    /// <returns>A new instance of a <see cref="PageTypeElement"/>.</returns>
    protected override ConfigurationElement CreateNewElement() {
      return new PageTypeElement();
    }

    /*==========================================================================================================================
    | METHOD: GET ELEMENT KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the key for the <see cref="ConfigurationElement"/> item in the collection.
    /// </summary>
    /// <param name="element">The <see cref="ConfigurationElement"/> element object from which to extract the key.</param>
    /// <returns>The Name string value for the <see cref="PageTypeElement"/> as the element's key.</returns>
    protected override object GetElementKey(ConfigurationElement element) {
      return ((PageTypeElement)element).Name;
    }

  } // Class

} // Namespace
