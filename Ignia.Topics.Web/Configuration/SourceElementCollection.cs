/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Configuration;
using System.Diagnostics.Contracts;

namespace Ignia.Topics.Web.Configuration {

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
    /// <requires description="The value from the getter must not be null." exception="T:System.ArgumentNullException">
    ///   value != null
    /// </requires>
    public SourceElement this[int index] {
      get => base.BaseGet(index) as SourceElement;
      set {
        Contract.Requires<ArgumentNullException>(value != null, "The value from the getter must not be null.");
        if (base.BaseGet(index) != null) {
          base.BaseRemoveAt(index);
        }
        BaseAdd(index, value);
      }
    }

    /*==========================================================================================================================
    | METHOD: CREATE NEW ELEMENT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a new <see cref="ConfigurationElement"/>.
    /// </summary>
    /// <returns>A new instance of a <see cref="ConfigurationElement"/>.</returns>
    protected override ConfigurationElement CreateNewElement() => new SourceElement();

    /*==========================================================================================================================
    | METHOD: GET ELEMENT KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the key for the <see cref="ConfigurationElement"/> item in the collection.
    /// </summary>
    /// <param name="element">The <see cref="ConfigurationElement"/> element object from which to extract the key.</param>
    /// <returns>The Source string value for the <see cref="SourceElement"/> as the element's key.</returns>
    protected override object GetElementKey(ConfigurationElement element) {
      Contract.Assume(
        ((SourceElement)element).Source != null,
        "Confirm the element's Source value is available when deriving its key."
      );
      return ((SourceElement)element).Source;
    }

  } // Class

} // Namespace
