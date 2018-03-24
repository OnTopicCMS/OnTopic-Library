/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Ignia.Topics.Collections;

namespace Ignia.Topics {

  /*============================================================================================================================
  | CLASS: TOPIC MAPPING SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="ITopicMappingService"/> interface provides an abstraction for mapping <see cref="Topic"/> instances to
  ///   Data Transfer Objects, such as View Models.
  /// </summary>
  public class TopicMappingService : ITopicMappingService {

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
    ///       <item>Scalar values of types <see cref="Boolean"/>, <see cref="Int32"/>, or <see cref="String"/></item>
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
        var typeLookup = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        var matchedTypes = AppDomain
          .CurrentDomain
          .GetAssemblies()
          .SelectMany(t => t.GetTypes())
          .Where(t => t.IsClass && t.Name.EndsWith("TopicViewModel", StringComparison.InvariantCultureIgnoreCase))
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
      return typeof(object);

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
    ///     upfront, and it is imperative that it be strongly-typed, then prefer <see cref="Map{T}(Topic, Boolean, Boolean)"/>.
    ///   </para>
    ///   <para>
    ///     Because the target object is being dynamically constructed, it must implement a default constructor.
    ///   </para>
    /// </remarks>
    /// <param name="topic">The <see cref="Topic"/> entity to derive the data from.</param>
    /// <param name="includeRelationships">Determines whether the mapping should follow relationships to other topics.</param>
    /// <param name="includeParents">Determines whether the mapping should follow the ancestry via parents.</param>
    /// <returns>An instance of the dynamically determined View Model with properties appropriately mapped.</returns>
    public object Map(Topic topic, bool includeRelationships = true, bool includeParents = true) {

      /*----------------------------------------------------------------------------------------------------------------------
      | Handle null source
      \---------------------------------------------------------------------------------------------------------------------*/
      if (topic == null) return null;

      /*----------------------------------------------------------------------------------------------------------------------
      | Instantiate object
      \---------------------------------------------------------------------------------------------------------------------*/
      var contentType = topic.ContentType;
      var viewModelType = TopicMappingService.GetViewModelType(contentType);
      var target = Activator.CreateInstance(viewModelType);

      /*----------------------------------------------------------------------------------------------------------------------
      | Provide mapping
      \---------------------------------------------------------------------------------------------------------------------*/
      return Map(topic, target, includeRelationships, includeParents);

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
    /// <param name="includeParents">Determines whether the mapping should follow the ancestry via parents.</param>
    /// <returns>
    ///   An instance of the requested View Model <typeparamref name="T"/> with properties appropriately mapped.
    /// </returns>
    public T Map<T>(Topic topic, bool includeRelationships=true, bool includeParents = true) where T : class, new() {

      if (typeof(Topic).IsAssignableFrom(typeof(T))) {
        return topic as T;
      }
      var target = new T();
      return (T)Map(topic, target, includeRelationships, includeParents);

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
    /// <param name="includeParents">Determines whether the mapping should follow the ancestry via parents.</param>
    /// <returns>
    ///   The target view model with the properties appropriately mapped.
    /// </returns>
    public object Map(Topic topic, object target, bool includeRelationships = true, bool includeParents = true) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic == null || topic.IsDisabled) {
        return target;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle topics
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (typeof(Topic).IsAssignableFrom(target.GetType())) {
        return topic;
      }

      var sourceType = topic.GetType();
      var targetType = target.GetType();

