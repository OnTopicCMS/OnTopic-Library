/*==============================================================================================================================
| Author        Katherine Trunkey, Ignia LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Configuration;

namespace Ignia.Topics.Configuration {

  /*============================================================================================================================
  | CLASS: SOURCE ELEMENT COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a custom implementation of a <see cref="ConfigurationElementCollection"/> responsible for encapsulating a set
  ///   of <see cref="SourceElement"/> elements.
  /// </summary>
  /// <remarks>
  ///   Adapted DIRECTLY from the Ignia Localization library; in the future, these libraries may (and should) share custom
  ///   configuration classes.
  /// </remarks>
  public class SourceElementCollection : ConfigurationElementCollection {

    /*==========================================================================================================================
    | INDEXER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the <see cref="SourceElement"/> at the specified index.
    /// </summary>
    /// <param name="index">The integer index for the <see cref="SourceElement"/> item in the collection.</param>
    public SourceElement this[int index] {
      get {
        return base.BaseGet(index) as SourceElement;
      }
      set {
        if (base.BaseGet(index) != null) {
          base.BaseRemoveAt(index);
        }
        this.BaseAdd(index, value);
      }
    }

    /*==========================================================================================================================
    | METHOD: CREATE NEW ELEMENT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a new <see cref="ConfigurationElement"/>.
    /// </summary>
    /// <returns>A new instance of a <see cref="ConfigurationElement"/>.</returns>
    protected override ConfigurationElement CreateNewElement() {
      return new SourceElement();
    }

    /*==========================================================================================================================
    | METHOD: GET ELEMENT KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the key for the <see cref="ConfigurationElement"/> item in the collection.
    /// </summary>
    /// <param name="element">The <see cref="ConfigurationElement"/> element object from which to extract the key.</param>
    /// <returns>The Source string value for the <see cref="SourceElement"/> as the element's key.</returns>
    protected override object GetElementKey(ConfigurationElement element) {
      return ((SourceElement)element).Source;
    }

  } //Class

} //Namespace
