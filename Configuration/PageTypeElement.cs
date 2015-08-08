/*==============================================================================================================================
| Author        Katherine Trunkey, Ignia LLC
| Client        Ignia
| Project       Topics Library
|
| Purpose       Provides a custom configuration element which represents a page type (default: TopicPage) as developed for the
|               application. Permits the application to define multiple page types.
|
\=============================================================================================================================*/
using System;
using System.ComponentModel;
using System.Configuration;

namespace Ignia.Topics.Configuration {

  /*===========================================================================================================================
  | CLASS
  \--------------------------------------------------------------------------------------------------------------------------*/
  public class PageTypeElement : ConfigurationElement {

  /*=========================================================================================================================
  | ATTRIBUTE: NAME
  \------------------------------------------------------------------------------------------------------------------------*/
  [ ConfigurationProperty("name", IsRequired=true, IsKey=true) ]
    public string Name {
      get {
        return this["name"] as string;
      }
    }

  /*=========================================================================================================================
  | ATTRIBUTE: TYPE
  \------------------------------------------------------------------------------------------------------------------------*/
  [ TypeConverter(typeof(TypeNameConverter)) ]
  [ ConfigurationProperty("type", IsRequired = false) ]
    public Type Type {
      get {
        return this["type"] as Type;
      }
    }

  } //Class

} //Namepsace