      /*------------------------------------------------------------------------------------------------------------------------
      | Loop through properties, mapping each one
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var property in _typeCache.GetProperties(targetType)) {

        /*----------------------------------------------------------------------------------------------------------------------
        | Assign default value
        \---------------------------------------------------------------------------------------------------------------------*/
        var defaultValueAttribute = (DefaultValueAttribute)property.GetCustomAttribute(typeof(DefaultValueAttribute), true);
        var defaultValue = "";
        if (defaultValueAttribute != null) {
          property.SetValue(target, defaultValueAttribute.Value);
          defaultValue = defaultValueAttribute.Value.ToString();
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Case: Scalar Value
        \---------------------------------------------------------------------------------------------------------------------*/
        if (_typeCache.HasSettableProperty(targetType, property.Name)) {
          var getterMethod = sourceType.GetRuntimeMethod("Get" + property.Name, new Type[] { });
          var attributeValue = (string)null;
          //Attempt to get value from topic.Get{Property}()
          if (getterMethod != null) {
            attributeValue = getterMethod.Invoke(topic, new object[] { }).ToString();
          }
          //Otherwise, attempts to get value from topic.Attributes.GetValue({Property})
          if (String.IsNullOrEmpty(attributeValue)) {
            attributeValue = topic.Attributes.GetValue(property.Name, defaultValue);
          }
          if (attributeValue != null) {
            _typeCache.SetProperty(target, property.Name, attributeValue);
          }
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Case: Collections
        \---------------------------------------------------------------------------------------------------------------------*/
        else if (typeof(IList).IsAssignableFrom(property.PropertyType)) {

          //Determine the type of item in the list
          var listType = typeof(ITopicViewModelCore);
          if (property.PropertyType.IsGenericType) {
            //Uses last argument in case it's a KeyedCollection; in that case, we want the TItem type
            listType = property.PropertyType.GetGenericArguments().Last();
          }

          //Get source for list
          IList listSource = new Topic[] { };

          //Handle children
          if (property.Name.Equals("Children") && includeRelationships) {
            listSource = topic.Children.Sorted.ToList();
          }

          //Handle (outgoing) relationships
          if (listSource.Count == 0 && includeRelationships) {
            var relationshipName = property.Name;
            if (topic.Relationships.Contains(relationshipName)) {
              listSource = topic.Relationships.GetTopics(relationshipName);
            }
          }

          //Handle nested topics
          if (listSource.Count == 0) {
            if (topic.Children.Contains(property.Name) && topic.Children[property.Name].ContentType.Equals("List")) {
              listSource = topic.Children[property.Name].Children.Sorted.ToList();
            }
          }

          //Handle (incoming) relationships
          if (listSource.Count == 0 && includeRelationships) {
            var relationshipName = property.Name;
            if (topic.IncomingRelationships.Contains(relationshipName)) {
              listSource = topic.IncomingRelationships.GetTopics(relationshipName);
            }
          }

          //Ensure list is created
          var list = (IList)property.GetValue(target, null);
          if (list == null) {
            list = (IList)Activator.CreateInstance(property.PropertyType);
            property.SetValue(target, list);
          }

          //Validate and populate target collection
          if (listSource != null) {
            foreach (Topic childTopic in listSource) {
              if (!childTopic.IsDisabled) {
                //Handle scenario where the list type derives from Topic
                if (typeof(Topic).IsAssignableFrom(listType)) {
                  //Ensure the list item derives from the list type (which may be more derived than Topic)
                  if (listType.IsAssignableFrom(childTopic.GetType())) {
                    list.Add(childTopic);
                  }
                }
                //Otherwise, assume the list type is a DTO
                else {
                  var childDto = Map(childTopic, false, false);
                  //Ensure the mapped type derives from the list type
                  if (listType.IsAssignableFrom(childDto.GetType())) {
                    list.Add(childDto);
                  }
                }
              }
            }
          }

        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Case: Parent
        \---------------------------------------------------------------------------------------------------------------------*/
        else if (property.Name.Equals("Parent") && includeParents) {
          if (topic.Parent != null) {
            var parent = Map(topic.Parent, false, true);
            if (property.PropertyType.IsAssignableFrom(parent.GetType())) {
              property.SetValue(target, parent);
            }
          }
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Validate fields
        \---------------------------------------------------------------------------------------------------------------------*/
        foreach (ValidationAttribute validator in property.GetCustomAttributes(typeof(ValidationAttribute))) {
          validator.Validate(property.GetValue(target), property.Name);
        }

      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return result
      \-----------------------------------------------------------------------------------------------------------------------*/
      return target;

    }

  } //Interface
} //Namespace
