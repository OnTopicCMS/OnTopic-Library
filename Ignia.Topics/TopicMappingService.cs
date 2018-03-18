﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Ignia.Topics.Collections;
using Ignia.Topics.Models;

namespace Ignia.Topics {

  /*============================================================================================================================
  | CLASS: TOPIC MAPPING SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="ITopicMappingService"/> interface provides an abstraction for mapping <see cref="Topic"/> instances to
  ///   Data Transfer Objects, such as View Models.
  /// </summary>
  public class TopicMappingService {

    /*==========================================================================================================================
    | STATIC VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    static                      Dictionary<string, Type>        _typeLookup                     = null;
    static                      TypeCollection                  _typeCache                      = new TypeCollection();

    /*==========================================================================================================================
    | METHOD: GET VIEW MODEL TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Static helper method for looking up a class type based on a string name.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     According to the default mapping rules, the following will each be mapped:
    ///     <list type="bullet">
    ///       <item>Scalar values of types <see cref="bool"/>, <see cref="int"/>, or <see cref="string"/></item>
    ///       <item>Properties named <c>Parent</c> (will reference <see cref="Topic.Parent"/>)</item>
    ///       <item>Collections named <c>Children</c> (will reference <see cref="Topic.Children"/>)</item>
    ///       <item>
    ///         Collections starting with <c>Related</c> (will reference corresponding <see cref="Topic.Relationships"/>)
    ///       </item>
    ///       <item>
    ///         Collections corresponding to any <see cref="Topic.Children"/> of the same name, and the type <c>ListItem</c>,
    ///         representing a nested topic list
    ///       </item>
    ///     </list>
    ///   </para>
    ///   <para>
    ///     Currently, this method uses reflection to lookup all types ending with <c>TopicViewModel</c> across all assemblies
    ///     and namespaces. This is incredibly non-performant, and can take over a second to execute. As such, this data is
    ///     cached for the duration of the application (it is not expected that new classes will be generated during the scope
    ///     of the application).
    ///   </para>
    /// </remarks>
    /// <param name="contentType">A string representing the key of the target content type.</param>
    /// <returns>A class type corresponding to the specified string, and ending with "TopicViewModel".</returns>
    /// <requires description="The contentType key must be specified." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(contentType)
    /// </requires>
    /// <requires
    ///   decription="The contentType should be an alphanumeric sequence; it should not contain spaces or symbols."
    ///   exception="T:System.ArgumentException">
    ///   !contentType.Contains(" ")
    /// </requires>
    private static Type GetViewModelType(string contentType) {

      /*----------------------------------------------------------------------------------------------------------------------
      | Validate contracts
      \---------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(contentType));
      Contract.Ensures(Contract.Result<Type>() != null);
      TopicFactory.ValidateKey(contentType);

      /*----------------------------------------------------------------------------------------------------------------------
      | Ensure cache is populated
      \---------------------------------------------------------------------------------------------------------------------*/
      if (_typeLookup == null) {
        var typeLookup = new Dictionary<string, Type>();
        var matchedTypes = AppDomain
          .CurrentDomain
          .GetAssemblies()
          .SelectMany(t => t.GetTypes())
          .Where(t => t.IsClass && t.Name.EndsWith("TopicViewModel"))
          .ToList();
        foreach (var type in matchedTypes) {
          var associatedContentType = type.Name.Replace("TopicViewModel", "");
          if (!typeLookup.ContainsKey(associatedContentType)) {
            typeLookup.Add(associatedContentType, type);
          }
        }
        _typeLookup = typeLookup;
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Return cached entry
      \---------------------------------------------------------------------------------------------------------------------*/
      if (_typeLookup.Keys.Contains(contentType)) {
        return _typeLookup[contentType];
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Return default
      \---------------------------------------------------------------------------------------------------------------------*/
      return typeof(Object);

    }

    /*==========================================================================================================================
    | METHOD: MAP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a topic, will identify any View Models named, by convention, "{ContentType}TopicViewModel" and populate them
    ///   according to the rules of the mapping implementation.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Because the class is using reflection to determine the target View Models, the return type is <see cref="Object"/>.
    ///     These results may need to be cast to a specific type, depending on the context. That said, strongly-typed views
    ///     should be able to cast the object to the appropriate View Model type. If the type of the View Model is known
    ///     upfront, and it is imperative that it be strongly-typed, then prefer <see cref="Map{T}(Topic)"/>.
    ///   </para>
    ///   <para>
    ///     Because the target object is being dynamically constructed, it must implement a default constructor.
    ///   </para>
    /// </remarks>
    /// <param name="topic">The <see cref="Topic"/> entity to derive the data from.</param>
    /// <param name="includeRelationships">Determines whether the mapping should follow relationships to other topics.</param>
    /// <returns>An instance of the dynamically determined View Model with properties appropriately mapped.</returns>
    public object Map(Topic topic, bool includeRelationships = true) {

      var contentType = topic.ContentType;
      var viewModelType = TopicMappingService.GetViewModelType(contentType);
      var target = Activator.CreateInstance(viewModelType);
      return Map(topic, target, includeRelationships);

    }

    /*==========================================================================================================================
    | METHOD: MAP (T)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a topic and a generic type, will instantiate a new instance of the generic type and populate it according to the
    ///   rules of the mapping implementation.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Because the target object is being dynamically constructed, it must implement a default constructor.
    ///   </para>
    /// </remarks>
    /// <param name="topic">The <see cref="Topic"/> entity to derive the data from.</param>
    /// <param name="includeRelationships">Determines whether the mapping should follow relationships to other topics.</param>
    /// <returns>
    ///   An instance of the requested View Model <typeparamref name="T"/> with properties appropriately mapped.
    /// </returns>
    public T Map<T>(Topic topic, bool includeRelationships=true) where T : class, new() {

      var target = new T();
      return (T)Map(topic, target, includeRelationships);

    }

    /*==========================================================================================================================
    | METHOD: MAP (OBJECTS)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a topic and an instance of a DTO, will populate the DTO according to the default mapping rules.
    /// </summary>
    /// <param name="topic">The <see cref="Topic"/> entity to derive the data from.</param>
    /// <param name="target">The target object to map the data to.</param>
    /// <param name="includeRelationships">Determines whether the mapping should follow relationships to other topics.</param>
    /// <returns>
    ///   The target view model with the properties appropriately mapped.
    /// </returns>
    public object Map(Topic topic, object target, bool includeRelationships = true) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic.IsDisabled) {
        return target;
      }

      var targetType = target.GetType();

      /*------------------------------------------------------------------------------------------------------------------------
      | Loop through properties, mapping each one
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (PropertyInfo property in _typeCache.GetProperties(targetType)) {

        /*----------------------------------------------------------------------------------------------------------------------
        | Case: Scalar Value
        \---------------------------------------------------------------------------------------------------------------------*/
        if (_typeCache.HasSettableProperty(targetType, property.Name)) {
          var attributeValue = topic.Attributes.GetValue(property.Name);
          if (attributeValue != null) {
            _typeCache.SetProperty(target, property.Name, attributeValue);
          }
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Case: Collections
        \---------------------------------------------------------------------------------------------------------------------*/
        else if (typeof(IList).IsAssignableFrom(property.PropertyType)) {

          //Determine the type of item in the list
          var listType = typeof(TopicViewModel);
          if (property.PropertyType.IsGenericType) {
            listType = property.PropertyType.GetGenericArguments()[0];
          }

          //Get source for list
          IList listSource = new Topic[] { };

          //Handle children
          if (property.Name.Equals("Children") && includeRelationships) {
            listSource = topic.Children;
          }

          //Handle relationships
          if (listSource.Count == 0 && includeRelationships) {
            var relationshipName = property.Name;
            if (topic.Relationships.Contains(relationshipName)) {
              listSource = topic.Relationships.GetTopics(relationshipName);
            }
          }

          //Handle nested topics
          if (listSource.Count == 0) {
            if (topic.Children.Contains(property.Name) && topic.Children[property.Name].ContentType.Equals("TopicList")) {
              listSource = topic.Children[property.Name].Children;
            }
          }

          //Validate and populate target collection
          IList list = (IList)property.GetValue(target, null);
          foreach (Topic childTopic in listSource) {
            if (!childTopic.IsDisabled) {
              var childDto = Map(childTopic, false);
              if (listType.IsAssignableFrom(childDto.GetType())) {
                list.Add(childDto);
              }
            }
          }

        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Case: Parent
        \---------------------------------------------------------------------------------------------------------------------*/
        else if (property.Name.Equals("Parent") && includeRelationships) {
          if (topic.Parent != null) {
            var parent = Map(topic.Parent, false);
            if (property.PropertyType.IsAssignableFrom(parent.GetType())) {
              property.SetValue(target, parent);
            }
          }
        }

      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return result
      \-----------------------------------------------------------------------------------------------------------------------*/
      return target;

    }

  } //Interface
} //Namespace